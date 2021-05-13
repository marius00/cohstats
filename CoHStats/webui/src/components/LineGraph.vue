<template>
    <v-chart class="chart" :option="option"/>
</template>

<script>
  import {use} from "echarts/core";
  import {CanvasRenderer} from "echarts/renderers";

  import {GridComponent, LegendComponent, TitleComponent, ToolboxComponent, TooltipComponent} from "echarts/components";
  import VChart, {THEME_KEY} from "vue-echarts";
  import {LineChart} from "echarts/charts";

  use([
    CanvasRenderer,
    LineChart,
    GridComponent,
    ToolboxComponent,
    TitleComponent,
    TooltipComponent,
    LegendComponent
  ]);

  export default {
    name: "LineGraph",
    components: {
      VChart
    },
    provide: {
      [THEME_KEY]: "light"
    },
    props: {
      label: String,
    },
    data() {
      return {
        option: {
          title: {
            text: this.label
          },
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
          series: []
        }
      };
    }
  };
</script>

<style scoped>
    .chart {
        height: 300px;
    }
</style>