<template>
    <h1 v-if="!isConnected" class="game-not-running">Not connected to the stats engine</h1>
    <div v-if="isConnected">
        <h1 v-if="!isGameRunning" class="game-not-running">The game does not appear to be running</h1>
        <button v-if="!isGameRunning && !isEmbedded" v-on:click="swapData">Swap data</button>

        <div>
            <LineGraph :option="deltaGraphOptions" label="Recent combat" ref="chart"/>


            <LineGraph :option="totalGraphOptions" label="Entire game" />

        </div>
    </div>

    <CheatSheet />
</template>

<script>
  import _ from 'lodash';
  import LineGraph from './components/LineGraph.vue';
  import CheatSheet from './components/CheatSheet.vue';
  import RegisterCallback, {isEmbedded} from './websocket';
  import {LineChart} from "echarts/charts";
  import {CreateLinegraphDataSeries, CreateLinegraphTitle} from "./logic/DataConversion";
  import {WarnIfLostBuilding} from "./logic/BuildingLostWarning";


  const V = {
    name: 'App',
    components: {
      CheatSheet,
      LineGraph,
      LineChart
    },
    data() {
      return {
        isConnected: isEmbedded,
        isGameRunning: false,
        isEmbedded: isEmbedded,

        // See https://morioh.com/p/b18a8267ce29
        deltaGraphOptions: {
          series: [],
          xAxis: {
            type: 'category',
            boundaryGap: false,
            data: [],
            name: 'Seconds into the game'
          },
          yAxis: [
            {type: 'value', name: 'Losses'},
            {type: 'value', name: 'Kills'}
          ],
          tooltip: {
            show: true,
            trigger: 'axis'
          },
          grid: {
            left: '3%',
            right: '4%',
            bottom: '3%',
            containLabel: true
          },
          legend: {
          },
        },
        totalGraphOptions: {
          series: [],
          xAxis: {
            type: 'category',
            boundaryGap: false,
            data: [],
            name: 'Seconds into the game'
          },
          yAxis: [
            {type: 'value', name: 'Losses'},
            {type: 'value', name: 'Kills'}
          ],
          tooltip: {
            show: true,
            trigger: 'axis'
          },
          grid: {
            left: '0%',
            right: '4%',
            bottom: '3%',
            containLabel: true
          },
        }
      }
    },
    methods: {
      /*
      swapData: function () {
        // console.log(this.$refs.chart.refreshOption, (this.$refs.chart));
        this.deltaGraphOptions = {...this.deltaGraphOptions, series: CreateLinegraphDataSeries(DummyThree.data, 'deltas')};
        this.totalGraphOptions = {...this.totalGraphOptions, series: CreateLinegraphDataSeries(DummyThree.data, 'stats')};
        this.totalGraphTitle = CreateLinegraphTitle(DummyThree.data);
      },*/
      setData: function (d) {
        if (!d || !d.data)
          return;

        this.isGameRunning = !!d.isGameRunning;
        let seriesDelta = CreateLinegraphDataSeries(d.data, 'deltas');
        const length = seriesDelta.length > 0 ? seriesDelta[0].data.length : 0;
        this.deltaGraphOptions = {
          ...this.deltaGraphOptions,
          series: seriesDelta,
          xAxis: {
            type: 'category',
            boundaryGap: false,
            data: _.range(0, length),
            name: 'Seconds into the game'
          },
          title: {
            text: "Kills per second " + CreateLinegraphTitle(d.data)
          },
        };
        console.log("Set delta graph to", seriesDelta, this.deltaGraphOptions.xAxis);

        let seriesTotal = CreateLinegraphDataSeries(d.data, 'stats');
        this.totalGraphOptions = {
          ...this.totalGraphOptions,
          series: seriesTotal,
          xAxis: {
            type: 'category',
            boundaryGap: false,
            data: _.range(0, length),
            name: 'Seconds into the game'
          },
          title: {
            text: "Total kills " + CreateLinegraphTitle(d.data)
          },
        };

        WarnIfLostBuilding(d.data);
      }
    },
    created: function () {
      if (isEmbedded) {
        console.log("Running inside embedded CefSharp, using magic data object");
        setInterval(() => {
          /* eslint-disable-next-line */
          if (typeof data !== 'undefined' && typeof data.graphJson !== 'undefined') {
            // TODO: Deduplicate this
            /* eslint-disable-next-line */
            var d = JSON.parse(data.graphJson);
            this.setData(d);
          }
        }, 1000);
      } else {
        console.log("Running outside of embedded CefSharp, using websockets");
        RegisterCallback((event, socketData) => {
          switch (event) {
            case 'CONNECTED':
              this.isConnected = true;
              break;
            case 'DISCONNECTED ':
              this.isConnected = false;
              break;
            case 'DATA':
              console.log("yayt data", this.setData);

              this.setData(JSON.parse(socketData));
              break;

          }

          console.debug('Type:', event, 'Data:', socketData);
        });
      }
    }
  };

  export default V;


</script>

<style scoped>
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
