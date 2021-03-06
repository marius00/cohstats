﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
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
        private DataAggregator _aggregator;
        private readonly bool _showDevtools;
        private readonly bool _skipChromium;
        private FormWindowState _previousWindowState = FormWindowState.Normal;
        private readonly WebsocketServer _server = new WebsocketServer(59123);
        private string _url = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)}\\Resources\\index.html";

        public Form1(bool showDevtools, bool skipChromium) {
            InitializeComponent();
            this._showDevtools = showDevtools;
            _skipChromium = skipChromium;
            RecreateAggregators();
            _server.Start();
            _server.OnClientConnect += (sender, args) => ExportData();

            if (_skipChromium) {
                WindowState = FormWindowState.Minimized;
                this.Load += (_, __) => this.Hide();
                trayIcon.Visible = true;
            }
        }

        private void RecreateAggregators() {
            _aggregator = new DataAggregator(_gameReader, new PlayerService(_gameReader));
        }

        class OuterJsonExportFormat {
            public bool IsGameRunning { get; set; }
            public List<JsonExportFormat> Data { get; set; }
        }

        private void ExportData() {
            //_server.Write(_graphConverter.ToJson());

            //return;
            var toJson = new OuterJsonExportFormat {
                IsGameRunning = _gameReader.IsActive,
                Data = _aggregator.Export()
            };
            var json = JsonConverter.Convert(toJson);

            _browser.JsPojo.GraphJson = json;
            _server.Write(json);
        }

        private void Form1_Load(object sender, EventArgs e) {
            if (Thread.CurrentThread.Name == null) {
                Thread.CurrentThread.Name = "UI";
            }

            if (!_skipChromium) {
#if DEBUG
                try {
                    using (var client = new WebClient()) {
                        Logger.Debug("Checking if NodeJS is running...");
                        client.DownloadString("http://localhost:8080/");
                        _url = "http://localhost:8080/";
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
                    _aggregator.Tick();
                    ExportData();
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
                ExportData();
            }


#if DEBUG
            _aggregator.AddDebugData();
            ExportData();
#endif
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