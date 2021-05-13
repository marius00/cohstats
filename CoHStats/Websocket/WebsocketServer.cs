using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CoHStats.Websocket {
    class WebsocketServer : IDisposable {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WebsocketServer));
        private readonly int _port;
        private const int Timeout = 30000;

        private readonly ConcurrentDictionary<string, ClientData> _clients = new ConcurrentDictionary<string, ClientData>();

        private Thread _listener;
        private Thread _processor;
        private TcpListener _server;

        public event EventHandler OnReceived;
        public event EventHandler OnClientConnect;

        public WebsocketServer(int port) {
            _port = port;
        }

        public void Write(string data) {
            foreach (var client in _clients.Values) {
                Write(client, data);
            }
        }

        public void Write(ClientData clientDetails, string data) {
            NetworkStream stream = clientDetails.Stream;

            Logger.Debug($"Sending data to {clientDetails.Id}");
            byte[] d = GetFrameFromString(data);
            try {
                stream.Write(d, 0, d.Length);
            }
            catch (System.IO.IOException ex) {
                Logger.Warn($"Error writing to client {clientDetails.Id}");
            }
        }

        private void StartListening() {
            _server = new TcpListener(IPAddress.Parse("127.0.0.1"), _port);
            _server.Start();

            while (_listener != null && _listener.IsAlive) {
                try {
                    TcpClient client = _server.AcceptTcpClient(); // TODO: Exception thrown: 'System.Net.Sockets.SocketException' in System.dll -- when closing the program
                    client.SendTimeout = Timeout;
                    client.ReceiveTimeout = Timeout;
                    NetworkStream stream = client.GetStream();

                    var c = new ClientData(Guid.NewGuid().ToString()) {
                        Client = client,
                        Stream = stream,
                        LastAck = DateTimeOffset.UtcNow,
                    };

                    _clients.TryAdd(c.Id, c);
                }
                catch (System.Net.Sockets.SocketException ex) {
                    Logger.Info(ex.Message, ex);
                }
            }
        }

        private void Processor() {
            while (_processor != null && _processor.IsAlive) {
                foreach (var client in _clients.Values) {
                    Process(client);
                }

                Thread.Sleep(1);
            }
        }


        public void Start() {
            if (_listener != null) {
                throw new ThreadStateException("Attempting to create multiple threads");
            }

            _listener = new Thread(StartListening);
            _listener.Start();

            _processor = new Thread(Processor);
            _processor.Start();
        }

        public void Process(ClientData data) {
            TcpClient client = data.Client;
            NetworkStream stream = data.Stream;

            if (data.LastAck.AddMilliseconds(Timeout) < DateTimeOffset.UtcNow) {
                Logger.Debug($"Sending ACK to {data.Id}");
                byte[] d = GetFrameFromString("ACK", EOpcodeType.Ping);
                try {
                    stream.Write(d, 0, d.Length);
                }
                catch (System.IO.IOException ex) {
                    // Client disconnected
                    Logger.Info($"Client {data.Id} disconnected");
                    _clients.TryRemove(data.Id, out _);
                }

                data.LastAck = DateTimeOffset.UtcNow;
            }

            if (!stream.DataAvailable) return;
            while (client.Available < 3) ; // match against "get"

            byte[] bytes = new byte[client.Available];
            try {
                stream.Read(bytes, 0, bytes.Length);
            }
            catch (IOException ex) {
                Logger.Warn($"Error reading from websocket, disconnecting client. ({ex.Message})", ex);
                _clients.TryRemove(data.Id, out _);
                return;
            }

            string s = Encoding.UTF8.GetString(bytes);

            if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase)) {
                Logger.Debug($"=====Handshaking from client=====\n{s}");

                // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                // 3. Compute SHA-1 and Base64 hash of the new value
                // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                byte[] response = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 101 Switching Protocols\r\n" +
                    "Connection: Upgrade\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                try {
                    stream.Write(response, 0, response.Length);
                }
                catch (IOException ex) {
                    Logger.Warn($"Error writing to websocket, disconnecting client. ({ex.Message})", ex);
                    _clients.TryRemove(data.Id, out _);
                    return;
                }

                OnClientConnect?.Invoke(this, new OnSocketReadEventArg {Id = data.Id});
            }
            else {
                bool fin = (bytes[0] & 0b10000000) != 0,
                    mask = (bytes[1] & 0b10000000) !=
                           0; // must be true, "All messages from the client to the server have this bit set"

                int opcode = bytes[0] & 0b00001111, // expecting 1 - text message
                    msglen = bytes[1] - 128, // & 0111 1111
                    offset = 2;

                if (msglen == 126) {
                    // was ToUInt16(bytes, offset) but the result is incorrect
                    msglen = BitConverter.ToUInt16(new byte[] {bytes[3], bytes[2]}, 0);
                    offset = 4;
                }
                else if (msglen == 127) {
                    Logger.Debug("TODO: msglen == 127, needs qword to store msglen");
                    // i don't really know the byte order, please edit this
                    // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
                    // offset = 10;
                }

                if (msglen == 0)
                    Logger.Debug("msglen == 0");
                else if (mask) {
                    byte[] decoded = new byte[msglen];
                    byte[] masks = new byte[4] {bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]};
                    offset += 4;

                    for (int i = 0; i < msglen; ++i)
                        decoded[i] = (byte) (bytes[offset + i] ^ masks[i % 4]);

                    if (decoded.Length == 2 && decoded[0] == 3 && decoded[1] == 233) {
                        Logger.Info($"Client {data.Id} disconnected gracefully");
                        _clients.TryRemove(data.Id, out _);
                        return;
                    }

                    string text = Encoding.UTF8.GetString(decoded);

                    Logger.Debug($"Received OpCode=`{(EOpcodeType)opcode}` Text=`{text}`");
                    OnReceived?.Invoke(this, new OnSocketReadEventArg {Id = data.Id, Content = text});
                    data.LastAck = DateTimeOffset.UtcNow;
                }
                else
                    Logger.Warn("mask bit not set");
            }
        }


        private enum EOpcodeType {
            /* Denotes a continuation code */
            Fragment = 0,

            /* Denotes a text code */
            Text = 1,

            /* Denotes a binary code */
            Binary = 2,

            /* Denotes a closed connection */
            ClosedConnection = 8,

            /* Denotes a ping*/
            Ping = 9,

            /* Denotes a pong */
            Pong = 10
        }

        private static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text) {
            byte[] response;
            byte[] bytesRaw = Encoding.Default.GetBytes(Message);
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte) (128 + (int) Opcode);
            if (length <= 125) {
                frame[1] = (byte) length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535) {
                frame[1] = (byte) 126;
                frame[2] = (byte) ((length >> 8) & 255);
                frame[3] = (byte) (length & 255);
                indexStartRawData = 4;
            }
            else {
                frame[1] = (byte) 127;
                frame[2] = (byte) ((length >> 56) & 255);
                frame[3] = (byte) ((length >> 48) & 255);
                frame[4] = (byte) ((length >> 40) & 255);
                frame[5] = (byte) ((length >> 32) & 255);
                frame[6] = (byte) ((length >> 24) & 255);
                frame[7] = (byte) ((length >> 16) & 255);
                frame[8] = (byte) ((length >> 8) & 255);
                frame[9] = (byte) (length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++) {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++) {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            Logger.Debug($"Produced Frame(Message:`{Message}`, OpCode=`{Opcode}`)");
            return response;
        }

        public void Dispose() {
            _server.Stop();
            // _listener?.Abort();
            _listener = null;

            // _processor?.Abort();
            _processor = null;
        }
    }
}