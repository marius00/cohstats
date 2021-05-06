using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
using CoHStats.Aggregator;
using CoHStats.Game;
using CoHStats.Integration;
using CoHStats.Websocket;
using log4net;

namespace CoHStats {
    public partial class Form1 : Form {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Form1));
        private readonly CefBrowserHandler _browser = new CefBrowserHandler();
        private readonly GameReader _gameReader = new GameReader();
        private GraphConverter _graphConverter;
        private DataAggregator _aggregator;
        private readonly bool _showDevtools;
        private readonly bool _skipChromium;
        private FormWindowState _previousWindowState = FormWindowState.Normal;
        private readonly int _resolution = 1;
        private readonly WebsocketServer _server = new WebsocketServer(59123);
        private string _url = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)}\\Resources\\index.html";

        public Form1(bool showDevtools, bool skipChromium) {
            InitializeComponent();
            this._showDevtools = showDevtools;
            _skipChromium = skipChromium;
            RecreateAggregators();
            _server.Start();

            if (_skipChromium) {
                WindowState = FormWindowState.Minimized;
                this.Load += (_, __) => this.Hide();
                trayIcon.Visible = true;
            }
        }

        private void RecreateAggregators() {
            _graphConverter = new GraphConverter(_gameReader, _resolution, new PlayerService(_gameReader));
            _aggregator = new DataAggregator(_gameReader, new PlayerService(_gameReader));

        }

        private void Form1_Load(object sender, EventArgs e) {
            if (!_skipChromium) {
#if DEBUG
                try {
                    using (var client = new WebClient()) {
                        Logger.Debug("Checking if NodeJS is running...");
                        client.DownloadString("http://localhost:3000/");
                        _url = "http://localhost:3000/";
                    }
                }
                catch (System.Net.WebException) {
                    Logger.Debug("NodeJS not running, defaulting to standard view");
                }
#endif
                Logger.Info($"Running with UI: {_url}");


                _browser.InitializeChromium(_url, _showDevtools);
                this.Controls.Add(_browser.BrowserControl);
                _browser.BrowserControl.Show();
            }

            this.SizeChanged += Form1_SizeChanged;
            this.FormClosing += Form1_FormClosing;


            var gameTickTimer = new System.Timers.Timer();
            gameTickTimer.Start();
            bool wasActiveLastTick = false;
            gameTickTimer.Elapsed += (a1, a2) => {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Data";

                if (_gameReader.IsActive) {
                    _graphConverter.Tick();
                    _aggregator.Tick();
                    var json = _graphConverter.ToJson();
                    _server.Write(json);
                }

                // Reset cross games
                if (_gameReader.IsActive && !wasActiveLastTick) {
                    RecreateAggregators();
                    Logger.Info("A new game has started, resetting the stats.");

                    if (Screen.AllScreens.Length > 1) {
                        Logger.Info($"Screens detected: {Screen.AllScreens.Length}, restoring CoH:Stats in .");
                        trayIcon_MouseDoubleClick(null, null);
                    }
                }

                wasActiveLastTick = _gameReader.IsActive;
            };

            gameTickTimer.Interval = 1000;
            gameTickTimer.AutoReset = true;
            gameTickTimer.Start();

            if (!_skipChromium) {
                EnableAutoCloseTimer();
            }
        }

        /// <summary>
        /// Lame workaround for the chromium embedded browser crashing once in a while
        /// So after a while, just close coh:stats as it has probably crashed.
        /// </summary>
        private void EnableAutoCloseTimer() {
            var timerAutoClose = new System.Timers.Timer();
            timerAutoClose.Elapsed += (a1, a2) => {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "AutoClose";
                if (InvokeRequired) {
                    Invoke((MethodInvoker) Close);
                }
                else {
                    Close();
                }
            };
            timerAutoClose.Interval = 1000 * 60 * 60 * 2; // 2H
            timerAutoClose.AutoReset = true;
            timerAutoClose.Start();
        }

        /// <summary>
        /// Cleanup, end threads etc.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            this.SizeChanged -= Form1_SizeChanged;
            if (!_skipChromium)
                _browser.Dispose();
            _server.Dispose();
        }


        private void Form1_SizeChanged(object sender, EventArgs e) {
            try {
                if (WindowState == FormWindowState.Minimized) {
                    Hide();
                    trayIcon.Visible = true;
                }
                else {
                    trayIcon.Visible = false;
                    _previousWindowState = WindowState;
                }
            }
            catch (Exception ex) {
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
            }
        }


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (_skipChromium) {
                Process.Start(_url);
                return;
            }

            if (InvokeRequired) {
                Invoke((MethodInvoker) delegate { trayIcon_MouseDoubleClick(sender, e); });
            }
            else {
                if (!Visible) {
                    // Visible = true;
                    ShowWindow(this.Handle, 4);
                    trayIcon.Visible = false;
                    WindowState = _previousWindowState;
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            trayIcon_MouseDoubleClick(this, null);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }
    }
}