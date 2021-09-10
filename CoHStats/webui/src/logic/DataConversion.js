import _ from 'lodash';

export function CreateLinegraphDataSeries(dataset, subset) {
  const lossColors = ['#ff0000','#ff560d','#ff5a58','#ff57aa'];
  let lossIdx = 0;
  let result = [];
  for (let idx = 0; idx < dataset.length; idx++) {
    const player = dataset[idx];

    result.push({
      name: `${player.name} - Kills`,
      type: 'line',
      data: _.map(player[subset], (p) => p.totalKilled),
      smooth: true
    });

    result.push({
      name: `${player.name} - Losses`,
      type: 'line',
      data: _.map(player[subset], (p) => p.infantryLost + p.vehiclesLost + p.buildingsLost),
      smooth: true,
      lineStyle: {
        color: lossColors[lossIdx++],
        width: 2,
        type: 'dotted',
        opacity: 0.10,
      },
      areaStyle: {
        opacity: 0.15,
      },
    });
  }

  return result;
}

export function CreateLinegraphTitle(dataset) {
  let result = [];
  console.log('yada', dataset);
  for (let idx = 0; idx < dataset.length; idx++) {
    const player = dataset[idx];
    if (player.stats.length > 0)
      result.push(`${player.name}: ${player.stats[player.stats.length - 1].totalKilled}`);
    else
      result.push(`${player.name}`);
  }

  console.log('resulty', result);

  return result.join(' - ');
}