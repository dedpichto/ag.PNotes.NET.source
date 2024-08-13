// PNotes.NET - open source desktop notes manager
// Copyright (C) 2016 Andrey Gruber

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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  

using System;
using System.Text;
using System.Windows;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndConfigureReport.xaml
    /// </summary>
    public partial class WndConfigureReport
    {
        public WndConfigureReport()
        {
            InitializeComponent();
            _Loaded = true;
            DataContext = PNSingleton.Instance.FontUser;
        }

        private readonly bool _Loaded;

        private void DlgConfigureReport_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                chkCreated.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateCreated];
                chkGroup.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowGroup];
                chkReceivedAt.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateReceived];
                chkReceivedFrom.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowReceivedFrom];
                chkSaved.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSaved];
                chkSentAt.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSent];
                chkSentTo.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowSentTo];
                chkAllDates.IsChecked = !PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterByCreated] &&
                                        !PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterBySaved];
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterByCreated])
                {
                    chkCreation.IsChecked = true;
                    var dates = PNRuntimes.Instance.Settings.Config.ReportDates.Split("|");
                    if (dates.Length >= 2 && dates[0].IsDate() && dates[1].IsDate())
                    {
                        dtpCrFrom.DateValue = Convert.ToDateTime(dates[0], PNRuntimes.Instance.CultureInvariant);
                        dtpCrTo.DateValue = Convert.ToDateTime(dates[1], PNRuntimes.Instance.CultureInvariant);
                    }
                }
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterBySaved])
                {
                    chkSaving.IsChecked = true;
                    var dates = PNRuntimes.Instance.Settings.Config.ReportDates.Split("|");
                    if (dates.Length >= 4 && dates[2].IsDate() && dates[3].IsDate())
                    {
                        dtpSvFrom.DateValue = Convert.ToDateTime(dates[2], PNRuntimes.Instance.CultureInvariant);
                        dtpSvTo.DateValue = Convert.ToDateTime(dates[3], PNRuntimes.Instance.CultureInvariant);
                    }
                }
                chkFlags.IsChecked = PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowFlags];
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void oKClick()
        {
            try
            {
                var creationValid = false;
                var savingValid = false;
                if (chkCreation.IsChecked != null && chkCreation.IsChecked.Value)
                {
                    creationValid = true;
                    if (dtpCrFrom.DateValue > dtpCrTo.DateValue)
                    {
                        WPFMessageBox.Show(
                            PNLang.Instance.GetMessageText("invalid_start_date",
                                "Starting date should not be more than ending date"), PNStrings.PROG_NAME,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }
                if (chkSaving.IsChecked != null && chkSaving.IsChecked.Value)
                {
                    savingValid = true;
                    if (dtpSvFrom.DateValue > dtpSvTo.DateValue)
                    {
                        WPFMessageBox.Show(
                            PNLang.Instance.GetMessageText("invalid_start_date",
                                "Starting date should not be more than ending date"), PNStrings.PROG_NAME,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }
                if (chkCreated.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateCreated] = chkCreated.IsChecked.Value;
                if (chkGroup.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowGroup] = chkGroup.IsChecked.Value;
                if (chkReceivedAt.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateReceived] = chkReceivedAt.IsChecked.Value;
                if (chkReceivedFrom.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowReceivedFrom] = chkReceivedFrom.IsChecked.Value;
                if (chkSaved.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSaved] = chkSaved.IsChecked.Value;
                if (chkSentAt.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSent] = chkSentAt.IsChecked.Value;
                if (chkSentTo.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowSentTo] = chkSentTo.IsChecked.Value;
                if (chkCreation.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterByCreated] = chkCreation.IsChecked.Value;
                if (chkSaving.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterBySaved] = chkSaving.IsChecked.Value;
                if (chkFlags.IsChecked != null)
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowFlags] = chkFlags.IsChecked.Value;
                if (chkAllDates.IsChecked != null && chkAllDates.IsChecked.Value)
                {
                    PNRuntimes.Instance.Settings.Config.ReportDates = "";
                    PNData.SaveReportFields(PNRuntimes.Instance.Settings.Config.ReportSettings, "");
                }
                else
                {
                    var sb = new StringBuilder();
                    if (creationValid)
                    {
                        sb.Append(dtpCrFrom.DateValue.ToString(PNStrings.DATE_FORMAT, PNRuntimes.Instance.CultureInvariant));
                        sb.Append("|");
                        sb.Append(dtpCrTo.DateValue.ToString(PNStrings.DATE_FORMAT, PNRuntimes.Instance.CultureInvariant));
                        sb.Append("|");
                    }
                    else
                    {
                        sb.Append("||");
                    }
                    if (savingValid)
                    {
                        sb.Append(dtpSvFrom.DateValue.ToString(PNStrings.DATE_FORMAT, PNRuntimes.Instance.CultureInvariant));
                        sb.Append("|");
                        sb.Append(dtpSvTo.DateValue.ToString(PNStrings.DATE_FORMAT, PNRuntimes.Instance.CultureInvariant));
                        sb.Append("|");
                    }
                    else
                    {
                        sb.Append("||");
                    }
                    //remove last '|' character
                    if (sb.Length > 0) sb.Length -= 1;
                    PNRuntimes.Instance.Settings.Config.ReportDates = sb.ToString();
                    PNData.SaveReportFields(PNRuntimes.Instance.Settings.Config.ReportSettings, sb.ToString());
                }
                
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void chkAllDates_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                chkCreation.IsChecked = chkSaving.IsChecked = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgConfigureReport_Closed(object sender, EventArgs e)
        {
            PNWindows.Instance.FormConfigReport = null;
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
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
