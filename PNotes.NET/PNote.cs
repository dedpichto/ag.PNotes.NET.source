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
using System.Timers;
using System.Windows.Media;
using System.Linq;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace PNotes.NET
{
    internal class PNote : ICloneable, IDisposable
    {
        internal event EventHandler<NoteBooleanChangedEventArgs> NoteBooleanChanged;
        internal event EventHandler<NoteNameChangedEventArgs> NoteNameChanged;
        internal event EventHandler<NoteDateChangedEventArgs> NoteDateChanged;
        internal event EventHandler<NoteGroupChangedEventArgs> NoteGroupChanged;
        internal event EventHandler<NoteDockStatusChangedEventArgs> NoteDockStatusChanged;
        internal event EventHandler<NoteSendReceiveStatusChangedEventArgs> NoteSendReceiveStatusChanged;
        internal event EventHandler NoteTagsChanged;
        internal event EventHandler NoteScheduleChanged;

        internal PNote()
        {
            Id = DateTime.Now.ToString("yyMMddHHmmssfff");
            Opacity = PNRuntimes.Instance.Settings.Behavior.Opacity;
            _Name = PNLang.Instance.GetNoteString("def_caption", "Untitled");
            var group = PNCollections.Instance.Groups.GetGroupById(_GroupId);
            if (group != null)
            {
                group.GroupPropertyChanged += group_GroupPropertyChanged;
            }
            initFields();
        }

        internal PNote(int groupId)
        {
            Id = DateTime.Now.ToString("yyMMddHHmmssfff");
            Opacity = PNRuntimes.Instance.Settings.Behavior.Opacity;
            if (groupId != (int)SpecialGroups.Diary)
            {
                _Name = PNLang.Instance.GetNoteString("def_caption", "Untitled");
            }
            else
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                _Name = DateTime.Today.ToString(PNRuntimes.Instance.Settings.Diary.DateFormat, ci);
            }
            _GroupId = groupId;
            var group = PNCollections.Instance.Groups.GetGroupById(_GroupId);
            if (group != null)
            {
                group.GroupPropertyChanged += group_GroupPropertyChanged;
            }
            initFields();
        }

        internal PNote(PNote src)
        {
            Id = DateTime.Now.ToString("yyMMddHHmmssfff");
            if (src.Skin != null)
            {
                Skin = src.Skin.Clone();
            }
            if (src.Skinless != null)
            {
                Skinless = src.Skinless.Clone();
            }
            CustomOpacity = src.CustomOpacity;
            _GroupId = src._GroupId;
            PrevGroupId = src.PrevGroupId;
            Opacity = src.Opacity;
            _Schedule = src._Schedule.Clone();
            _Protected = src._Protected;
            _Completed = src._Completed;
            _Priority = src._Priority;
            _Pinned = src._Pinned;
            _Name = src._Name;
            _Topmost = src._Topmost;
            Tags = new List<string>(src.Tags);
            LinkedNotes = new List<string>(src.LinkedNotes);
            PinClass = src.PinClass;
            PinText = src.PinText;
            _Scrambled = src._Scrambled;

            var group = PNCollections.Instance.Groups.GetGroupById(_GroupId);
            if (group != null)
            {
                group.GroupPropertyChanged += group_GroupPropertyChanged;
            }
            initFields();
        }

        private string _Name;
        private int _GroupId;
        private bool _Visible;
        private bool _FromDB;
        private bool _Favorite;
        private bool _Changed;
        private bool _Protected;
        private bool _Completed;
        private bool _Priority;
        private bool _Topmost;
        private bool _Rolled;
        private bool _Pinned;
        private DateTime _DateCreated = DateTime.Now;
        private DateTime _DateSaved;
        private DateTime _DateSent;
        private DateTime _DateReceived;
        private DateTime _DateDeleted;
        private string _PasswordString = "";
        private DockStatus _DockStatus = DockStatus.None;
        private SendReceiveStatus _SentReceived = SendReceiveStatus.None;
        private PNNoteSchedule _Schedule = new PNNoteSchedule();
        private bool _Scrambled;

        internal Timer Timer { get; } = new Timer(300);

        public bool Scrambled
        {
            get => _Scrambled;
            set
            {
                var temp = _Scrambled;
                _Scrambled = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Scrambled, null));
                }
            }
        }

        internal bool Thumbnail { get; set; }

        internal double AutoHeight { get; set; }

        internal int DockOrder { get; set; } = -1;
        internal string PinText { get; set; } = "";
        internal string PinClass { get; set; } = "";
        internal string ReceivedFrom { get; set; } = "";

        internal string ReceivedIp { get; set; } = "";

        internal string SentTo { get; set; } = "";
        internal PNNoteSchedule Schedule
        {
            get => _Schedule;
            set
            {
                if (_Schedule != value)
                {
                    _Schedule = value;

                    if (Timer.Enabled)
                    {
                        Timer.Stop();
                    }
                    PNNotesOperations.SaveNoteSchedule(this);

                    if (_Schedule.Type != ScheduleType.None)
                    {
                        Timer.Start();
                    }

                    NoteScheduleChanged?.Invoke(this, new EventArgs());
                }
            }
        }
        internal System.Drawing.Size EditSize { get; set; } = System.Drawing.Size.Empty;
        internal List<string> LinkedNotes { get; set; } = new List<string>();
        internal List<string> Tags { get; set; } = new List<string>();
        internal bool CustomOpacity { get; set; }
        internal double YFactor { get; set; }
        internal double XFactor { get; set; }
        internal DateTime DateDeleted
        {
            get => _DateDeleted;
            set
            {
                var temp = _DateDeleted;
                _DateDeleted = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Deletion));
                }
            }
        }
        internal DateTime DateReceived
        {
            get => _DateReceived;
            set
            {
                var temp = _DateReceived;
                _DateReceived = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Receiving));
                }
            }
        }
        internal DateTime DateSent
        {
            get => _DateSent;
            set
            {
                var temp = _DateSent;
                _DateSent = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Sending));
                }
            }
        }
        internal DateTime DateSaved
        {
            get => _DateSaved;
            set
            {
                var temp = _DateSaved;
                _DateSaved = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Saving));
                }
            }
        }
        internal DateTime DateCreated
        {
            get => _DateCreated;
            set
            {
                var temp = _DateCreated;
                _DateCreated = value;
                if (NoteDateChanged != null && temp != value)
                {
                    NoteDateChanged(this, new NoteDateChangedEventArgs(value, temp, NoteDateType.Creation));
                }
            }
        }
        internal SendReceiveStatus SentReceived
        {
            get => _SentReceived;
            set
            {
                var temp = _SentReceived;
                _SentReceived = value;
                if (NoteSendReceiveStatusChanged == null || temp == value) return;
                NoteSendReceiveStatusChanged(this, new NoteSendReceiveStatusChangedEventArgs(value, temp));
                if (Dialog == null) return;
                if (value == SendReceiveStatus.Sent || value == SendReceiveStatus.Received || value == SendReceiveStatus.Both)
                {
                    Dialog.ApplySentReceivedStatus(true);
                }
                else
                {
                    Dialog.ApplySentReceivedStatus(false);
                }
            }
        }
        internal DockStatus DockStatus
        {
            get => _DockStatus;
            set
            {
                var temp = _DockStatus;
                _DockStatus = value;
                if (NoteDockStatusChanged != null && temp != value)
                {
                    NoteDockStatusChanged(this, new NoteDockStatusChangedEventArgs(value, temp));
                }
            }
        }
        internal bool Rolled
        {
            get => _Rolled;
            set
            {
                var temp = _Rolled;
                _Rolled = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Roll, null));
                }
            }
        }
        internal bool Topmost
        {
            get => _Topmost;
            set
            {
                var temp = _Topmost;
                _Topmost = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Topmost, null));
                }
            }
        }
        internal string Name
        {
            get => _Name;
            set
            {
                var temp = _Name;
                _Name = value;
                if (NoteNameChanged != null && temp != value)
                {
                    NoteNameChanged(this, new NoteNameChangedEventArgs(temp, value));
                }
            }
        }
        internal bool Pinned
        {
            get => _Pinned;
            set
            {
                var temp = _Pinned;
                _Pinned = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Pin, null));
                }
            }
        }
        internal string PasswordString
        {
            get => _PasswordString;
            set
            {
                var temp = _PasswordString;
                _PasswordString = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs((value.Trim().Length > 0), NoteBooleanTypes.Password, null));
                }
            }
        }
        internal bool Priority
        {
            get => _Priority;
            set
            {
                var temp = _Priority;
                _Priority = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Priority, null));
                }
            }
        }
        internal bool Completed
        {
            get => _Completed;
            set
            {
                var temp = _Completed;
                _Completed = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Complete, null));
                }
            }
        }
        internal bool Protected
        {
            get => _Protected;
            set
            {
                var temp = _Protected;
                _Protected = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Protection, null));
                }
            }
        }
        internal bool Changed
        {
            get => _Changed;
            set
            {
                var temp = _Changed;
                _Changed = value;
                if (NoteBooleanChanged == null || temp == value) return;
                var se = new SaveAsNoteNameSetEventArgs(_Name, _GroupId);
                NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Change, se));
            }
        }
        internal int PrevGroupId { get; set; }
        internal bool Favorite
        {
            get => _Favorite;
            set
            {
                var temp = _Favorite;
                _Favorite = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Favorite, null));
                }
            }
        }
        internal bool FromDB
        {
            get => _FromDB;
            set
            {
                var temp = _FromDB;
                _FromDB = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.FromDB, null));
                }
            }
        }
        internal bool Visible
        {
            get => _Visible;
            set
            {
                var temp = _Visible;
                _Visible = value;
                if (NoteBooleanChanged != null && temp != value)
                {
                    NoteBooleanChanged(this, new NoteBooleanChangedEventArgs(value, NoteBooleanTypes.Visible, null));
                }
            }
        }
        internal double Opacity { get; set; }
        internal WndNote Dialog { get; set; }
        internal string Id { get; set; }
        internal int GroupId
        {
            get => _GroupId;
            set
            {
                PrevGroupId = _GroupId;
                _GroupId = value;
                if (_GroupId != PrevGroupId)
                {
                    var group = PNCollections.Instance.Groups.GetGroupById(PrevGroupId);
                    if (group != null)
                    {
                        group.GroupPropertyChanged -= group_GroupPropertyChanged;
                    }
                    group = PNCollections.Instance.Groups.GetGroupById(_GroupId);
                    if (group != null)
                    {
                        group.GroupPropertyChanged += group_GroupPropertyChanged;
                    }

                    NoteGroupChanged?.Invoke(this, new NoteGroupChangedEventArgs(_GroupId, PrevGroupId));
                }
            }
        }
        internal PNSkinlessDetails Skinless { get; set; }
        internal PNSkinDetails Skin { get; set; }
        internal Point NoteLocation { get; set; } = new Point(double.NaN, double.NaN);
        internal Size NoteSize { get; set; } = Size.Empty;

        internal System.Drawing.Color DrawingColor()
        {
            var noteGroup = PNCollections.Instance.Groups.GetGroupById(_GroupId) ?? PNCollections.Instance.Groups.GetGroupById(0);
            var skn = Skinless ?? noteGroup.Skinless;
            return System.Drawing.Color.FromArgb(skn.BackColor.A, skn.BackColor.R,
                skn.BackColor.G, skn.BackColor.B);
        }

        //internal SolidColorBrush Background()
        //{
        //    var noteGroup = PNCollections.Instance.Groups.GetGroupById(_GroupId) ?? PNCollections.Instance.Groups.GetGroupById(0);
        //    var skn = _Skinless ?? noteGroup.Skinless;
        //    return new SolidColorBrush(skn.BackColor);
        //}

        internal void RaiseTagsChangedEvent()
        {
            NoteTagsChanged?.Invoke(this, new EventArgs());
        }

        //internal void RaiseDeleteCompletelyEvent()
        //{
        //    if (NoteDeletedCompletely != null)
        //    {
        //        NoteDeletedCompletely(this, new NoteDeletedCompletelyEventArgs(_Id, _GroupId));
        //    }
        //}

        private void group_GroupPropertyChanged(object sender, GroupPropertyChangedEventArgs e)
        {
            if (!_Visible || Dialog == null) return;
            switch (e.Type)
            {
                case GroupChangeType.BackColor:
                    if (Skinless == null && !PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    {
                        Dialog.ApplyBackColor((Color)e.NewStateObject);
                    }
                    break;
                case GroupChangeType.CaptionColor:
                    if (Skinless == null && !PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    {
                        Dialog.ApplyCaptionFontColor((Color)e.NewStateObject);
                    }
                    break;
                case GroupChangeType.CaptionFont:
                    if (Skinless == null && !PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    {
                        Dialog.ApplyCaptionFont((PNFont)e.NewStateObject);
                    }
                    break;
                case GroupChangeType.Skin:
                    if (Skin == null && PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    {
                        PNSkinsOperations.ApplyNoteSkin(Dialog, this);
                    }
                    break;
            }
        }

        private void initFields()
        {
            try
            {
                Timer.Elapsed += _Timer_Elapsed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void ElapsedDelegate(object sender, ElapsedEventArgs e);

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var continueNext = true;
            try
            {
                var now = e.SignalTime;
                Timer.Stop();
                var doAlarm = false;
                long seconds;
                DateTime start, alarmDate;
                var changeZone = !_Schedule.TimeZone.Equals(TimeZoneInfo.Local);

                switch (_Schedule.Type)
                {
                    case ScheduleType.Once:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.IsDateEqual(now))
                        {
                            doAlarm = true;
                            continueNext = false;
                        }
                        break;
                    case ScheduleType.After:
                        start = _Schedule.StartFrom == ScheduleStart.ExactTime
                            ? (changeZone
                                ? TimeZoneInfo.ConvertTime(_Schedule.StartDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                                : _Schedule.StartDate)
                            : PNRuntimes.Instance.StartTime;
                        PNStatic.NormalizeStartDate(ref start);
                        seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;
                        if (seconds == _Schedule.AlarmAfter.TotalSeconds)
                        {
                            doAlarm = true;
                            continueNext = false;
                        }
                        break;
                    case ScheduleType.RepeatEvery:
                        if (_Schedule.LastRun == DateTime.MinValue)
                        {
                            start = _Schedule.StartFrom == ScheduleStart.ExactTime
                                ? (changeZone
                                    ? TimeZoneInfo.ConvertTime(_Schedule.StartDate, TimeZoneInfo.Local,
                                        _Schedule.TimeZone)
                                    : _Schedule.StartDate)
                                : PNRuntimes.Instance.StartTime;
                        }
                        else
                        {
                            start = _Schedule.LastRun;
                        }
                        PNStatic.NormalizeStartDate(ref start);

                        seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;

                        if (seconds == _Schedule.AlarmAfter.TotalSeconds || (seconds > 0 && seconds % _Schedule.AlarmAfter.TotalSeconds == 0))
                        {
                            doAlarm = true;
                        }
                        break;
                    case ScheduleType.EveryDay:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.IsTimeEqual(now)
                            && (_Schedule.LastRun == DateTime.MinValue || _Schedule.LastRun <= now.AddDays(-1)))
                        {
                            doAlarm = true;
                        }
                        break;
                    case ScheduleType.Weekly:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.IsTimeEqual(now))
                        {
                            if (_Schedule.Weekdays.Contains(now.DayOfWeek)
                                && (_Schedule.LastRun == DateTime.MinValue || _Schedule.LastRun <= now.AddDays(-1)))
                            {
                                doAlarm = true;
                            }
                        }
                        break;
                    case ScheduleType.MonthlyExact:
                        alarmDate = changeZone
                            ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                            : _Schedule.AlarmDate;
                        if (alarmDate.Day == now.Day)
                        {
                            if (alarmDate.IsTimeEqual(now))
                            {
                                if (_Schedule.LastRun == DateTime.MinValue
                                    || _Schedule.LastRun.Month < now.Month
                                    || _Schedule.LastRun.Year < now.Year)
                                {
                                    doAlarm = true;
                                }
                            }
                        }
                        break;
                    case ScheduleType.MonthlyDayOfWeek:
                        if (now.DayOfWeek == _Schedule.MonthDay.WeekDay)
                        {
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(_Schedule.AlarmDate, TimeZoneInfo.Local, _Schedule.TimeZone)
                                : _Schedule.AlarmDate;
                            if (alarmDate.IsTimeEqual(now))
                            {
                                var isLast = false;
                                var ordinal = PNStatic.WeekdayOrdinal(now, _Schedule.MonthDay.WeekDay, ref isLast);
                                if (_Schedule.MonthDay.OrdinalNumber == DayOrdinal.Last)
                                {
                                    if (isLast)
                                    {
                                        if (_Schedule.LastRun == DateTime.MinValue
                                            || _Schedule.LastRun.Month < now.Month
                                            || _Schedule.LastRun.Year < now.Year)
                                        {
                                            doAlarm = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if ((int)_Schedule.MonthDay.OrdinalNumber == ordinal)
                                    {
                                        if (_Schedule.LastRun == DateTime.MinValue
                                            || _Schedule.LastRun.Month < now.Month
                                            || _Schedule.LastRun.Year < now.Year)
                                        {
                                            doAlarm = true;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ScheduleType.MultipleAlerts:
                        foreach (var ma in _Schedule.MultiAlerts.Where(a => !a.Raised))
                        {
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(ma.Date, TimeZoneInfo.Local, _Schedule.TimeZone)
                                : ma.Date;
                            if (!alarmDate.IsDateEqual(now)) continue;
                            ma.Checked = true;
                            doAlarm = true;
                            break;
                        }
                        break;
                }
                if (!doAlarm) return;

                if (!PNWindows.Instance.FormMain.Dispatcher.CheckAccess())
                {
                    ElapsedDelegate d = _Timer_Elapsed;
                    PNWindows.Instance.FormMain.Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    var save = false;
                    switch (_Schedule.Type)
                    {
                        case ScheduleType.EveryDay:
                        case ScheduleType.RepeatEvery:
                        case ScheduleType.Weekly:
                        case ScheduleType.MonthlyExact:
                        case ScheduleType.MonthlyDayOfWeek:
                            _Schedule.LastRun = now;
                            save = true;
                            break;
                        case ScheduleType.MultipleAlerts:
                            var ma = _Schedule.MultiAlerts.FirstOrDefault(a => a.Checked);
                            if (ma != null)
                            {
                                ma.Checked = false;
                                ma.Raised = true;
                            }
                            save = true;
                            break;
                    }
                    PNWindows.Instance.FormMain.ApplyDoAlarm(this);
                    if (save)
                    {
                        PNNotesOperations.SaveNoteSchedule(this);
                    }
                    if (!continueNext)
                    {
                        PNNotesOperations.DeleteNoteSchedule(this);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (continueNext)
                {
                    Timer.Start();
                }
            }
        }

        #region ICloneable Members

        public PNote Clone()
        {
            return ((PNote)((ICloneable)this).Clone());
        }

        object ICloneable.Clone()
        {
        var note = new PNote
            {
                AutoHeight = AutoHeight,
                _Changed = Changed,
                _Completed = Completed,
                CustomOpacity = CustomOpacity,
                _DateCreated = DateCreated,
                _DateDeleted = DateDeleted,
                _DateReceived = DateReceived,
                _DateSaved = DateSaved,
                _DateSent = DateSent,
                DockOrder = DockOrder,
                _DockStatus = DockStatus,
                EditSize = EditSize,
                _Favorite = Favorite,
                _FromDB = FromDB,
                _GroupId = GroupId,
                _Name = Name,
                NoteLocation = NoteLocation,
                NoteSize = NoteSize,
                Opacity = Opacity,
                PinClass = PinClass,
                _Pinned = Pinned,
                PinText = PinText,
                PrevGroupId = PrevGroupId,
                _PasswordString = PasswordString,
                _Priority = Priority,
                _Protected = Protected,
                ReceivedFrom = ReceivedFrom,
                ReceivedIp = ReceivedIp,
                _Rolled = Rolled,
                _Schedule = Schedule.Clone(),
                _Scrambled = Scrambled,
                _SentReceived = SentReceived,
                SentTo = SentTo,
                Tags = new List<string>(Tags.Select(t => t)),
                Thumbnail = Thumbnail,
                _Topmost = Topmost,
                _Visible = Visible,
                XFactor = XFactor,
                YFactor = YFactor,
                LinkedNotes = new List<string>(LinkedNotes.Select(n => n)),
                Dialog = null,
                Id = DateTime.Now.ToString("yyMMddHHmmssfff")
            };
            if (Skin != null)
                note.Skin = Skin.Clone();
            if (Skinless != null)
                note.Skinless = Skinless.Clone();
            return note;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            var group = PNCollections.Instance.Groups.GetGroupById(_GroupId);
            if (group != null)
            {
                group.GroupPropertyChanged -= group_GroupPropertyChanged;
            }
            if (Skin != null)
            {
                Skin.Dispose();
            }
        }

        #endregion
    }
}
