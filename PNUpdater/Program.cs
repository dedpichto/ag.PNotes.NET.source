using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PNUpdater
{
    static class Program
    {
        private const string PROGRAM_ZIP = "PNotes.NET_bin.zip";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 7) return;
            Params.Instance.UpdateType = (UpdateType)(Convert.ToInt32(args[1]));
            //if (Params.Instance.UpdateType == UpdateType.PostPlugin)
            //{
            //    Params.Instance.PluginsList.AddRange(args[6].Split(','));
            //}
            //else
            //{
                Params.Instance.ProgramDir = args[6];
            //}
            Params.Instance.Captions.AddRange(args[2].Split(','));
            Params.Instance.UpdateUrl = args[3];
            Params.Instance.ProgramToRun = args[4];
            Params.Instance.TargetDir = args[5];
            if (args.Length > 7)
            {
                if (!string.IsNullOrWhiteSpace(args[7]))
                    Params.Instance.UpdateZip = args[7];
                else
                    Params.Instance.UpdateZip = PROGRAM_ZIP;
            }
            else
            {
                Params.Instance.UpdateZip = PROGRAM_ZIP;
            }
            try
            {
                Application.Run(new DlgUpdate());
            }
            catch (ObjectDisposedException)
            {
            }

            if (String.IsNullOrEmpty(Params.Instance.SelfPath))
            {
                foreach (var d in Params.Instance.DirectoriesToDelete)
                {
                    Directory.Delete(d, true);
                }
            }
            Process.Start(Params.Instance.ProgramToRun);
        }

        internal static void LogThis(string message)
        {
            try
            {
                using (var w = new StreamWriter("pnupdater.log", true))
                {
                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    sb.AppendLine();
                    sb.Append(message);
                    sb.AppendLine();
                    sb.Append("***************************");
                    w.WriteLine(sb.ToString());
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
