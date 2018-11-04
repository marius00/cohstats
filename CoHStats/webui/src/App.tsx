import * as React from 'react';
import './App.css';
import {AreaChart, LineData} from 'react-easy-chart';

// tslint:disable-next-line
declare abstract class GlobalMagic {
  public static refresh(points: Array<Array<LineData>>): void;
  public static setPlayer(index: string): void;
  public static setTimeAggregation(index: string): void;
}

class App extends React.Component {
  state = {
    r: 0,
    points: [] as Array<Array<LineData>>
  };

  constructor(props: any) {
    super(props);
    if (typeof GlobalMagic === 'object') {
      GlobalMagic.refresh = (points: Array<Array<LineData>>) => {
        console.log('Setting dummy state to trigger graph refresh');
        this.setState({
          points: points,
          r: new Date().getTime()
        });
      };
    }
  }

  handleChange(event: any) {
    GlobalMagic.setPlayer(event.target.value);
  }

  setTimeAggregation(event: any) {
    GlobalMagic.setTimeAggregation(event.target.value);
  }

  public render() {
    return (
      <div className="App">
        <h1>Company of Heroes - Kill Statistics</h1>
        <select onChange={this.handleChange}>
          <option value={1}>Player #1</option>
          <option value={2}>Player #2</option>
          <option value={3}>Player #3 - Broken</option>
          <option value={4}>Player #4 - Broken</option>
        </select>
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
        <AreaChart
          axes={true}
          margin={{top: 10, right: 10, bottom: 50, left: 50}}
          axisLabels={{x: 'Time', y: 'Number of Kills'}}
          interpolate={'cardinal'}
          grid={true}
          width={1680}
          height={250}
          data={this.state.points}
        />
      </div>
    );
  }
}

export default App;
