using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CoHStats.Integration;
using log4net;

namespace CoHStats {
    public partial class Form1 : Form {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Form1));
        private readonly CefBrowserHandler _browser = new CefBrowserHandler();
        private readonly GameReader _gameReader = new GameReader();
        private GraphConverter _graphConverter;
        private readonly bool _showDevtools;

        public Form1(bool showDevtools) {
            InitializeComponent();
            this._showDevtools = showDevtools;
            _graphConverter = new GraphConverter(_gameReader);
        }

        private void Form1_Load(object sender, EventArgs e) {
            var pojo = new WebViewJsPojo {
                GraphJson = _graphConverter.ToJson() // Initial data will be empty, this is fine.
            };

            string url = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\Resources\\index.html";
#if DEBUG
            var client = new WebClient();
            try {
                Logger.Debug("Checking if NodeJS is running...");
                client.DownloadString("http://localhost:3000/");
                url = "http://localhost:3000/";
            }
            catch (System.Net.WebException) {
                Logger.Debug("NodeJS not running, defaulting to standard view");
            }
#else
            url = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\Resources\\index.html";
#endif
            Logger.Info($"Running with UI: {url}");

            _browser.InitializeChromium(url, pojo, _showDevtools);
            this.Controls.Add(_browser.BrowserControl);
            _browser.BrowserControl.Show();

            this.FormClosing += Form1_FormClosing;



            var timerReportUsage = new System.Timers.Timer();
            timerReportUsage.Start();
            bool wasActiveLastTick = false;
            timerReportUsage.Elapsed += (a1, a2) => {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Data";

                if (_gameReader.IsActive) {
                    _graphConverter.Tick();
                    pojo.GraphJson = _graphConverter.ToJson();
                }

                // Reset cross games
                if (_gameReader.IsActive && !wasActiveLastTick) {
                    _graphConverter = new GraphConverter(_gameReader);
                    Logger.Info("A new game has started, resettings the stats.");
                }

                wasActiveLastTick = _gameReader.IsActive;
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