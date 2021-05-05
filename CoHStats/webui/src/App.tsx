import * as React from 'react';
import './App.css';
import {AreaChart, LineData} from 'react-easy-chart';
import CheatSheet from "./CheatSheet";
import UnitData from './UnitData.js';
import RegisterCallback from "./WebSocket";

interface HumanAiKillCountAggregate {
  cpuKills: Array<Array<LineData>>;
  playerKills: Array<Array<LineData>>;
  playerLabel: string;
}

interface GraphAggregate {
  perPlayerGraph: Array<MapperData>;
  humanAiGraph: HumanAiKillCountAggregate;
  isGameRunning: boolean;
}

interface MapperData {
  graph: Array<Array<LineData>>;
  name: string;
}

class App extends React.Component {
  /*state = {
    dataset: [] as Array<MapperData>,
    humanAiGraph: {cpuKills: [], playerKills: [], playerLabel: ''} as HumanAiKillCountAggregate,
    isGameRunning: false,
    isConnected: false,
  };*/

  state = {
    "dataset": [
      {
        "graph": [
          [
            {
              "x": 239,
              "y": 6
            },
            {
              "x": 240,
              "y": 6
            },
            {
              "x": 241,
              "y": 9
            },
            {
              "x": 242,
              "y": 9
            },
            {
              "x": 243,
              "y": 9
            },
            {
              "x": 244,
              "y": 9
            },
            {
              "x": 245,
              "y": 9
            },
            {
              "x": 246,
              "y": 9
            },
            {
              "x": 247,
              "y": 9
            },
            {
              "x": 248,
              "y": 9
            },
            {
              "x": 249,
              "y": 9
            },
            {
              "x": 250,
              "y": 9
            },
            {
              "x": 251,
              "y": 9
            },
            {
              "x": 252,
              "y": 9
            },
            {
              "x": 253,
              "y": 9
            },
            {
              "x": 254,
              "y": 9
            },
            {
              "x": 255,
              "y": 9
            },
            {
              "x": 256,
              "y": 9
            },
            {
              "x": 257,
              "y": 9
            },
            {
              "x": 258,
              "y": 9
            },
            {
              "x": 259,
              "y": 9
            },
            {
              "x": 260,
              "y": 9
            },
            {
              "x": 261,
              "y": 9
            },
            {
              "x": 262,
              "y": 9
            },
            {
              "x": 263,
              "y": 9
            },
            {
              "x": 264,
              "y": 9
            },
            {
              "x": 265,
              "y": 9
            },
            {
              "x": 266,
              "y": 9
            },
            {
              "x": 267,
              "y": 9
            },
            {
              "x": 268,
              "y": 9
            },
            {
              "x": 269,
              "y": 9
            },
            {
              "x": 270,
              "y": 9
            },
            {
              "x": 271,
              "y": 9
            },
            {
              "x": 272,
              "y": 9
            },
            {
              "x": 273,
              "y": 9
            },
            {
              "x": 274,
              "y": 9
            },
            {
              "x": 275,
              "y": 9
            },
            {
              "x": 276,
              "y": 9
            },
            {
              "x": 277,
              "y": 9
            },
            {
              "x": 278,
              "y": 9
            },
            {
              "x": 279,
              "y": 9
            },
            {
              "x": 280,
              "y": 9
            },
            {
              "x": 281,
              "y": 9
            },
            {
              "x": 282,
              "y": 9
            },
            {
              "x": 283,
              "y": 9
            },
            {
              "x": 284,
              "y": 9
            },
            {
              "x": 285,
              "y": 9
            },
            {
              "x": 286,
              "y": 9
            },
            {
              "x": 287,
              "y": 9
            },
            {
              "x": 288,
              "y": 9
            },
            {
              "x": 289,
              "y": 9
            },
            {
              "x": 290,
              "y": 9
            },
            {
              "x": 291,
              "y": 9
            },
            {
              "x": 292,
              "y": 9
            },
            {
              "x": 293,
              "y": 9
            },
            {
              "x": 294,
              "y": 9
            },
            {
              "x": 295,
              "y": 9
            },
            {
              "x": 296,
              "y": 9
            },
            {
              "x": 297,
              "y": 9
            },
            {
              "x": 298,
              "y": 9
            },
            {
              "x": 299,
              "y": 9
            },
            {
              "x": 300,
              "y": 9
            },
            {
              "x": 301,
              "y": 9
            },
            {
              "x": 302,
              "y": 9
            },
            {
              "x": 303,
              "y": 9
            },
            {
              "x": 304,
              "y": 9
            },
            {
              "x": 305,
              "y": 9
            },
            {
              "x": 306,
              "y": 9
            },
            {
              "x": 307,
              "y": 9
            },
            {
              "x": 308,
              "y": 9
            },
            {
              "x": 309,
              "y": 9
            },
            {
              "x": 310,
              "y": 9
            },
            {
              "x": 311,
              "y": 9
            },
            {
              "x": 312,
              "y": 9
            },
            {
              "x": 313,
              "y": 9
            },
            {
              "x": 314,
              "y": 9
            },
            {
              "x": 315,
              "y": 9
            },
            {
              "x": 316,
              "y": 9
            },
            {
              "x": 317,
              "y": 9
            },
            {
              "x": 318,
              "y": 9
            },
            {
              "x": 319,
              "y": 9
            },
            {
              "x": 320,
              "y": 9
            },
            {
              "x": 321,
              "y": 9
            },
            {
              "x": 322,
              "y": 9
            },
            {
              "x": 323,
              "y": 9
            },
            {
              "x": 324,
              "y": 9
            },
            {
              "x": 325,
              "y": 9
            },
            {
              "x": 326,
              "y": 9
            },
            {
              "x": 327,
              "y": 9
            },
            {
              "x": 328,
              "y": 9
            },
            {
              "x": 329,
              "y": 9
            },
            {
              "x": 330,
              "y": 9
            },
            {
              "x": 331,
              "y": 9
            },
            {
              "x": 332,
              "y": 9
            },
            {
              "x": 333,
              "y": 9
            },
            {
              "x": 334,
              "y": 9
            },
            {
              "x": 335,
              "y": 9
            },
            {
              "x": 336,
              "y": 9
            },
            {
              "x": 337,
              "y": 9
            },
            {
              "x": 338,
              "y": 9
            },
            {
              "x": 339,
              "y": 9
            },
            {
              "x": 340,
              "y": 9
            },
            {
              "x": 341,
              "y": 9
            },
            {
              "x": 342,
              "y": 9
            },
            {
              "x": 343,
              "y": 9
            },
            {
              "x": 344,
              "y": 9
            },
            {
              "x": 345,
              "y": 9
            },
            {
              "x": 346,
              "y": 9
            },
            {
              "x": 347,
              "y": 9
            },
            {
              "x": 348,
              "y": 9
            },
            {
              "x": 349,
              "y": 9
            },
            {
              "x": 350,
              "y": 9
            },
            {
              "x": 351,
              "y": 9
            },
            {
              "x": 352,
              "y": 9
            },
            {
              "x": 353,
              "y": 9
            },
            {
              "x": 354,
              "y": 9
            },
            {
              "x": 355,
              "y": 9
            }
          ]
        ],
        "name": "Slippery Pete"
      },

    ],
    "humanAiGraph": {
      "playerKills": [
        [
          {
            "x": 233,
            "y": 0
          },
          {
            "x": 234,
            "y": 0
          },
          {
            "x": 235,
            "y": 0
          },
          {
            "x": 236,
            "y": 0
          },
          {
            "x": 237,
            "y": 0
          },
          {
            "x": 238,
            "y": 0
          },
          {
            "x": 239,
            "y": 8
          },
          {
            "x": 240,
            "y": 8
          },
          {
            "x": 241,
            "y": 9
          },
          {
            "x": 242,
            "y": 9
          },
          {
            "x": 243,
            "y": 9
          },
          {
            "x": 244,
            "y": 9
          },
          {
            "x": 245,
            "y": 10
          },
          {
            "x": 246,
            "y": 10
          },
          {
            "x": 247,
            "y": 10
          },
          {
            "x": 248,
            "y": 11
          },
          {
            "x": 249,
            "y": 11
          },
          {
            "x": 250,
            "y": 11
          },
          {
            "x": 251,
            "y": 11
          },
          {
            "x": 252,
            "y": 11
          },
          {
            "x": 253,
            "y": 11
          },
          {
            "x": 254,
            "y": 11
          },
          {
            "x": 255,
            "y": 11
          },
          {
            "x": 256,
            "y": 11
          },
          {
            "x": 257,
            "y": 11
          },
          {
            "x": 258,
            "y": 11
          },
          {
            "x": 259,
            "y": 11
          },
        ]
      ],
      "cpuKills": [
        [
          {
            "x": 0,
            "y": 0
          },
        ]
      ],
      "playerLabel": "Slippery Pete: 11"
    },
    "isGameRunning": true,
    "isConnected": false
  };

