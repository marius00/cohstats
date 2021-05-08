<template>
    <h1 v-if="!isConnected">Not connected to the stats engine</h1>
    <div v-if="isConnected">
        <h1 v-if="!isGameRunning" className="game-not-running">The game does not appear to be running</h1>
        <button v-if="!isGameRunning" v-on:click="swapData">Swap data</button>

        <div v-if="isGameRunning">
            <h2>{{deltaGraphTitle}}</h2>
            <LineGraph :option="deltaGraphOptions" label="Recent combat" ref="chart"/>


            <h2>{{totalGraphTitle}}</h2>
            <LineGraph :option="totalGraphOptions" label="Entire game" ref="totalChart"/>

        </div>
    </div>

    <CheatSheet />
</template>

<script>
  import _ from 'lodash';
  import LineGraph from './components/LineGraph.vue';
  import CheatSheet from './components/CheatSheet.vue';
  import RegisterCallback from './websocket';
  import {LineChart} from "echarts/charts";
  import ECharts from 'echarts';

  const DummyDataset = [
    {
      name: 'T.o - Kills',
      type: 'line',
      data: [120, 132, 101, 134, 90, 230, 210],
      yAxisIndex: 1,
      smooth: true
    },
    {
      name: 'Evil - Kills',
      type: 'line',
      data: [220, 182, 191, 234, 290, 730, 310],
      yAxisIndex: 1,
      smooth: true
    },
    {
      name: 'Evil - Losses',
      type: 'line',
      data: [2, 3, 3, 0, 0, 4, 15],
      smooth: true,
      itemStyle: {
        normal: {
          lineStyle: {
            color: '#ff3700',
            width: 2,
            type: 'dotted'
          }
        }
      }
    },
    {
      name: 'T.o - Losses',
      type: 'line',
      data: [0, 55, 22, 23, 24, 10, 0],
      smooth: true,
      itemStyle: {
        normal: {
          lineStyle: {
            color: '#ff8602',
            width: 2,
            type: 'dotted'
          }
        }
      }
    }
  ];
  const DummyTwo = {
    "isGameRunning": true,
    "data": [
      {
        "stats": [
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 33,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          }
        ],
        "deltas": [
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          }
        ],
        "name": "Slippery Pete"
      }
    ]
  };
  const DummyThree = {
    "isGameRunning": true,
    "data": [
      {
        "stats": [
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          }
        ],
        "deltas": [
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 2,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 0,
            "buildingsDestroyed": 0,
            "infantryLost": 0,
            "vehiclesLost": 0,
            "buildingsLost": 0
          },
          {
            "totalKilled": 0,
            "infantryKilled": 0,
            "vehiclesDestroyed": 5,
            "buildingsDestroyed": 0,
            "infantryLost": 2,
            "vehiclesLost": 0,
            "buildingsLost": 0
          }
        ],
        "name": "Slippery Pete"
      }
    ]
  };

  function CreateLinegraphDataSeries(dataset, subset) {
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
              color: '#ff8602',
              width: 2,
              type: 'dotted'
            }
          }
        }
      });
    }

    return result;
  }


  function CreateLinegraphTitle(dataset) {
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

  const V = {
    name: 'App',
    components: {
      CheatSheet,
      LineGraph,
      LineChart, ECharts // Does nothing
    },
    data() {
      return {
        isConnected: false,
        isGameRunning: false,
        deltaGraphTitle: '',
        totalGraphTitle: '',

        // See https://morioh.com/p/b18a8267ce29
        deltaGraphOptions: {
          // TODO: Could be we need to inherit some default options here, tooltip stopped working once this was used.
          // See https://github.com/ecomfe/vue-echarts "const option = ref({"
          series: CreateLinegraphDataSeries(DummyTwo.data, 'deltas'),
          xAxis: {
            type: 'category',
            boundaryGap: false,
            data: _.range(0, CreateLinegraphDataSeries(DummyThree.data, 'deltas')),
            name: 'Seconds into the game'
          },
          yAxis: [
            {type: 'value', name: 'Losses'},
            {type: 'value', name: 'Kills'}
          ],
        },
        totalGraphOptions: {
          series: CreateLinegraphDataSeries(DummyTwo.data, 'stats'),
          xAxis: {
            type: 'category',
            boundaryGap: false,
            data: _.range(0, CreateLinegraphDataSeries(DummyThree.data, 'stats')),
            name: 'Seconds into the game'
          },
          yAxis: [
            {type: 'value', name: 'Losses'},
            {type: 'value', name: 'Kills'}
          ],
        }
      }
    },
    methods: {
      swapData: function () {
        // console.log(this.$refs.chart.refreshOption, (this.$refs.chart));
        this.deltaGraphOptions = {...this.deltaGraphOptions, series: CreateLinegraphDataSeries(DummyThree.data, 'deltas')};
        this.totalGraphOptions = {...this.totalGraphOptions, series: CreateLinegraphDataSeries(DummyThree.data, 'stats')};
        this.deltaGraphTitle = CreateLinegraphTitle(DummyThree.data);
        this.totalGraphTitle = CreateLinegraphTitle(DummyThree.data);
      }
    },
    created: function () {
      RegisterCallback((event, data) => {
        switch (event) {
          case 'CONNECTED':
            this.isConnected = true;
            break;
          case 'DISCONNECTED ':
            this.isConnected = false;
            break;
          case 'DATA': {
            var d = JSON.parse(data);
            this.isGameRunning = !!d.isGameRunning;
            let seriesDelta = CreateLinegraphDataSeries(d.data, 'deltas');
            this.deltaGraphOptions = {...this.deltaGraphOptions, series: seriesDelta};
            this.deltaGraphTitle = CreateLinegraphTitle(d.data);

            let seriesTotal = CreateLinegraphDataSeries(d.data, 'stats');
            this.totalGraphOptions = {...this.totalGraphOptions, series: seriesTotal};
            this.totalGraphTitle = CreateLinegraphTitle(d.data);
            break;
          }
        }
      })
    }
  };

  export default V;


</script>

<style>
    #app {
        font-family: Avenir, Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-align: center;
        color: #2c3e50;
        margin-top: 60px;
    }


    .game-not-running {
        color: red;
    }

</style>
