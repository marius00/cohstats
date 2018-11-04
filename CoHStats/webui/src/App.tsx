import * as React from 'react';
import './App.css';
import {AreaChart, LineData} from 'react-easy-chart';

// tslint:disable-next-line
declare abstract class GlobalMagic {
  public static refresh(points: Array<Array<Array<LineData>>>): void;
  public static setTimeAggregation(index: string): void;
}

class App extends React.Component {
  state = {
    points: [[],[],[],[]] as Array<Array<Array<LineData>>>
  };

  constructor(props: any) {
    super(props);
    if (typeof GlobalMagic === 'object') {
      GlobalMagic.refresh = (points: Array<Array<Array<LineData>>>) => {
        this.setState({
          points: points
        });
      };
    }
  }

  setTimeAggregation(event: any) {
    GlobalMagic.setTimeAggregation(event.target.value);
  }

  public render() {
    return (
      <div className="App">
        <h1>Company of Heroes - Kill Statistics</h1>
        <select onChange={this.setTimeAggregation}>
          <option value={1}>Every second</option>
          <option value={2}>Every two seconds</option>
          <option value={3} selected={true}>Every three seconds</option>
          <option value={5}>Every five seconds</option>
          <option value={10}>Every 10 seconds</option>
          <option value={15}>Every 15 seconds</option>
          <option value={30}>Every 30 seconds</option>
          <option value={60}>Every minute</option>
        </select>
        <h2>Player #1</h2>
        <AreaChart
          axes={true}
          margin={{top: 10, right: 10, bottom: 50, left: 50}}
          axisLabels={{x: 'Time', y: 'Number of Kills'}}
          interpolate={'cardinal'}
          grid={true}
          width={1680}
          height={250}
          data={this.state.points[0]}
        />
        <h2>Player #2</h2>
        <AreaChart
          axes={true}
          margin={{top: 10, right: 10, bottom: 50, left: 50}}
          axisLabels={{x: 'Time', y: 'Number of Kills'}}
          interpolate={'cardinal'}
          grid={true}
          width={1680}
          height={250}
          data={this.state.points[1]}
        />
        <h2>Player #3</h2>
        <AreaChart
          axes={true}
          margin={{top: 10, right: 10, bottom: 50, left: 50}}
          axisLabels={{x: 'Time', y: 'Number of Kills'}}
          interpolate={'cardinal'}
          grid={true}
          width={1680}
          height={250}
          data={this.state.points[2]}
        />
        <h2>Player #4</h2>
        <AreaChart
          axes={true}
          margin={{top: 10, right: 10, bottom: 50, left: 50}}
          axisLabels={{x: 'Time', y: 'Number of Kills'}}
          interpolate={'cardinal'}
          grid={true}
          width={1680}
          height={250}
          data={this.state.points[3]}
        />
      </div>
    );
  }
}

export default App;
