using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CoHStats.Integration;
using log4net;
using Timer = System.Threading.Timer;

namespace CoHStats {
    public partial class Form1 : Form {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Form1));
        private readonly CefBrowserHandler _browser = new CefBrowserHandler();
        private readonly GameReader _gameReader = new GameReader();
        private readonly GraphConverter _graphConverter = new GraphConverter();
        private Player _selectedPlayer = Player.One;
        private int _stepSize = 3;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            // _browser.BrowserControl.TopLevel = false;
            var pojo = new WebViewJsPojo {
                GraphJson = _graphConverter.ToJson(Player.One, _stepSize) // Initial data will be empty, this is fine.
            };
            pojo.OnUpdatePlayer += (o, args) => { _selectedPlayer = (args as UpdatePlayerArg).Player; };
            pojo.OnUpdateTimeAggregation += (o, args) => { _stepSize = (args as TimeStepArg).StepSize; };

            string url;
#if DEBUG
            url = "http://localhost:3000/";
#else
            url = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\Resources\\index.html";
#endif
            Logger.Info($"Running with UI: {url}");

            _browser.InitializeChromium(url, pojo);
            this.Controls.Add(_browser.BrowserControl);
            _browser.BrowserControl.Show();

            this.FormClosing += Form1_FormClosing;



            var timerReportUsage = new System.Timers.Timer();
            timerReportUsage.Start();
            timerReportUsage.Elapsed += (a1, a2) => {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Data";

                _graphConverter.Add(_gameReader.FetchStats(Player.One), Player.One);
                _graphConverter.Add(_gameReader.FetchStats(Player.Two), Player.Two);
                pojo.GraphJson = _graphConverter.ToJson(_selectedPlayer, _stepSize);
            };
            timerReportUsage.Interval = 1000;
            timerReportUsage.AutoReset = true;
            timerReportUsage.Start();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            _browser.Dispose();
        }
    }
}