  constructor(props: any) {
    super(props);
    RegisterCallback((state, json) => {
      if (state === 'DATA') {
        const data = JSON.parse(json) as GraphAggregate;
        console.log('Data:', data);
        this.setState({
          dataset: data.perPlayerGraph,
          humanAiGraph: data.humanAiGraph,
          isGameRunning: data.isGameRunning,
          isConnected: true,
        });
      } else if (state === 'CONNECTED') {
        this.setState({isConnected: true});
      } else if (state === 'DISCONNECTED') {
        this.setState({isConnected: false});
      }
    });
  }


  toPeriodicAggregate(dataset: LineData[][]) {
    const numberOfPointsToInclude = 10;
    let converted = [];

    // For each player..
    for (let idx = 0; idx < dataset.length; idx++) {
      let datapoints = [];
      for (let dp = numberOfPointsToInclude; dp < dataset[idx].length; dp++) {
        datapoints.push({
            x: dataset[idx][dp].x,
          // @ts-ignore
            y: dataset[idx][dp].y - dataset[idx][dp-numberOfPointsToInclude].y // Number of kills the past numberOfPointsToInclude seconds
          });
      }

      converted.push(datapoints);
    }

    return converted;
  }

  /**
   *
   * @param dataset Dataset where the first index represents the player, and the second dimension is a list of datapoints [x:ticks,y:kills]
   * @param label
   */
  renderAggregate(dataset: LineData[][], label: string) {
    // 1 + Math.max.apply(Math, JSON.parse(data.graphJson).humanAiGraph.playerKills[0].map(e => e.y))
    // @ts-ignore
    const yAxisWidth = Math.max(5, 1 + Math.max.apply(Math, dataset[0].map(e => e.y)) / 10);

    return (
      <div>
        <h2>{label}</h2>
        <AreaChart
          axes={true}
          margin={{top: 10, right: 10, bottom: 50, left: 50}}
          axisLabels={{x: 'Time', y: 'Number of Kills'}}
          interpolate={'step'}
          grid={true}
          width={window.innerWidth - 44}
          yTicks={yAxisWidth}
          height={250}
          data={dataset}
        />
      </div>
    );
  }

