<template>
    <h1 v-if="!isConnected" className="game-not-running">Not connected to the stats engine</h1>
    <div v-if="isConnected">
        <h1 v-if="!isGameRunning" className="game-not-running">The game does not appear to be running</h1>
        <button v-if="!isGameRunning" v-on:click="swapData">Swap data</button>

        <div v-if="isGameRunning">
            <h2>{{deltaGraphTitle}}</h2>
            <LineGraph :option="deltaGraphOptions" label="Recent combat" ref="chart"/>


            <h2>{{totalGraphTitle}}</h2>
            <LineGraph :option="totalGraphOptions" label="Entire game" />

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
  import {DummyThree, DummyTwo} from "./TestData";
  import {CreateLinegraphDataSeries, CreateLinegraphTitle} from "./DataConversion";



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
