// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using SQLiteWrapper;
using WPFStandardStyles;

namespace PNotes.NET
{
    static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                var nonetwork = false;
                var nosplash = false;
                var args = Environment.GetCommandLineArgs();
                var argsLength = args.Length;
                //check for restart flag
                var restart = argsLength > 1 && args[argsLength - 1] == PNStrings.RESTART;
                if (restart)
                {
                    Array.Resize(ref args, argsLength - 1);
                }
                PNRuntimes.Instance.CultureInvariant = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                PNRuntimes.Instance.CultureInvariant.NumberFormat.NumberDecimalSeparator = ".";

                var currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += currentDomain_UnhandledException;
                checkPreRun();

                if (args.Length > 1)
                {
                    switch (args[1])
                    {
                        case "-x":
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_CLOSE_PROG);
                            return;
                        case "-xs":
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_CLOSE_SILENT_SAVE);
                            return;
                        case "-xn":
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_CLOSE_SILENT_WO_SAVE);
                            return;
                        case "-c":
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_NEW_NOTE);
                            return;
                        case "-cr":
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_NEW_NOTE_FROM_CB);
                            return;
                        case "-cd":
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_NEW_DIARY);
                            return;
                        case "-cn":
                            if (args.Length > 2)
                                _newNoteString = args[2];
                            if (args.Length > 3)
                                _newNoteName = args[3];
                            if (args.Length > 4)
                                _newNoteTags = args[4];
                            _copyDataType = CopyDataType.NewNote;
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WM_COPYDATA);
                            return;
                        case "-l":
                            if (args.Length > 2)
                            {
                                _FilesToLoad.AddRange(args.Skip(2));
                            }
                            _copyDataType = CopyDataType.LoadNotes;
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WM_COPYDATA);
                            return;
                        case "-i":
                            if (args.Length > 2)
                            {
                                _idToShow = args[2];
                            }
                            _copyDataType = CopyDataType.ShowNoteById;
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WM_COPYDATA);
                            if (_mainWindowExists)
                            {
                                //return if main window exists and program is already running
                                return;
                            }
                            //continue to start the program and add note's id to be proceeded later
                            PNSingleton.Instance.NoteFromShortcut = _idToShow;
                            break;
                        case "-exp":
                            if (args.Length > 2)
                            {
                                _repArguments = args[2];
                                if (args.Length > 4)
                                {
                                    _repArguments += "|" + args[3] + "|" + args[4];
                                }
                                if (args.Length > 6)
                                {
                                    _repArguments += "|" + args[5] + "|" + args[6];
                                }
                                _copyDataType = CopyDataType.ExportNotes;
                                PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WM_COPYDATA);
                                if (!_mainWindowExists)
                                {
                                    exportNotes(_repArguments);
                                }
                            }
                            return;
                        case "-r":
                            PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_RELOAD_NOTES);
                            return;
                        case "-b":
                            if (args.Length >= 3)
                            {
                                var backFile = Path.Combine(args[2], DateTime.Now.ToString("yyyyMMddHHmmss") + PNStrings.FULL_BACK_EXTENSION);
                                PNStatic.CreateFullBackup(backFile);
                                return;
                            }
                            if (PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_BACKUP))
                            {
                                // if no previos instance of PNotes is running - create backup at default directory and exit
                                var backFile = Path.Combine(PNPaths.Instance.BackupDir, DateTime.Now.ToString("yyyyMMddHHmmss") + PNStrings.FULL_BACK_EXTENSION);
                                PNStatic.CreateFullBackup(backFile);
                            }
                            return;
                        case "-nosplash":
                            if (isPrevInstance()) return;
                            nosplash = true;
                            break;
                        case "-nonetwork":
                            if (isPrevInstance()) return;
                            nonetwork = true;
                            if (args[args.Length - 1] == "nosplash")
                            {
                                nosplash = true;
                            }
                            break;
                        case "-updater":
                            if (args.Length == 3)
                            {
                                var updPath = args[2];
                                if (!string.IsNullOrEmpty(updPath))
                                {
                                    var name = Path.GetFileName(updPath);
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        var localPath = Path.Combine(System.Windows.Forms.Application.StartupPath, name);
                                        if (File.Exists(updPath))
                                        {
                                            File.Copy(updPath, localPath, true);
                                            var dir = Path.GetDirectoryName(updPath);
                                            if (!string.IsNullOrEmpty(dir))
                                            {
                                                Directory.Delete(dir, true);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "-conf":
                        case "-config":
                        case "-confnonetwork":
                        case "-confignonetwork":
                            if (isPrevInstance())
                                return;
                            if (args.Length >= 3 && args[2].Trim().Length > 0)
                            {
                                PNPaths.Instance.SettingsDir = args[2];
                            }
                            if (args.Length >= 4 && args[3].Trim().Length > 0)
                            {
                                PNPaths.Instance.DataDir = args[3];
                            }
                            if (args.Length >= 5 && args[4].Trim().Length > 0)
                            {
                                PNPaths.Instance.SkinsDir = args[4];
                            }
                            if (args.Length >= 6 && args[5].Trim().Length > 0)
                            {
                                PNPaths.Instance.BackupDir = args[5];
                            }
                            if (args.Length >= 7 && args[6].Trim().Length > 0)
                            {
                                PNPaths.Instance.LangDir = args[6];
                            }
                            if (args.Length >= 8 && args[7].Trim().Length > 0)
                            {
                                PNPaths.Instance.SoundsDir = args[7];
                            }
                            if (args.Length >= 9 && args[8].Trim().Length > 0)
                            {
                                PNPaths.Instance.FontsDir = args[8];
                            }
                            if (args.Length >= 10 && args[9].Trim().Length > 0)
                            {
                                PNPaths.Instance.DictDir = args[9];
                            }
                            if (args.Length >= 11 && args[10].Trim().Length > 0)
                            {
                                PNPaths.Instance.PluginsDir = args[10];
                            }
                            if (args.Length >= 12 && args[11].Trim().Length > 0)
                            {
                                PNPaths.Instance.ThemesDir = args[11];
                            }
                            if (args.Length >= 13 && args[12].Trim().Length > 0)
                            {
                                PNPaths.Instance.ContactsDir = args[12];
                            }
                            if (args[1] == "-confnonetwork" || args[1] == "-confignonetwork")
                            {
                                nonetwork = true;
                            }
                            if (args[args.Length - 1] == "nosplash")
                            {
                                nosplash = true;
                            }
                            break;
                        default:
                            if (args[1].StartsWith("/u"))
                            {
                                string[] pars = args[1].Split('=');
                                if (pars.Length == 2)
                                {
                                    string guid = pars[1];
                                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                                                        "msiexec.exe");
                                    var psi = new ProcessStartInfo(path, "/x " + guid);
                                    Process.Start(psi);
                                    return;
                                }
                            }
                            break;
                    }
                }

                if (!isPrevInstance() || restart)
                {
                    installFonts();
                    if (!nosplash && File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH)))
                        nosplash = true;
                    if (!nosplash)
                    {
                        PNRuntimes.Instance.SplashThread = new Thread(PNStatic.ShowSplash);
                        PNRuntimes.Instance.SplashThread.SetApartmentState(ApartmentState.STA);
                        PNRuntimes.Instance.SplashThread.Start();
                    }
                    PNStatic.StartProgram(nonetwork);

                    PNWindows.Instance.FormMain = new WndMain();
                    var app = new Application();

                    var res1 = new ResourceDictionary
                    {
                        Source = new Uri(@"/styles/Resources.xaml", UriKind.Relative)
                    };
                    var res2 = new ResourceDictionary
                    {
                        Source = new Uri(@"/styles/Images.xaml", UriKind.Relative)
                    };
                    var name = res1["ThemeName"] as string;
                    var sampleImage = res2["sample"] as BitmapImage;
                    PNCollections.Instance.Themes.Add(name,
                        Tuple.Create(new Uri(@"/styles/Resources.xaml", UriKind.Relative),
                            new Uri(@"/styles/Images.xaml", UriKind.Relative), sampleImage, "", new Version()));

                    loadThemes();

                    app.Resources.MergedDictionaries.Add(res1);
                    app.Resources.MergedDictionaries.Add(res2);
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri(@"/styles/Styles.xaml", UriKind.Relative)
                    });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri(@"/Themes/generic.xaml", UriKind.Relative)
                    });

                    PNStatic.ApplyTheme(PNRuntimes.Instance.Settings.Behavior.Theme);

                    app.Run(PNWindows.Instance.FormMain);

                    if (PNRuntimes.Instance.Settings != null)
                    {
                        PNRuntimes.Instance.Settings.Dispose();
                    }
                    if (PNSingleton.Instance.Restart)
                    {
                        //System.Windows.Forms.Application.Restart();
                        var arrArgs = Environment.GetCommandLineArgs();
                        var commandLine = arrArgs.Length > 0 ? string.Join(" ", arrArgs) : "";
                        commandLine += " ";
                        commandLine += PNStrings.RESTART;
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, commandLine);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(PNSingleton.Instance.UpdaterCommandLine))
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = Path.Combine(System.Windows.Forms.Application.StartupPath, "PNUpdater.exe"),
                                Arguments = PNSingleton.Instance.UpdaterCommandLine,
                                UseShellExecute = true
                            };
                            var prc = new Process { StartInfo = psi };
                            prc.Start();
                        }
                    }
                }
                else
                {
                    PNInterop.EnumWindows(enumAllWindowsProc, PNInterop.WPM_START_FROM_ANOTHER_INSTANCE);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            PNStatic.LogException((Exception) e.ExceptionObject);
            Application.Current.Shutdown(-1);
        }

        private static void exportNotes(string parameters)
        {
            try
            {
                PNStatic.StartProgram(false);
                PNData.ConnectionString = SQLiteDataObject.CheckAndCreateDatabase(PNPaths.Instance.DBPath);
                //no report for password protected program directly from command line
                if (PNRuntimes.Instance.Settings.Protection.PasswordString != "")
                {
                    PNStatic.LogThis(
                        "The attempt has been made to create notes report while program is password protected ");
                    return;
                }
                var arr = parameters.Split("|");
                var fileName = arr[0];
                var index = parameters.IndexOf("|", StringComparison.Ordinal);
                string[] dates = null;
                if (index >= 0)
                {
                    dates = parameters.Substring(index + 1).Split("|");
                    if (!PNStatic.areDatesValidInCommandLine(dates))
                        return;
                }
                var extension = Path.GetExtension(fileName);
                if (extension == null) return;
                extension = extension.ToUpper();
                switch (extension)
                {
                    case ".PDF":
                        PNExport.ExportNotes(ReportType.Pdf, fileName, dates);
                        break;
                    case ".TIF":
                        PNExport.ExportNotes(ReportType.Tif, fileName, dates);
                        break;
                    case ".DOC":
                        PNExport.ExportNotes(ReportType.Doc, fileName, dates);
                        break;
                    case ".RTF":
                        PNExport.ExportNotes(ReportType.Rtf, fileName, dates);
                        break;
                    case ".TXT":
                        PNExport.ExportNotes(ReportType.Txt, fileName, dates);
                        break;
                    default:
                        PNStatic.LogThis("Invalid file specified for notes report");
                        WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_exp_comm_line",
                            "Invalid file specified for notes report"));
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void loadThemes()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.ThemesDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.ThemesDir);
                var files = di.GetFiles(PNStrings.THEME_FILE_MASK);

                foreach (var f in files)
                {
                    var assName = AssemblyName.GetAssemblyName(f.FullName);
                    var assembly = Assembly.Load(assName);
                    var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".g.resources");
                    if (stream == null) continue;
                    var resourceReader = new ResourceReader(stream);
                    ResourceDictionary rd1 = null, rd2 = null;
                    foreach (DictionaryEntry resource in resourceReader)
                    {


                        if (new FileInfo(resource.Key.ToString()).Extension.Equals(".baml"))
                        {
                            var uri =
                                new Uri(
                                    "/" + assembly.GetName().Name + ";component/" +
                                    resource.Key.ToString().Replace(".baml", ".xaml"), UriKind.Relative);
                            if (uri.ToString().ToUpper().EndsWith("RESOURCES.XAML"))
                            {
                                rd1 = new ResourceDictionary { Source = uri };
                            }
                            else if (uri.ToString().ToUpper().EndsWith("IMAGES.XAML"))
                            {
                                rd2 = new ResourceDictionary { Source = uri };
                            }
                        }
                    }
                    if (rd1 == null || rd2 == null) continue;
                    var name = rd1["ThemeName"] as string ?? "";
                    var sampleImage = rd2["sample"] as BitmapImage;
                    PNCollections.Instance.Themes.Add(name,
                        Tuple.Create(rd1.Source, rd2.Source, sampleImage, f.FullName, assembly.GetName().Version));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
   
        private static void installFonts()
        {
            try
            {
                var fonts = new InstalledFontCollection();

                var fontNames = new Dictionary<string, string>
            {
                {"SEGOE SCRIPT", "segoesc.ttf"},
                {"LUCIDA SANS UNICODE", "l_10646.ttf"}
            };
                foreach (var fn in fontNames)
                {
                    if (fonts.Families.Any(f => f.Name.ToUpper().StartsWith(fn.Key))) continue;
                    var streamData = Application.GetResourceStream(new Uri("fonts/" + fn.Value, UriKind.Relative));
                    // receive resource stream
                    if (streamData != null && streamData.Stream != null)
                    {
                        // create an unsafe memory block for the font data
                        IntPtr data = Marshal.AllocCoTaskMem((int)streamData.Stream.Length);

                        if (data != IntPtr.Zero)
                        {
                            // create a buffer to read in to
                            var fontdata = new byte[streamData.Stream.Length];

                            // read the font data from the resource
                            streamData.Stream.Read(fontdata, 0, (int)streamData.Stream.Length);

                            // copy the bytes to the unsafe memory block
                            Marshal.Copy(fontdata, 0, data, (int)streamData.Stream.Length);

                            // pass the font to the font collection
                            PNCollections.Instance.PrivateFonts.AddMemoryFont(data, (int)streamData.Stream.Length);

                            // close the resource stream
                            streamData.Stream.Close();

                            // free up the unsafe memory
                            Marshal.FreeCoTaskMem(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static bool isPrevInstance()
        {
            try
            {

//#if DEBUG
//                return false;
//#endif
                var pc = Process.GetCurrentProcess();
                var procs = Process.GetProcessesByName(pc.ProcessName);
                return procs.Any(p => p.Id != pc.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private static void checkPreRun()
        {
            try
            {
                //check whether file pre-run exists
                var filePreRun = Path.Combine(Path.GetTempPath(), PNStrings.PRE_RUN_FILE);
                if (!File.Exists(filePreRun)) return;
                var xdoc = XDocument.Load(filePreRun);
                var xroot = xdoc.Root;
                if (xroot == null) return;

                var listCriticals = new List<string>();

                var xrem = xroot.Element(PNStrings.ELM_REMOVE);
                if (xrem != null)
                {
                    var xdirs = xrem.Elements(PNStrings.ELM_DIR);
                    //delete all directories found in "remove" section
                    foreach (var xe in xdirs)
                    {
                        if (Directory.Exists(xe.Value))
                        {
                            var files = new DirectoryInfo(xe.Value).GetFiles();
                            foreach (var f in files)
                            {
                                try
                                {
                                    File.Delete(f.FullName);
                                }
                                catch (Exception e)
                                {
                                    PNStatic.LogException(e,false);
                                }
                            }
                            try
                            {
                                Directory.Delete(xe.Value, true);
                            }
                            catch (Exception e)
                            {
                                PNStatic.LogException(e, false);
                            }
                        }
                    }
                }
                //copy all files
                var xcopyfiles = xroot.Element(PNStrings.ELM_COPY_FILES);
                if (xcopyfiles != null)
                {
                    var xcopies = xcopyfiles.Elements(PNStrings.ELM_COPY);
                    foreach (var xe in xcopies)
                    {
                        var xf = xe.Attribute(PNStrings.ATT_FROM);
                        if (xf == null) continue;
                        var xt = xe.Attribute(PNStrings.ATT_TO);
                        if (xt == null) continue;
                        if (File.Exists(xf.Value))
                        {
                            File.Copy(xf.Value, xt.Value, true);
                            File.Delete(xf.Value);
                        }
                    }
                }
                //copy all appropriate plugins directories
                var xcopydir = xroot.Element(PNStrings.ELM_COPY_PLUGS);
                if (xcopydir != null)
                {
                    var xcopies = xcopydir.Elements(PNStrings.ELM_COPY);
                    foreach (var xe in xcopies)
                    {
                        var xf = xe.Attribute(PNStrings.ATT_FROM);
                        if (xf == null) continue;
                        var xt = xe.Attribute(PNStrings.ATT_TO);
                        if (xt == null) continue;
                        if (!Directory.Exists(xf.Value)) continue;
                        var at = xe.Attribute(PNStrings.ATT_DEL_DIR);
                        var deleteDir = at == null || Convert.ToBoolean(at.Value);
                        if (deleteDir)
                        {
                            if (Directory.Exists(xt.Value)) Directory.Delete(xt.Value, true);
                            Directory.CreateDirectory(xt.Value);
                        }
                        at = xe.Attribute(PNStrings.ATT_IS_CRITICAL);
                        if (at != null && Convert.ToBoolean(at.Value))
                        {
                            at = xe.Attribute(PNStrings.ATT_NAME);
                            if (at != null)
                            {
                                listCriticals.Add(at.Value);
                            }
                        }
                        var di = new DirectoryInfo(xf.Value);
                        var files = di.GetFiles();
                        foreach (var f in files)
                        {
                            var pathLocal = Path.Combine(xt.Value, f.Name);
                            File.Copy(f.FullName, pathLocal, true);
                        }
                        Directory.Delete(xf.Value, true);
                    }
                }

                //copy all appropriate themes directories
                xcopydir = xroot.Element(PNStrings.ELM_COPY_THEMES);
                if (xcopydir != null)
                {
                    var xcopies = xcopydir.Elements(PNStrings.ELM_COPY);
                    foreach (var xe in xcopies)
                    {
                        var xf = xe.Attribute(PNStrings.ATT_FROM);
                        if (xf == null) continue;
                        var xt = xe.Attribute(PNStrings.ATT_TO);
                        if (xt == null) continue;
                        File.Copy(xf.Value, xt.Value, true);
                    }
                }

                //delete file pre-run
                File.Delete(filePreRun);

                //log critical updates
                if (!listCriticals.Any()) return;
                using (var sw = new StreamWriter(Path.Combine(Path.GetTempPath(), PNStrings.CRITICAL_UPDATE_LOG), true))
                {
                    foreach (var s in listCriticals)
                        sw.WriteLine(s);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static string _newNoteString = "";
        private static string _newNoteName = "";
        private static string _newNoteTags = "";
        private static string _idToShow = "";
        private static bool _mainWindowExists;
        private static string _repArguments = "";
        private static readonly List<string> _FilesToLoad = new List<string>();
        private static CopyDataType _copyDataType = CopyDataType.NewNote;

        private static bool enumAllWindowsProc(IntPtr hwnd, int msg)
        {
            try
            {
                var sb = new StringBuilder(256);
                PNInterop.GetWindowText(hwnd, sb, 256);
                if (sb.Length > 0 && sb.ToString() == "PNotes.NET Main Window")
                {
                    if (msg == PNInterop.WM_COPYDATA)
                    {
                        var cpdata = new PNInterop.CopyDataStruct { dwData = new IntPtr((int)_copyDataType) };
                        var data = new StringBuilder();
                        switch (_copyDataType)
                        {
                            case CopyDataType.NewNote:
                                cpdata.cbData = Encoding.Default.GetBytes(_newNoteString).Length +
                                                    Encoding.Default.GetBytes(_newNoteName).Length +
                                                    Encoding.Default.GetBytes(_newNoteTags).Length + 3;
                                data.Append(_newNoteString);
                                data.Append(PNStrings.DEL_CHAR);
                                data.Append(_newNoteName);
                                data.Append(PNStrings.DEL_CHAR);
                                data.Append(_newNoteTags);
                                break;
                            case CopyDataType.LoadNotes:
                                foreach (var f in _FilesToLoad)
                                {
                                    cpdata.cbData += Encoding.Default.GetBytes(f).Length + 1;
                                    data.Append(f);
                                    data.Append(PNStrings.DEL_CHAR);
                                }
                                if (data.Length > 0) data = data.Remove(data.Length - 1, 1);
                                break;
                            case CopyDataType.ShowNoteById:
                                cpdata.cbData = Encoding.Default.GetBytes(_idToShow).Length;
                                data.Append(_idToShow);
                                break;
                            case CopyDataType.ExportNotes:
                                cpdata.cbData = Encoding.Default.GetBytes(_repArguments).Length;
                                data.Append(_repArguments);
                                break;
                        }
                        cpdata.lpData = data.ToString();
                        PNInterop.SendMessageCopyData(hwnd, (uint)msg, IntPtr.Zero, ref cpdata);
                    }
                    else
                    {
                        PNInterop.SendMessage(hwnd, (uint)msg, 0, 0);
                    }
                    _mainWindowExists = true;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }
    }
}
