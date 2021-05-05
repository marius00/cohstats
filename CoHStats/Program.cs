using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using log4net.Repository.Hierarchy;

namespace CoHStats {
    static class Program {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build)
                .AddSeconds(version.Revision * 2);

            Logger.InfoFormat("Running version {0}.{1}.{2}.{3} from {4:dd/MM/yyyy}", version.Major, version.Minor,
                version.Build, version.Revision, buildDate);

            bool showDevtools;
            bool skipChromium = args != null && args.Length > 0 && args.Any(x => x.Contains("nochrome"));
#if DEBUG
            showDevtools = true;
#else
            showDevtools = args != null && args.Length > 0 && args.Any(x => x.Contains("devtools"));
            if (showDevtools && skipChromium) {
                Logger.Warn("Show devtools is enabled with -devtools, but embedded browser is disabled with -nochrome");
            }
#endif



                AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);


            Logger.Info($"Running with {Screen.AllScreens.Length} monitors.");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(showDevtools, skipChromium));
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args) {
            Exception e = (Exception) args.ExceptionObject;
            Logger.Fatal(e.Message);
            Logger.Fatal(e.StackTrace);
        }
    }
}