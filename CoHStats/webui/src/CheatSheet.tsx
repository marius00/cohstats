import * as React from "react";

interface UpgradeTier {
  level: number;
  type: string;
}

interface SheetEntry {
  name: string;
  url: string;
  upgrades: UpgradeTier[];
  tier: number;
  type: string;
}

interface Props {
  data: SheetEntry[];
}


class CheatSheet extends React.PureComponent<Props> {

  toTierElement(entry: UpgradeTier) {
    return <span className={entry.type} key={Math.random()}>{entry.level}</span>;
  }


  renderEntry(entry: SheetEntry) {
    let upgrades = entry.upgrades
      .map(this.toTierElement)
      .map((item, index) => [index > 0 && ', ', item ]);

    return (
      <tr className="slds-hint-parent linkrow" key={entry.url} onClick={() => window.open(entry.url)}>
        <td>
          <div className="slds-truncate">{entry.name}</div>
        </td>
        <td>
          <div className="slds-truncate">{upgrades}</div>
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