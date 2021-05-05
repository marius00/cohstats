let state = 'DISCONNECTED';
let wsUri = "ws://127.0.0.1:59123/";
let websocket = createSocket();

type State = 'CONNECTED' | 'DISCONNECTED' | 'DATA';
let gCallback = (a: State, b: string) => {
  // NOOP
};

setInterval(
  () => {
    if (state !== 'CONNECTED') {
      console.log("RECONNECTING..");
      websocket = createSocket();
    }
  }, 1500);

function createSocket(): WebSocket {
  websocket?.close();
  websocket = new WebSocket(wsUri);

  websocket.onopen = function (e: Event) {
    gCallback("CONNECTED", '');
    websocket.send("Client active");
    state = 'CONNECTED';
  };

  websocket.onclose = function (e: CloseEvent) {
    gCallback("DISCONNECTED", '');
    state = 'DISCONNECTED';
  };

  websocket.onmessage = function (e: MessageEvent) {
    gCallback('DATA', e.data);
  };

  websocket.onerror = function (e: any) {
    console.log(JSON.stringify(e.data));
  };

  return websocket;
}

export default function RegisterCallback(callback: (state: State, data: string) => void) {
  gCallback = callback;
}
