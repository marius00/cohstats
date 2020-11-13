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
        private FormWindowState _previousWindowState = FormWindowState.Normal;
        private readonly int _resolution = 3;

        public Form1(bool showDevtools) {
            InitializeComponent();
            this._showDevtools = showDevtools;
            _graphConverter = new GraphConverter(_gameReader, _resolution);
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

            this.SizeChanged += Form1_SizeChanged;
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
                    _graphConverter = new GraphConverter(_gameReader, _resolution);
                    Logger.Info("A new game has started, resettings the stats.");

                    if (Screen.AllScreens.Length > 1) {
                        Logger.Info($"Screens detected: {Screen.AllScreens.Length}, restoring CoH:Stats in .");
                        trayIcon_MouseDoubleClick(null, null);
                    }

                }

                wasActiveLastTick = _gameReader.IsActive;
            };
            timerReportUsage.Interval = 1000;
            timerReportUsage.AutoReset = true;
            timerReportUsage.Start();






            var timerAutoClose = new System.Timers.Timer();
            timerAutoClose.Elapsed += (a1, a2) => {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "AutoClose";
                if (InvokeRequired) {
                    Invoke((MethodInvoker)Close);
                }
                else {
                    Close();
                }

            };
            timerAutoClose.Interval = 1000 * 60 * 60 * 2; // 2H
            timerAutoClose.AutoReset = true;
            timerAutoClose.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            this.SizeChanged -= Form1_SizeChanged;
            _browser.Dispose();
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
            if (InvokeRequired) {
                Invoke((MethodInvoker)delegate { trayIcon_MouseDoubleClick(sender, e); });
            } else {
                if (!Visible) {
                    // Visible = true;
                    ShowWindow(this.Handle, 4);
                    trayIcon.Visible = false;
                    WindowState = _previousWindowState;
                }
            }
        }
    }
}