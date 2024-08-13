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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using PNDateTimePicker;
using WPFStandardStyles;

namespace PNotes.NET
{
    internal sealed class PNLang
    {
        private static readonly Lazy<PNLang> _Lazy = new Lazy<PNLang>(() => new PNLang());

        private PNLang()
        {
        }

        internal static PNLang Instance => _Lazy.Value;

        private XDocument XLang { get; set; }

        internal FlowDirection GetFlowDirection()
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }

        internal void LoadLanguage(string path)
        {
            var flow1 = GetFlowDirection();
            if (File.Exists(path))
            {
                Instance.XLang = XDocument.Load(path);
                var ci = new CultureInfo(Instance.GetLanguageCulture());
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = ci;
            }
            else
            {
                var ci = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = ci;
            }

            var flow2 = GetFlowDirection();
            if (flow1 == flow2 || Application.Current == null) return;
            foreach (Window w in Application.Current.Windows)
            {
                var type = w.GetType();
                if (type.In(typeof(WndAbout), typeof(WndArrow),
                    typeof(WndSplash), typeof(WndSync), typeof(WndPanel)))
                    continue;
                if (type == typeof(WndNote))
                {
                    if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    {
                        w.FlowDirection = flow2;
                    }
                }
                else
                {
                    w.FlowDirection = flow2;
                }
            }
        }

