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

using PNotes.NET.Annotations;
using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FontWeight = System.Windows.FontWeight;

namespace PNotes.NET
{

    internal class PNSyncComp : ICloneable
    {
        internal bool UseDataDir { get; set; } = true;
        internal string DBDir { get; set; } = "";
        internal string DataDir { get; set; } = "";
        internal string CompName { get; set; } = "";

        #region ICloneable members
        internal PNSyncComp Clone()
        {
            return (PNSyncComp)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNContact : ICloneable
    {
        internal int Id { get; set; }
        internal int GroupId { get; set; } = -1;
        internal bool UseComputerName { get; set; } = true;
        internal string IpAddress { get; set; } = "";
        internal string ComputerName { get; set; } = "";
        internal string Name { get; set; } = "";

        public override string ToString()
        {
            return Name;
        }

        #region ICloneable members
        internal PNContact Clone()
        {
            return (PNContact)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNContactGroup : ICloneable
    {
        internal int Id { get; set; }
        internal string Name { get; set; } = "";

        public override string ToString()
        {
            return Name;
        }

        #region ICloneable members
        internal PNContactGroup Clone()
        {
            return (PNContactGroup)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNSmtpProfile : ICloneable
    {
        internal int Id { get; set; }
        internal bool Active { get; set; }
        internal string HostName { get; set; }
        internal string SenderAddress { get; set; }
        internal string Password { get; set; }
        internal int Port { get; set; }
        internal string DisplayName { get; set; }

        #region ICloneable members
        internal PNSmtpProfile Clone()
        {
            return (PNSmtpProfile)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNMailContact : ICloneable
    {
        internal int Id { get; set; }
        internal string DisplayName { get; set; }
        internal string Address { get; set; }

        #region ICloneable members
        internal PNMailContact Clone()
        {
            return (PNMailContact)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNSearchProvider : ICloneable
    {
        internal string QueryString { get; set; } = "";
        internal string Name { get; set; } = "";

        #region ICloneable members
        internal PNSearchProvider Clone()
        {
            return (PNSearchProvider)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNExternal : ICloneable
    {
        internal string CommandLine { get; set; } = "";
        internal string Program { get; set; } = "";
        internal string Name { get; set; } = "";

        #region ICloneable members
        internal PNExternal Clone()
        {
            return (PNExternal)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNHotKey : ICloneable
    {
        private string _MenuName = "";
        private int _Id;
        private HotkeyModifiers _Modifiers = 0;
        private uint _Vk;
        private string _Shortcut = "";
        private HotkeyType _Type = HotkeyType.Main;

        internal HotkeyType Type
        {
            get => _Type;
            set => _Type = value;
        }
        internal string Shortcut
        {
            get => _Shortcut;
            set => _Shortcut = value;
        }
        internal uint Vk
        {
            get => _Vk;
            set => _Vk = value;
        }
        internal HotkeyModifiers Modifiers
        {
            get => _Modifiers;
            set => _Modifiers = value;
        }
        internal int Id
        {
            get => _Id;
            set => _Id = value;
        }
        internal string MenuName
        {
            get => _MenuName;
            set => _MenuName = value;
        }
        internal void CopyFrom(PNHotKey hk)
        {
            _Modifiers = hk.Modifiers;
            _Shortcut = hk._Shortcut;
            _Vk = hk._Vk;
        }
        internal void Clear()
        {
            _Modifiers = HotkeyModifiers.ModNone;
            _Shortcut = "";
            _Vk = 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNHotKey hk))
                return false;
            return Equals(hk);
        }

        public bool Equals(PNHotKey hk)
        {
            if ((object)hk == null)
                return false;
            return (_Vk == hk._Vk
                && _Shortcut == hk._Shortcut
                && _Modifiers == hk._Modifiers
                && _MenuName == hk._MenuName
                && _Id == hk._Id
                && _Type == hk._Type);
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += Id.GetHashCode();
            result *= 37;
            result += MenuName.GetHashCode();
            result *= 37;
            result += Modifiers.GetHashCode();
            result *= 37;
            result += Shortcut.GetHashCode();
            result *= 37;
            result += Vk.GetHashCode();
            result *= 37;
            result += Type.GetHashCode();
            return result;
        }

        public static bool operator ==(PNHotKey a, PNHotKey b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNHotKey a, PNHotKey b)
        {
            return (!(a == b));
        }

        #region ICloneable Members
        internal PNHotKey Clone()
        {
            return (PNHotKey)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNMenu : ICloneable
    {
        public string Name { get; }
        public string Text { get; }
        public string ParentName { get; }
        public List<PNMenu> Items { get; }
        public string ContextName { get; }

        internal PNMenu(string name, string text, string parentName, string context)
        {
            Name = name;
            Text = text;
            ParentName = parentName;
            ContextName = context;
            Items = new List<PNMenu>();
        }

        #region ICloneable Members
        internal PNMenu Clone()
        {
            return (PNMenu)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            var menu = new PNMenu(Name, Text, ParentName, ContextName);
            menu.Items.AddRange(Items.Select(i => i.Clone()));
            return menu;
        }
        #endregion
    }
    
    internal class PNHiddenMenu : ICloneable
    {
        public string Name { get; }
        public MenuType Type { get; }

        internal PNHiddenMenu(string name, MenuType type)
        {
            Name = name;
            Type = type;
        }

        #region ICloneable Members
        internal PNHiddenMenu Clone()
        {
            return (PNHiddenMenu)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return new PNHiddenMenu(Name, Type);
        }
        #endregion
    }
    
    internal class AlarmAfterValues : ICloneable
    {
        private int _Years;
        private int _Months;
        private int _Weeks;
        private int _Days;
        private int _Hours;
        private int _Minutes;
        private int _Seconds;

        internal int Seconds
        {
            get => _Seconds;
            set => _Seconds = value;
        }
        internal int Minutes
        {
            get => _Minutes;
            set => _Minutes = value;
        }
        internal int Hours
        {
            get => _Hours;
            set => _Hours = value;
        }
        internal int Days
        {
            get => _Days;
            set => _Days = value;
        }
        internal int Weeks
        {
            get => _Weeks;
            set => _Weeks = value;
        }
        internal int Months
        {
            get => _Months;
            set => _Months = value;
        }
        internal int Years
        {
            get => _Years;
            set => _Years = value;
        }
        internal long TotalSeconds => (_Years * TimeSpan.TicksPerDay * 365) / TimeSpan.TicksPerSecond
                                      + (_Months * TimeSpan.TicksPerDay * 30) / TimeSpan.TicksPerSecond
                                      + (_Weeks * TimeSpan.TicksPerDay * 7) / TimeSpan.TicksPerSecond
                                      + (_Days * TimeSpan.TicksPerDay) / TimeSpan.TicksPerSecond
                                      + (_Hours * TimeSpan.TicksPerHour) / TimeSpan.TicksPerSecond
                                      + (_Minutes * TimeSpan.TicksPerMinute) / TimeSpan.TicksPerSecond
                                      + _Seconds;

        public override bool Equals(object obj)
        {
            if (!(obj is AlarmAfterValues aa))
                return false;
            return Equals(aa);
        }

        public bool Equals(AlarmAfterValues aa)
        {
            if ((object)aa == null)
                return false;
            return (TotalSeconds == aa.TotalSeconds);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += TotalSeconds.GetHashCode();
            return result;
        }

        #region ICloneable Members
        internal AlarmAfterValues Clone()
        {
            return (AlarmAfterValues)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion

        public static bool operator ==(AlarmAfterValues a, AlarmAfterValues b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(AlarmAfterValues a, AlarmAfterValues b)
        {
            return (!(a == b));
        }
    }
    
    internal class MonthDay : ICloneable
    {
        private DayOrdinal _OrdinalNumber = DayOrdinal.First;
        private DayOfWeek _WeekDay = new DateTimeFormatInfo().FirstDayOfWeek;

        internal DayOfWeek WeekDay
        {
            get => _WeekDay;
            set => _WeekDay = value;
        }

        internal DayOrdinal OrdinalNumber
        {
            get => _OrdinalNumber;
            set => _OrdinalNumber = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MonthDay md))
                return false;
            return Equals(md);
        }

        public bool Equals(MonthDay md)
        {
            if ((object)md == null)
                return false;
            return (OrdinalNumber == md.OrdinalNumber
                && WeekDay == md.WeekDay);
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += OrdinalNumber.GetHashCode();
            result *= 37;
            result += WeekDay.GetHashCode();
            return result;
        }

        #region ICloneable Members
        public MonthDay Clone()
        {
            return (MonthDay)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion

        public static bool operator ==(MonthDay a, MonthDay b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(MonthDay a, MonthDay b)
        {
            return (!(a == b));
        }
    }
    
    internal class PNNoteSchedule : ICloneable
    {
        private TimeZoneInfo _TimeZone = TimeZoneInfo.Local;
        private List<MultiAlert> _MultiAlerts = new List<MultiAlert>();
        private string _ProgramToRunOnAlert;
        private bool _CloseOnNotification;
        private List<DayOfWeek> _Weekdays = new List<DayOfWeek>();
        private AlarmAfterValues _AlarmAfter = new AlarmAfterValues();
        private MonthDay _MonthDay = new MonthDay();
        private ScheduleStart _StartFrom = ScheduleStart.ExactTime;
        private bool _UseTts;
        private bool _SoundInLoop = true;
        private int _RepeatCount;
        private bool _Track = true;
        private int _StopAfter = -1;
        private string _Sound = PNSchedule.DEF_SOUND;
        private DateTime _LastRun = DateTime.MinValue;
        private DateTime _StartDate;
        private DateTime _AlarmDate;
        private ScheduleType _Type = ScheduleType.None;

        internal TimeZoneInfo TimeZone
        {
            get => _TimeZone;
            set => _TimeZone = value;
        }

        internal List<MultiAlert> MultiAlerts
        {
            get => _MultiAlerts;
            set => _MultiAlerts = value;
        }

        internal string ProgramToRunOnAlert
        {
            get => _ProgramToRunOnAlert;
            set => _ProgramToRunOnAlert = value;
        }

        internal bool CloseOnNotification
        {
            get => _CloseOnNotification;
            set => _CloseOnNotification = value;
        }

        internal List<DayOfWeek> Weekdays
        {
            get => _Weekdays;
            set => _Weekdays = value;
        }

        internal AlarmAfterValues AlarmAfter
        {
            get => _AlarmAfter;
            set => _AlarmAfter = value;
        }

        internal MonthDay MonthDay
        {
            get => _MonthDay;
            set => _MonthDay = value;
        }

        internal ScheduleStart StartFrom
        {
            get => _StartFrom;
            set => _StartFrom = value;
        }

        internal bool UseTts
        {
            get => _UseTts;
            set => _UseTts = value;
        }

        internal bool SoundInLoop
        {
            get => _SoundInLoop;
            set => _SoundInLoop = value;
        }

        internal int RepeatCount
        {
            get => _RepeatCount;
            set => _RepeatCount = value;
        }

        internal bool Track
        {
            get => _Track;
            set => _Track = value;
        }

        internal int StopAfter
        {
            get => _StopAfter;
            set => _StopAfter = value;
        }

        internal string Sound
        {
            get => _Sound;
            set => _Sound = value;
        }

        internal DateTime LastRun
        {
            get => _LastRun;
            set => _LastRun = value;
        }

        internal DateTime StartDate
        {
            get => _StartDate;
            set => _StartDate = value;
        }

        internal DateTime AlarmDate
        {
            get => _AlarmDate;
            set => _AlarmDate = value;
        }

        internal ScheduleType Type
        {
            get => _Type;
            set => _Type = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNNoteSchedule sc))
                return false;
            return Equals(sc);
        }

        public bool Equals(PNNoteSchedule sc)
        {
            if ((object)sc == null)
                return false;
            return (Type == sc.Type
                && _AlarmDate.IsDateEqual(sc._AlarmDate)
                && _Sound == sc._Sound
                && _StopAfter == sc._StopAfter
                && _Track == sc._Track
                && _SoundInLoop == sc._SoundInLoop
                && _RepeatCount == sc._RepeatCount
                && _UseTts == sc._UseTts
                && _StartFrom == sc._StartFrom
                && _MonthDay == sc._MonthDay
                && _AlarmAfter == sc._AlarmAfter
                && !_Weekdays.Inequals(sc._Weekdays)
                && _ProgramToRunOnAlert == sc._ProgramToRunOnAlert
                && _CloseOnNotification == sc._CloseOnNotification
                && _MultiAlerts.IsEqual(sc._MultiAlerts)
                && _TimeZone.Equals(sc._TimeZone)
                && _StartDate.Equals(sc._StartDate));
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += Type.GetHashCode();
            result *= 37;
            result += AlarmDate.GetHashCode();
            result *= 37;
            result += Sound.GetHashCode();
            result *= 37;
            result += StopAfter.GetHashCode();
            result *= 37;
            result += Track.GetHashCode();
            result *= 37;
            result += SoundInLoop.GetHashCode();
            result *= 37;
            result += RepeatCount.GetHashCode();
            result *= 37;
            result += UseTts.GetHashCode();
            result *= 37;
            result += StartFrom.GetHashCode();
            result *= 37;
            result += MonthDay.GetHashCode();
            result *= 37;
            result += Weekdays.GetHashCode();
            result *= 37;
            result += ProgramToRunOnAlert.GetHashCode();
            result *= 37;
            result += CloseOnNotification.GetHashCode();
            result *= 37;
            result += MultiAlerts.GetHashCode();
            result *= 37;
            result += TimeZone.GetHashCode();
            result *= 37;
            result += StartDate.GetHashCode();
            return result;
        }

        public static bool operator ==(PNNoteSchedule a, PNNoteSchedule b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNNoteSchedule a, PNNoteSchedule b)
        {
            return (!(a == b));
        }

        #region ICloneable Members
        public PNNoteSchedule Clone()
        {
            return (PNNoteSchedule)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            var sd = new PNNoteSchedule
            {
                AlarmDate = AlarmDate,
                Sound = Sound,
                StopAfter = StopAfter,
                Track = Track,
                SoundInLoop = SoundInLoop,
                RepeatCount = RepeatCount,
                UseTts = UseTts,
                StartFrom = StartFrom,
                MonthDay = MonthDay.Clone(),
                AlarmAfter = AlarmAfter.Clone(),
                ProgramToRunOnAlert = ProgramToRunOnAlert,
                CloseOnNotification = CloseOnNotification,
                TimeZone = TimeZone,
                Type = Type,
                StartDate = StartDate,
                LastRun = LastRun
            };
            sd.Weekdays.AddRange(Weekdays);
            sd.MultiAlerts.AddRange(MultiAlerts.Select(ma => ma.Clone()));
            return sd;
        }
        #endregion
    }
    
    internal class MultiAlert : ICloneable
    {
        internal DateTime Date { get; set; }
        internal bool Raised { get; set; }
        internal bool Checked { get; set; }

        #region ICloneable Members
        internal MultiAlert Clone()
        {
            return (MultiAlert)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
    
    internal class PNGroup : ICloneable, IDisposable
    {
        internal const int DEFAULT_FONTSIZE = 8;
        private readonly System.Drawing.Color _DefaultFontColor = System.Drawing.Color.Black;

        internal event EventHandler<GroupPropertyChangedEventArgs> GroupPropertyChanged;

        private int _Id = -1;
        private int _ParentId = -1;
        private string _Name = "";
        private string _PasswordString = "";
        private List<PNGroup> _Subgroups = new List<PNGroup>();
        private PNSkinDetails _Skin = new PNSkinDetails();
        private PNSkinlessDetails _Skinless = new PNSkinlessDetails();
        private LOGFONT _Font;
        private System.Drawing.Color _FontColor = System.Drawing.Color.Black;
        private BitmapImage _Image;
        private bool _IsDefaultImage;

        internal PNGroup()
        {
            //in order to register 'application' and 'pack' schemes if they are not registered yet
            // ReSharper disable once UnusedVariable
            var c = Application.Current;
            Image = new FrameworkElement().TryFindResource("gr") as BitmapImage;
            _Font.Init();
            _Font.lfFaceName = PNStrings.DEFAULT_FONT_NAME;
        }

        internal string ImageName
        {
            get
            {
                var arr = Image.ToString().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                return arr.Length > 0 ? Path.GetFileNameWithoutExtension(arr[arr.Length - 1]) : "";
            }
        }

        internal BitmapImage Image
        {
            get => _Image;
            set => _Image = value;
        }

        internal List<PNGroup> Subgroups
        {
            get => _Subgroups;
            set => _Subgroups = value;
        }
        internal PNSkinDetails Skin
        {
            get => _Skin;
            set => _Skin = value;
        }
        internal PNSkinlessDetails Skinless
        {
            get => _Skinless;
            set => _Skinless = value;
        }
        internal LOGFONT Font
        {
            get => _Font;
            set => _Font = value;
        }
        internal System.Drawing.Color FontColor
        {
            get => _FontColor;
            set => _FontColor = value;
        }
        internal string PasswordString
        {
            get => _PasswordString;
            set => _PasswordString = value;
        }
        internal int ParentId
        {
            get => _ParentId;
            set => _ParentId = value;
        }
        internal string Name
        {
            get => _Name;
            set => _Name = value;
        }
        internal int Id
        {
            get => _Id;
            set => _Id = value;
        }

        internal bool IsDefaultImage
        {
            get => _IsDefaultImage;
            set => _IsDefaultImage = value;
        }

        internal PNGroup GetGroupById(int id)
        {
            return _Id == id ? this : subGroupByID(this, id);
        }

        private PNGroup subGroupByID(PNGroup parent, int id)
        {
            foreach (var g in parent._Subgroups)
            {
                if (g._Id == id)
                {
                    return g;
                }
                var result = subGroupByID(g, id);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        internal void Clear()
        {
            _Skin.Dispose();
            _Skin = new PNSkinDetails();
            _Skinless = new PNSkinlessDetails();
            var lf = new LOGFONT();
            lf.Init();
            lf.SetFontFace(PNStrings.DEFAULT_FONT_NAME);
            lf.SetFontSize(DEFAULT_FONTSIZE);
            _Font = lf;
            _FontColor = _DefaultFontColor;
        }

        internal void CopyTo(PNGroup pg)
        {
            var changed = (pg._Skinless != _Skinless);
            var changedBackColor = (pg._Skinless.BackColor != _Skinless.BackColor);
            var changedCaptionFont = (!pg._Skinless.CaptionFont.Equals(_Skinless.CaptionFont));
            var changedCaptionColor = (pg._Skinless.CaptionColor != _Skinless.CaptionColor);

            pg._Skinless = _Skinless.Clone();
            if (changed && pg.GroupPropertyChanged != null)
            {
                if (changedBackColor)
                {
                    pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Skinless.BackColor, GroupChangeType.BackColor));
                }
                if (changedCaptionFont)
                {
                    pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Skinless.CaptionFont, GroupChangeType.CaptionFont));
                }
                if (changedCaptionColor)
                {
                    pg.GroupPropertyChanged(pg, new GroupPropertyChangedEventArgs(pg._Skinless.CaptionColor, GroupChangeType.CaptionColor));
                }
            }
            changed = (pg._Skin.SkinName != _Skin.SkinName);
            pg._Skin.Dispose();
            pg._Skin = _Skin.Clone();
            if (changed)
            {
                pg.GroupPropertyChanged?.Invoke(pg, new GroupPropertyChangedEventArgs(pg._Skin.SkinName, GroupChangeType.Skin));
            }
            changed = (!pg._Font.Equals(_Font));
            pg._Font = _Font;
            if (changed)
            {
                pg.GroupPropertyChanged?.Invoke(pg, new GroupPropertyChangedEventArgs(pg._Font, GroupChangeType.Font));
            }
            changed = (pg._FontColor != _FontColor);
            pg._FontColor = _FontColor;
            if (changed)
            {
                pg.GroupPropertyChanged?.Invoke(pg, new GroupPropertyChangedEventArgs(pg._FontColor, GroupChangeType.FontColor));
            }
            changed = (!pg.Image.Equals(Image));
            pg.Image = Image.Clone();
            pg.IsDefaultImage = IsDefaultImage;
            if (changed)
            {
                pg.GroupPropertyChanged?.Invoke(pg, new GroupPropertyChangedEventArgs(pg.Image, GroupChangeType.Image));
            }
        }

        public override string ToString()
        {
            return _Name;
        }

        public override bool Equals(object obj)
        {
            var gr = obj as PNGroup;
            return (object)gr != null && Equals(gr);
        }

        public bool Equals(PNGroup gr)
        {
            if ((object)gr == null)
                return false;
            return (_Skin == gr._Skin
                && _Skinless == gr._Skinless
                && _PasswordString == gr._PasswordString
                && _FontColor.ToArgb() == gr._FontColor.ToArgb()
                && _Font == gr._Font
                && Image.Equals(gr.Image)
                && IsDefaultImage == gr.IsDefaultImage);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += Id.GetHashCode();
            result *= 37;
            result += Name.GetHashCode();
            result *= 37;
            result += Skinless.GetHashCode();
            result *= 37;
            result += Skin.GetHashCode();
            result *= 37;
            result += Image.GetHashCode();
            result *= 37;
            result += PasswordString.GetHashCode();
            result *= 37;
            result += Font.GetHashCode();
            result *= 37;
            result += FontColor.ToArgb().GetHashCode();
            result *= 37;
            result += IsDefaultImage.GetHashCode();

            return result;
        }

        public static bool operator ==(PNGroup a, PNGroup b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNGroup a, PNGroup b)
        {
            return (!(a == b));
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (PNGroup g in _Subgroups)
            {
                g.Dispose();
            }
            if (_Skin != null)
            {
                _Skin.Dispose();
            }
        }

        #endregion

        #region ICloneable Members
        internal PNGroup Clone()
        {
            return (PNGroup)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return new PNGroup
            {
                Skinless = Skinless.Clone(),
                Skin = Skin.Clone(),
                PasswordString = PasswordString,
                ParentId = ParentId,
                Name = Name,
                Id = Id,
                FontColor = FontColor,
                Image = Image?.Clone(),
                IsDefaultImage = IsDefaultImage,
                Font = Font
            };
        }

        #endregion
    }

    internal class SearchNotesPrefs : ICloneable
    {
        internal bool WholeWord { get; set; }
        internal bool MatchCase { get; set; }
        internal bool IncludeHidden { get; set; }
        internal int Criteria { get; set; }
        internal int Scope { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(WholeWord);
            sb.Append("|");
            sb.Append(MatchCase);
            sb.Append("|");
            sb.Append(IncludeHidden);
            sb.Append("|");
            sb.Append(Criteria);
            sb.Append("|");
            sb.Append(Scope);
            return sb.ToString();
        }

        #region ICloneable members
        internal SearchNotesPrefs Clone()
        {
            return (SearchNotesPrefs)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }

    internal class GrdSort
    {
        internal ListSortDirection SortOrder { get; set; } = ListSortDirection.Ascending;
        internal bool LastSorted { get; set; }
        internal string Key { get; set; }
    }

    public class ThemesUpdate
    {
        internal string FriendlyName { get; set; }
        internal string Name { get; set; }
        internal string FileName { get; set; }
        internal string Suffix { get; set; }

        public override string ToString()
        {
            return FriendlyName + " " + Suffix;
        }
    }

    public class PluginsUpdate
    {
        internal string Name { get; set; }
        internal string ProductName { get; set; }
        internal string Suffix { get; set; }
        internal int Type { get; set; }

        public override string ToString()
        {
            return Name + " " + Suffix;
        }
    }

    internal class CriticalPluginUpdate
    {
        internal string ProductName { get; set; }
        internal string FileName { get; set; }
    }

    internal class DictData
    {
        internal string LangName { get; set; }
        internal string ZipFile { get; set; }
    }

    public class SplashTextProvider : INotifyPropertyChanged
    {
        private string _SplashText;
        public event PropertyChangedEventHandler PropertyChanged;

        public string SplashText
        {
            get => _SplashText;
            set
            {
                if (_SplashText == value) return;
                _SplashText = value;
                OnSplashTextChanged(nameof(SplashText));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnSplashTextChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class PNFont : INotifyPropertyChanged, ICloneable
    {
        private FontFamily _FontFamily = new FontFamily(PNStrings.DEF_CAPTION_FONT);
        private double _FontSize = 12;
        private FontStyle _FontStyle = FontStyles.Normal;
        private FontWeight _FontWeight = FontWeights.Normal;
        private FontStretch _FontStretch = FontStretches.Normal;

        public FontFamily FontFamily
        {
            get => _FontFamily;
            set
            {
                if (Equals(_FontFamily, value)) return;
                _FontFamily = value;
                OnPropertyChanged();
            }
        }

        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (!(Math.Abs(_FontSize - value) > double.Epsilon)) return;
                _FontSize = value;
                OnPropertyChanged();
            }
        }

        public FontStyle FontStyle
        {
            get => _FontStyle;
            set
            {
                if (Equals(_FontStyle, value)) return;
                _FontStyle = value;
                OnPropertyChanged();
            }
        }

        public FontWeight FontWeight
        {
            get => _FontWeight;
            set
            {
                if (Equals(_FontWeight, value)) return;
                _FontWeight = value;
                OnPropertyChanged();
            }
        }

        public FontStretch FontStretch
        {
            get => _FontStretch;
            set
            {
                if (Equals(_FontStretch, value)) return;
                _FontStretch = value;
                OnPropertyChanged();
            }
        }

        public override bool Equals(object obj)
        {
            var pf = obj as PNFont;
            return pf != null && Equals(pf);
        }

        public bool Equals(PNFont pf)
        {
            if (pf == null)
                return false;
            return (Equals(_FontFamily, pf._FontFamily) && Math.Abs(_FontSize - pf._FontSize) < double.Epsilon && _FontStretch == pf._FontStretch &&
                    _FontStyle == pf._FontStyle && _FontWeight == pf._FontWeight);
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += FontFamily.GetHashCode();
            result *= 37;
            result += FontSize.GetHashCode();
            result *= 37;
            result += FontStretch.GetHashCode();
            result *= 37;
            result += FontStyle.GetHashCode();
            result *= 37;
            result += FontWeight.GetHashCode();

            return result;
        }

        public override string ToString()
        {
            return _FontFamily.ToString();
        }

        public static bool operator ==(PNFont a, PNFont b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNFont a, PNFont b)
        {
            return (!(a == b));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region ICloneable Members
        public PNFont Clone()
        {
            return (PNFont)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return new PNFont
            {
                FontFamily = new FontFamily(PNStrings.DEF_CAPTION_FONT),
                FontSize = FontSize,
                FontStretch = FontStretch,
                FontStyle = FontStyle,
                FontWeight = FontWeight
            };
        }
        #endregion
    }

    public class PNListBoxItem : ListBoxItem
    {
        internal event EventHandler<ListBoxItemCheckChangedEventArgs> ListBoxItemCheckChanged;

        private readonly CheckBox _CheckBox = new CheckBox
        {
            Margin = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Visibility = Visibility.Collapsed,
            Focusable = false
        };

        public PNListBoxItem(ImageSource imageSource, string text, object tag, string key, bool isChecked)
        {
            Image = imageSource;
            _CheckBox.IsChecked = isChecked;
            _CheckBox.Checked += chk_Checked;
            _CheckBox.Unchecked += chk_Unchecked;
            _CheckBox.PreviewMouseLeftButtonDown += chk_PreviewMouseLeftButtonDown;
            _CheckBox.Visibility = Visibility.Visible;
            var img = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var tb = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_CheckBox);
            st.Children.Add(img);
            st.Children.Add(tb);
            Key = key;
            IsChecked = isChecked;
            Tag = tag;
            Text = text;
            Content = st;
        }

        public PNListBoxItem(ImageSource imageSource, string text, object tag, bool isChecked)
            : this(imageSource, text, tag, "", isChecked)
        {
        }

        public PNListBoxItem(ImageSource imageSource, string text, object tag, string key)
            : this(imageSource, text, tag)
        {
            Key = key;
        }

        public PNListBoxItem(ImageSource imageSource, string text, object tag)
        {
            Image = imageSource;
            var img = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var tb = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(img);
            st.Children.Add(tb);
            Tag = tag;
            Text = text;
            Content = st;
        }

        public int Index
        {
            get
            {
                var parent = ItemsControl.ItemsControlFromItemContainer(this);
                if (parent is ListBox listBox)
                {
                    return listBox.Items.IndexOf(this);
                }
                return -1;
            }
        }

        public ImageSource Image { get; }

        public string Key { get; }

        public bool? IsChecked
        {
            get => _CheckBox.IsChecked;
            set => _CheckBox.IsChecked = value;
        }

        public string Text { get; }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            IsChecked = false;
            ListBoxItemCheckChanged?.Invoke(this, new ListBoxItemCheckChangedEventArgs(false));
        }

        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            IsChecked = true;
            ListBoxItemCheckChanged?.Invoke(this, new ListBoxItemCheckChangedEventArgs(true));
        }

        private void chk_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = true;
            Focus();
        }
    }

    public class PNTreeItem : TreeViewItem
    {
        internal event EventHandler<TreeViewItemCheckChangedEventArgs> TreeViewItemCheckChanged;

        private readonly CheckBox _CheckBox = new CheckBox
        {
            Margin = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Visibility = Visibility.Collapsed,
            Focusable = false
        };

        private readonly Image _Image;
        private readonly TextBlock _TextBlock;

        public PNTreeItem(ImageSource imageSource, string text, object tag, string key, bool isChecked)
        {
            _CheckBox.IsChecked = isChecked;
            _CheckBox.Checked += chk_Checked;
            _CheckBox.Unchecked += chk_Unchecked;
            _CheckBox.PreviewMouseLeftButtonDown += _CheckBox_PreviewMouseLeftButtonDown;
            _CheckBox.Visibility = Visibility.Visible;
            _Image = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_CheckBox);
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Key = key;
            IsChecked = isChecked;
            Tag = tag;
            Header = st;
        }
        
        public PNTreeItem(ImageSource imageSource, string text, object tag, bool isChecked)
            : this(imageSource, text, tag, "", isChecked)
        {
        }

        public PNTreeItem(string imageResource, string text, object tag, bool isChecked)
            : this(imageResource, text, tag, "", isChecked)
        {
        }

        public PNTreeItem(string imageResource, string text, object tag, string key, bool isChecked)
        {
            _CheckBox.IsChecked = isChecked;
            _CheckBox.Checked += chk_Checked;
            _CheckBox.Unchecked += chk_Unchecked;
            _CheckBox.PreviewMouseLeftButtonDown += _CheckBox_PreviewMouseLeftButtonDown;
            _CheckBox.Visibility = Visibility.Visible;
            _Image = new Image
            {
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _Image.SetResourceReference(System.Windows.Controls.Image.SourceProperty, imageResource);
            PNUtils.SetIsResourceImage(this, true);
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_CheckBox);
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Key = key;
            IsChecked = isChecked;
            Tag = tag;
            Header = st;
        }

        public PNTreeItem(ImageSource imageSource, string text, object tag, string key)
            : this(imageSource, text, tag)
        {
            Key = key;
        }

        public PNTreeItem(ImageSource imageSource, string text, object tag)
        {
            _Image = new Image
            {
                Source = imageSource,
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Tag = tag;
            Header = st;
        }

        public PNTreeItem(string imageResource, string text, object tag, string key)
            : this(imageResource, text, tag)
        {
            Key = key;
        }

        public PNTreeItem(string imageResource, string text, object tag)
        {
            _Image = new Image
            {
                Stretch = Stretch.None,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _Image.SetResourceReference(System.Windows.Controls.Image.SourceProperty, imageResource);
            PNUtils.SetIsResourceImage(this, true);
            _TextBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var st = new StackPanel { Orientation = Orientation.Horizontal };
            st.Children.Add(_Image);
            st.Children.Add(_TextBlock);
            Tag = tag;
            Header = st;
        }

        public PNTreeItem Clone()
        {
            PNTreeItem pti;
            if (PNUtils.GetIsResourceImage(this))
            {
                var arr = _Image.Source.ToString().Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);
                pti = new PNTreeItem(Path.GetFileNameWithoutExtension(arr[arr.Length - 1]), Text, Tag, Key,
                    IsChecked.HasValue && IsChecked.Value)
                {
                    IsExpanded = IsExpanded,
                    IsEnabled = IsEnabled,
                    IsSelected = IsSelected
                };
                PNUtils.SetIsResourceImage(pti, true);
            }
            else
            {
                pti = new PNTreeItem(Image, Text, Tag, Key, IsChecked.HasValue && IsChecked.Value)
                {
                    IsExpanded = IsExpanded,
                    IsEnabled = IsEnabled,
                    IsSelected = IsSelected
                };
            }

            foreach (var p in Items.OfType<PNTreeItem>())
            {
                pti.Items.Add(p.Clone());
            }
            return pti;
        }

        public void SetImageResource(string key)
        {
            _Image.SetResourceReference(System.Windows.Controls.Image.SourceProperty, key);
            PNUtils.SetIsResourceImage(this, true);
        }

        public ImageSource Image
        {
            get => _Image.Source;
            set => _Image.Source = value;
        }

        public string Key { get; }

        public bool? IsChecked
        {
            get => _CheckBox.IsChecked;
            set => _CheckBox.IsChecked = value;
        }

        public string Text
        {
            get => _TextBlock.Text;
            set => _TextBlock.Text = value;
        }

        public int Index
        {
            get
            {
                var parent = ItemsControlFromItemContainer(this);
                if (parent is PNTreeItem item)
                {
                    return item.Items.IndexOf(this);
                }

                if (parent is TreeView tree)
                {
                    return tree.Items.IndexOf(this);
                }
                return -1;
            }
        }

        public PNTreeItem ItemParent
        {
            get
            {
                var parent = ItemsControlFromItemContainer(this);
                return parent as PNTreeItem;
            }
        }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            IsChecked = false;
            var parent = ItemsControlFromItemContainer(this);
            while (!(parent is TreeView))
            {
                parent = ItemsControlFromItemContainer(parent);
            }
            var parentTreeView = parent as TreeView;
            TreeViewItemCheckChanged?.Invoke(this, new TreeViewItemCheckChangedEventArgs(false, parentTreeView));
        }

        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            IsChecked = true;
            var parent = ItemsControlFromItemContainer(this);
            while (!(parent is TreeView))
            {
                parent = ItemsControlFromItemContainer(parent);
            }
            var parentTreeView = parent as TreeView;
            TreeViewItemCheckChanged?.Invoke(this, new TreeViewItemCheckChangedEventArgs(true, parentTreeView));
        }

        private void _CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = true;
            Focus();
        }
    }

    public class PNTreeView : TreeView
    {
        public event EventHandler<MouseButtonEventArgs> PNTreeViewLeftMouseDoubleClick;

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                PNTreeViewLeftMouseDoubleClick?.Invoke(this, e);
                e.Handled = true;
                return;
            }
            base.OnPreviewMouseDoubleClick(e);
        }
    }

    internal class ColumnProps
    {
        internal string Name;
        internal Visibility Visibility;
        internal double Width;
    }

    internal struct TempColor
    {
        internal string Id;
        internal string Color;
    }
    
    internal struct DateBitField : ICloneable
    {
        private int _Bits;

        internal DateBitField(int initValue)
        {
            _Bits = initValue;
        }

        internal bool this[DatePart index]
        {
            get => (_Bits & (1 << (int)index)) != 0;
            set
            {
                if (value)
                {
                    _Bits |= (1 << (int)index);
                }
                else
                {
                    _Bits &= ~(1 << (int)index);
                }
            }
        }

        #region ICloneable Members
        internal DateBitField Clone()
        {
            return (DateBitField)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return new DateBitField(_Bits);
        }
        #endregion
    }

    internal struct DayOfWeekStruct
    {
        internal DayOfWeek DayOfW;
        internal string Name;
    }

    internal class SettingsPanel
    {
        internal string Name;
        internal string Language;
    }

    // has to be public in order to use interop
    public class PinWindow
    {
        public string TextWnd { get; set; }
        public string ClassWnd { get; set; }
    }

    internal struct PinClass
    {
        internal string Class;
        internal string Pattern;
        internal IntPtr Hwnd;
    }

    internal class ReportBitField : ICloneable
    {
        internal ReportBitField()
        {
        }

        internal ReportBitField(int initValue)
        {
            Value = initValue;
        }

        internal bool this[ReportSetting index]
        {
            get => (Value & (1 << (int)index)) != 0;
            set
            {
                if (value)
                {
                    Value |= (1 << (int)index);
                }
                else
                {
                    Value &= ~(1 << (int)index);
                }
            }
        }

        internal int Value { get; private set; } = 0xfe;

        #region ICloneable members
        internal ReportBitField Clone()
        {
            return (ReportBitField)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return new ReportBitField(Value);
        }
        #endregion
    }

    internal class PNSendFailedException : Exception
    {
        internal PNSendFailedException(string message) : base(message)
        {
        }
    }
}
