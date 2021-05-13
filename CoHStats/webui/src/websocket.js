export const isEmbedded = typeof cefSharp === 'object';
let gCallback = () => {
  // NOOP
};


let wsUri = "ws://127.0.0.1:59123/";
let websocket = undefined;

if (!isEmbedded) {
  websocket = createSocket();

  setInterval(
    () => {
      if (!isConnected()) {
        console.log("RECONNECTING..");
        websocket = createSocket();
      }
    }, 5000);
}

const isConnected = () => websocket && (websocket.readyState === WebSocket.OPEN || websocket.readyState === WebSocket.CONNECTING);

function createSocket() {
  console.log('CreateSocket');
  if (websocket && (websocket.readyState === WebSocket.OPEN || websocket.readyState === WebSocket.CONNECTING)) {
    websocket?.close();
  }
  websocket = new WebSocket(wsUri);

  websocket.onopen = function () {
    gCallback("CONNECTED", '');
    websocket.send("Client active");
  };

  websocket.onclose = function () {
    gCallback("DISCONNECTED", '');
  };

  websocket.onmessage = function (e) {
    gCallback('DATA', e.data);
  };

  // TODO: see into better 'close' logic? https://stackoverflow.com/questions/22431751/websocket-how-to-automatically-reconnect-after-it-dies
  websocket.onerror = function (e) {
    if (e && e.data)
      console.error(JSON.stringify(e.data));

    websocket?.close();
  };

  console.log("Socket created");
  return websocket;
}

export default function RegisterCallback(callback) {
  gCallback = callback;
  console.log("Websocket call registered");
}