        internal XElement GetLangElement(string name)
        {
            try
            {
                return Instance.XLang?.Root?.Element(name);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        internal string GetLanguageCulture()
        {
            var xa = Instance.XLang?.Root?.Attribute("culture");
            return xa?.Value ?? "en-US";
        }

        internal string GetDateFormatsText()
        {
            var xp = Instance.XLang?.Root?.Element("date_format_chars");

            if (xp == null)
            {
                return PNStrings.DATE_FORMAT_CHARS;
            }
            var sb = new StringBuilder();
            foreach (var xe in xp.Elements())
            {
                sb.Append(xe.Name);
                sb.Append('\t');
                sb.Append(xe.Value);
                sb.Append('\n');
            }
            return sb.ToString();
        }

        internal string GetTimeFormatsText()
        {
            if (Instance.XLang?.Root == null)
                return PNStrings.TIME_FORMAT_CHARS;
            var sb = new StringBuilder();
            var x12 = Instance.XLang.Root.Element("time_format_chars_h12");
            var x24 = Instance.XLang.Root.Element("time_format_chars_h24");
            var xt = Instance.XLang.Root.Element("time_format_chars");
            if (x12 == null || x24 == null || xt == null) return PNStrings.TIME_FORMAT_CHARS;
            foreach (var xe in x12.Elements())
            {
                sb.Append(xe.Name);
                sb.Append('\t');
                sb.Append(xe.Value);
                sb.Append('\n');
            }
            foreach (var xe in x24.Elements())
            {
                sb.Append(xe.Name);
                sb.Append('\t');
                sb.Append(xe.Value);
                sb.Append('\n');
            }
            foreach (var xe in xt.Elements())
            {
                sb.Append(xe.Name);
                sb.Append('\t');
                sb.Append(xe.Value);
                sb.Append('\n');
            }
            return sb.ToString();
        }

        internal string GetScheduleDescription(ScheduleType type)
        {
            string result = "";
            switch (type)
            {
                case ScheduleType.None:
                    result = "No schedule";
                    break;
                case ScheduleType.After:
                    result = "After:";
                    break;
                case ScheduleType.EveryDay:
                    result = "Every day at:";
                    break;
                case ScheduleType.Weekly:
                    result = "Weekly on:";
                    break;
                case ScheduleType.MonthlyDayOfWeek:
                    result = "Monthly (day of week)";
                    break;
                case ScheduleType.MonthlyExact:
                    result = "Monthly (exact date)";
                    break;
                case ScheduleType.Once:
                    result = "Once at:";
                    break;
                case ScheduleType.RepeatEvery:
                    result = "Repeat every:";
                    break;
                case ScheduleType.MultipleAlerts:
                    result = "Multiple alerts";
                    break;
            }
            if (Instance.XLang == null)
                return result;
            if (Instance.XLang.Root == null) return result;
            var xp = Instance.XLang.Root.Element("schedule_description");
            if (xp == null) return result;
            var name = "_" + (int)type;
            var xe = xp.Element(name);
            if (xe != null)
                result = xe.Value;
            return result;
        }

        internal string GetReportParameter(string name, string defMessage)
        {
            if (Instance.XLang == null)
                return defMessage;
            if (Instance.XLang.Root == null) return defMessage;
            var xp = Instance.XLang.Root.Element("report_parameters");
            if (xp == null) return defMessage;
            var xe = xp.Element(name);
            return xe?.Value ?? defMessage;
        }

        internal string GetSpellText(string name, string defMessage)
        {
            if (Instance.XLang == null)
                return defMessage;
            if (Instance.XLang.Root == null) return defMessage;
            var xp = Instance.XLang.Root.Element("spellchecking");
            if (xp == null) return defMessage;
            var xe = xp.Element(name);
            return xe?.Value ?? defMessage;
        }

        internal string GetMessageText(string name, string defMessage)
        {
            if (Instance.XLang == null)
                return defMessage;
            if (Instance.XLang.Root == null) return defMessage;
            var xp = Instance.XLang.Root.Element("messages");
            if (xp == null) return defMessage;
            var xe = xp.Element(name);
            return xe?.Value ?? defMessage;
        }

        internal string GetNoteString(string name, string defString)
        {
            if (Instance.XLang == null)
                return defString;
            if (Instance.XLang.Root == null) return defString;
            var xp = Instance.XLang.Root.Element("note");
            if (xp == null) return defString;
            var xe = xp.Element(name);
            return xe?.Value ?? defString;
        }

        internal string GetGroupName(string name, string defName)
        {
            if (Instance.XLang == null)
                return defName;
            if (Instance.XLang.Root == null) return defName;
            var xp = Instance.XLang.Root.Element("groups");
            if (xp == null) return defName;
            var xe = xp.Element(name);
            return xe?.Value ?? defName;
        }

        internal string GetCaptionText(string name, string defCaption)
        {
            if (Instance.XLang == null)
                return defCaption;
            if (Instance.XLang.Root == null) return defCaption;
            var xp = Instance.XLang.Root.Element("captions");
            if (xp == null) return defCaption;
            var xe = xp.Element(name);
            return xe?.Value ?? defCaption;
        }

        internal string GetMiscText(string name, string defMisc)
        {
            if (Instance.XLang == null)
                return defMisc;
            if (Instance.XLang.Root == null) return defMisc;
            var xp = Instance.XLang.Root.Element("misc");
            if (xp == null) return defMisc;
            var xe = xp.Element(name);
            return xe?.Value ?? defMisc;
        }

        internal string GetMenuText(string menuSection, string name, string defText)
        {
            try
            {
                if (Instance.XLang == null)
                    return defText;
                if (Instance.XLang.Root == null) return defText;
                var xp = Instance.XLang.Root.Element(menuSection);
                if (xp == null) return defText;
                var xe = xp.Element(name);
                return xe?.Value ?? defText;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return defText;
            }
        }

        internal void ApplyCommandsLanguage()
        {
            try
            {
                var props = new List<PropertyInfo>();
                var type = typeof(ControlPanelCommands);
                props.AddRange(type.GetProperties());
                type = typeof(OkCancelCommands);
                props.AddRange(type.GetProperties());
                type = typeof(SearchCommands);
                props.AddRange(type.GetProperties());
                type = typeof(GroupViewCommands);
                props.AddRange(type.GetProperties());
                type = typeof(ControlGroupsCommands);
                props.AddRange(type.GetProperties());
                type = typeof(MainCommands);
                props.AddRange(type.GetProperties());
                type = typeof(CommonCommands);
                props.AddRange(type.GetProperties());
                type = typeof(NoteCommands);
                props.AddRange(type.GetProperties());
                type = typeof(MenusCommands);
                props.AddRange(type.GetProperties());
                type = typeof(SettingsCommands);
                props.AddRange(type.GetProperties());
                type = typeof(EditCommands);
                props.AddRange(type.GetProperties());
                type = typeof(DropCommands);
                props.AddRange(type.GetProperties());

                foreach (var command in props.Select(pr => pr.GetValue(type, null)).OfType<PNRoutedUICommand>())
                {
                    command.Text = string.IsNullOrEmpty(command.Section)
                        ? GetControlText(command.Name, command.Text)
                        : GetMenuText(command.Section, command.Name, command.Text);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyMenuItemLanguage(MenuItem ti, string section)
        {
            try
            {
                if (Instance.XLang == null)
                    return;
                if (ti.Name == "") return;
                var xElement = Instance.XLang.Root?.Element(section);
                var xe = xElement?.Element(ti.Name);
                if (xe != null)
                    ti.Header = xe.Value;
                foreach (var di in ti.Items.OfType<MenuItem>())
                {
                    ApplyMenuItemLanguage(di, section);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal string GetGridColumnCaption(string columnName, string defaultText)
        {
            try
            {
                var result = defaultText;
                if (Instance.XLang?.Root == null) return result;
                var xp = Instance.XLang.Root.Element("grid_columns");
                if (xp == null) return result;
                xp = xp.Element(columnName);
                if (xp != null)
                {
                    result = xp.Value;
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return defaultText;
            }
        }

        //internal void ApplyColumnsMenuLanguage(ContextMenu ctm)
        //{
        //    try
        //    {
        //        if (Instance.XLang == null || Instance.XLang.Root == null) return;
        //        XElement xp = Instance.XLang.Root.Element("columns");
        //        if (xp != null)
        //        {
        //            var elements = xp.Elements().Where(e => e.Name.ToString().StartsWith("lvwNotes")).ToList();
        //            foreach (MenuItem ti in ctm.Items.OfType<MenuItem>())
        //            {
        //                XElement xe = elements.FirstOrDefault(e => e.Name.ToString().EndsWith('_' + ti.Tag.ToString()));
        //                if (xe != null)
        //                {
        //                    ti.Header = xe.Value;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        PNStatic.LogException(ex);
        //    }
        //}

        internal void ApplyColumnsVisibilityMenuLanguage(ContextMenu ctm, string gridName)
        {
            try
            {
                var xp = Instance.XLang?.Root?.Element("grid_columns");
                if (xp == null) return;
                var elements = xp.Elements().Where(e => e.Name.ToString().StartsWith(gridName)).ToList();
                var items = ctm.Items.OfType<MenuItem>();
                foreach (var item in items)
                {
                    var xe = elements.FirstOrDefault(e => e.Name.ToString().EndsWith((string)item.Tag));
                    if (xe != null)
                    {
                        item.Header = xe.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal string GetControlText(string key, string defString)
        {
            try
            {
                var result = defString;
                var xp = Instance.XLang?.Root?.Element("controls");
                if (xp == null) return result;
                xp = xp.Element(key);
                if (xp != null)
                {
                    result = xp.Value;
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return defString;
            }
        }

        internal string GetOrdinalName(DayOrdinal ordinal)
        {
            try
            {
                var result = ordinal.ToString();
                var xp = Instance.XLang?.Root?.Element("lists");
                if (xp == null) return result;
                xp = xp.Element("cboOrdinal");
                if (xp == null) return result;
                var arr = xp.Value.Split('|');
                result = arr[(int)ordinal - 1];
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return ordinal.ToString();
            }
        }

        internal void ApplyControlLanguage(object c)
        {
            try
            {
                if (Instance.XLang?.Root == null || c == null) return;
                var xp = Instance.XLang.Root.Element("controls");
                if (xp == null) return;
                if (!(c is FrameworkElement fe)) return;
                //if (c is Button button && button.Command != null) return;
                var xe = string.IsNullOrEmpty(fe.Name) ? null : xp.Element(fe.Name);
                if (c is Window window)
                {
                    if (xe != null) window.Title = xe.Value;
                    ApplyControlLanguage(window.Content);
                }
                else if (c is HeaderedContentControl headeredContentControl)
                {
                    if (headeredContentControl.Header is string)
                    {
                        if (xe != null) headeredContentControl.Header = xe.Value;
                    }
                    else
                    {
                        ApplyControlLanguage(headeredContentControl.Header);
                    }
                    ApplyControlLanguage(headeredContentControl.Content);
                }
                else if (c is TextBlock block)
                {
                    if (xe != null) block.Text = xe.Value;
                }
                else if (c is TextBox bx)
                {
                    if (xe != null) bx.Text = xe.Value;
                }
                else if (c is ListView list)
                {
                    if (list.View is GridView grid)
                    {
                        applyGridLanguage(grid, list.Name);
                    }
                }
                else if (c is TabControl tab)
                {
                    foreach (var ti in tab.Items)
                    {
                        ApplyControlLanguage(ti);
                    }
                }
                else if (c is ContentControl contentControl)
                {
                    if (contentControl is SmallButton)
                    {
                        if (xe != null) contentControl.ToolTip = xe.Value;
                    }
                    else if (contentControl.Content is string)
                    {
                        if (xe != null) contentControl.Content = xe.Value;
                    }
                    else
                    {
                        ApplyControlLanguage(contentControl.Content);
                    }
                }

                else if (c is Image image)
                {
                    if (xe != null) image.ToolTip = xe.Value;
                }
                else if (c is ComboBox combo)
                {
                    applyComboItems(combo);
                }
                else if (c is Panel panel)
                {
                    foreach (var ch in panel.Children)
                    {
                        ApplyControlLanguage(ch);
                    }
                }
                else if (c is Border border)
                {
                    ApplyControlLanguage(border.Child);
                }
                else if (c is Popup popup)
                {
                    ApplyControlLanguage(popup.Child);
                }
                else if (c is DateTimePicker dtp)
                {
                    dtp.NowButtonContent = GetCaptionText("now", "Now");
                }
                //foreach (Control ctl in c.Controls)
                //{
                //    ApplyControlLanguage(ctl);
                //}
                //if (c.GetType() == typeof(ToolStrip))
                //{
                //    applyToolstripLanguage((ToolStrip)c);
                //}
                //if (c.GetType() == typeof(TabControl))
                //{
                //    applyTabLanguage((TabControl)c);
                //}
                //if (c.GetType() == typeof(ComboBox))
                //{
                //    applyComboItems((ComboBox)c);
                //}
                //if (c.GetType() == typeof(ListView))
                //{
                //    applyListViewLanguage((ListView)c);
                //}
                //if (c.GetType() == typeof(PNListView))
                //{
                //    applyListViewLanguage((ListView)c);
                //}
                //if (c.GetType() == typeof(PNDataGridView))
                //{
                //    applyGridLanguage((PNDataGridView)c);
                //}
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ChangeScheduleDescriptions()
        {
            try
            {
                var xp = Instance.XLang?.Root?.Element("schedule_description");
                if (xp == null) return;
                var sb = new StringBuilder();
                var element = xp.Element("_1");
                if (element != null) sb.Append(element.Value);
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                PNCollections.Instance.ScheduleDescriptions[ScheduleType.Once] = sb.ToString();
                sb = new StringBuilder();
                element = xp.Element("_2");
                if (element != null) sb.Append(element.Value);
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                PNCollections.Instance.ScheduleDescriptions[ScheduleType.EveryDay] = sb.ToString();
                sb = new StringBuilder();
                element = xp.Element("_3");
                if (element != null) sb.Append(element.Value);
                sb.Append(" ");
                sb.Append(PNStrings.YEARS);
                sb.Append(" ");
                sb.Append(PNStrings.MONTHS);
                sb.Append(" ");
                sb.Append(PNStrings.WEEKS);
                sb.Append(" ");
                sb.Append(PNStrings.DAYS);
                sb.Append(" ");
                sb.Append(PNStrings.HOURS);
                sb.Append(" ");
                sb.Append(PNStrings.MINUTES);
                sb.Append(" ");
                sb.Append(PNStrings.SECONDS);
                PNCollections.Instance.ScheduleDescriptions[ScheduleType.RepeatEvery] = sb.ToString();
                sb = new StringBuilder();
                element = xp.Element("_4");
                if (element != null) sb.Append(element.Value);
                PNCollections.Instance.ScheduleDescriptions[ScheduleType.Weekly] = sb.ToString();
                sb = new StringBuilder();
                element = xp.Element("_5");
                if (element != null) sb.Append(element.Value);
                sb.Append(" ");
                sb.Append(PNStrings.YEARS);
                sb.Append(" ");
                sb.Append(PNStrings.MONTHS);
                sb.Append(" ");
                sb.Append(PNStrings.WEEKS);
                sb.Append(" ");
                sb.Append(PNStrings.DAYS);
                sb.Append(" ");
                sb.Append(PNStrings.HOURS);
                sb.Append(" ");
                sb.Append(PNStrings.MINUTES);
                sb.Append(" ");
                sb.Append(PNStrings.SECONDS);
                PNCollections.Instance.ScheduleDescriptions[ScheduleType.After] = sb.ToString();
                sb = new StringBuilder();
                element = xp.Element("_6");
                if (element != null) sb.Append(element.Value);
                sb.Append(" ");
                element = Instance.XLang.Root.Element("controls");
                if (element != null)
                {
                    var xElement = element.Element("lblExactDate");
                    if (xElement != null)
                        sb.Append(xElement.Value);
                }
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                sb.Append(" ");
                element = Instance.XLang.Root.Element("controls");
                if (element != null)
                {
                    var xElement = element.Element("lblExactTime");
                    if (xElement != null)
                        sb.Append(xElement.Value);
                }
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER2);
                sb.Append(" ");
                PNCollections.Instance.ScheduleDescriptions[ScheduleType.MonthlyExact] = sb.ToString();
                sb = new StringBuilder();
                element = xp.Element("_7");
                if (element != null) sb.Append(element.Value);
                sb.Append(" ");
                element = Instance.XLang.Root.Element("controls");
                if (element != null)
                {
                    var xElement = element.Element("lblDW");
                    if (xElement != null) sb.Append(xElement.Value);
                }
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                sb.Append(" ");
                element = Instance.XLang.Root.Element("controls");
                if (element != null)
                {
                    var xElement = element.Element("lblOrdinal");
                    if (xElement != null) sb.Append(xElement.Value);
                }
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER2);
                sb.Append(" ");
                element = Instance.XLang.Root.Element("controls");
                if (element != null)
                {
                    var xElement = element.Element("lblDWTime");
                    if (xElement != null) sb.Append(xElement.Value);
                }
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER3);
                sb.Append(" ");
                PNCollections.Instance.ScheduleDescriptions[ScheduleType.MonthlyDayOfWeek] = sb.ToString();
                element = xp.Element("_8");
                if (element != null)
                {
                    PNCollections.Instance.ScheduleDescriptions[ScheduleType.MultipleAlerts] = element.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal string GetNoteScheduleDescription(PNNoteSchedule sc, DayOfWeekStruct[] days)
        {
            try
            {
                if (sc.Type == ScheduleType.None)
                    return "";
                var ci = new CultureInfo(Instance.GetLanguageCulture());
                var result = PNCollections.Instance.ScheduleDescriptions[sc.Type];
                switch (sc.Type)
                {
                    case ScheduleType.Once:
                        result = result.Replace(PNStrings.PLACEHOLDER1, sc.AlarmDate.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci));
                        break;
                    case ScheduleType.EveryDay:
                        result = result.Replace(PNStrings.PLACEHOLDER1, sc.AlarmDate.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci));
                        break;
                    case ScheduleType.RepeatEvery:
                    case ScheduleType.After:
                        result = sc.AlarmAfter.Years > 0
                                     ? result.Replace(PNStrings.YEARS,
                                                      sc.AlarmAfter.Years.ToString(CultureInfo.InvariantCulture) + " " +
                                                      Instance.GetControlText("lblAfterYears", "Years"))
                                     : result.Replace(PNStrings.YEARS, "");
                        result = sc.AlarmAfter.Months > 0
                                     ? result.Replace(PNStrings.MONTHS,
                                                      sc.AlarmAfter.Months.ToString(CultureInfo.InvariantCulture) + " " +
                                                      Instance.GetControlText("lblAfterMonths", "Months"))
                                     : result.Replace(PNStrings.MONTHS, "");
                        result = sc.AlarmAfter.Weeks > 0
                                     ? result.Replace(PNStrings.WEEKS,
                                                      sc.AlarmAfter.Weeks.ToString(CultureInfo.InvariantCulture) + " " +
                                                      Instance.GetControlText("lblAfterWeeks", "Weeks"))
                                     : result.Replace(PNStrings.WEEKS, "");
                        result = sc.AlarmAfter.Days > 0
                                     ? result.Replace(PNStrings.DAYS,
                                                      sc.AlarmAfter.Days.ToString(CultureInfo.InvariantCulture) + " " +
                                                      Instance.GetControlText("lblAfterDays", "Days"))
                                     : result.Replace(PNStrings.DAYS, "");
                        result = sc.AlarmAfter.Hours > 0
                                     ? result.Replace(PNStrings.HOURS,
                                                      sc.AlarmAfter.Hours.ToString(CultureInfo.InvariantCulture) + " " +
                                                      Instance.GetControlText("lblAfterHours", "Hours"))
                                     : result.Replace(PNStrings.HOURS, "");
                        result = sc.AlarmAfter.Minutes > 0
                                     ? result.Replace(PNStrings.MINUTES,
                                                      sc.AlarmAfter.Minutes.ToString(CultureInfo.InvariantCulture) + " " +
                                                      Instance.GetControlText("lblAfterMinutes", "Minutes"))
                                     : result.Replace(PNStrings.MINUTES, "");
                        result = sc.AlarmAfter.Seconds > 0
                                     ? result.Replace(PNStrings.SECONDS,
                                                      sc.AlarmAfter.Seconds.ToString(CultureInfo.InvariantCulture) + " " +
                                                      Instance.GetControlText("lblAfterSeconds", "Seconds"))
                                     : result.Replace(PNStrings.SECONDS, "");
                        result += " ";
                        result += Instance.GetControlText("lblAfterStart", "Starting from:");
                        result += " ";
                        if (sc.StartFrom == ScheduleStart.ExactTime)
                        {
                            result += Instance.GetControlText("optAfterExact", "Exact time");
                            result += " ";
                            result += sc.StartDate.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci);
                        }
                        else
                        {
                            result += Instance.GetControlText("optAfterProgram", "Program start");
                        }
                        break;
                    case ScheduleType.Weekly:
                        result += " ";
                        foreach (DayOfWeek wd in sc.Weekdays)
                        {
                            result += days.FirstOrDefault(dw => dw.DayOfW == wd).Name;
                            result += ", ";
                        }
                        if (result.EndsWith(", "))
                        {
                            result = result.Substring(0, result.Length - 2);
                        }
                        result += " ";
                        result += Instance.GetControlText("lblWeeklyAt", "At:");
                        result += " ";
                        result += sc.AlarmDate.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci);
                        break;
                    case ScheduleType.MonthlyExact:
                        result = result.Replace(PNStrings.PLACEHOLDER1, sc.AlarmDate.Day.ToString(CultureInfo.InvariantCulture));
                        result = result.Replace(PNStrings.PLACEHOLDER2, sc.AlarmDate.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci));
                        break;
                    case ScheduleType.MonthlyDayOfWeek:
                        result = result.Replace(PNStrings.PLACEHOLDER1, days.FirstOrDefault(wd => wd.DayOfW == sc.MonthDay.WeekDay).Name);
                        result = result.Replace(PNStrings.PLACEHOLDER2, Instance.GetOrdinalName(sc.MonthDay.OrdinalNumber));
                        result = result.Replace(PNStrings.PLACEHOLDER3, sc.AlarmDate.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci));
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void applyComboItems(ComboBox cbo)
        {
            try
            {
                if (Instance.XLang?.Root == null) return;
                var index = cbo.SelectedIndex;
                var xp = Instance.XLang.Root.Element("list_" + cbo.Name);
                if (xp != null)
                {
                    var arr =
                        (from e in xp.Elements() select e.Value).ToArray();
                    cbo.Items.Clear();
                    foreach (var s in arr)
                    {
                        cbo.Items.Add(s);
                    }
                }
                if (cbo.Items.Count > index)
                    cbo.SelectedIndex = index;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        //private void applyPNComboItems(PNComboBox cbo)
        //{
        //    try
        //    {
        //        if (Instance.XLang == null || Instance.XLang.Root == null) return;
        //        int index = cbo.SelectedIndex;
        //        XElement xp = Instance.XLang.Root.Element("list_" + cbo.Name);
        //        if (xp != null)
        //        {
        //            var arr =
        //                (from e in xp.Elements() select e.Value).ToArray();
        //            cbo.Items.Clear();
        //            foreach (var s in arr)
        //            {
        //                cbo.Items.Add(s);
        //            }
        //        }
        //        if (cbo.Items.Count > index)
        //            cbo.SelectedIndex = index;
        //    }
        //    catch (Exception ex)
        //    {
        //        PNStatic.LogException(ex);
        //    }
        //}

        //private void applyListViewLanguage(ListView lv)
        //{
        //    try
        //    {
        //        if (Instance.XLang == null || Instance.XLang.Root == null)
        //            return;
        //        XElement xp = Instance.XLang.Root.Element("columns");
        //        if (xp == null) return;
        //        var elements = xp.Elements().Where(e => e.Name.ToString().StartsWith(lv.Name)).ToList();
        //        for (int i = 0; i < lv.Columns.Count; i++)
        //        {
        //            var xe = elements.FirstOrDefault(e => e.Name.ToString() == lv.Name + '_' + i);
        //            if (xe != null)
        //            {
        //                lv.Columns[i].Text = xe.Value;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        PNStatic.LogException(ex);
        //    }
        //}

        private static void applyGridLanguage(GridView grd, string parentName)
        {
            try
            {
                var xp = Instance.XLang?.Root?.Element("grid_columns");
                if (xp == null) return;
                var elements = xp.Elements().Where(e => e.Name.ToString().StartsWith(parentName)).ToList();
                foreach (var c in grd.Columns)
                {
                    var name = c.GetValue(WPFGridViewHelper.ColumnNameProperty).ToString();
                    if (string.IsNullOrEmpty(name)) continue;
                    var elm = elements.FirstOrDefault(e => e.Name.ToString().EndsWith(name));
                    if (elm == null) continue;
                    if (!(c.Header is Image))
                        c.Header = elm.Value;
                    else
                        ((Image)c.Header).ToolTip = elm.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        //private void applyTabLanguage(TabControl tab)
        //{
        //    try
        //    {
        //        if (Instance.XLang == null)
        //            return;
        //        if (Instance.XLang.Root != null)
        //        {
        //            XElement xp = Instance.XLang.Root.Element("controls");
        //            foreach (TabPage tp in tab.TabPages)
        //            {
        //                if (xp != null)
        //                {
        //                    XElement xe = xp.Element(tp.Name);
        //                    if (xe != null)
        //                    {
        //                        tp.Text = xe.Value;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        PNStatic.LogException(ex);
        //    }
        //}

        //private void applyToolstripLanguage(ToolStrip tst)
        //{
        //    try
        //    {
        //        if (Instance.XLang == null)
        //            return;
        //        if (Instance.XLang.Root != null)
        //        {
        //            XElement xp = Instance.XLang.Root.Element("controls");
        //            foreach (ToolStripItem ti in tst.Items)
        //            {
        //                if (ti.GetType() == typeof(ToolStripButton) || ti.GetType() == typeof(ToolStripDropDownButton))
        //                {
        //                    if (xp != null)
        //                    {
        //                        XElement xe = xp.Element(ti.Name);
        //                        if (xe != null)
        //                        {
        //                            ti.ToolTipText = ti.Text = xe.Value;
        //                        }
        //                    }
        //                }
        //                else if (ti.GetType() == typeof(ToolStripTextBox))
        //                {
        //                    if (xp != null)
        //                    {
        //                        XElement xe = xp.Element(ti.Name);
        //                        if (xe != null)
        //                        {
        //                            ti.Text = xe.Value;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        PNStatic.LogException(ex);
        //    }
        //}
    }
}
