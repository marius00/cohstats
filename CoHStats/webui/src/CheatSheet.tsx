import * as React from "react";

interface SheetEntry {
  name: string;
  url: string;
  upgrades: number[];
  tier: number;
  type: string;
}

interface Props {
  data: SheetEntry[];
}
class CheatSheet extends React.PureComponent<Props> {
  renderEntry(entry: SheetEntry) {
    return (
      <tr className="slds-hint-parent linkrow" key={entry.url} onClick={() => window.open(entry.url)}>
        <td>
          <div className="slds-truncate">{entry.name}</div>
        </td>
        <td>
          <div className="slds-truncate">{entry.upgrades.join(', ')}</div>
        </td>
        <td>
          <div className="slds-truncate">{entry.tier === 0 ? '-' : entry.tier}</div>
        </td>
      </tr>
    );
  }

  render() {
    return (
      <table className="slds-table slds-table_cell-buffer slds-table_bordered cheatsheet">
        <thead>
        <tr className="slds-line-height_reset">
          <th className="" scope="col">
            <div className="slds-truncate">Name</div>
          </th>
          <th className="" scope="col">
            <div className="slds-truncate">Upgrades</div>
          </th>
          <th className="" scope="col">
            <div className="slds-truncate">Tier</div>
          </th>
        </tr>
        </thead>
        <tbody>
        {this.props.data.map(entry => this.renderEntry(entry))}
        </tbody>
      </table>
    );
  }
}

export default CheatSheet;