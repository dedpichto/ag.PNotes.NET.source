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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using WinApp = System.Windows.Forms.Application;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndAbout.xaml
    /// </summary>
    public partial class WndAbout
    {
        public WndAbout()
        {
            InitializeComponent();
        }

        private const string THANKS =
            "Sergey Hristov, Holger Stöhr, Andrey Tuliev, Jean-Pierre Dagomet, Robert, SimonC, rickytbf, Kakha, pekingduckman, unegreuche, majkinetor, Langua, Cengiz, Dy Nama, Martijn van Cauteren, Yukto8492, kduchow, lparis70, Sunil, Didier Bunel, Miroslav Abrahám, Scott Sell, Charlie Smith, gilmar, Yann, Salvafito, Timor Gruber, Paul Beutel, Leopoldus, Sussi Petkova, Hilsen Jørn, sri3kandi, Jack Butcher, Seyed, Robert Thomas, Leandro Martín Drudi, Peter L. Albert, JM";

        private void cmdLicense_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;
                var path = Path.Combine(WinApp.StartupPath, "License.txt");
                if (!File.Exists(path)) return;
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdOK_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void progMail_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Process.Start("mailto:" + progMail.Text);
        }

        private void DlgAbout_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var fontSegoe = PNCollections.Instance.PrivateFonts.Families.FirstOrDefault(f => f.Name.ToUpper().StartsWith("SEGOE SCRIPT"));
                var family = fontSegoe?.Name ?? "Segoe Script";
                FontFamily = new FontFamily(family);
                FontSize = 16;
                PNLang.Instance.ApplyControlLanguage(this);

                var caption = PNLang.Instance.GetCaptionText("about", "About");
                var ass = Assembly.GetExecutingAssembly();
                var assName = ass.GetName();

                Title = caption + @" - " + assName.Name;

                progName.Text = assName.Name + " - " + WinApp.ProductVersion + "\n";
                var attrs = ass.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attrs.Length > 0)
                {
                    if (attrs[0] is AssemblyDescriptionAttribute ata)
                    {
                        progDesc.Text = ata.Description + "\n";
                    }
                    else
                    {
                        progDesc.Text = "";
                    }
                }
                else
                {
                    progDesc.Text = "";
                }
                attrs = ass.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attrs.Length > 0)
                {
                    if (attrs[0] is AssemblyCopyrightAttribute ata)
                    {
                        progCopy.Text = ata.Copyright + "\n";
                    }
                    else
                    {
                        progCopy.Text = "";
                    }
                }
                else
                {
                    progCopy.Text = "";
                }

                lblGPL.Text = PNLang.Instance.GetMessageText("license",
                    "This program is distributed under the terms of the GNU General Public License version 2 or later.");

                cntAbout.AddTextBlock("SQLite database engine by Hwaci");
                cntAbout.AddUrl("http://www.sqlite.org/");
                cntAbout.AddTextBlock("NHunspell spell checking library by Thomas Maierhofer");
                cntAbout.AddUrl("http://www.maierhofer.de/en/");
                cntAbout.AddTextBlock("NotifyIcon control by Philipp Sumi");
                cntAbout.AddUrl("http://www.hardcodet.net/wpf-notifyicon");
                cntAbout.AddTextBlock("DotNetZip library by Dino Chiesa");
                cntAbout.AddUrl("http://dotnetzip.codeplex.com/");
                cntAbout.AddTextBlock("Interop.Domino library by nbranzburg");
                cntAbout.AddUrl("https://www.nuget.org/packages/interop.domino.dll/");
                cntAbout.AddTextBlock("Icons embedding utility by Einar Egilsson");
                cntAbout.AddUrl("http://einaregilsson.com/add-multiple-icons-to-a-dotnet-application/");
                cntAbout.AddTextBlock(PNLang.Instance.GetMessageText("code_thanks",
                    "Special thanks for very helpful code snippets and examples to:"), new Thickness(0, 16, 0, 0));
                cntAbout.AddTextBlock("DeeKey (custom WPF window style), Bulat Gafurov (grid lines for ListView), Sacha Barber (retreiving list of network computers), Thomas Levesque (ListView sorting)");
                cntAbout.AddTextBlock(PNLang.Instance.GetMessageText("sug_thanks",
                    "Thanks for suggestions and selfless testing to:"), new Thickness(0, 16, 0, 0));
                cntAbout.AddTextBlock(THANKS);
                cntAbout.AddTextBlock(PNLang.Instance.GetMessageText("thanks",
                    "Thanks to all over the world who tests, translates and simply uses the program."),
                    new Thickness(0, 16, 0, 0));

                cntAbout.StartAnimation();
                var wa = SystemInformation.WorkingArea;// Screen.GetWorkingArea(new System.Drawing.Point((int)Left, (int)Top));
                Left = (wa.Width - ActualWidth) / 2;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgAbout_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Escape) return;
                e.Handled = true;
                Close();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void AboutControl_AboutLinkClicked(object sender, AboutLinkClickedEventArgs e)
        {
            PNStatic.LoadPage(e.Link);
        }

        private void DlgAbout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
