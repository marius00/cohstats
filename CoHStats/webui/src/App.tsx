import * as React from 'react';
import './App.css';
import {AreaChart, LineData} from 'react-easy-chart';

// tslint:disable-next-line
declare abstract class GlobalMagic {
  public static refresh(data: GraphAggregate): void;
}

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
  state = {
    dataset: [] as Array<MapperData>,
    humanAiGraph: {cpuKills: [], playerKills: [], playerLabel: ''} as HumanAiKillCountAggregate,
    isGameRunning: false
  };

  constructor(props: any) {
    super(props);
    if (typeof GlobalMagic === 'object') {
      GlobalMagic.refresh = (data: GraphAggregate) => {
        this.setState({
          dataset: data.perPlayerGraph,
          humanAiGraph: data.humanAiGraph,
          isGameRunning: data.isGameRunning
        });
      };
    }
  }

  renderAggregate(dataset: LineData[][], label: string) {
    return (
      <div>
        <h2>{label}</h2>
        <AreaChart
          axes={true}
          margin={{top: 10, right: 10, bottom: 50, left: 50}}
          axisLabels={{x: 'Time', y: 'Number of Kills'}}
          interpolate={'cardinal'}
          grid={true}
          width={window.innerWidth - 44}
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
    return (
      <div className="App">
        <h1>Company of Heroes - Kill Statistics</h1>
        {!this.state.isGameRunning && <span className="game-not-running">The game does not appear to be running</span>}
        {this.state.humanAiGraph.playerKills.length > 0 && this.state.humanAiGraph.playerKills[0].length > 1 && this.renderAggregate(this.state.humanAiGraph.playerKills, this.state.humanAiGraph.playerLabel)}
        {this.state.humanAiGraph.cpuKills.length > 0 && this.state.humanAiGraph.cpuKills[0].length > 1 && this.renderAggregate(this.state.humanAiGraph.cpuKills, 'AI')}
        {this.state.dataset.map((elem, idx) => this.renderPlayer(idx))}
      </div>
    );
  }
}

export default App;
