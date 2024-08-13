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

using Microsoft.Reporting.WinForms;
using PNEncryption;
using PNRichEdit;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using WPFStandardStyles;

namespace PNotes.NET
{
    internal class PNExport
    {
        private const string REPORT_QUERY =
            @"SELECT N.ID, N.NAME, '' AS TEXTHOLDER, G.GROUP_NAME, N.DATE_CREATED, N.DATE_CREATED AS DATE_CREATED_TO_CHECK, N.DATE_SAVED, N.DATE_SAVED AS DATE_SAVED_TO_CHECK, N.DATE_SENT, N.DATE_SENT AS DATE_SENT_TO_CHECK, N.DATE_RECEIVED, N.DATE_RECEIVED AS DATE_RECEIVED_TO_CHECK, N.SENT_TO, N.RECEIVED_FROM, N.FAVORITE, N.COMPLETED, N.PRIORITY, N.PROTECTED, N.SCRAMBLED FROM NOTES N LEFT JOIN GROUPS G ON N.GROUP_ID = G.GROUP_ID WHERE N.GROUP_ID <> -2";

        internal static void ExportNotes(ReportType reportType, string fileName, string[] dates)
        {
            try
            {
                switch (reportType)
                {
                    case ReportType.Pdf:
                    case ReportType.Tif:
                    case ReportType.Doc:
                        try
                        {
                            createReport(reportType, fileName, dates);
                        }
                        catch (FileNotFoundException fex)
                        {
                            Mouse.OverrideCursor = null;

                            if (fex.Message.Contains("'Microsoft.ReportViewer"))
                            {
                                PNStatic.LogException(fex, false);
                                if (downloadReportLibs())
                                {
                                    var part1 = PNLang.Instance.GetMessageText("confirm_restart_1",
                                        "In order for the new settings to take effect you have to restart the program.");
                                    var part2 = PNLang.Instance.GetMessageText("confirm_restart_2",
                                        "Press 'Yes' to restart it now, or 'No' to restart later.");
                                    var msg = part1 + '\n' + part2;
                                    if (
                                        WPFMessageBox.Show(msg, PNStrings.PROG_NAME,
                                            System.Windows.MessageBoxButton.YesNo,
                                            System.Windows.MessageBoxImage.Question) ==
                                        System.Windows.MessageBoxResult.Yes)
                                    {
                                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                                    }
                                }
                                return;
                            }
                            PNStatic.LogException(fex);
                        }
                        catch (Exception ex)
                        {
                            PNStatic.LogException(ex);
                        }
                        break;
                    case ReportType.Rtf:
                        exportToRtf(fileName, dates);
                        break;
                    case ReportType.Txt:
                        exportToTxt(fileName, dates);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static bool downloadReportLibs()
        {
            try
            {
                if (WPFMessageBox.Show(
                    PNLang.Instance.GetMessageText("report_lib_missing",
                        "Export to PDF, DOC or TIF requires downloading of additional libraries. Continue?"),
                    PNStrings.PROG_NAME, System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes) return false;
                var d = new WndDownload(PNStrings.REPORT_VIEWER_LIB,
                    System.Windows.Forms.Application.StartupPath);
                var showDialog = d.ShowDialog();
                return showDialog != null && showDialog.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private static IEnumerable<DataRow> reportRows(DataTable t, string[] dates)
        {
            try
            {
                var rows = t.AsEnumerable()
                            .OrderByDescending(r => Convert.ToDateTime(r["DATE_CREATED_TO_CHECK"]))
                            .ThenByDescending(r => Convert.ToDateTime(r["DATE_SAVED_TO_CHECK"])).ToList();
                if (dates != null)
                {
                    if (dates.Length >= 2 && dates[0].IsDate() && dates[1].IsDate())
                    {
                        var from = Convert.ToDateTime(dates[0]);
                        var to = Convert.ToDateTime(dates[1]);
                        rows =
                            rows.Where(
                                r =>
                                    Convert.ToDateTime(r["DATE_CREATED_TO_CHECK"]) >= from &&
                                    Convert.ToDateTime(r["DATE_CREATED_TO_CHECK"]) <= to).ToList();
                    }
                    if (dates.Length >= 4 && dates[2].IsDate() && dates[3].IsDate())
                    {
                        var from = Convert.ToDateTime(dates[2]);
                        var to = Convert.ToDateTime(dates[3]);
                        rows =
                            rows.Where(
                                r =>
                                    Convert.ToDateTime(r["DATE_SAVED_TO_CHECK"]) >= from &&
                                    Convert.ToDateTime(r["DATE_SAVED_TO_CHECK"]) <= to).ToList();
                    }
                }
                return rows;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private static void exportToRtf(string fileName, string[] dates)
        {
            var t = new DataTable();
            try
            {
                loadTableData(ref t, true);
                var rows = reportRows(t, dates);
                if (rows == null) return;
                var rtfBox = new PNRichEditBox();
                var rtfTemp = new PNRichEditBox();

                rtfBox.AppendText(createTxtRtfHeader(dates));

                foreach (var r in rows)
                {
                    rtfBox.AppendText(createTextRtfNoteHeader(r));

                    rtfTemp.Clear();
                    rtfTemp.Rtf = Convert.ToString(r["TEXTHOLDER"]);
                    rtfTemp.Refresh();
                    rtfTemp.SelectAll();
                    rtfTemp.Cut();
                    rtfBox.Paste();

                    rtfBox.AppendText("\n\n");
                    rtfBox.AppendText(new string('*', 55));
                    rtfBox.AppendText("\n\n");
                }
                rtfBox.SaveFile(fileName, System.Windows.Forms.RichTextBoxStreamType.RichText);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                t.Dispose();
            }
        }

        private static void exportToTxt(string fileName, string[] dates)
        {
            var t = new DataTable();
            try
            {
                loadTableData(ref t);
                var rows = reportRows(t, dates);
                if (rows == null) return;
                var sb = new StringBuilder(createTxtRtfHeader(dates));

                foreach (var r in rows)
                {
                    sb.Append(createTextRtfNoteHeader(r));
                    sb.Append(Convert.ToString(r["TEXTHOLDER"]));
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append(new string('*', 55));
                    sb.AppendLine();
                    sb.AppendLine();
                }
                using (var sw = new StreamWriter(fileName))
                {
                    sw.Write(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                t.Dispose();
            }
        }

        private static string createTextRtfNoteHeader(DataRow r)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(PNLang.Instance.GetReportParameter("NoteName", "Note name"));
                sb.Append(": ");
                sb.Append(r["NAME"]);
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowGroup])
                {
                    sb.Append('\t');
                    sb.Append(PNLang.Instance.GetReportParameter("NoteGroup", "Note group"));
                    sb.Append(": ");
                    sb.Append(r["GROUP_NAME"]);
                }
                sb.AppendLine();
                sb.AppendLine();
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowFlags])
                {
                    const string STAR = "(*) ";
                    const string SPACE = "    ";
                    if (Convert.ToBoolean(r["FAVORITE"]))
                    {
                        sb.Append(STAR);
                        sb.Append(PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Favorites", "Favorites"));
                        sb.Append(SPACE);
                    }
                    if (Convert.ToBoolean(r["COMPLETED"]))
                    {
                        sb.Append(STAR);
                        sb.Append(PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Completed", "Completed"));
                        sb.Append(SPACE);
                    }
                    if (Convert.ToBoolean(r["PRIORITY"]))
                    {
                        sb.Append(STAR);
                        sb.Append(PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Priority", "Priority"));
                        sb.Append(SPACE);
                    }
                    if (Convert.ToBoolean(r["PROTECTED"]))
                    {
                        sb.Append(STAR);
                        sb.Append(PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Locked", "Protected"));
                        sb.Append(SPACE);
                    }
                    if (Convert.ToBoolean(r["SCRAMBLED"]))
                    {
                        sb.Append("(*) ");
                        sb.Append(PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Encrypted", "Encrypted"));
                        sb.Append(SPACE);
                    }
                    sb.AppendLine();
                    sb.AppendLine();
                }
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateCreated] || PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSaved])
                {
                    if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateCreated])
                    {
                        sb.Append(PNLang.Instance.GetReportParameter("Created", "Created at"));
                        sb.Append(": ");
                        sb.Append(r["DATE_CREATED"]);
                        sb.Append('\t');
                    }
                    if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSaved])
                    {
                        sb.Append(PNLang.Instance.GetReportParameter("Saved", "Last saved"));
                        sb.Append(": ");
                        sb.Append(r["DATE_SAVED"]);
                    }
                    sb.AppendLine();
                    sb.AppendLine();
                }
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSent] || PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowSentTo])
                {
                    var dt = Convert.ToDateTime(r["DATE_SENT_TO_CHECK"]);
                    if (dt.Year > 1)
                    {
                        if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSent])
                        {
                            sb.Append(PNLang.Instance.GetReportParameter("Sent", "Sent at"));
                            sb.Append(": ");
                            sb.Append(r["DATE_SENT"]);
                            sb.Append('\t');
                        }
                        if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowSentTo])
                        {
                            sb.Append(PNLang.Instance.GetReportParameter("SentTo", "Sent to"));
                            sb.Append(": ");
                            sb.Append(r["SENT_TO"]);
                        }
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                }
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateReceived] || PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowReceivedFrom])
                {
                    var dt = Convert.ToDateTime(r["DATE_RECEIVED_TO_CHECK"]);
                    if (dt.Year > 1)
                    {
                        if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateReceived])
                        {
                            sb.Append(PNLang.Instance.GetReportParameter("Received", "Received at"));
                            sb.Append(": ");
                            sb.Append(r["DATE_RECEIVED"]);
                            sb.Append('\t');
                        }
                        if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowReceivedFrom])
                        {
                            sb.Append(PNLang.Instance.GetReportParameter("RceceivedFrom", "Received from"));
                            sb.Append(": ");
                            sb.Append(r["RECEIVED_FROM"]);
                        }
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                }

                sb.Append(PNLang.Instance.GetReportParameter("NoteText", "Note text"));
                sb.Append(": ");
                sb.AppendLine();
                sb.AppendLine();

                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private static void loadRtf(PNRichEditBox rtfBox, string id)
        {
            try
            {
                var filePath = Path.Combine(PNPaths.Instance.DataDir, id + PNStrings.NOTE_EXTENSION);
                if (PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0 &&
                    PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                {
                    var name = Path.GetFileName(filePath);
                    if (!string.IsNullOrEmpty(name))
                    {
                        var tempPath = Path.Combine(Path.GetTempPath(), name);
                        File.Copy(filePath, tempPath, true);
                        using (var pne = new PNEncryptor(PNRuntimes.Instance.Settings.Protection.PasswordString))
                        {
                            pne.DecryptTextFile(tempPath);
                        }
                        rtfBox.LoadFile(tempPath, System.Windows.Forms.RichTextBoxStreamType.RichText);
                        File.Delete(tempPath);
                    }
                }
                else
                {
                    rtfBox.LoadFile(filePath, System.Windows.Forms.RichTextBoxStreamType.RichText);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void loadTableData(ref DataTable table, bool getRtf = false)
        {
            try
            {
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                    var rtfBox = new PNRichEditBox();
                    table = oData.FillDataTable(REPORT_QUERY);
                    foreach (DataRow r in table.Rows)
                    {
                        var id = Convert.ToString(r["ID"]);

                        loadRtf(rtfBox, id);

                        r.BeginEdit();
                        r["TEXTHOLDER"] = !getRtf ? rtfBox.Text : rtfBox.Rtf;
                        try
                        {
                            r["DATE_CREATED"] =
                                                Convert.ToDateTime(r["DATE_CREATED"])
                                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                                              PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci);
                        }
                        catch (FormatException)
                        {
                            
                            r["DATE_CREATED"] =
                                Convert.ToDateTime(r["DATE_CREATED"])
                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                              PNRuntimes.Instance.CultureInvariant.DateTimeFormat.ShortTimePattern, ci);
                        }
                        try
                        {
                            r["DATE_SAVED"] =
                                                Convert.ToDateTime(r["DATE_SAVED"])
                                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                                              PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci);
                        }
                        catch (FormatException)
                        {

                            r["DATE_SAVED"] =
                                Convert.ToDateTime(r["DATE_SAVED"])
                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                              PNRuntimes.Instance.CultureInvariant.DateTimeFormat.ShortTimePattern, ci);
                        }
                        try
                        {
                            r["DATE_SENT"] =
                                                Convert.ToDateTime(r["DATE_SENT"])
                                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                                              PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci);
                        }
                        catch (FormatException)
                        {

                            r["DATE_SENT"] =
                                Convert.ToDateTime(r["DATE_SENT"])
                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                              PNRuntimes.Instance.CultureInvariant.DateTimeFormat.ShortTimePattern, ci);
                        }
                        try
                        {
                            r["DATE_RECEIVED"] =
                                                Convert.ToDateTime(r["DATE_RECEIVED"])
                                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                                              PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci);
                        }
                        catch (FormatException)
                        {

                            r["DATE_RECEIVED"] =
                                Convert.ToDateTime(r["DATE_RECEIVED"])
                                    .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                              PNRuntimes.Instance.CultureInvariant.DateTimeFormat.ShortTimePattern, ci);
                        }

                        r.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static string createTxtRtfHeader(string[] dates)
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var sb = new StringBuilder();
                sb.AppendLine(PNLang.Instance.GetReportParameter("Title", "PNotes.NET notes report"));
                sb.Append(PNLang.Instance.GetReportParameter("DateTaken", "Printed at"));
                sb.Append('\t');
                try
                {
                    sb.Append(
                                DateTime.Now.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                                      PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci));
                }
                catch (FormatException)
                {
                    
                    sb.Append(
                        DateTime.Now.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                                              PNRuntimes.Instance.CultureInvariant.DateTimeFormat.ShortTimePattern, ci));
                }
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterByCreated] ||
                    PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterBySaved])
                {
                    sb.AppendLine();
                    if (dates.Length >= 2 && dates[0].IsDate() && dates[1].IsDate())
                    {
                        var caption = PNLang.Instance.GetReportParameter("filter_creation",
                            "Creation date between %PLACEHOLDER1% and %PLACEHOLDER2%");
                        caption = caption.Replace(PNStrings.PLACEHOLDER1, Convert.ToDateTime(dates[0])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        caption = caption.Replace(PNStrings.PLACEHOLDER2, Convert.ToDateTime(dates[1])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        sb.Append('\t');
                        sb.AppendLine(caption);
                    }
                    if (dates.Length >= 4 && dates[2].IsDate() && dates[3].IsDate())
                    {
                        var caption = PNLang.Instance.GetReportParameter("filter_saving",
                            "Saving date between %PLACEHOLDER1% and %PLACEHOLDER2%");
                        caption = caption.Replace(PNStrings.PLACEHOLDER1, Convert.ToDateTime(dates[2])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        caption = caption.Replace(PNStrings.PLACEHOLDER2, Convert.ToDateTime(dates[3])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        sb.Append('\t');
                        sb.AppendLine(caption);
                    }
                }
                sb.AppendLine();
                sb.AppendLine();
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private static void createReport(ReportType reportType, string fileName, string[] dates)
        {
            var table = new DataTable();

            try
            {
                const string deviceInfo = @"<DeviceInfo>
                <PageWidth>8.5in</PageWidth>
                <PageHeight>11in</PageHeight>
                <MarginTop>0.25in</MarginTop>
                <MarginLeft>0.25in</MarginLeft>
                <MarginRight>0.25in</MarginRight>
                <MarginBottom>0.25in</MarginBottom>
            </DeviceInfo>";
                var renderType = reportType == ReportType.Pdf
                    ? "PDF"
                    : (reportType == ReportType.Tif ? "Image" : "Word");
                loadTableData(ref table);
                var parameters = new ReportParameterCollection
                {
                    new ReportParameter("Title",
                        PNLang.Instance.GetReportParameter("Title", "PNotes.NET notes report")),
                    new ReportParameter("NoteName",
                        PNLang.Instance.GetReportParameter("NoteName", "Note name")),
                    new ReportParameter("NoteGroup",
                        PNLang.Instance.GetReportParameter("NoteGroup", "Note group")),
                    new ReportParameter("DateTaken",
                        PNLang.Instance.GetReportParameter("DateTaken", "Printed at")),
                    new ReportParameter("Created",
                        PNLang.Instance.GetReportParameter("Created", "Created at")),
                    new ReportParameter("Saved",
                        PNLang.Instance.GetReportParameter("Saved", "Last saved")),
                    new ReportParameter("Sent",
                        PNLang.Instance.GetReportParameter("Sent", "Sent at")),
                    new ReportParameter("Received",
                        PNLang.Instance.GetReportParameter("Received", "Received at")),
                    new ReportParameter("SentTo",
                        PNLang.Instance.GetReportParameter("SentTo", "Sent to")),
                    new ReportParameter("RceceivedFrom",
                        PNLang.Instance.GetReportParameter("RceceivedFrom", "Received from")),
                    new ReportParameter("NoteText",
                        PNLang.Instance.GetReportParameter("NoteText", "Note text")),
                    new ReportParameter("ShowGroup",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowGroup].ToString()),
                    new ReportParameter("ShowCreated",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateCreated].ToString()),
                    new ReportParameter("ShowSaved",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSaved].ToString()),
                    new ReportParameter("ShowSentAt",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateSent].ToString()),
                    new ReportParameter("ShowSentTo",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowSentTo].ToString()),
                    new ReportParameter("ShowReceivedAt",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowDateReceived].ToString()),
                    new ReportParameter("ShowReceivedFrom",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowReceivedFrom].ToString()),
                    new ReportParameter("ShowFlags",
                        PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.ShowFlags].ToString()),
                    new ReportParameter("TextFavorite",
                        PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Favorites", "Favorites")),
                    new ReportParameter("TextCompleted",
                        PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Completed", "Completed")),
                    new ReportParameter("TextPriority",
                        PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Priority", "Priority")),
                    new ReportParameter("TextProtected",
                        PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Locked", "Locked")),
                    new ReportParameter("TextScrambled",
                        PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Encrypted", "Encrypted"))
                };

                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                if (PNRuntimes.Instance.Settings.Config.ReportSettings[ReportSetting.FilterByCreated])
                {
                    var sb = new StringBuilder();
                    if (dates.Length >= 2 && dates[0].IsDate() && dates[1].IsDate())
                    {
                        var caption = PNLang.Instance.GetReportParameter("filter_creation",
                            "Creation date between %PLACEHOLDER1% and %PLACEHOLDER2%");
                        caption = caption.Replace(PNStrings.PLACEHOLDER1, Convert.ToDateTime(dates[0])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        caption = caption.Replace(PNStrings.PLACEHOLDER2, Convert.ToDateTime(dates[1])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        sb.Append(caption);
                        sb.AppendLine();
                    }
                    if (dates.Length >= 4 && dates[2].IsDate() && dates[3].IsDate())
                    {
                        var caption = PNLang.Instance.GetReportParameter("filter_saving",
                            "Saving date between %PLACEHOLDER1% and %PLACEHOLDER2%");
                        caption = caption.Replace(PNStrings.PLACEHOLDER1, Convert.ToDateTime(dates[2])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        caption = caption.Replace(PNStrings.PLACEHOLDER2, Convert.ToDateTime(dates[3])
                            .ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        sb.Append(caption);
                    }
                    parameters.Add(new ReportParameter("Filter", sb.ToString()));
                }

                var rv = new ReportViewer();
                rv.Reset();
                rv.LocalReport.ReportEmbeddedResource = "PNotes.NET.notes.rdlc";
                rv.LocalReport.SetParameters(parameters);
                var rows = reportRows(table, dates).ToArray();
                if (rows.Length == table.Rows.Count)
                {
                    rv.LocalReport.DataSources.Add(new ReportDataSource("dsData", table));
                }
                else
                {
                    var t = table.Clone();
                    try
                    {
                        foreach (var r in rows)
                            t.Rows.Add(r.ItemArray);
                        rv.LocalReport.DataSources.Add(new ReportDataSource("dsData", t));
                    }
                    finally
                    {
                        t.Dispose();
                    }
                }
                rv.RefreshReport();

                var bytes = rv.LocalReport.Render(renderType, deviceInfo, out _, out _,
                    out _, out _, out var _);
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (LocalProcessingException lpex)
            {
                PNStatic.LogException(lpex.InnerException);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                table.Dispose();
            }
        }
    }
}
