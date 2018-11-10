import * as React from 'react';
import './App.css';
import {AreaChart, LineData} from 'react-easy-chart';

// tslint:disable-next-line
declare abstract class GlobalMagic {
  public static refresh(data: Array<MapperData>): void;
}

interface MapperData {
  graph: Array<Array<LineData>>;
  isValidPlayer: boolean;
  name: string;
}

class App extends React.Component {
  state = {
    dataset: [] as Array<MapperData>
    //points: [[],[],[],[]] as Array<Array<Array<LineData>>>,
    //isValidPlayer: [true, true, true, true]
  };

  constructor(props: any) {
    super(props);
    if (typeof GlobalMagic === 'object') {
      GlobalMagic.refresh = (data: Array<MapperData>) => {
        this.setState({
          dataset: data
        });
      };
    }
  }

  renderPlayer(idx: number) {
    if (this.state.dataset.length > idx) {
      const p1 = this.state.dataset[idx];
      return (
        <div>
          {p1.isValidPlayer && p1.graph.length > 0 &&
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
        {this.renderPlayer(0)}
        {this.renderPlayer(1)}
        {this.renderPlayer(2)}
        {this.renderPlayer(3)}
      </div>
    );
  }
}

export default App;
