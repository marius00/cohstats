let state = 'DISCONNECTED';
let wsUri = "ws://127.0.0.1:59123/";
let websocket = createSocket();


let gCallback = () => {
  // NOOP
};

setInterval(
  () => {
    if (state !== 'CONNECTED') {
      console.debug("RECONNECTING..");
      websocket = createSocket();
    }
  }, 5000);

function createSocket() {
  if (websocket) {
    if (websocket.readyState === WebSocket.OPEN || websocket.readyState === WebSocket.CONNECTING) {
      websocket?.close();
    }
  }
  websocket = new WebSocket(wsUri);

  websocket.onopen = function () {
    gCallback("CONNECTED", '');
    websocket.send("Client active");
    state = 'CONNECTED';
  };

  websocket.onclose = function () {
    gCallback("DISCONNECTED", '');
    state = 'DISCONNECTED';
  };

  websocket.onmessage = function (e) {
    gCallback('DATA', e.data);
  };

  // TODO: see into better 'close' logic? https://stackoverflow.com/questions/22431751/websocket-how-to-automatically-reconnect-after-it-dies
  websocket.onerror = function (e) {
    if (e && e.data)
      console.log(JSON.stringify(e.data));
  };

  return websocket;
}

export default function RegisterCallback(callback) {
  gCallback = callback;
}
