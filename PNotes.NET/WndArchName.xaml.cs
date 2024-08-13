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
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndArchName.xaml
    /// </summary>
    public partial class WndArchName
    {
        public WndArchName()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        public WndArchName(List<string> files)
            : this()
        {
            _Files = files;
        }

        private readonly List<string> _Files;

        private void oKClick()
        {
            try
            {
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    Directory.Delete(PNPaths.Instance.TempDir, true);
                }
                Directory.CreateDirectory(PNPaths.Instance.TempDir);
                var zipPath = Path.Combine(PNPaths.Instance.TempDir, txtArchName.Text.Trim() + ".zip");
                using (var package = Package.Open(zipPath, FileMode.OpenOrCreate))
                {
                    foreach (string f in _Files)
                    {
                        var fileName = Path.GetFileName(f);
                        if (fileName == null) continue;
                        var partUriFile = PackUriHelper.CreatePartUri(new Uri(fileName, UriKind.Relative));
                        var packagePartFile = package.CreatePart(partUriFile, MediaTypeNames.Text.RichText, CompressionOption.Normal);
                        if (packagePartFile == null) continue;
                        package.CreateRelationship(partUriFile, TargetMode.Internal, fileName);
                        using (var fileStream = new FileStream(f, FileMode.Open, FileAccess.Read))
                        {
                            PNStatic.CopyStream(fileStream, packagePartFile.GetStream());
                        }
                    }
                }
                var archives = new List<string> { zipPath };
                PNNotesOperations.SendNotesAsAttachments(archives);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgArchName_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    PNLang.Instance.ApplyControlLanguage(this);
                    Title = lblArchName.Text;
                    FlowDirection = PNLang.Instance.GetFlowDirection();
                    txtArchName.Focus();
                }
                catch (Exception ex)
                {
                    PNStatic.LogException(ex);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
                        e.CanExecute = txtArchName.Text.Trim().Length > 0;
                        break;
                    case CommandType.Cancel:
                        e.CanExecute = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
                        oKClick();
                        break;
                    case CommandType.Cancel:
                        DialogResult = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