  renderPlayer(idx: number) {
    if (this.state.dataset.length > idx) {
      const p1 = this.state.dataset[idx];
      return (
        <div key={'player-graph-' + idx}>
          {p1.graph.length > 0 &&
          <div>
              <h2>{p1.name}</h2>
              <AreaChart
                  axes={true}
                  margin={{top: 10, right: 10, bottom: 50, left: 50}}
                  axisLabels={{x: 'Time', y: 'Number of Kills'}}
                  interpolate={'cardinal'}
                  grid={true}
                  width={window.innerWidth - 44}
                  height={250}
                  data={p1.graph}
              />
          </div>
          }
        </div>
      );
    } else {
      return null;
    }
  }

  public render() {
    //console.log(JSON.stringify(this.state));
    //console.log(this.toPeriodicAggregate(this.state.humanAiGraph.playerKills));
    return (
      <div className="App">
        <h1>Company of Heroes - Kill Statistics</h1>
        {this.state.isConnected && !this.state.isGameRunning && <span className="game-not-running">The game does not appear to be running</span>}
        {!this.state.isConnected && <span className="game-not-running">CoH:Stats client does not appear to be running</span>}

        {this.state.humanAiGraph.playerKills.length > 0 && this.state.humanAiGraph.playerKills[0].length > 1 && this.renderAggregate(this.toPeriodicAggregate(this.state.humanAiGraph.playerKills), this.state.humanAiGraph.playerLabel)}


        {this.state.humanAiGraph.playerKills.length > 0 && this.state.humanAiGraph.playerKills[0].length > 1 && this.renderAggregate(this.state.humanAiGraph.playerKills, this.state.humanAiGraph.playerLabel)}

        <br/>
        <div className="chatsheet-container">
          <CheatSheet data={UnitData.filter(e => e.type === 'Infantry')}/>
          <CheatSheet data={UnitData.filter(e => e.type === 'Support')}/>
          <CheatSheet data={UnitData.filter(e => e.type === 'LightVehicle')}/>
          <CheatSheet data={UnitData.filter(e => e.type === 'HeavyVehicle')}/>
        </div>
        {this.state.humanAiGraph.cpuKills.length > 0 && this.state.humanAiGraph.cpuKills[0].length > 1 && this.renderAggregate(this.state.humanAiGraph.cpuKills, 'AI')}
        {this.state.dataset.map((elem, idx) => this.renderPlayer(idx))}
      </div>
    );
  }
}

export default App;
