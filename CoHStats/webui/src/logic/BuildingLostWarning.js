function play() {
  var audio = new Audio('https://interactive-examples.mdn.mozilla.net/media/cc0-audio/t-rex-roar.mp3');
  audio.play();
}

let lastIdx = -1;
export function WarnIfLostBuilding(dataset) {
  for (let idx = 0; idx < dataset.length; idx++) {
    const player = dataset[idx];

    console.log("Checking" , player);
    if (player.deltas.length > 0 && player.deltas[player.deltas.length-1].buildingsLost > 0) {
      if (idx !== lastIdx) {
        play();
        console.log("Oh noes! Building lost!");
        lastIdx = idx;
      }
    }
  }
}