﻿using System;
using System.IO;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using log4net;

namespace CoHStats {
    public class CefBrowserHandler : IDisposable {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CefBrowserHandler));
        private ChromiumWebBrowser _browser;

        public WebViewJsPojo JsPojo { get; private set; } = new WebViewJsPojo();

        public Control BrowserControl => _browser;

        private object _lockObj = new object();

        ~CefBrowserHandler() {
            Dispose();
        }

        public void Dispose() {
            lock (_lockObj) {
                if (_browser != null) {
                    CefSharpSettings.WcfTimeout = TimeSpan.Zero;
                    _browser.Dispose();

                    Cef.Shutdown();
                    _browser = null;
                }
            }
        }

        public void ShowDevTools() {
            if (_browser.IsBrowserInitialized) {
                _browser.ShowDevTools();
            }
            else {
                MessageBox.Show("Chill the fuck out\nChromium is still initializing.", "Chill, man",
                    MessageBoxButtons.OK);
            }
        }

        public void InitializeChromium(string startPage, bool showDevtools) {

            try {
                Logger.Info("Creating Chromium instance..");
                CefSharpSettings.LegacyJavascriptBindingEnabled = true;
                CefSharpSettings.WcfEnabled = true;
                Cef.EnableHighDPISupport();


                _browser = new ChromiumWebBrowser(startPage);

                _browser.JavascriptObjectRepository.Register("data", JsPojo, isAsync: false, options: BindingOptions.DefaultBinder);

                if (showDevtools) {
                    _browser.IsBrowserInitializedChanged += (sender, args) => _browser.ShowDevTools();
                }

                Logger.Info("Chromium created..");
            }
            catch (System.IO.FileNotFoundException ex) {
                MessageBox.Show("Error \"File Not Found\" loading Chromium, did you forget to install Visual C++ runtimes?\n\nvc_redist86 in the IA folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            }
            catch (IOException ex) {
                MessageBox.Show("Error loading Chromium, did you forget to install Visual C++ runtimes?\n\nvc_redist86 in the IA folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            }
            catch (Exception ex) {
                MessageBox.Show("Unknown error loading Chromium, please see log file for more information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            }
        }
    }
}
