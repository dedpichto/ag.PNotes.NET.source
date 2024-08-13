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
using System.Drawing;
using System.Linq;
using System.Windows;
using PNCommon;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using SystemColors = System.Windows.SystemColors;

namespace PNotes.NET
{

    public class PNSettings : IDisposable, ICloneable
    {
        internal const int DEF_WIDTH = 256;
        internal const int DEF_HEIGHT = 256;

        private PNGeneralSettings _GeneralSettings = new PNGeneralSettings();
        private PNSchedule _Schedule = new PNSchedule();
        private PNBehavior _Behavior = new PNBehavior();
        private PNProtection _Protection = new PNProtection();
        private PNDiary _Diary = new PNDiary();
        private PNNetwork _Network = new PNNetwork();
        private PNConfig _Config = new PNConfig();

        internal PNGeneralSettings GeneralSettings
        {
            get => _GeneralSettings;
            set => _GeneralSettings = value;
        }
        internal PNConfig Config
        {
            get => _Config;
            set => _Config = value;
        }
        internal PNNetwork Network
        {
            get => _Network;
            set => _Network = value;
        }
        internal PNDiary Diary
        {
            get => _Diary;
            set => _Diary = value;
        }
        internal PNProtection Protection
        {
            get => _Protection;
            set => _Protection = value;
        }
        internal PNBehavior Behavior
        {
            get => _Behavior;
            set => _Behavior = value;
        }
        internal PNSchedule Schedule
        {
            get => _Schedule;
            set => _Schedule = value;
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is PNSettings set))
                return false;
            return Equals(set);
        }

        public bool Equals(PNSettings set)
        {
            if ((object)set == null)
                return false;
            return _GeneralSettings == set._GeneralSettings
                   && _Schedule == set._Schedule
                   && _Behavior == set._Behavior
                   && _Protection == set._Protection
                   && _Diary == set._Diary
                   && _Network == set._Network;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += GeneralSettings.GetHashCode();
            result *= 37;
            result += Schedule.GetHashCode();
            result *= 37;
            result += Behavior.GetHashCode();
            result *= 37;
            result += Protection.GetHashCode();
            result *= 37;
            result += Diary.GetHashCode();
            result *= 37;
            result += Network.GetHashCode();
            return result;
        }

        public static bool operator ==(PNSettings a, PNSettings b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNSettings a, PNSettings b)
        {
            return !(a == b);
        }

        #region ICloneable members

        internal PNSettings Clone()
        {
            return (PNSettings)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            // without Config
            return new PNSettings
            {
                Behavior = Behavior.Clone(),
                Diary = Diary.Clone(),
                GeneralSettings = GeneralSettings.Clone(),
                Network = Network.Clone(),
                Protection = Protection.Clone(),
                Schedule = Schedule.Clone()
            };
        }
        #endregion
    }

    public class PNGeneralSettings : ICloneable
    {
        private bool _RunOnStart;
        private bool _ShowCPOnStart;
        private bool _CheckNewVersionOnStart;
        private string _Language = "english_us.xml";
        private bool _HideToolbar;
        private bool _UseCustomFonts;
        private System.Windows.Forms.RichTextBoxScrollBars _ShowScrollbar;
        private bool _HideDeleteButton;
        private bool _ChangeHideToDelete;
        private bool _HideHideButton;
        private short _BulletsIndent = 400;
        private short _MarginWidth = 4;
        private bool _SaveOnExit = true;
        private bool _ConfirmSaving = true;
        private bool _ConfirmBeforeDeletion = true;
        private bool _SaveWithoutConfirmOnHide;
        private bool _WarnOnAutomaticalDelete;
        private int _RemoveFromBinPeriod;
        private bool _Autosave;
        private int _AutosavePeriod = 5;
        private string _DateFormat = "dd MMM yyyy";
        private string _TimeFormat = "HH:mm";
        private int _Width = PNSettings.DEF_WIDTH;
        private int _Height = PNSettings.DEF_HEIGHT;
        private System.Drawing.Color _SpellColor = System.Drawing.Color.FromArgb(255, 255, 0, 0);
        private bool _UseSkins;
        private int _SpellMode;
        private string _SpellDict = "";
        private int _DockWidth = PNSettings.DEF_WIDTH;
        private int _DockHeight = PNSettings.DEF_HEIGHT;
        private DateBitField _DateBits = new DateBitField(0x1f);
        private DateBitField _TimeBits = new DateBitField(0x3);
        private bool _ShowPriorityOnStart;
        private ToolStripButtonSize _ButtonsSize = ToolStripButtonSize.Normal;
        private bool _AutomaticSmilies;
        private int _SpacePoints = 12;
        private bool _RestoreAuto;
        private int _ParagraphIndent = 400;
        private bool _CheckCriticalOnStart;
        private bool _CheckCriticalPeriodically;
        private bool _AutoHeight;
        private bool _DeleteShortcutsOnExit;
        private bool _RestoreShortcutsOnStart;
        private bool _CloseOnShortcut;

        internal bool CloseOnShortcut
        {
            get => _CloseOnShortcut;
            set => _CloseOnShortcut = value;
        }
        internal bool DeleteShortcutsOnExit
        {
            get => _DeleteShortcutsOnExit;
            set => _DeleteShortcutsOnExit = value;
        }
        internal bool RestoreShortcutsOnStart
        {
            get => _RestoreShortcutsOnStart;
            set => _RestoreShortcutsOnStart = value;
        }
        internal bool AutoHeight
        {
            get => _AutoHeight;
            set => _AutoHeight = value;
        }
        internal int ParagraphIndent
        {
            get => _ParagraphIndent;
            set => _ParagraphIndent = value;
        }
        internal bool RestoreAuto
        {
            get => _RestoreAuto;
            set => _RestoreAuto = value;
        }
        internal int SpacePoints
        {
            get => _SpacePoints;
            set => _SpacePoints = value;
        }
        internal bool AutomaticSmilies
        {
            get => _AutomaticSmilies;
            set => _AutomaticSmilies = value;
        }
        internal ToolStripButtonSize ButtonsSize
        {
            get => _ButtonsSize;
            set => _ButtonsSize = value;
        }
        internal bool ShowPriorityOnStart
        {
            get => _ShowPriorityOnStart;
            set => _ShowPriorityOnStart = value;
        }
        internal DateBitField DateBits => _DateBits;
        internal DateBitField TimeBits => _TimeBits;
        internal int DockHeight
        {
            get => _DockHeight;
            set => _DockHeight = value;
        }
        internal int DockWidth
        {
            get => _DockWidth;
            set => _DockWidth = value;
        }
        internal string SpellDict
        {
            get => _SpellDict;
            set => _SpellDict = value;
        }
        internal int SpellMode
        {
            get => _SpellMode;
            set => _SpellMode = value;
        }
        internal bool UseSkins
        {
            get => _UseSkins;
            set => _UseSkins = value;
        }
        internal System.Drawing.Color SpellColor
        {
            get => _SpellColor;
            set => _SpellColor = value;
        }
        internal int Height
        {
            get => _Height;
            set => _Height = value;
        }
        internal int Width
        {
            get => _Width;
            set => _Width = value;
        }
        internal string TimeFormat
        {
            get => _TimeFormat;
            set
            {
                _TimeFormat = value;
                _TimeBits[DatePart.Hour] = _TimeFormat.Contains("H");
                _TimeBits[DatePart.Minute] = _TimeFormat.Contains("m");
                _TimeBits[DatePart.Second] = _TimeFormat.Contains("s");
                _TimeBits[DatePart.Millisecond] = _TimeFormat.Contains("f");
            }
        }
        internal string DateFormat
        {
            get => _DateFormat;
            set
            {
                _DateFormat = value;
                _DateBits[DatePart.Year] = _DateFormat.Contains("y");
                _DateBits[DatePart.Month] = _DateFormat.Contains("M");
                _DateBits[DatePart.Day] = _DateFormat.Contains("d");
                _DateBits[DatePart.Hour] = _TimeFormat.Contains("H");
                _DateBits[DatePart.Minute] = _TimeFormat.Contains("m");
                _DateBits[DatePart.Second] = _TimeFormat.Contains("s");
                _DateBits[DatePart.Millisecond] = _TimeFormat.Contains("f");
            }
        }
        internal int AutosavePeriod
        {
            get => _AutosavePeriod;
            set => _AutosavePeriod = value;
        }
        internal bool Autosave
        {
            get => _Autosave;
            set => _Autosave = value;
        }
        internal int RemoveFromBinPeriod
        {
            get => _RemoveFromBinPeriod;
            set => _RemoveFromBinPeriod = value;
        }
        internal bool WarnOnAutomaticalDelete
        {
            get => _WarnOnAutomaticalDelete;
            set => _WarnOnAutomaticalDelete = value;
        }
        internal bool SaveWithoutConfirmOnHide
        {
            get => _SaveWithoutConfirmOnHide;
            set => _SaveWithoutConfirmOnHide = value;
        }
        internal bool ConfirmBeforeDeletion
        {
            get => _ConfirmBeforeDeletion;
            set => _ConfirmBeforeDeletion = value;
        }
        internal bool ConfirmSaving
        {
            get => _ConfirmSaving;
            set => _ConfirmSaving = value;
        }
        internal bool SaveOnExit
        {
            get => _SaveOnExit;
            set => _SaveOnExit = value;
        }
        internal short MarginWidth
        {
            get => _MarginWidth;
            set => _MarginWidth = value;
        }
        internal short BulletsIndent
        {
            get => _BulletsIndent;
            set => _BulletsIndent = value;
        }
        internal bool HideHideButton
        {
            get => _HideHideButton;
            set => _HideHideButton = value;
        }
        internal bool ChangeHideToDelete
        {
            get => _ChangeHideToDelete;
            set => _ChangeHideToDelete = value;
        }
        internal bool HideDeleteButton
        {
            get => _HideDeleteButton;
            set => _HideDeleteButton = value;
        }
        internal System.Windows.Forms.RichTextBoxScrollBars ShowScrollbar
        {
            get => _ShowScrollbar;
            set => _ShowScrollbar = value;
        }
        internal bool UseCustomFonts
        {
            get => _UseCustomFonts;
            set => _UseCustomFonts = value;
        }
        internal bool HideToolbar
        {
            get => _HideToolbar;
            set => _HideToolbar = value;
        }
        internal string Language
        {
            get => _Language;
            set => _Language = value;
        }
        internal bool CheckNewVersionOnStart
        {
            get => _CheckNewVersionOnStart;
            set => _CheckNewVersionOnStart = value;
        }
        internal bool ShowCPOnStart
        {
            get => _ShowCPOnStart;
            set => _ShowCPOnStart = value;
        }
        internal bool RunOnStart
        {
            get => _RunOnStart;
            set => _RunOnStart = value;
        }
        internal bool CheckCriticalOnStart
        {
            get => _CheckCriticalOnStart;
            set => _CheckCriticalOnStart = value;
        }
        internal bool CheckCriticalPeriodically
        {
            get => _CheckCriticalPeriodically;
            set => _CheckCriticalPeriodically = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNGeneralSettings gen))
                return false;
            return Equals(gen);
        }

        public bool Equals(PNGeneralSettings o)
        {
            if ((object)o == null)
                return false;
            return _CheckNewVersionOnStart == o._CheckNewVersionOnStart
                   && _Language == o._Language
                   && _RunOnStart == o._RunOnStart
                   && _ShowCPOnStart == o._ShowCPOnStart
                   && _BulletsIndent == o._BulletsIndent
                   && _ChangeHideToDelete == o._ChangeHideToDelete
                   && _HideDeleteButton == o._HideDeleteButton
                   && _HideHideButton == o._HideHideButton
                   && _HideToolbar == o._HideToolbar
                   && _MarginWidth == o._MarginWidth
                   && _UseCustomFonts == o._UseCustomFonts
                   && _ShowScrollbar == o._ShowScrollbar
                   && _Autosave == o._Autosave
                   && _AutosavePeriod == o._AutosavePeriod
                   && _ConfirmBeforeDeletion == o._ConfirmBeforeDeletion
                   && _ConfirmSaving == o._ConfirmSaving
                   && _RemoveFromBinPeriod == o._RemoveFromBinPeriod
                   && _SaveOnExit == o._SaveOnExit
                   && _SaveWithoutConfirmOnHide == o._SaveWithoutConfirmOnHide
                   && _WarnOnAutomaticalDelete == o._WarnOnAutomaticalDelete
                   && _DateFormat == o._DateFormat
                   && _TimeFormat == o._TimeFormat
                   && _Height == o._Height
                   && _Width == o._Width
                   && _SpellColor == o._SpellColor
                   && _UseSkins == o._UseSkins
                   && _SpellDict == o._SpellDict
                   && _SpellMode == o._SpellMode
                   && _DockWidth == o._DockWidth
                   && _DockHeight == o._DockHeight
                   && _ShowPriorityOnStart == o._ShowPriorityOnStart
                   && _ButtonsSize == o._ButtonsSize
                   && _AutomaticSmilies == o._AutomaticSmilies
                   && _SpacePoints == o._SpacePoints
                   && _RestoreAuto == o._RestoreAuto
                   && _ParagraphIndent == o._ParagraphIndent
                   && _AutoHeight == o._AutoHeight
                   && _CheckCriticalOnStart == o._CheckCriticalOnStart
                   && _CheckCriticalPeriodically == o._CheckCriticalPeriodically
                   && _DeleteShortcutsOnExit == o._DeleteShortcutsOnExit
                   && _RestoreShortcutsOnStart == o._RestoreShortcutsOnStart
                   && _CloseOnShortcut == o._CloseOnShortcut;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += ShowCPOnStart.GetHashCode();
            result *= 37;
            result += RunOnStart.GetHashCode();
            result *= 37;
            result += Language.GetHashCode();
            result *= 37;
            result += CheckNewVersionOnStart.GetHashCode();
            result *= 37;
            result += BulletsIndent.GetHashCode();
            result *= 37;
            result += ChangeHideToDelete.GetHashCode();
            result *= 37;
            result += HideDeleteButton.GetHashCode();
            result *= 37;
            result += HideHideButton.GetHashCode();
            result *= 37;
            result += HideToolbar.GetHashCode();
            result *= 37;
            result += MarginWidth.GetHashCode();
            result *= 37;
            result += ShowScrollbar.GetHashCode();
            result *= 37;
            result += UseCustomFonts.GetHashCode();
            result *= 37;
            result += WarnOnAutomaticalDelete.GetHashCode();
            result *= 37;
            result += SaveWithoutConfirmOnHide.GetHashCode();
            result *= 37;
            result += SaveOnExit.GetHashCode();
            result *= 37;
            result += RemoveFromBinPeriod.GetHashCode();
            result *= 37;
            result += ConfirmSaving.GetHashCode();
            result *= 37;
            result += ConfirmBeforeDeletion.GetHashCode();
            result *= 37;
            result += AutosavePeriod.GetHashCode();
            result *= 37;
            result += Autosave.GetHashCode();
            result *= 37;
            result += DateFormat.GetHashCode();
            result *= 37;
            result += TimeFormat.GetHashCode();
            result *= 37;
            result += Height.GetHashCode();
            result *= 37;
            result += Width.GetHashCode();
            result *= 37;
            result += SpellColor.GetHashCode();
            result *= 37;
            result += UseSkins.GetHashCode();
            result *= 37;
            result += SpellMode.GetHashCode();
            result *= 37;
            result += SpellDict.GetHashCode();
            result *= 37;
            result += DockWidth.GetHashCode();
            result *= 37;
            result += DockHeight.GetHashCode();
            result *= 37;
            result += ShowPriorityOnStart.GetHashCode();
            result *= 37;
            result += ButtonsSize.GetHashCode();
            result *= 37;
            result += AutomaticSmilies.GetHashCode();
            result *= 37;
            result += SpacePoints.GetHashCode();
            result *= 37;
            result += RestoreAuto.GetHashCode();
            result *= 37;
            result += ParagraphIndent.GetHashCode();
            result *= 37;
            result += AutoHeight.GetHashCode();
            result *= 37;
            result += CheckCriticalOnStart.GetHashCode();
            result *= 37;
            result += CheckCriticalPeriodically.GetHashCode();
            result *= 37;
            result += DeleteShortcutsOnExit.GetHashCode();
            result *= 37;
            result += RestoreShortcutsOnStart.GetHashCode();
            result *= 37;
            result += CloseOnShortcut.GetHashCode();

            return result;
        }

        public static bool operator ==(PNGeneralSettings a, PNGeneralSettings b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNGeneralSettings a, PNGeneralSettings b)
        {
            return !(a == b);
        }

        #region ICloneable Members

        internal PNGeneralSettings Clone()
        {
            return (PNGeneralSettings)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }

    public class PNSkinlessDetails : ICloneable
    {
        internal static readonly Color DefColor = Color.FromArgb(255, 242, 221, 116);

        private PNFont _CaptionFont = new PNFont { FontWeight = FontWeights.Bold };
        private Color _BackColor = DefColor;
        private Color _CaptionColor = SystemColors.ControlTextColor;

        public Color CaptionColor
        {
            get => _CaptionColor;
            set => _CaptionColor = value;
        }
        public Color BackColor
        {
            get => _BackColor;
            set => _BackColor = value;
        }
        public PNFont CaptionFont
        {
            get => _CaptionFont;
            set => _CaptionFont = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNSkinlessDetails sk))
                return false;
            return Equals(sk);
        }

        public bool Equals(PNSkinlessDetails sk)
        {
            if ((object)sk == null)
                return false;
            return _BackColor == sk._BackColor
                   && _CaptionFont == sk._CaptionFont
                   && _CaptionColor == sk._CaptionColor;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += BackColor.GetHashCode();
            result *= 37;
            result += CaptionFont.GetHashCode();
            result *= 37;
            result += CaptionColor.GetHashCode();
            return result;
        }

        public static bool operator ==(PNSkinlessDetails a, PNSkinlessDetails b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNSkinlessDetails a, PNSkinlessDetails b)
        {
            return !(a == b);
        }

        #region ICloneable Members
        public PNSkinlessDetails Clone()
        {
            return (PNSkinlessDetails)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return new PNSkinlessDetails
            {
                BackColor = _BackColor,
                CaptionColor = _CaptionColor,
                CaptionFont = CaptionFont.Clone()
            };
        }
        #endregion
    }

    public class PNSkinDetails : IDisposable, ICloneable
    {
        public const string NO_SKIN = "(no skin)";

        private string _SkinName = NO_SKIN;

        public Bitmap BitmapInactivePattern { get; set; }

        public Bitmap BitmapPattern { get; set; }

        public Rectangle PositionTooltip { get; set; }

        public Rectangle PositionEdit { get; set; }

        public Rectangle PositionToolbar { get; set; }

        public Rectangle PositionMarks { get; set; }

        public Rectangle PositionDelHide { get; set; }

        public System.Drawing.Color MaskColor { get; set; } = System.Drawing.Color.FromArgb(255, 255, 0, 255);

        public int MarksCount { get; set; } = 7;

        public Bitmap BitmapCommands { get; set; }

        public Bitmap BitmapDelHide { get; set; }

        public Bitmap BitmapMarks { get; set; }

        public Bitmap BitmapSkin { get; set; }

        public Bitmap BitmapForEdit { get; set; }

        public bool VerticalToolbar { get; set; }

        public string SkinInfo { get; set; } = "";

        public string SkinName
        {
            get => _SkinName;
            set => _SkinName = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNSkinDetails sk))
                return false;
            return Equals(sk);
        }

        public bool Equals(PNSkinDetails sk)
        {
            if ((object)sk == null)
                return false;
            return _SkinName == sk._SkinName;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += SkinName.GetHashCode();
            return result;
        }

        public static bool operator ==(PNSkinDetails a, PNSkinDetails b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNSkinDetails a, PNSkinDetails b)
        {
            return !(a == b);
        }

        #region IDisposable Members

        public void Dispose()
        {
            BitmapCommands?.Dispose();
            BitmapDelHide?.Dispose();
            BitmapSkin?.Dispose();
            BitmapForEdit?.Dispose();
            BitmapMarks?.Dispose();
            BitmapPattern?.Dispose();
            BitmapInactivePattern?.Dispose();
        }
        #endregion

        #region ICloneable Members
        public PNSkinDetails Clone()
        {
            return (PNSkinDetails)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return new PNSkinDetails
            {
                MarksCount = MarksCount,
                MaskColor = MaskColor,
                PositionDelHide = PositionDelHide,
                PositionEdit = PositionEdit,
                PositionMarks = PositionMarks,
                PositionToolbar = PositionToolbar,
                PositionTooltip = PositionTooltip,
                SkinInfo = SkinInfo,
                SkinName = SkinName,
                VerticalToolbar = VerticalToolbar,
                BitmapCommands = (Bitmap)BitmapCommands?.Clone(),
                BitmapDelHide = (Bitmap)BitmapDelHide?.Clone(),
                BitmapForEdit = (Bitmap)BitmapForEdit?.Clone(),
                BitmapInactivePattern = (Bitmap)BitmapInactivePattern?.Clone(),
                BitmapMarks = (Bitmap)BitmapMarks?.Clone(),
                BitmapPattern = (Bitmap)BitmapPattern?.Clone(),
                BitmapSkin = (Bitmap)BitmapSkin?.Clone()
            };
        }

        #endregion
    }

    public class PNSchedule : ICloneable
    {
        internal const string DEF_SOUND = "(Default)";

        private string _Sound = DEF_SOUND;
        private string _Voice = "";
        private bool _AllowSoundAlert = true;
        private bool _VisualNotification = true;
        private bool _TrackOverdue;
        private bool _CenterScreen = true;
        private int _VoiceVolume = 100;
        private int _VoiceSpeed;
        private int _VoicePitch;
        private FirstDayOfWeekType _FirstDayOfWeekType = FirstDayOfWeekType.Standard;
        private DayOfWeek _FirstDayOfWeek;
        private int _StopAfter;

        internal PNSchedule()
        {
            if (PNCollections.Instance.Voices.Count > 0)
                _Voice = PNCollections.Instance.Voices[0];
        }
        internal bool CenterScreen
        {
            get => _CenterScreen;
            set => _CenterScreen = value;
        }
        internal bool TrackOverdue
        {
            get => _TrackOverdue;
            set => _TrackOverdue = value;
        }
        internal bool VisualNotification
        {
            get => _VisualNotification;
            set => _VisualNotification = value;
        }
        internal bool AllowSoundAlert
        {
            get => _AllowSoundAlert;
            set => _AllowSoundAlert = value;
        }
        internal string Voice
        {
            get => _Voice;
            set => _Voice = value;
        }
        internal string Sound
        {
            get => _Sound;
            set => _Sound = value;
        }
        internal int VoiceVolume
        {
            get => _VoiceVolume;
            set => _VoiceVolume = value;
        }
        internal int VoiceSpeed
        {
            get => _VoiceSpeed;
            set => _VoiceSpeed = value;
        }
        internal int VoicePitch
        {
            get => _VoicePitch;
            set => _VoicePitch = value;
        }
        internal FirstDayOfWeekType FirstDayOfWeekType
        {
            get => _FirstDayOfWeekType;
            set => _FirstDayOfWeekType = value;
        }
        internal DayOfWeek FirstDayOfWeek
        {
            get => _FirstDayOfWeek;
            set => _FirstDayOfWeek = value;
        }
        internal int StopAfter
        {
            get { return _StopAfter; }
            set { _StopAfter = value; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNSchedule sch))
                return false;
            return Equals(sch);
        }

        public bool Equals(PNSchedule sch)
        {
            if ((object)sch == null)
                return false;
            return _Sound == sch.Sound
                   && _Voice == sch._Voice
                   && _AllowSoundAlert == sch._AllowSoundAlert
                   && _CenterScreen == sch._CenterScreen
                   && _TrackOverdue == sch._TrackOverdue
                   && _VisualNotification == sch._VisualNotification
                   && _VoicePitch == sch._VoicePitch
                   && _VoiceSpeed == sch._VoiceSpeed
                   && _VoiceVolume == sch._VoiceVolume
                   && _FirstDayOfWeek == sch._FirstDayOfWeek
                   && _FirstDayOfWeekType == sch._FirstDayOfWeekType
                   && _StopAfter == sch._StopAfter;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += Sound.GetHashCode();
            result *= 37;
            result += VisualNotification.GetHashCode();
            result *= 37;
            result += TrackOverdue.GetHashCode();
            result *= 37;
            result += CenterScreen.GetHashCode();
            result *= 37;
            result += AllowSoundAlert.GetHashCode();
            result *= 37;
            result += VoiceVolume.GetHashCode();
            result *= 37;
            result += VoiceSpeed.GetHashCode();
            result *= 37;
            result += VoicePitch.GetHashCode();
            result *= 37;
            result += FirstDayOfWeek.GetHashCode();
            result *= 37;
            result += FirstDayOfWeekType.GetHashCode();
            result *= 37;
            result += StopAfter.GetHashCode();

            return result;
        }

        public static bool operator ==(PNSchedule a, PNSchedule b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNSchedule a, PNSchedule b)
        {
            return !(a == b);
        }

        #region ICloneable Members

        internal PNSchedule Clone()
        {
            return (PNSchedule)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }

    public class PNDiary : ICloneable
    {
        private bool _CustomSettings;
        private bool _AddWeekday;
        private bool _FullWeekdayName;
        private bool _WeekdayAtTheEnd;
        private bool _DoNotShowPrevious;
        private bool _AscendingOrder;
        private int _NumberOfPages = 7;
        private string _DateFormat = "MMMM dd, yyyy";

        internal string DateFormat
        {
            get => _DateFormat;
            set => _DateFormat = value;
        }
        internal int NumberOfPages
        {
            get => _NumberOfPages;
            set => _NumberOfPages = value;
        }
        internal bool AscendingOrder
        {
            get => _AscendingOrder;
            set => _AscendingOrder = value;
        }
        internal bool DoNotShowPrevious
        {
            get => _DoNotShowPrevious;
            set => _DoNotShowPrevious = value;
        }
        internal bool WeekdayAtTheEnd
        {
            get => _WeekdayAtTheEnd;
            set => _WeekdayAtTheEnd = value;
        }
        internal bool FullWeekdayName
        {
            get => _FullWeekdayName;
            set => _FullWeekdayName = value;
        }
        internal bool AddWeekday
        {
            get => _AddWeekday;
            set => _AddWeekday = value;
        }
        internal bool CustomSettings
        {
            get => _CustomSettings;
            set => _CustomSettings = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNDiary dia))
                return false;
            return Equals(dia);
        }

        public bool Equals(PNDiary dia)
        {
            if ((object)dia == null)
                return false;
            return _AddWeekday == dia._AddWeekday
                   && _AscendingOrder == dia._AscendingOrder
                   && _CustomSettings == dia._CustomSettings
                   && _DateFormat == dia._DateFormat
                   && _DoNotShowPrevious == dia._DoNotShowPrevious
                   && _FullWeekdayName == dia._FullWeekdayName
                   && _NumberOfPages == dia._NumberOfPages
                   && _WeekdayAtTheEnd == dia._WeekdayAtTheEnd;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += WeekdayAtTheEnd.GetHashCode();
            result *= 37;
            result += NumberOfPages.GetHashCode();
            result *= 37;
            result += FullWeekdayName.GetHashCode();
            result *= 37;
            result += DoNotShowPrevious.GetHashCode();
            result *= 37;
            result += DateFormat.GetHashCode();
            result *= 37;
            result += CustomSettings.GetHashCode();
            result *= 37;
            result += AscendingOrder.GetHashCode();
            result *= 37;
            result += AddWeekday.GetHashCode();
            return result;
        }

        public static bool operator ==(PNDiary a, PNDiary b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNDiary a, PNDiary b)
        {
            return !(a == b);
        }

        #region ICloneable Members
        internal PNDiary Clone()
        {
            return (PNDiary)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }

    public class PNBehavior : ICloneable
    {
        private bool _NewNoteAlwaysOnTop;
        private bool _RelationalPositioning;
        private bool _HideCompleted;
        private bool _BigIconsOnCP;
        private bool _DoNotShowNotesInList;
        private bool _KeepVisibleOnShowDesktop;
        private TrayMouseAction _DoubleClickAction = TrayMouseAction.NewNote;
        private TrayMouseAction _SingleClickAction = TrayMouseAction.None;
        private DefaultNaming _DefaultNaming = DefaultNaming.FirstCharacters;
        private int _DefaultNameLength = 128;
        private int _ContentColumnLength = 24;
        private bool _HideFluently;
        private bool _PlaySoundOnHide;
        private double _Opacity = 1.0;
        private bool _RandomBackColor;
        private bool _InvertTextColor;
        private bool _RollOnDblClick;
        private bool _FitWhenRolled;
        private bool _ShowSeparateNotes;
        private PinClickAction _PinClickAction = PinClickAction.Toggle;
        private NoteStartPosition _StartPosition = NoteStartPosition.Center;
        private bool _HideMainWindow = true;
        private string _Theme = PNStrings.DEF_THEME;
        private bool _PreventAutomaticResizing = true;
        private bool _ShowNotesPanel;
        private NotesPanelOrientation _NotesPanelDock = NotesPanelOrientation.Top;
        private bool _PanelAutoHide;
        private PanelRemoveMode _PanelRemoveMode = PanelRemoveMode.SingleClick;
        private bool _PanelSwitchOffAnimation;
        private int _PanelEnterDelay;

        internal int PanelEnterDelay
        {
            get => _PanelEnterDelay;
            set => _PanelEnterDelay = value;
        }
        internal PanelRemoveMode PanelRemoveMode
        {
            get => _PanelRemoveMode;
            set => _PanelRemoveMode = value;
        }
        internal bool PanelAutoHide
        {
            get => _PanelAutoHide;
            set => _PanelAutoHide = value;
        }
        internal NotesPanelOrientation NotesPanelOrientation
        {
            get => _NotesPanelDock;
            set => _NotesPanelDock = value;
        }
        internal bool ShowNotesPanel
        {
            get => _ShowNotesPanel;
            set => _ShowNotesPanel = value;
        }
        internal bool PreventAutomaticResizing
        {
            get => _PreventAutomaticResizing;
            set => _PreventAutomaticResizing = value;
        }
        internal bool HideMainWindow
        {
            get => _HideMainWindow;
            set => _HideMainWindow = value;
        }
        internal NoteStartPosition StartPosition
        {
            get => _StartPosition;
            set => _StartPosition = value;
        }
        internal PinClickAction PinClickAction
        {
            get => _PinClickAction;
            set => _PinClickAction = value;
        }
        internal bool ShowSeparateNotes
        {
            get => _ShowSeparateNotes;
            set => _ShowSeparateNotes = value;
        }
        internal bool FitWhenRolled
        {
            get => _FitWhenRolled;
            set => _FitWhenRolled = value;
        }
        internal bool RollOnDblClick
        {
            get => _RollOnDblClick;
            set => _RollOnDblClick = value;
        }
        internal bool InvertTextColor
        {
            get => _InvertTextColor;
            set => _InvertTextColor = value;
        }
        internal bool RandomBackColor
        {
            get => _RandomBackColor;
            set => _RandomBackColor = value;
        }
        internal double Opacity
        {
            get => _Opacity;
            set => _Opacity = value;
        }
        internal bool PlaySoundOnHide
        {
            get => _PlaySoundOnHide;
            set => _PlaySoundOnHide = value;
        }
        internal bool HideFluently
        {
            get => _HideFluently;
            set => _HideFluently = value;
        }
        internal int ContentColumnLength
        {
            get => _ContentColumnLength;
            set => _ContentColumnLength = value;
        }
        internal int DefaultNameLength
        {
            get => _DefaultNameLength;
            set => _DefaultNameLength = value;
        }
        internal DefaultNaming DefaultNaming
        {
            get => _DefaultNaming;
            set => _DefaultNaming = value;
        }
        internal TrayMouseAction SingleClickAction
        {
            get => _SingleClickAction;
            set => _SingleClickAction = value;
        }
        internal TrayMouseAction DoubleClickAction
        {
            get => _DoubleClickAction;
            set => _DoubleClickAction = value;
        }
        internal bool KeepVisibleOnShowDesktop
        {
            get => _KeepVisibleOnShowDesktop;
            set => _KeepVisibleOnShowDesktop = value;
        }
        internal bool DoNotShowNotesInList
        {
            get => _DoNotShowNotesInList;
            set => _DoNotShowNotesInList = value;
        }
        internal bool BigIconsOnCP
        {
            get => _BigIconsOnCP;
            set => _BigIconsOnCP = value;
        }
        internal bool HideCompleted
        {
            get => _HideCompleted;
            set => _HideCompleted = value;
        }
        internal bool RelationalPositioning
        {
            get => _RelationalPositioning;
            set => _RelationalPositioning = value;
        }
        internal bool NewNoteAlwaysOnTop
        {
            get => _NewNoteAlwaysOnTop;
            set => _NewNoteAlwaysOnTop = value;
        }
        internal string Theme
        {
            get => _Theme;
            set => _Theme = value;
        }
        internal bool PanelSwitchOffAnimation
        {
            get => _PanelSwitchOffAnimation;
            set => _PanelSwitchOffAnimation = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNBehavior bh))
                return false;
            return Equals(bh);
        }

        public bool Equals(PNBehavior bh)
        {
            if ((object)bh == null)
                return false;
            return _FitWhenRolled == bh._FitWhenRolled
                   && _InvertTextColor == bh._InvertTextColor
                   && _RandomBackColor == bh._RandomBackColor
                   && _RollOnDblClick == bh._RollOnDblClick
                   && _BigIconsOnCP == bh._BigIconsOnCP
                   && _ContentColumnLength == bh._ContentColumnLength
                   && _DefaultNameLength == bh._DefaultNameLength
                   && _DefaultNaming == bh._DefaultNaming
                   && _DoNotShowNotesInList == bh._DoNotShowNotesInList
                   && _DoubleClickAction == bh._DoubleClickAction
                   && _HideCompleted == bh._HideCompleted
                   && _KeepVisibleOnShowDesktop == bh._KeepVisibleOnShowDesktop
                   && _NewNoteAlwaysOnTop == bh._NewNoteAlwaysOnTop
                   && _RelationalPositioning == bh._RelationalPositioning
                   && _SingleClickAction == bh._SingleClickAction
                   && _HideFluently == bh._HideFluently
                   && _PlaySoundOnHide == bh._PlaySoundOnHide
                   && Math.Abs(_Opacity - bh._Opacity) < double.Epsilon
                   && _ShowSeparateNotes == bh._ShowSeparateNotes
                   && _PinClickAction == bh._PinClickAction
                   && _StartPosition == bh._StartPosition
                   && _HideMainWindow == bh._HideMainWindow
                   && _Theme == bh._Theme
                   && _PreventAutomaticResizing == bh._PreventAutomaticResizing
                   && _ShowNotesPanel == bh._ShowNotesPanel
                   && _NotesPanelDock == bh._NotesPanelDock
                   && _PanelAutoHide == bh._PanelAutoHide
                   && _PanelRemoveMode == bh._PanelRemoveMode
                   && _PanelSwitchOffAnimation == bh._PanelSwitchOffAnimation
                   && _PanelEnterDelay == bh._PanelEnterDelay;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += FitWhenRolled.GetHashCode();
            result *= 37;
            result += InvertTextColor.GetHashCode();
            result *= 37;
            result += RandomBackColor.GetHashCode();
            result *= 37;
            result += RollOnDblClick.GetHashCode();
            result *= 37;
            result += SingleClickAction.GetHashCode();
            result *= 37;
            result += RelationalPositioning.GetHashCode();
            result *= 37;
            result += NewNoteAlwaysOnTop.GetHashCode();
            result *= 37;
            result += KeepVisibleOnShowDesktop.GetHashCode();
            result *= 37;
            result += HideCompleted.GetHashCode();
            result *= 37;
            result += DoubleClickAction.GetHashCode();
            result *= 37;
            result += DoNotShowNotesInList.GetHashCode();
            result *= 37;
            result += DefaultNaming.GetHashCode();
            result *= 37;
            result += DefaultNameLength.GetHashCode();
            result *= 37;
            result += ContentColumnLength.GetHashCode();
            result *= 37;
            result += BigIconsOnCP.GetHashCode();
            result *= 37;
            result += PlaySoundOnHide.GetHashCode();
            result *= 37;
            result += HideFluently.GetHashCode();
            result *= 37;
            result += Opacity.GetHashCode();
            result *= 37;
            result += ShowSeparateNotes.GetHashCode();
            result *= 37;
            result += PinClickAction.GetHashCode();
            result *= 37;
            result += StartPosition.GetHashCode();
            result *= 37;
            result += HideMainWindow.GetHashCode();
            result *= 37;
            result += Theme.GetHashCode();
            result *= 37;
            result += PreventAutomaticResizing.GetHashCode();
            result *= 37;
            result += ShowNotesPanel.GetHashCode();
            result *= 37;
            result += NotesPanelOrientation.GetHashCode();
            result *= 37;
            result += PanelAutoHide.GetHashCode();
            result *= 37;
            result += PanelRemoveMode.GetHashCode();
            result *= 37;
            result += PanelSwitchOffAnimation.GetHashCode();
            result *= 37;
            result += PanelEnterDelay.GetHashCode();

            return result;
        }

        public static bool operator ==(PNBehavior a, PNBehavior b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNBehavior a, PNBehavior b)
        {
            return !(a == b);
        }

        #region ICloneable Members

        internal PNBehavior Clone()
        {
            return (PNBehavior)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }

    public class PNProtection : ICloneable
    {
        private bool _StoreAsEncrypted;
        private bool _HideTrayIcon;
        private bool _BackupBeforeSaving;
        private bool _SilentFullBackup;
        private int _BackupDeepness = 3;
        private bool _DontShowContent;
        private bool _IncludeBinInSync;
        private string _PasswordString = "";
        private List<DayOfWeek> _FullBackupDays = new List<DayOfWeek>();
        private DateTime _FullBackupTime = DateTime.MinValue;
        private DateTime _FullBackupDate = DateTime.MinValue;
        private bool _PromptForPassword;
        private string _LocalSyncFilesLocation = "";
        private int _LocalSyncPeriod = -1;
        private TimeSpan _LocalSyncTime = TimeSpan.Parse("00:00");
        private SilentSyncType _LocalSyncType = SilentSyncType.Period;
        private bool _AutomaticSyncRestart;

        internal bool PromptForPassword
        {
            get => _PromptForPassword;
            set => _PromptForPassword = value;
        }
        internal DateTime FullBackupDate
        {
            get => _FullBackupDate;
            set => _FullBackupDate = value;
        }
        internal DateTime FullBackupTime
        {
            get => _FullBackupTime;
            set => _FullBackupTime = value;
        }
        internal List<DayOfWeek> FullBackupDays
        {
            get => _FullBackupDays;
            set => _FullBackupDays = value;
        }
        internal string PasswordString
        {
            get => _PasswordString;
            set => _PasswordString = value;
        }
        internal bool IncludeBinInSync
        {
            get => _IncludeBinInSync;
            set => _IncludeBinInSync = value;
        }
        internal bool DontShowContent
        {
            get => _DontShowContent;
            set => _DontShowContent = value;
        }
        internal int BackupDeepness
        {
            get => _BackupDeepness;
            set => _BackupDeepness = value;
        }
        internal bool SilentFullBackup
        {
            get => _SilentFullBackup;
            set => _SilentFullBackup = value;
        }
        internal bool BackupBeforeSaving
        {
            get => _BackupBeforeSaving;
            set => _BackupBeforeSaving = value;
        }
        internal bool HideTrayIcon
        {
            get => _HideTrayIcon;
            set => _HideTrayIcon = value;
        }
        internal bool StoreAsEncrypted
        {
            get => _StoreAsEncrypted;
            set => _StoreAsEncrypted = value;
        }
        internal string LocalSyncFilesLocation
        {
            get => _LocalSyncFilesLocation;
            set => _LocalSyncFilesLocation = value;
        }
        internal int LocalSyncPeriod
        {
            get => _LocalSyncPeriod;
            set => _LocalSyncPeriod = value;
        }
        internal TimeSpan LocalSyncTime
        {
            get => _LocalSyncTime;
            set => _LocalSyncTime = value;
        }
        internal SilentSyncType LocalSyncType
        {
            get => _LocalSyncType;
            set => _LocalSyncType = value;
        }
        internal bool AutomaticSyncRestart
        {
            get => _AutomaticSyncRestart;
            set => _AutomaticSyncRestart = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNProtection pr))
                return false;
            return Equals(pr);
        }

        public bool Equals(PNProtection pr)
        {
            if ((object)pr == null)
                return false;
            return _BackupBeforeSaving == pr._BackupBeforeSaving
                   && _BackupDeepness == pr._BackupDeepness
                   && _DontShowContent == pr._DontShowContent
                   && _HideTrayIcon == pr._HideTrayIcon
                   && _IncludeBinInSync == pr._IncludeBinInSync
                   && _SilentFullBackup == pr._SilentFullBackup
                   && _StoreAsEncrypted == pr._StoreAsEncrypted
                   && _PasswordString == pr._PasswordString
                   && !_FullBackupDays.Inequals(pr._FullBackupDays)
                   && _FullBackupTime == pr._FullBackupTime
                   && _FullBackupDate == pr._FullBackupDate
                   && _PromptForPassword == pr._PromptForPassword
                   && _LocalSyncFilesLocation == pr._LocalSyncFilesLocation
                   && _LocalSyncPeriod == pr._LocalSyncPeriod
                   && _LocalSyncTime == pr._LocalSyncTime
                   && _LocalSyncType == pr._LocalSyncType
                   && _AutomaticSyncRestart == pr._AutomaticSyncRestart;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += StoreAsEncrypted.GetHashCode();
            result *= 37;
            result += SilentFullBackup.GetHashCode();
            result *= 37;
            result += IncludeBinInSync.GetHashCode();
            result *= 37;
            result += HideTrayIcon.GetHashCode();
            result *= 37;
            result += DontShowContent.GetHashCode();
            result *= 37;
            result += BackupDeepness.GetHashCode();
            result *= 37;
            result += BackupBeforeSaving.GetHashCode();
            result *= 37;
            result += PasswordString.GetHashCode();
            result *= 37;
            result += FullBackupDate.GetHashCode();
            result *= 37;
            result += FullBackupDays.GetHashCode();
            result *= 37;
            result += FullBackupTime.GetHashCode();
            result *= 37;
            result += PromptForPassword.GetHashCode();
            result *= 37;
            result += LocalSyncFilesLocation.GetHashCode();
            result *= 37;
            result += LocalSyncPeriod.GetHashCode();
            result *= 37;
            result += LocalSyncTime.GetHashCode();
            result *= 37;
            result += LocalSyncType.GetHashCode();
            result *= 37;
            result += AutomaticSyncRestart.GetHashCode();

            return result;
        }


        public static bool operator ==(PNProtection a, PNProtection b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNProtection a, PNProtection b)
        {
            return !(a == b);
        }

        #region ICloneable members

        internal PNProtection Clone()
        {
            return (PNProtection)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            var pr = (PNProtection)MemberwiseClone();
            pr.FullBackupDays = new List<DayOfWeek>(FullBackupDays.Select(fd => fd));

            return pr;
        }
        #endregion
    }

    public class PNConfig : ICloneable
    {
        internal string ReportDates { get; set; } = "";
        internal bool CPPvwRight { get; set; }
        internal string ControlsStyle { get; set; } = "";
        internal Point CPLocation { get; set; } = default(Point);
        internal Size CPSize { get; set; } = Size.Empty;
        internal bool CPUseCustPvwColor { get; set; }
        internal Color CPPvwColor { get; set; } = Color.FromArgb(255, 242, 221, 116);
        internal bool Skinnable { get; set; }
        internal int CPLastGroup { get; set; } = -1;
        internal int ExitFlag { get; set; }
        internal int LastPage { get; set; }
        internal bool CPPvwShow { get; set; } = true;
        internal bool CPGroupsShow { get; set; } = true;
        internal SearchNotesPrefs SearchNotesSettings { get; set; } = new SearchNotesPrefs();
        internal ReportBitField ReportSettings { get; set; } = new ReportBitField(0);
        internal bool CPIgnoreCase { get; set; } = true;
        internal DateTime LastLocalSync { get; set; }
        internal DateTime LastPluginSync { get; set; }

        #region ICloneable members
        internal PNConfig Clone()
        {
            return (PNConfig)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            var config = (PNConfig)MemberwiseClone();
            config.ReportSettings = ReportSettings.Clone();
            config.SearchNotesSettings = SearchNotesSettings.Clone();
            return config;
        }
        #endregion
    }

    public class PNNetwork : ICloneable
    {
        internal const int PORT_EXCHANGE = 27951;

        private bool _IncludeBinInSync;
        private bool _SyncOnStart;
        private bool _SaveBeforeSync;
        private bool _EnableExchange;
        private bool _SaveBeforeSending;
        private bool _NoNotificationOnArrive;
        private bool _ShowReceivedOnClick;
        private bool _ShowIncomingOnClick;
        private bool _NoSoundOnArrive;
        private bool _NoNotificationOnSend;
        private bool _ShowAfterArrive;
        private bool _HideAfterSending;
        private bool _NoContactsInContextMenu;
        private int _ExchangePort = PORT_EXCHANGE;
        private int _PostCount = 20;
        private bool _AllowPing = true;
        private bool _ReceivedOnTop = true;
        private bool _StoreOnServer;
        private string _ServerIp;
        private int _ServerPort = PNServerConstants.DEF_PORT;
        private int _SendTimeout = PNServerConstants.SEND_TIMEOUT;
        private int _SilentSyncPeriod = -1;
        private TimeSpan _SilentSyncTime = TimeSpan.Parse("00:00");
        private SilentSyncType _SilentSyncType = SilentSyncType.Period;
        private bool _AutomaticSyncRestart;

        internal int PostCount
        {
            get => _PostCount;
            set => _PostCount = value;
        }
        internal int ExchangePort
        {
            get => _ExchangePort;
            set => _ExchangePort = value;
        }
        internal bool NoContactsInContextMenu
        {
            get => _NoContactsInContextMenu;
            set => _NoContactsInContextMenu = value;
        }
        internal bool HideAfterSending
        {
            get => _HideAfterSending;
            set => _HideAfterSending = value;
        }
        internal bool ShowAfterArrive
        {
            get => _ShowAfterArrive;
            set => _ShowAfterArrive = value;
        }
        internal bool NoNotificationOnSend
        {
            get => _NoNotificationOnSend;
            set => _NoNotificationOnSend = value;
        }
        internal bool NoSoundOnArrive
        {
            get => _NoSoundOnArrive;
            set => _NoSoundOnArrive = value;
        }
        internal bool ShowIncomingOnClick
        {
            get => _ShowIncomingOnClick;
            set => _ShowIncomingOnClick = value;
        }
        internal bool ShowReceivedOnClick
        {
            get => _ShowReceivedOnClick;
            set => _ShowReceivedOnClick = value;
        }
        internal bool NoNotificationOnArrive
        {
            get => _NoNotificationOnArrive;
            set => _NoNotificationOnArrive = value;
        }
        internal bool SaveBeforeSending
        {
            get => _SaveBeforeSending;
            set => _SaveBeforeSending = value;
        }
        internal bool EnableExchange
        {
            get => _EnableExchange;
            set => _EnableExchange = value;
        }
        internal bool SaveBeforeSync
        {
            get => _SaveBeforeSync;
            set => _SaveBeforeSync = value;
        }
        internal bool SyncOnStart
        {
            get => _SyncOnStart;
            set => _SyncOnStart = value;
        }
        internal bool IncludeBinInSync
        {
            get => _IncludeBinInSync;
            set => _IncludeBinInSync = value;
        }
        internal bool AllowPing
        {
            get => _AllowPing;
            set => _AllowPing = value;
        }
        internal bool ReceivedOnTop
        {
            get => _ReceivedOnTop;
            set => _ReceivedOnTop = value;
        }
        internal bool StoreOnServer
        {
            get => _StoreOnServer;
            set => _StoreOnServer = value;
        }
        internal string ServerIp
        {
            get => _ServerIp;
            set => _ServerIp = value;
        }
        internal int ServerPort
        {
            get => _ServerPort;
            set => _ServerPort = value;
        }
        internal int SendTimeout
        {
            get => _SendTimeout;
            set => _SendTimeout = value;
        }
        internal int SilentSyncPeriod
        {
            get => _SilentSyncPeriod;
            set => _SilentSyncPeriod = value;
        }
        internal TimeSpan SilentSyncTime
        {
            get => _SilentSyncTime;
            set => _SilentSyncTime = value;
        }
        internal SilentSyncType SilentSyncType
        {
            get => _SilentSyncType;
            set => _SilentSyncType = value;
        }
        internal bool AutomaticSyncRestart
        {
            get => _AutomaticSyncRestart;
            set => _AutomaticSyncRestart = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PNNetwork nt))
                return false;
            return Equals(nt);
        }

        public bool Equals(PNNetwork nt)
        {
            if ((object)nt == null)
                return false;
            return _EnableExchange == nt._EnableExchange
                   && _ExchangePort == nt._ExchangePort
                   && _HideAfterSending == nt._HideAfterSending
                   && _IncludeBinInSync == nt._IncludeBinInSync
                   && _NoContactsInContextMenu == nt._NoContactsInContextMenu
                   && _NoNotificationOnArrive == nt._NoNotificationOnArrive
                   && _NoNotificationOnSend == nt._NoNotificationOnSend
                   && _NoSoundOnArrive == nt._NoSoundOnArrive
                   && _SaveBeforeSending == nt._SaveBeforeSending
                   && _SaveBeforeSync == nt._SaveBeforeSync
                   && _ShowAfterArrive == nt._ShowAfterArrive
                   && _ShowIncomingOnClick == nt._ShowIncomingOnClick
                   && _ShowReceivedOnClick == nt._ShowReceivedOnClick
                   && _SyncOnStart == nt._SyncOnStart
                   && _PostCount == nt._PostCount
                   && _AllowPing == nt._AllowPing
                   && _ReceivedOnTop == nt._ReceivedOnTop
                   && _StoreOnServer == nt._StoreOnServer
                   && _ServerIp == nt._ServerIp
                   && _ServerPort == nt._ServerPort
                   && _SendTimeout == nt._SendTimeout
                   && _SilentSyncPeriod == nt._SilentSyncPeriod
                   && _SilentSyncTime == nt._SilentSyncTime
                   && _SilentSyncType == nt._SilentSyncType
                   && _AutomaticSyncRestart == nt._AutomaticSyncRestart;
        }

        public override int GetHashCode()
        {
            var result = 17;
            result *= 37;
            result += SyncOnStart.GetHashCode();
            result *= 37;
            result += ShowReceivedOnClick.GetHashCode();
            result *= 37;
            result += ShowIncomingOnClick.GetHashCode();
            result *= 37;
            result += ShowAfterArrive.GetHashCode();
            result *= 37;
            result += SaveBeforeSync.GetHashCode();
            result *= 37;
            result += SaveBeforeSending.GetHashCode();
            result *= 37;
            result += NoSoundOnArrive.GetHashCode();
            result *= 37;
            result += NoNotificationOnSend.GetHashCode();
            result *= 37;
            result += NoNotificationOnArrive.GetHashCode();
            result *= 37;
            result += NoContactsInContextMenu.GetHashCode();
            result *= 37;
            result += IncludeBinInSync.GetHashCode();
            result *= 37;
            result += HideAfterSending.GetHashCode();
            result *= 37;
            result += ExchangePort.GetHashCode();
            result *= 37;
            result += EnableExchange.GetHashCode();
            result *= 37;
            result += PostCount.GetHashCode();
            result *= 37;
            result += AllowPing.GetHashCode();
            result *= 37;
            result += ReceivedOnTop.GetHashCode();
            result *= 37;
            result += StoreOnServer.GetHashCode();
            result *= 37;
            result += ServerIp.GetHashCode();
            result *= 37;
            result += ServerPort.GetHashCode();
            result *= 37;
            result += SendTimeout.GetHashCode();
            result *= 37;
            result += SilentSyncPeriod.GetHashCode();
            result *= 37;
            result += SilentSyncTime.GetHashCode();
            result *= 37;
            result += SilentSyncType.GetHashCode();
            result *= 37;
            result += AutomaticSyncRestart.GetHashCode();

            return result;
        }

        public static bool operator ==(PNNetwork a, PNNetwork b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(PNNetwork a, PNNetwork b)
        {
            return !(a == b);
        }

        #region ICloneable members
        internal PNNetwork Clone()
        {
            return (PNNetwork)((ICloneable)this).Clone();
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
