using System;
using System.Net.Sockets;

namespace CoHStats.Websocket {

    class ClientData {
        public ClientData(string id) {
            Id = id;
        }
        public string Id { get; }
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public DateTimeOffset LastAck { get; set; }
    }
}
