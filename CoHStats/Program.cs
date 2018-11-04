using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        static void Main() {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build)
                .AddSeconds(version.Revision * 2);

            Logger.InfoFormat("Running version {0}.{1}.{2}.{3} from {4:dd/MM/yyyy}", version.Major, version.Minor, version.Build, version.Revision, buildDate);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
