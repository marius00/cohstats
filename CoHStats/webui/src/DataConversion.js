import _ from 'lodash';

export function CreateLinegraphDataSeries(dataset, subset) {
  let result = [];
  for (let idx = 0; idx < dataset.length; idx++) {
    const player = dataset[idx];

    result.push({
      name: `${player.name} - Kills`,
      type: 'line',
      data: _.map(player[subset], (p) => p.totalKilled),
      yAxisIndex: 1,
      smooth: true
    });

    result.push({
      name: `${player.name} - Losses`,
      type: 'line',
      data: _.map(player[subset], (p) => p.infantryLost + p.vehiclesLost + p.buildingsLost),
      smooth: true,
      itemStyle: {
        normal: {
          lineStyle: {
            color: '#ff8602', // TODO: Add a mix of red ish colors here
            width: 2,
            type: 'dotted'
          }
        }
      }
    });
  }

  return result;
}

export function CreateLinegraphTitle(dataset) {
  let result = [];
  for (let idx = 0; idx < dataset.length; idx++) {
    const player = dataset[idx];
    if (player.stats.length > 0)
      result.push(`${player.name}: ${player.stats[player.stats.length - 1].totalKilled}`);
    else
      result.push(`${player.name}`);
  }

  return result.join(' - ');
}