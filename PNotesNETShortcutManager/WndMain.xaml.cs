using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Path = System.IO.Path;

namespace PNotesNETShortcutManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private OpenFileDialog ofd = new OpenFileDialog
        {
            Filter = "PNotes.NET executable|PNotes.NET.exe",
            Title = "PNotes.NET executable"
        };

        private CommonOpenFileDialog fbd = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            AddToMostRecentlyUsedList = false,
            AllowNonFileSystemItems = false,
            EnsurePathExists = true,
            EnsureFileExists = true,
            EnsureReadOnly = false,
            EnsureValidNames = true,
            Multiselect = false,
            ShowPlacesList = true
        };

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            new WndAbout {Owner = this}.ShowDialog();
        }

        private void cmdConfDb_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Configuration database location", txtConfDb);
        }

        private void cmdNotesDb_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Notes database location", txtNotesDb);
        }

        private void cmdSkins_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Skins location", txtSkins);
        }

        private void cmdBackup_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Backup location", txtBackup);
        }

        private void cmdLang_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Language files location", txtLang);
        }

        private void cmdSounds_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Sounds location", txtSounds);
        }

        private void cmdFonts_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Custom fonts location", txtFonts);
        }

        private void cmdDict_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Spell check dictionaries location", txtDict);
        }

        private void cmdPlugins_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Plugins location", txtPlugins);
        }

        private void cmdThemess_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Visual themes", txtThemes);
        }

        private void cmdCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (optShortcut.IsChecked != null && optShortcut.IsChecked.Value)
                {
                    if (createShortcut())
                    {
                        MessageBox.Show(@"Shortcut created successfully");
                    }
                }
                else if (optVbs.IsChecked != null && optVbs.IsChecked.Value)
                {
                    if (createVbs())
                    {
                        MessageBox.Show(@"VBScript file created successfully");
                    }
                }
                else if (optJs.IsChecked != null && optJs.IsChecked.Value)
                {
                    if (createJs())
                    {
                        MessageBox.Show(@"JavaScript file created successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var os = Environment.OSVersion;
                var vs = os.Version;
                if (os.Platform == PlatformID.Win32NT)
                {
                    if (vs.Major >= 6) return;
                    optShortcut.IsEnabled = false;
                    optVbs.IsChecked = true;
                }
                else
                {
                    optShortcut.IsEnabled = false;
                    optVbs.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void LogException(Exception ex, bool showMessage = true)
        {
            try
            {
                var type = ex.GetType();
                using (var w = new StreamWriter("pnnetshmngr.log", true))
                {
                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    sb.Append(" ");
                    sb.Append(type);
                    sb.Append(" ");
                    sb.Append(ex.Message);
                    sb.AppendLine();
                    sb.Append(ex.StackTrace);
                    w.WriteLine(sb.ToString());
                }
                if (showMessage)
                    MessageBox.Show(type.ToString() + '\n' + ex.Message);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void cmdExe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ofd.ShowDialog(this).Value) return;
                txtExe.Text = ofd.FileName;
                var v = AssemblyName.GetAssemblyName(txtExe.Text).Version;
                tbVersion.Text = @"PNotes.NET version: " + v;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void cmdShortcut_Click(object sender, RoutedEventArgs e)
        {
            chooseFolder("Shortcut location", txtShortcut);
        }

        private void chooseFolder(string caption, TextBox textBox)
        {
            try
            {
                fbd.Title = caption;
                if (fbd.ShowDialog(this)== CommonFileDialogResult.Ok)
                {
                    textBox.Text = fbd.FileName;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmdCreate.IsEnabled = txtExe.Text.Trim().Length > 0 && txtShortcut.Text.Trim().Length > 0;
        }

        private bool createJs()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("var command = \"\\\"\" + \"");
                sb.Append(txtExe.Text.Replace("\\", "\\\\"));
                sb.Append("\" + \"\\\"\"");
                var isText = false;
                foreach (var grd in stpOptional.Children.OfType<Grid>())
                {
                    if (grd.Children.OfType<TextBox>().Any(tx => tx.Text.Trim().Length > 0))
                    {
                        isText = true;
                    }
                    if (isText)
                    {
                        break;
                    }
                }
                if (!isText)
                {
                    if (chkExchange.IsChecked != null && !chkExchange.IsChecked.Value)
                    {
                        sb.Append(" + \" -nonetwork\"");
                        if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                        {
                            sb.Append(" + \" nosplash\"");
                        }
                    }
                    else
                    {
                        if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                        {
                            sb.Append(" + \" -nosplash\"");
                        }
                    }
                }
                else
                {
                    sb.Append(chkExchange.IsChecked != null && chkExchange.IsChecked.Value ? " + \" -conf\"" : " + \" -confnonetwork\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtConfDb.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");

                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtNotesDb.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtSkins.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtBackup.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtLang.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtSounds.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtFonts.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtDict.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtPlugins.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    sb.Append(" + \" \" + \"\\\"\" + \"");
                    sb.Append(txtThemes.Text.Trim().Replace("\\", "\\\\"));
                    sb.Append("\" + \"\\\"\"");
                    if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                    {
                        sb.Append(" + \" nosplash\"");
                    }
                }
                using (var sw = new StreamWriter(Path.Combine(txtShortcut.Text, "PNotes.NET.js")))
                {
                    sw.WriteLine("var WshShell = new ActiveXObject(\"WScript.Shell\");");
                    sw.WriteLine(sb.ToString());
                    sw.WriteLine("WshShell.Exec(command);");
                    sw.WriteLine("WshShell = null;");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        private bool createVbs()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("command = Chr(34) + \"");
                sb.Append(txtExe.Text);
                sb.Append("\" + Chr(34)");
                var isText = false;
                foreach (var grd in stpOptional.Children.OfType<Grid>())
                {
                    if (grd.Children.OfType<TextBox>().Any(tx => tx.Text.Trim().Length > 0))
                    {
                        isText = true;
                    }
                    if (isText)
                    {
                        break;
                    }
                }
                if (!isText)
                {
                    if (chkExchange.IsChecked != null && !chkExchange.IsChecked.Value)
                    {
                        sb.Append(" + \" -nonetwork\"");
                        if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                        {
                            sb.Append(" + \" nosplash\"");
                        }
                    }
                    else
                    {
                        if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                        {
                            sb.Append(" + \" -nosplash\"");
                        }
                    }
                }
                else
                {
                    sb.Append(chkExchange.IsChecked != null && !chkExchange.IsChecked.Value ? " + \" -confnonetwork \"" : " + \" -conf \"");
                    sb.Append(" + Chr(34) + \"");
                    sb.Append(txtConfDb.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtNotesDb.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtSkins.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtBackup.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtLang.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtSounds.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtFonts.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtDict.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtPlugins.Text.Trim());
                    sb.Append("\" + Chr(34) + Chr(32) + Chr(34) + \"");
                    sb.Append(txtThemes.Text.Trim());
                    sb.Append("\" + Chr(34)");
                    if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                    {
                        sb.Append(" + \" nosplash\"");
                    }
                }
                using (var sw = new StreamWriter(Path.Combine(txtShortcut.Text, "PNotes.NET.vbs")))
                {
                    sw.WriteLine("Dim WshShell, command");
                    sw.WriteLine("Set WshShell = CreateObject(\"WScript.Shell\")");
                    sw.WriteLine(sb.ToString());
                    sw.WriteLine("WshShell.Exec(command)");
                    sw.WriteLine("Set WshShell = Nothing");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        private bool createShortcut()
        {
            try
            {
                using (var link = new PNShellLink())
                {
                    link.ShortcutFile = Path.Combine(txtShortcut.Text.Trim(), "PNotes.NET.lnk");
                    link.Target = txtExe.Text;
                    link.WorkingDirectory = Path.GetDirectoryName(txtExe.Text);
                    link.Description = "Shortcut to PNotes.NET";
                    link.IconIndex = 0;
                    var sb = new StringBuilder();
                    var isText = false;
                    foreach (var grd in stpOptional.Children.OfType<Grid>())
                    {
                        if (grd.Children.OfType<TextBox>().Any(tx => tx.Text.Trim().Length > 0))
                        {
                            isText = true;
                        }
                        if (isText)
                        {
                            break;
                        }
                    }
                    if (!isText)
                    {
                        if (chkExchange.IsChecked != null && !chkExchange.IsChecked.Value)
                        {
                            sb.Append("-nonetwork");
                            if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                            {
                                sb.Append(" nosplash");
                            }
                        }
                        else
                        {
                            if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                            {
                                sb.Append("-nosplash");
                            }
                        }
                    }
                    else
                    {
                        sb.Append(chkExchange.IsChecked != null && !chkExchange.IsChecked.Value ? "-confnonetwork" : "-conf");
                        sb.Append(" \"");
                        sb.Append(txtConfDb.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtNotesDb.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtSkins.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtBackup.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtLang.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtSounds.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtFonts.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtDict.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtPlugins.Text.Trim());
                        sb.Append("\"");
                        sb.Append(" \"");
                        sb.Append(txtThemes.Text.Trim());
                        sb.Append("\"");
                        if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                        {
                            sb.Append(" nosplash");
                        }
                    }
                    if (sb.Length > 0)
                    {
                        link.Arguments = sb.ToString();
                    }
                    link.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }
    }
}
