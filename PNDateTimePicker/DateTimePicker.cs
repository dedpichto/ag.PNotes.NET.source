using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Calendar = System.Windows.Controls.Calendar;

namespace PNDateTimePicker
{
    /// <summary>
    /// Specifies the current status of DateTimePicker
    /// </summary>
    public enum DateBoxStatus
    {
        /// <summary>
        /// Status normal
        /// </summary>
        Normal,
        /// <summary>
        /// Status error
        /// </summary>
        Error,
        /// <summary>
        /// Status significant date
        /// </summary>
        Significant,
        /// <summary>
        /// Status day-off
        /// </summary>
        DayOff
    }
    /// <summary>
    /// Specifies the date selection mode of calendar
    /// </summary>
    public enum DateBoxSelectionMode
    {
        /// <summary>
        /// Simple mode
        /// </summary>
        SimpleSelection,
        /// <summary>
        /// Single click mode
        /// </summary>
        SingleClick,
        /// <summary>
        /// Double click mode
        /// </summary>
        DoubleClick
    }
    /// <summary>
    /// Specifies the date/time format of DateTimePicker
    /// </summary>
    public enum DateBoxFormat
    {
        /// <summary>
        /// Short date
        /// </summary>
        ShortDate,
        /// <summary>
        /// Short date and short time
        /// </summary>
        ShortDateAndShortTime,
        /// <summary>
        /// Short date and long time
        /// </summary>
        ShortDateAndLongTime,
        /// <summary>
        /// Long date
        /// </summary>
        LongDate,
        /// <summary>
        /// Long date and short time
        /// </summary>
        LongDateAndShortTime,
        /// <summary>
        /// Long date and long time
        /// </summary>
        LongDateAndLongTime,
        /// <summary>
        /// Short time
        /// </summary>
        ShortTime,
        /// <summary>
        /// Long time
        /// </summary>
        LongTime
    }

    /// <summary>
    /// Represents the custom control that allows the user to select a date and/or a time
    /// </summary>
    #region Named parts
    [TemplatePart(Name = ElementBorder, Type = typeof(Border))]
    [TemplatePart(Name = ElementPanel, Type = typeof(StackPanel))]
    [TemplatePart(Name = ElementButton, Type = typeof(Button))]
    [TemplatePart(Name = ElementPopup, Type = typeof(Popup))]
    [TemplatePart(Name = ElementCalendar, Type = typeof(Calendar))]
    [TemplatePart(Name = ElementInfoPopup, Type = typeof(Popup))]
    [TemplatePart(Name = ElementInfoGrid, Type = typeof(Grid))]
    [TemplatePart(Name = ElementInfoBorder, Type = typeof(Border))]
    [TemplatePart(Name = ElementNowButton, Type = typeof(Button))]
    #endregion

    public class DateTimePicker : Control
    {
        #region Constants
        private const string ElementBorder = "PART_Border";
        private const string ElementPanel = "PART_Panel";
        private const string ElementButton = "PART_Button";
        private const string ElementPopup = "PART_Popup";
        private const string ElementCalendar = "PART_Calendar";
        private const string ElementInfoPopup = "PART_InfoPopup";
        private const string ElementInfoGrid = "PART_InfoGrid";
        private const string ElementInfoBorder = "PART_InfoBorder";
        private const string ElementNowButton = "PART_NowButton";
        #endregion

        #region Elements
        private Border _Border;
        private StackPanel _Panel;
        private Button _Button;
        private Popup _Popup;
        private Calendar _Calendar;
        private Grid _InfoGrid;
        private Border _InfoBorder;
        private Button _NowButton;
        #endregion

        #region Dependency properties
        /// <summary>
        /// The identifier of the <see cref="DateValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DateValueProperty;
        /// <summary>
        /// The identifier of the <see cref="Status"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusProperty;
        /// <summary>
        /// The identifier of the <see cref="ErrorBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorBackgroundProperty;
        /// <summary>
        /// The identifier of the <see cref="DayOffBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DayOffBackgroundProperty;
        /// <summary>
        /// The identifier of the <see cref="SignificantDateBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SignificantDateBackgroundProperty;
        /// <summary>
        /// The identifier of the <see cref="SelectionMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty;
        /// <summary>
        /// The identifier of the <see cref="Format"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FormatProperty;
        /// <summary>
        /// The identifier of the <see cref="AllowErrorValues"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowErrorValuesProperty;
        /// <summary>
        /// The identifier of the <see cref="InfoPanelOrientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InfoPanelOrientationProperty;
        /// <summary>
        /// The identifier of the <see cref="SignificantDateForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SignificantDateForegroundProperty;
        /// <summary>
        /// The identifier of the <see cref="DayOffForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DayOffForegroundProperty;
        /// <summary>
        /// The identifier of the <see cref="IsBlackoutDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBlackoutDateProperty;
        /// <summary>
        /// The identifier of the <see cref="IsReadOnly"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty;
        /// <summary>
        /// The identifier of the <see cref="IsEmpty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEmptyProperty;
        /// <summary>
        /// The identifier of the <see cref="IsNowButtonVisible"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsNowButtonVisibleProperty;
        #endregion

        #region Private enums
        private enum BoxType
        {
            Day,
            Month,
            Year,
            Hour,
            Minute,
            Second,
            AmPm,
            Separator
        }
        #endregion

        #region Classes
        private class BoxData
        {
            internal BoxType Type;
            internal string FormatString;
            internal int Index = -1;
        }

        private class DayOff
        {
            internal DayOfWeek Day;
            internal bool Checked;
        }

        #endregion

        private TextBox _CurrentTextBox;
        private bool _AllowPopup = true;
        private Key _KeyCurrent = Key.None;
        private readonly SignificantDatesCollection _SignificantDates = new SignificantDatesCollection();
        private readonly Calendar _Blacks = new Calendar();
        private DayOfWeek _FirstDayOfWeek = Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

        private readonly DayOff[] _DaysOff =
        {
            new DayOff {Day = DayOfWeek.Sunday, Checked = false},
            new DayOff {Day = DayOfWeek.Monday, Checked = false},
            new DayOff {Day = DayOfWeek.Tuesday, Checked = false},
            new DayOff {Day = DayOfWeek.Wednesday, Checked = false},
            new DayOff {Day = DayOfWeek.Thursday, Checked = false},
            new DayOff {Day = DayOfWeek.Friday, Checked = false},
            new DayOff {Day = DayOfWeek.Saturday, Checked = false}
        };

        static DateTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker), new FrameworkPropertyMetadata(typeof(DateTimePicker)));
            DateValueProperty = DependencyProperty.Register("DateValue", typeof(DateTime?), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(DateTime.Now, OnDateValueChanged, CoerceDateValue));
            ErrorBackgroundProperty = DependencyProperty.Register("ErrorBackground", typeof(SolidColorBrush), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(Brushes.Red, OnErrorBackgroundChanged));
            DayOffBackgroundProperty = DependencyProperty.Register("DayOffBackground", typeof(SolidColorBrush), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(SystemColors.WindowBrush, OnDayOffBackgroundChanged));
            SignificantDateBackgroundProperty = DependencyProperty.Register("SignificantDateBackground", typeof(SolidColorBrush), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(SystemColors.WindowBrush, OnSignificantDateBackgroundChanged));
            StatusProperty = DependencyProperty.Register("Status", typeof(DateBoxStatus), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(DateBoxStatus.Normal, OnStatusChanged));
            SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(DateBoxSelectionMode),
                typeof(DateTimePicker),
                new FrameworkPropertyMetadata(DateBoxSelectionMode.DoubleClick, OnSelectionModeChanged));
            FormatProperty = DependencyProperty.Register("Format", typeof(DateBoxFormat), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(DateBoxFormat.ShortDate, OnFormatChanged));
            AllowErrorValuesProperty = DependencyProperty.Register("AllowErrorValues", typeof(bool), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(false, OnAllowErrorValuesChanged));
            InfoPanelOrientationProperty = DependencyProperty.Register("InfoPanelOrientation", typeof(Orientation),
                typeof(DateTimePicker), new FrameworkPropertyMetadata(Orientation.Vertical, OnInfoPanelOrientationChanged));
            DayOffForegroundProperty = DependencyProperty.Register("DayOffForeground", typeof(SolidColorBrush), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, OnDayOffForegroundChanged));
            SignificantDateForegroundProperty = DependencyProperty.Register("SignificantDateForeground", typeof(SolidColorBrush), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, OnSignificantDateForegroundChanged));
            IsBlackoutDateProperty = DependencyProperty.Register("IsBlackoutDate", typeof(bool), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(false, OnIsBlackoutDateChanged));
            IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));
            IsEmptyProperty = DependencyProperty.Register("IsEmpty", typeof(bool), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(false, OnIsEmptyChanged));
            IsNowButtonVisibleProperty = DependencyProperty.Register("IsNowButtonVisible", typeof(bool),
                typeof (DateTimePicker), new FrameworkPropertyMetadata(false, OnIsNowButtonVisibleChanged));
        }

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _SignificantDates.CollectionChanged += _SignificantDates_CollectionChanged;

            //add handler for FlowDirection changes in order to reverse text boxes
            var dp = DependencyPropertyDescriptor.FromProperty(FlowDirectionProperty, typeof(Window));
            if (dp != null)
            {
                dp.AddValueChanged(this, delegate
                {
                    if (_Panel == null) return;
                    removeTextBoxes();
                    prepareTextBoxes();
                    setStatus(DateValue);
                });
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //if (_NullBorder != null)
            //{
            //    BindingOperations.ClearAllBindings(_NullBorder);
            //}
            //_NullBorder = GetTemplateChild(ElementNullBorder) as Border;
            //if (_NullBorder != null)
            //{
            //    var nullBinding = new Binding("DateValue")
            //    {
            //        Converter = new DateToVisibilityConverter(),
            //        Mode = BindingMode.OneWay,
            //        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DateTimePicker), 1)
            //    };
            //    _NullBorder.SetBinding(VisibilityProperty, nullBinding);
            //}

            if (_InfoGrid != null)
            {
                _InfoGrid.Children.Clear();
                _InfoGrid.RowDefinitions.Clear();
                _InfoGrid.ColumnDefinitions.Clear();
            }
            _InfoGrid = GetTemplateChild(ElementInfoGrid) as Grid;

            _InfoBorder = GetTemplateChild(ElementInfoBorder) as Border;

            if (_Border != null)
            {
                _Border.MouseDown -= _Border_MouseDown;
            }
            _Border = GetTemplateChild(ElementBorder) as Border;
            if (_Border != null)
            {
                _Border.MouseDown += _Border_MouseDown;
            }

            if (_Button != null)
            {
                _Button.Click -= _Button_Click;
            }
            _Button = GetTemplateChild(ElementButton) as Button;
            if (_Button != null)
            {
                _Button.Click += _Button_Click;
            }

            if (_Popup != null)
            {
                _Popup.Opened -= _Popup_Opened;
                _Popup.Closed -= _Popup_Closed;
                _Popup.LostFocus -= _Popup_LostFocus;
            }
            _Popup = GetTemplateChild(ElementPopup) as Popup;
            if (_Popup != null)
            {
                _Popup.Opened += _Popup_Opened;
                _Popup.Closed += _Popup_Closed;
                _Popup.LostFocus += _Popup_LostFocus;
            }

            removeTextBoxes();
            _Panel = GetTemplateChild(ElementPanel) as StackPanel;
            
            prepareTextBoxes();

            if (_Calendar != null)
            {
                _Calendar.MouseDoubleClick -= _Calendar_MouseDoubleClick;
                _Calendar.SelectedDatesChanged -= _Calendar_SelectedDatesChanged;
                _Calendar.PreviewKeyDown -= _Calendar_PreviewKeyDown;
                _Calendar.PreviewMouseLeftButtonUp -= _Calendar_PreviewMouseLeftButtonUp;
                _Calendar.BlackoutDates.CollectionChanged += BlackoutDates_CollectionChanged;
            }
            _Calendar = GetTemplateChild(ElementCalendar) as Calendar;
            if (_Calendar != null)
            {
                if (_Blacks.BlackoutDates.Any())
                {
                    foreach (var d in _Blacks.BlackoutDates)
                    {
                        _Calendar.BlackoutDates.Add(d);
                    }
                    _Blacks.BlackoutDates.Clear();
                }
                _Calendar.DisplayMode = CalendarMode.Month;
                _Calendar.MouseDoubleClick += _Calendar_MouseDoubleClick;
                _Calendar.SelectedDatesChanged += _Calendar_SelectedDatesChanged;
                _Calendar.PreviewKeyDown += _Calendar_PreviewKeyDown;
                _Calendar.PreviewMouseLeftButtonUp += _Calendar_PreviewMouseLeftButtonUp;
                _Calendar.BlackoutDates.CollectionChanged += BlackoutDates_CollectionChanged;
            }
            if (_NowButton != null)
            {
                _NowButton.Click -= _NowButton_Click;
            }
            _NowButton = GetTemplateChild(ElementNowButton) as Button;
            if (_NowButton != null)
            {
                _NowButton.Click += _NowButton_Click;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds day-off
        /// </summary>
        /// <param name="day">Day of week</param>
        public void AddDayOff(DayOfWeek day)
        {
            var rd = _DaysOff.FirstOrDefault(d => d.Day == day);
            if (rd != null && !rd.Checked) rd.Checked = true;
        }
        /// <summary>
        /// Removes day-off
        /// </summary>
        /// <param name="day">Day of week</param>
        public void RemoveDayOff(DayOfWeek day)
        {
            var rd = _DaysOff.FirstOrDefault(d => d.Day == day);
            if (rd != null && !rd.Checked) rd.Checked = false;
        }
        /// <summary>
        /// Sets first day of week of calendar
        /// </summary>
        /// <param name="day">Day of week</param>
        public void SetFirstDayOfWeek(DayOfWeek day)
        {
            _FirstDayOfWeek = day;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the value specifies whether 'Now' button is visible
        /// </summary>
        public bool IsNowButtonVisible
        {
            get { return (bool)GetValue(IsNowButtonVisibleProperty); }
            set { SetValue(IsNowButtonVisibleProperty, value); }
        }
        /// <summary>
        /// Gets or sets the value specifies whether any date/time is shown
        /// </summary>
        public bool IsEmpty
        {
            get { return (bool)GetValue(IsEmptyProperty); }
            set { SetValue(IsEmptyProperty, value); }
        }
        /// <summary>
        /// Gets or sets the read-only state of control
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        /// <summary>
        /// Gets the value specifies whether currently selected date is blackout date
        /// </summary>
        public bool IsBlackoutDate
        {
            get { return (bool)GetValue(IsBlackoutDateProperty); }
            private set { SetValue(IsBlackoutDateProperty, value); }
        }
        /// <summary>
        /// Gets or sets signicant dates foreground
        /// </summary>
        public SolidColorBrush SignificantDateForeground
        {
            get { return (SolidColorBrush)GetValue(SignificantDateForegroundProperty); }
            set { SetValue(SignificantDateForegroundProperty, value); }
        }
        /// <summary>
        /// Gets or sets days-off foreground
        /// </summary>
        public SolidColorBrush DayOffForeground
        {
            get { return (SolidColorBrush)GetValue(DayOffForegroundProperty); }
            set { SetValue(DayOffForegroundProperty, value); }
        }
        /// <summary>
        /// Gets or sets orientation of info panel of signicant dates
        /// </summary>
        public Orientation InfoPanelOrientation
        {
            get { return (Orientation)GetValue(InfoPanelOrientationProperty); }
            set { SetValue(InfoPanelOrientationProperty, value); }
        }
        /// <summary>
        /// Specifies whether user is allowed to enter invalid dates
        /// </summary>
        public bool AllowErrorValues
        {
            get { return (bool)GetValue(AllowErrorValuesProperty); }
            set { SetValue(AllowErrorValuesProperty, value); }
        }
        /// <summary>
        /// Gets or sets format of DateTimePicker
        /// </summary>
        public DateBoxFormat Format
        {
            get { return (DateBoxFormat)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }
        /// <summary>
        /// Gets or sets selection mode of drop-down calendar of DateTimePicker
        /// </summary>
        public DateBoxSelectionMode SelectionMode
        {
            get { return (DateBoxSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }
        /// <summary>
        /// Gets or sets the date/time value of DateTimePicker
        /// </summary>
        public DateTime DateValue
        {
            get { return (DateTime)GetValue(DateValueProperty); }
            set { SetValue(DateValueProperty, value); }
        }
        /// <summary>
        /// Gets current status of DateTimePicker
        /// </summary>
        public DateBoxStatus Status
        {
            get { return (DateBoxStatus)GetValue(StatusProperty); }
            private set { SetValue(StatusProperty, value); }
        }
        /// <summary>
        /// Gets or sets significant dates background
        /// </summary>
        public SolidColorBrush SignificantDateBackground
        {
            get { return (SolidColorBrush)GetValue(SignificantDateBackgroundProperty); }
            set { SetValue(SignificantDateBackgroundProperty, value); }
        }
        /// <summary>
        /// Gets or sets days-off background
        /// </summary>
        public SolidColorBrush DayOffBackground
        {
            get { return (SolidColorBrush)GetValue(DayOffBackgroundProperty); }
            set { SetValue(DayOffBackgroundProperty, value); }
        }
        /// <summary>
        /// Gets or sets background for invalid dates
        /// </summary>
        public SolidColorBrush ErrorBackground
        {
            get { return (SolidColorBrush)GetValue(ErrorBackgroundProperty); }
            set { SetValue(ErrorBackgroundProperty, value); }
        }
        /// <summary>
        /// Gets collection of significant dates
        /// </summary>
        public SignificantDatesCollection SignificantDates
        {
            get { return _SignificantDates; }
        }
        /// <summary>
        /// Gets blackout dates collection
        /// </summary>
        public CalendarBlackoutDatesCollection BlackoutDates
        {
            get
            {
                return _Calendar == null ? _Blacks.BlackoutDates : _Calendar.BlackoutDates;
            }
        }
        /// <summary>
        /// Gets date/time value of DateTimePicker as string
        /// </summary>
        public string Text
        {
            get
            {
                if (_Panel == null || _Panel.Visibility != Visibility.Visible) return "";
                var sb = new StringBuilder();
                foreach (var e in _Panel.Children)
                {
                    var box = e as TextBox;
                    if (box != null)
                    {
                        sb.Append(box.Text);
                        continue;
                    }
                    var block = e as TextBlock;
                    if (block != null)
                        sb.Append(block.Text);
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// Gets or sets content of 'Now' button
        /// </summary>
        public object NowButtonContent
        {
            get { if (_NowButton != null) return _NowButton.Content; return null; }
            set { if (_NowButton != null)_NowButton.Content = value; }
        }

        #endregion

        #region Callback procedures
        /// <summary>
        /// Invoked just before the <see cref="IsNowButtonVisibleChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnIsNowButtonVisibleChanged(bool oldValue, bool newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue)
            {
                RoutedEvent = IsNowButtonVisibleEvent
            };
            RaiseEvent(e);
        }
        private static void OnIsNowButtonVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnIsNowButtonVisibleChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invoked just before the <see cref="IsEmptyChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnIsEmptyChanged(bool oldValue, bool newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue)
            {
                RoutedEvent = IsEmptyChangedEvent
            };
            RaiseEvent(e);
        }
        private static void OnIsEmptyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnIsEmptyChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invoked just before the <see cref="IsBlackoutDateChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnIsReadOnlyChanged(bool oldValue, bool newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue)
            {
                RoutedEvent = IsReadOnlyChangedEvent
            };
            RaiseEvent(e);
        }
        private static void OnIsReadOnlyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnIsReadOnlyChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invoked just before the <see cref="IsBlackoutDateChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnIsBlackoutDateChanged(bool oldValue, bool newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue)
            {
                RoutedEvent = IsBlackoutDateChangedEvent
            };
            RaiseEvent(e);
        }
        private static void OnIsBlackoutDateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnIsBlackoutDateChanged((bool)e.OldValue, (bool)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="FormatChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old format value</param>
        /// <param name="newValue">New format value</param>
        protected virtual void OnFormatChanged(DateBoxFormat oldValue, DateBoxFormat newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<DateBoxFormat>(oldValue, newValue)
            {
                RoutedEvent = FormatChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnFormatChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.removeTextBoxes();
            db.prepareTextBoxes();
            db.setStatus(db.DateValue);
            db.OnFormatChanged((DateBoxFormat)e.OldValue, (DateBoxFormat)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="InfoPanelOrientationChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old orientation value</param>
        /// <param name="newValue">New orientation value</param>
        protected virtual void OnInfoPanelOrientationChanged(Orientation oldValue, Orientation newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<Orientation>(oldValue, newValue)
            {
                RoutedEvent = InfoPanelOrientationChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnInfoPanelOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnInfoPanelOrientationChanged((Orientation)e.OldValue, (Orientation)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="SelectionModeChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old selection mode value</param>
        /// <param name="newValue">New selection mode value</param>
        protected virtual void OnSelectionModeChanged(DateBoxSelectionMode oldValue, DateBoxSelectionMode newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<DateBoxSelectionMode>(oldValue, newValue)
            {
                RoutedEvent = SelectionModeChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnSelectionModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnSelectionModeChanged((DateBoxSelectionMode)e.OldValue, (DateBoxSelectionMode)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="AllowErrorValuesChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnAllowErrorValuesChanged(bool oldValue, bool newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue)
            {
                RoutedEvent = AllowErrorValuesChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnAllowErrorValuesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnAllowErrorValuesChanged((bool)e.OldValue, (bool)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="StatusChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old status value</param>
        /// <param name="newValue">New status value</param>
        protected virtual void OnStatusChanged(DateBoxStatus oldValue, DateBoxStatus newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<DateBoxStatus>(oldValue, newValue)
            {
                RoutedEvent = StatusChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnStatusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnStatusChanged((DateBoxStatus)e.OldValue, (DateBoxStatus)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="ErrorBackgroundChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old background value</param>
        /// <param name="newValue">New background value</param>
        protected virtual void OnErrorBackgroundChanged(SolidColorBrush oldValue, SolidColorBrush newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(oldValue, newValue)
            {
                RoutedEvent = ErrorBackgroundChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnErrorBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnErrorBackgroundChanged((SolidColorBrush)e.OldValue, (SolidColorBrush)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="SignificantDateForegroundChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old foreground value</param>
        /// <param name="newValue">New foreground value</param>
        protected virtual void OnSignificantDateForegroundChanged(SolidColorBrush oldValue, SolidColorBrush newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(oldValue, newValue)
            {
                RoutedEvent = SignificantDateForegroundChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnSignificantDateForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnSignificantDateForegroundChanged((SolidColorBrush)e.OldValue, (SolidColorBrush)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="DayOffForegroundChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old foreground value</param>
        /// <param name="newValue">New foreground value</param>
        protected virtual void OnDayOffForegroundChanged(SolidColorBrush oldValue, SolidColorBrush newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(oldValue, newValue)
            {
                RoutedEvent = DayOffForegroundChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnDayOffForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnDayOffForegroundChanged((SolidColorBrush)e.OldValue, (SolidColorBrush)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="SignificantDateBackgroundChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old background value</param>
        /// <param name="newValue">New background value</param>
        protected virtual void OnSignificantDateBackgroundChanged(SolidColorBrush oldValue, SolidColorBrush newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(oldValue, newValue)
            {
                RoutedEvent = SignificantDateBackgroundChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnSignificantDateBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnSignificantDateBackgroundChanged((SolidColorBrush)e.OldValue, (SolidColorBrush)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="DayOffBackgroundChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old background value</param>
        /// <param name="newValue">New background value</param>
        protected virtual void OnDayOffBackgroundChanged(SolidColorBrush oldValue, SolidColorBrush newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(oldValue, newValue)
            {
                RoutedEvent = DayOffBackgroundChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnDayOffBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnDayOffBackgroundChanged((SolidColorBrush)e.OldValue, (SolidColorBrush)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="DateValueChanged"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old date value</param>
        /// <param name="newValue">New date value</param>
        protected virtual void OnDateValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<DateTime?>(oldValue, newValue)
            {
                RoutedEvent = DateValueChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnDateValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var db = sender as DateTimePicker;
            if (db == null) return;
            db.OnDateValueChanged(e.OldValue as DateTime?, e.NewValue as DateTime?);
        }

        private static object CoerceDateValue(DependencyObject d, object value)
        {
            var dtp = d as DateTimePicker;
            if (dtp == null) return value;
            var date = Convert.ToDateTime(value);
            switch (dtp.Format)
            {
                case DateBoxFormat.LongDate:
                case DateBoxFormat.ShortDate:
                    return new DateTime(date.Year, date.Month, date.Day);
                case DateBoxFormat.LongTime:
                case DateBoxFormat.LongDateAndLongTime:
                case DateBoxFormat.ShortDateAndLongTime:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                case DateBoxFormat.ShortTime:
                case DateBoxFormat.LongDateAndShortTime:
                case DateBoxFormat.ShortDateAndShortTime:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            }
            return value;
        }
        #endregion

        #region Routed events
        /// <summary>
        /// Occurs when the <see cref="IsNowButtonVisible"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> IsNowButtonVisibleChanged
        {
            add { AddHandler(IsNowButtonVisibleEvent, value); }
            remove { RemoveHandler(IsNowButtonVisibleEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="IsNowButtonVisibleEvent"/> routed event
        /// </summary>
        public static readonly RoutedEvent IsNowButtonVisibleEvent =
            EventManager.RegisterRoutedEvent("IsNowButtonVisibleChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>), typeof(DateTimePicker));

        /// <summary>
        /// Occurs when the <see cref="IsEmpty"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> IsEmptyChanged
        {
            add { AddHandler(IsEmptyChangedEvent, value); }
            remove { RemoveHandler(IsEmptyChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="IsEmptyChangedEvent"/> routed event
        /// </summary>
        public static readonly RoutedEvent IsEmptyChangedEvent =
            EventManager.RegisterRoutedEvent("IsEmptyChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>), typeof(DateTimePicker));

        /// <summary>
        /// Occurs when the <see cref="IsReadOnly"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> IsReadOnlyChanged
        {
            add { AddHandler(IsReadOnlyChangedEvent, value); }
            remove { RemoveHandler(IsReadOnlyChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="IsReadOnlyChangedEvent"/> routed event
        /// </summary>
        public static readonly RoutedEvent IsReadOnlyChangedEvent =
            EventManager.RegisterRoutedEvent("IsReadOnlyChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>), typeof(DateTimePicker));

        /// <summary>
        /// Occurs when the <see cref="IsBlackoutDate"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> IsBlackoutDateChanged
        {
            add { AddHandler(IsBlackoutDateChangedEvent, value); }
            remove { RemoveHandler(IsBlackoutDateChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="IsBlackoutDateChangedEvent"/> routed event
        /// </summary>
        public static readonly RoutedEvent IsBlackoutDateChangedEvent =
            EventManager.RegisterRoutedEvent("IsBlackoutDateChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>), typeof(DateTimePicker));

        /// <summary>
        /// Occurs when the <see cref="Format"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<DateBoxFormat> FormatChanged
        {
            add { AddHandler(FormatChangedEvent, value); }
            remove { RemoveHandler(FormatChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="FormatChangedEvent"/> routed event
        /// </summary>
        public static readonly RoutedEvent FormatChangedEvent =
            EventManager.RegisterRoutedEvent("FormatChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<DateBoxFormat>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="InfoPanelOrientation"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Orientation> InfoPanelOrientationChanged
        {
            add { AddHandler(InfoPanelOrientationChangedEvent, value); }
            remove { RemoveHandler(InfoPanelOrientationChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="InfoPanelOrientationChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent InfoPanelOrientationChangedEvent =
            EventManager.RegisterRoutedEvent("InfoPanelOrientationChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Orientation>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="SelectionMode"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<DateBoxSelectionMode> SelectionModeChanged
        {
            add { AddHandler(SelectionModeChangedEvent, value); }
            remove { RemoveHandler(SelectionModeChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="SelectionModeChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent SelectionModeChangedEvent =
            EventManager.RegisterRoutedEvent("SelectionModeChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<DateBoxSelectionMode>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="Status"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<DateBoxStatus> StatusChanged
        {
            add { AddHandler(StatusChangedEvent, value); }
            remove { RemoveHandler(StatusChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="StatusChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent StatusChangedEvent = EventManager.RegisterRoutedEvent("StatusChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<DateBoxStatus>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="SignificantDateForeground"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<SolidColorBrush> SignificantDateForegroundChanged
        {
            add { AddHandler(SignificantDateForegroundChangedEvent, value); }
            remove { RemoveHandler(SignificantDateForegroundChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="SignificantDateForegroundChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent SignificantDateForegroundChangedEvent =
            EventManager.RegisterRoutedEvent("SignificantDateForegroundChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="DayOffForeground"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<SolidColorBrush> DayOffForegroundChanged
        {
            add { AddHandler(DayOffForegroundChangedEvent, value); }
            remove { RemoveHandler(DayOffForegroundChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="DayOffForegroundChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent DayOffForegroundChangedEvent =
            EventManager.RegisterRoutedEvent("DayOffForegroundChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="SignificantDateBackground"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<SolidColorBrush> SignificantDateBackgroundChanged
        {
            add { AddHandler(SignificantDateBackgroundChangedEvent, value); }
            remove { RemoveHandler(SignificantDateBackgroundChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="SignificantDateBackgroundChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent SignificantDateBackgroundChangedEvent =
            EventManager.RegisterRoutedEvent("SignificantDateBackgroundChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="DayOffBackground"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<SolidColorBrush> DayOffBackgroundChanged
        {
            add { AddHandler(DayOffBackgroundChangedEvent, value); }
            remove { RemoveHandler(DayOffBackgroundChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="DayOffBackgroundChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent DayOffBackgroundChangedEvent =
            EventManager.RegisterRoutedEvent("DayOffBackgroundChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="ErrorBackground"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<SolidColorBrush> ErrorBackgroundChanged
        {
            add { AddHandler(ErrorBackgroundChangedEvent, value); }
            remove { RemoveHandler(ErrorBackgroundChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="ErrorBackgroundChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent ErrorBackgroundChangedEvent =
            EventManager.RegisterRoutedEvent("ErrorBackgroundChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="AllowErrorValues"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool?> AllowErrorValuesChanged
        {
            add { AddHandler(AllowErrorValuesChangedEvent, value); }
            remove { RemoveHandler(AllowErrorValuesChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="AllowErrorValuesChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent AllowErrorValuesChangedEvent = EventManager.RegisterRoutedEvent("AllowErrorValuesChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool?>), typeof(DateTimePicker));
        /// <summary>
        /// Occurs when the <see cref="DateValue"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<DateTime?> DateValueChanged
        {
            add { AddHandler(DateValueChangedEvent, value); }
            remove { RemoveHandler(DateValueChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="DateValueChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent DateValueChangedEvent = EventManager.RegisterRoutedEvent("DateValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<DateTime?>), typeof(DateTimePicker));
        #endregion

        #region Private event handlers

        private void BlackoutDates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsBlackoutDate = false;
            if (Format == DateBoxFormat.ShortTime || Format == DateBoxFormat.LongTime ||
                _Calendar.BlackoutDates.Count == 0) return;
            if (_Calendar.BlackoutDates.Any(d => d.Start <= DateValue.Date && d.End >= DateValue.Date))
                IsBlackoutDate = true;
        }

        private void _Calendar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectionMode != DateBoxSelectionMode.SingleClick || _Calendar.DisplayMode != CalendarMode.Month ||
                !(Mouse.Captured is CalendarItem)) return;
            switch (Format)
            {
                case DateBoxFormat.LongDateAndShortTime:
                case DateBoxFormat.ShortDateAndShortTime:
                    if (_Calendar.SelectedDate != null)
                    {
                        DateValue =
                                _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                    .AddMinutes(DateValue.Minute);
                        IsEmpty = false;
                    }
                    break;
                case DateBoxFormat.LongDateAndLongTime:
                case DateBoxFormat.ShortDateAndLongTime:
                    if (_Calendar.SelectedDate != null)
                    {
                        DateValue =
                                _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                    .AddMinutes(DateValue.Minute)
                                    .AddSeconds(DateValue.Second);
                        IsEmpty = false;
                    }
                    break;
                case DateBoxFormat.LongDate:
                case DateBoxFormat.ShortDate:
                    if (_Calendar.SelectedDate != null)
                    {
                        DateValue = _Calendar.SelectedDate.Value;
                        IsEmpty = false;
                    }
                    break;
            }
            _Popup.IsOpen = false;
        }

        private void _Calendar_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    _Popup.IsOpen = false;
                    e.Handled = true;
                    break;
                case Key.Space:
                case Key.Enter:
                    switch (Format)
                    {
                        case DateBoxFormat.LongDateAndShortTime:
                        case DateBoxFormat.ShortDateAndShortTime:
                            if (_Calendar.SelectedDate != null)
                            {
                                DateValue =
                                        _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                            .AddMinutes(DateValue.Minute);
                                IsEmpty = false;
                            }
                            break;
                        case DateBoxFormat.LongDateAndLongTime:
                        case DateBoxFormat.ShortDateAndLongTime:
                            if (_Calendar.SelectedDate != null)
                            {
                                DateValue =
                                        _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                            .AddMinutes(DateValue.Minute)
                                            .AddSeconds(DateValue.Second);
                                IsEmpty = false;
                            }
                            break;
                        case DateBoxFormat.LongDate:
                        case DateBoxFormat.ShortDate:
                            if (_Calendar.SelectedDate != null)
                            {
                                DateValue = _Calendar.SelectedDate.Value;
                                IsEmpty = false;
                            }
                            break;
                    }
                    _Popup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void _Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionMode == DateBoxSelectionMode.SimpleSelection && _Calendar.DisplayMode == CalendarMode.Month)
            {
                switch (Format)
                {
                    case DateBoxFormat.LongDateAndShortTime:
                    case DateBoxFormat.ShortDateAndShortTime:
                        if (_Calendar.SelectedDate != null)
                        {
                            DateValue =
                                    _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                        .AddMinutes(DateValue.Minute);
                            IsEmpty = false;
                        }
                        break;
                    case DateBoxFormat.LongDateAndLongTime:
                    case DateBoxFormat.ShortDateAndLongTime:
                        if (_Calendar.SelectedDate != null)
                        {
                            DateValue =
                                    _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                        .AddMinutes(DateValue.Minute)
                                        .AddSeconds(DateValue.Second);
                            IsEmpty = false;
                        }
                        break;
                    case DateBoxFormat.LongDate:
                    case DateBoxFormat.ShortDate:
                        if (_Calendar.SelectedDate != null)
                        {
                            DateValue = _Calendar.SelectedDate.Value;
                            IsEmpty = false;
                        }
                        break;
                }
            }
        }

        private void _Calendar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectionMode != DateBoxSelectionMode.DoubleClick || _Calendar.DisplayMode != CalendarMode.Month ||
                !(Mouse.Captured is CalendarItem)) return;
            switch (Format)
            {
                case DateBoxFormat.LongDateAndShortTime:
                case DateBoxFormat.ShortDateAndShortTime:
                    if (_Calendar.SelectedDate != null)
                    {
                        DateValue =
                                _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                    .AddMinutes(DateValue.Minute);
                        IsEmpty = false;
                    }
                    break;
                case DateBoxFormat.LongDateAndLongTime:
                case DateBoxFormat.ShortDateAndLongTime:
                    if (_Calendar.SelectedDate != null)
                    {
                        DateValue =
                                _Calendar.SelectedDate.Value.AddHours(DateValue.Hour)
                                    .AddMinutes(DateValue.Minute)
                                    .AddSeconds(DateValue.Second);
                        IsEmpty = false;
                    }
                    break;
                case DateBoxFormat.LongDate:
                case DateBoxFormat.ShortDate:
                    if (_Calendar.SelectedDate != null)
                    {
                        DateValue = _Calendar.SelectedDate.Value;
                        IsEmpty = false;
                    }
                    break;
            }
            _Popup.IsOpen = false;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }
            var tx = sender as TextBox;
            if (tx == null) return;
            var bd = tx.Tag as BoxData;
            if (bd == null) return;

            _KeyCurrent = e.Key;
            switch (e.Key)
            {
                case Key.Right:
                    if (bd.Type == BoxType.Day && bd.FormatString.Length <= 2 && tx.Text.Length < tx.MaxLength)
                        setDayValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Month && bd.FormatString.Length <= 2 && tx.Text.Length < tx.MaxLength)
                        setMonthValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Year && tx.Text.Length < tx.MaxLength)
                        setYearValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Hour && tx.Text.Length < tx.MaxLength)
                        setHoursValue(Convert.ToInt32(tx.Text), bd.FormatString);
                    else if (bd.Type == BoxType.Minute && tx.Text.Length < tx.MaxLength)
                        setMinutesValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Second && tx.Text.Length < tx.MaxLength)
                        setSecondsValue(Convert.ToInt32(tx.Text));
                    jumpRight(bd);
                    e.Handled = true;
                    break;
                case Key.Left:
                    if (bd.Type == BoxType.Day && bd.FormatString.Length <= 2 && tx.Text.Length < tx.MaxLength)
                        setDayValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Month && bd.FormatString.Length <= 2 && tx.Text.Length < tx.MaxLength)
                        setMonthValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Year && tx.Text.Length < tx.MaxLength)
                        setYearValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Hour && tx.Text.Length < tx.MaxLength)
                        setHoursValue(Convert.ToInt32(tx.Text), bd.FormatString);
                    else if (bd.Type == BoxType.Minute && tx.Text.Length < tx.MaxLength)
                        setMinutesValue(Convert.ToInt32(tx.Text));
                    else if (bd.Type == BoxType.Second && tx.Text.Length < tx.MaxLength)
                        setSecondsValue(Convert.ToInt32(tx.Text));
                    jumpLeft(bd);
                    e.Handled = true;
                    break;
                case Key.Up:
                case Key.OemPlus:
                case Key.Add:
                    switch (bd.Type)
                    {
                        case BoxType.Day:
                            if (tx.Text.Length < tx.MaxLength && bd.FormatString.Length <= 2) setDayValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddDays(1);
                            break;
                        case BoxType.Month:
                            if (tx.Text.Length < tx.MaxLength && bd.FormatString.Length <= 2) setMonthValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddMonths(1);
                            break;
                        case BoxType.Year:
                            if (tx.Text.Length < tx.MaxLength) setYearValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddYears(1);
                            break;
                        case BoxType.Hour:
                            if (tx.Text.Length < tx.MaxLength) setHoursValue(Convert.ToInt32(tx.Text), bd.FormatString);
                            DateValue = DateValue.AddHours(1);
                            break;
                        case BoxType.Minute:
                            if (tx.Text.Length < tx.MaxLength) setMinutesValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddMinutes(1);
                            break;
                        case BoxType.Second:
                            if (tx.Text.Length < tx.MaxLength) setSecondsValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddSeconds(1);
                            break;
                        default:    //AP:PM
                            DateValue = DateValue.AddHours(12);
                            break;
                    }
                    setStatus(DateValue);
                    tx.SelectAll();
                    e.Handled = true;
                    break;
                case Key.Down:
                case Key.OemMinus:
                case Key.Subtract:
                    switch (bd.Type)
                    {
                        case BoxType.Day:
                            if (tx.Text.Length < tx.MaxLength && bd.FormatString.Length <= 2) setDayValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddDays(-1);
                            break;
                        case BoxType.Month:
                            if (tx.Text.Length < tx.MaxLength && bd.FormatString.Length <= 2) setMonthValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddMonths(-1);
                            break;
                        case BoxType.Year:
                            if (tx.Text.Length < tx.MaxLength) setYearValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddYears(-1);
                            break;
                        case BoxType.Hour:
                            if (tx.Text.Length < tx.MaxLength) setHoursValue(Convert.ToInt32(tx.Text), bd.FormatString);
                            DateValue = DateValue.AddHours(-1);
                            break;
                        case BoxType.Minute:
                            if (tx.Text.Length < tx.MaxLength) setMinutesValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddMinutes(-1);
                            break;
                        case BoxType.Second:
                            if (tx.Text.Length < tx.MaxLength) setSecondsValue(Convert.ToInt32(tx.Text));
                            DateValue = DateValue.AddSeconds(-1);
                            break;
                        default:    //AM:PM
                            DateValue = DateValue.AddHours(-12);
                            break;
                    }
                    setStatus(DateValue);
                    tx.SelectAll();
                    e.Handled = true;
                    break;
                case Key.D0:
                case Key.NumPad0:
                case Key.D1:
                case Key.NumPad1:
                case Key.D2:
                case Key.NumPad2:
                case Key.D3:
                case Key.NumPad3:
                case Key.D4:
                case Key.NumPad4:
                case Key.D5:
                case Key.NumPad5:
                case Key.D6:
                case Key.NumPad6:
                case Key.D7:
                case Key.NumPad7:
                case Key.D8:
                case Key.NumPad8:
                case Key.D9:
                case Key.NumPad9:
                    if ((bd.Type == BoxType.Day && bd.FormatString.Length > 2) ||
                        (bd.Type == BoxType.Month && bd.FormatString.Length > 2) ||
                        bd.Type == BoxType.AmPm)
                    {
                        e.Handled = true;
                    }
                    break;
                case Key.Tab:
                    break;
                default:
                    e.Handled = true;
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (IsEmpty) return;
                var tx = sender as TextBox;
                if (tx == null) return;
                var bd = tx.Tag as BoxData;
                if (bd == null) return;
                switch (bd.Type)
                {
                    case BoxType.Day:
                        switch (bd.FormatString.Length)
                        {
                            case 2:
                                if (tx.Text.Length == 2 || !tx.Text.In("1", "2", "3"))
                                {
                                    setDayValue(Convert.ToInt32(tx.Text));
                                    if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                        Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                        Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                        jumpRight(bd);
                                    else
                                        tx.SelectAll();
                                }
                                break;
                            case 1:
                                switch (tx.Text.Length)
                                {
                                    case 2:
                                        setDayValue(Convert.ToInt32(tx.Text));
                                        if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                            Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                            Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                            jumpRight(bd);
                                        else
                                            tx.SelectAll();
                                        break;
                                    case 1:
                                        if (!tx.Text.In("1", "2", "3"))
                                        {
                                            setDayValue(Convert.ToInt32(tx.Text));
                                            if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2,
                                                Key.NumPad2,
                                                Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                                Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9,
                                                Key.NumPad9))
                                                jumpRight(bd);
                                            else
                                                tx.SelectAll();
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case BoxType.Month:
                        switch (bd.FormatString.Length)
                        {
                            case 2:
                                if (tx.Text.Length == 2 || !tx.Text.In("1"))
                                {
                                    setMonthValue(Convert.ToInt32(tx.Text));
                                    if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                        Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                        Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                        jumpRight(bd);
                                    else
                                        tx.SelectAll();
                                }
                                break;
                            case 1:
                                switch (tx.Text.Length)
                                {
                                    case 2:
                                        setMonthValue(Convert.ToInt32(tx.Text));
                                        if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                            Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                            Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                            jumpRight(bd);
                                        else
                                            tx.SelectAll();
                                        break;
                                    case 1:
                                        if (!tx.Text.In("1"))
                                        {
                                            setMonthValue(Convert.ToInt32(tx.Text));
                                            if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2,
                                                Key.NumPad2,
                                                Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                                Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9,
                                                Key.NumPad9))
                                                jumpRight(bd);
                                            else
                                                tx.SelectAll();
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case BoxType.Year://todo - for years
                        switch (bd.FormatString.Length)
                        {
                            case 4:
                            case 3:
                                if (tx.Text.Length == 4)
                                {
                                    setYearValue(Convert.ToInt32(tx.Text));
                                    if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                        Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                        Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                        jumpRight(bd);
                                    else
                                        tx.SelectAll();
                                }
                                break;
                            case 2:
                            case 1:
                                if (tx.Text.Length == 2)
                                {
                                    setYearValue(Convert.ToInt32(tx.Text));
                                    if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                        Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                        Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                        jumpRight(bd);
                                    else
                                        tx.SelectAll();
                                }
                                break;
                        }
                        break;
                    case BoxType.Hour:
                        switch (bd.FormatString.Length)
                        {
                            case 2:
                                if (tx.Text.Length == 2 || !tx.Text.In("1", "2"))
                                {
                                    setHoursValue(Convert.ToInt32(tx.Text), bd.FormatString);
                                    if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                        Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                        Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                        jumpRight(bd);
                                    else
                                        tx.SelectAll();
                                }
                                break;
                            case 1:
                                switch (tx.Text.Length)
                                {
                                    case 2:
                                        setHoursValue(Convert.ToInt32(tx.Text), bd.FormatString);
                                        if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                            Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                            Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                            jumpRight(bd);
                                        else
                                            tx.SelectAll();
                                        break;
                                    case 1:
                                        if (!tx.Text.In("1", "2"))
                                        {
                                            setHoursValue(Convert.ToInt32(tx.Text), bd.FormatString);
                                            if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2,
                                                Key.NumPad2,
                                                Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                                Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9,
                                                Key.NumPad9))
                                                jumpRight(bd);
                                            else
                                                tx.SelectAll();
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case BoxType.Minute:
                        switch (bd.FormatString.Length)
                        {
                            case 2:
                                if (tx.Text.Length == 2 || tx.Text.In("6", "7", "8", "9"))
                                {
                                    setMinutesValue(Convert.ToInt32(tx.Text));
                                    if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                        Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                        Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                        jumpRight(bd);
                                    else
                                        tx.SelectAll();
                                }
                                break;
                            case 1:
                                switch (tx.Text.Length)
                                {
                                    case 2:
                                        setMinutesValue(Convert.ToInt32(tx.Text));
                                        if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                            Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                            Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                            jumpRight(bd);
                                        else
                                            tx.SelectAll();
                                        break;
                                    case 1:
                                        if (tx.Text.In("6", "7", "8", "9"))
                                        {
                                            setMinutesValue(Convert.ToInt32(tx.Text));
                                            if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2,
                                                Key.NumPad2,
                                                Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                                Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9,
                                                Key.NumPad9))
                                                jumpRight(bd);
                                            else
                                                tx.SelectAll();
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case BoxType.Second:
                        switch (bd.FormatString.Length)
                        {
                            case 2:
                                if (tx.Text.Length == 2 || tx.Text.In("6", "7", "8", "9"))
                                {
                                    setSecondsValue(Convert.ToInt32(tx.Text));
                                    if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                        Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                        Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                        jumpRight(bd);
                                    else
                                        tx.SelectAll();
                                }
                                break;
                            case 1:
                                switch (tx.Text.Length)
                                {
                                    case 2:
                                        setSecondsValue(Convert.ToInt32(tx.Text));
                                        if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2, Key.NumPad2,
                                            Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                            Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9, Key.NumPad9))
                                            jumpRight(bd);
                                        else
                                            tx.SelectAll();
                                        break;
                                    case 1:
                                        if (tx.Text.In("6", "7", "8", "9"))
                                        {
                                            setSecondsValue(Convert.ToInt32(tx.Text));
                                            if (Status != DateBoxStatus.Error && _KeyCurrent.In(Key.D0, Key.NumPad0, Key.D1, Key.NumPad1, Key.D2,
                                                Key.NumPad2,
                                                Key.D3, Key.NumPad3, Key.D4, Key.NumPad4, Key.D5, Key.NumPad5, Key.D6,
                                                Key.NumPad6, Key.D7, Key.NumPad7, Key.D8, Key.NumPad8, Key.D9,
                                                Key.NumPad9))
                                                jumpRight(bd);
                                            else
                                                tx.SelectAll();
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
            finally
            {
                _KeyCurrent = Key.None;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tx = sender as TextBox;
            if (tx == null) return;
            _CurrentTextBox = tx;
            tx.SelectAll();
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEmpty) return;
            var tx = sender as TextBox;
            if (tx == null) return;
            tx.Focus();
            e.Handled = true;
        }

        private void TextBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void _Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsBlackoutDate || IsReadOnly) return;
            if (!_AllowPopup)
            {
                _AllowPopup = true;
                return;
            }
            _Popup.IsOpen = true;
        }

        private void _Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_CurrentTextBox != null)
            {
                _CurrentTextBox.Focus();
            }
            else
            {
                _Panel.Children.OfType<TextBox>().First().Focus();
            }
            e.Handled = true;
        }

        private void _NowButton_Click(object sender, RoutedEventArgs e)
        {
            DateValue = DateTime.Now;
        }

        private void _Popup_Opened(object sender, EventArgs e)
        {
            _Calendar.FirstDayOfWeek = _FirstDayOfWeek;
            _Calendar.SelectedDate = _Calendar.DisplayDate = DateValue;
            _Calendar.Focus();
        }

        private void _Popup_Closed(object sender, EventArgs e)
        {
            if (_CurrentTextBox != null)
            {
                _CurrentTextBox.Focus();
            }
        }

        private void _Popup_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_Popup.IsOpen) return;
            var button = Mouse.Captured as Button;
            if (button != null && button.Name == ElementButton)
            {
                _AllowPopup = false;
            }
        }

        private void _SignificantDates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            setStatus(DateValue);
        }
        #endregion

        #region Private procedures

        private List<BoxData> parseTimeFormatString(bool shortFormat)
        {
            var boxes = new List<BoxData>();
            var hoursFound = false;
            var minutesFound = false;
            var secondsFound = false;
            var ampmFound = false;
            var sepFound = false;
            var formatString = shortFormat
                ? Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern
                : Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern;

            if (formatString.Contains("H") && formatString.Contains("t"))
                formatString = formatString.Replace("t", "");
            formatString = formatString.Trim();
            while (formatString.Trim().EndsWith(":") || formatString.Trim().EndsWith("-"))
                formatString = formatString.Substring(0, formatString.Length - 1);
            formatString = formatString.Trim();

            foreach (var t in formatString)
            {
                switch (t)
                {
                    case 'h':
                    case 'H':
                        if (!hoursFound)
                        {
                            minutesFound = secondsFound = ampmFound = sepFound = false;
                            hoursFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Hour, FormatString = new string(t, 1) });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += t;
                        }
                        break;
                    case 'm':
                        if (!minutesFound)
                        {
                            hoursFound = secondsFound = ampmFound = sepFound = false;
                            minutesFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Minute, FormatString = new string(t, 1) });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += t;
                        }
                        break;
                    case 's':
                        if (!secondsFound)
                        {
                            hoursFound = minutesFound = ampmFound = sepFound = false;
                            secondsFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Second, FormatString = new string(t, 1) });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += t;
                        }
                        break;
                    case 't':
                        if (!ampmFound)
                        {
                            hoursFound = minutesFound = secondsFound = sepFound = false;
                            ampmFound = true;
                            boxes.Add(new BoxData { Type = BoxType.AmPm, FormatString = new string(t, 1) });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += t;
                        }
                        break;
                    default:
                        if (!sepFound)
                        {
                            hoursFound = minutesFound = secondsFound = ampmFound = false;
                            sepFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Separator, FormatString = new string(t, 1) });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += t;
                        }
                        break;
                }
            }
            return boxes;
        }

        private List<BoxData> parseDateFormatString(bool shortFormat)
        {
            var boxes = new List<BoxData>();
            var daysFound = false;
            var monthsFound = false;
            var yearsFound = false;
            var sepFound = false;
            var formatString = shortFormat
                ? Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern
                : Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern;
            formatString = formatString.Trim();
            foreach (var t in formatString)
            {
                switch (t)
                {
                    case 'd':
                        if (!daysFound)
                        {
                            monthsFound = yearsFound = sepFound = false;
                            daysFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Day, FormatString = "d" });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += 'd';
                        }
                        break;
                    case 'M':
                        if (!monthsFound)
                        {
                            daysFound = yearsFound = sepFound = false;
                            monthsFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Month, FormatString = "M" });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += 'M';
                        }
                        break;
                    case 'y':
                        if (!yearsFound)
                        {
                            daysFound = monthsFound = sepFound = false;
                            yearsFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Year, FormatString = "y" });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += 'y';
                        }
                        break;
                    default:
                        if (!sepFound)
                        {
                            daysFound = monthsFound = yearsFound = false;
                            sepFound = true;
                            boxes.Add(new BoxData { Type = BoxType.Separator, FormatString = new string(t, 1) });
                        }
                        else
                        {
                            boxes[boxes.Count - 1].FormatString += t;
                        }
                        break;
                }
            }
            return boxes;
        }

        private void removeTextBoxes()
        {
            if (_Panel == null) return;
            for (var i = _Panel.Children.Count - 1; i >= 0; i--)
            {
                var tx = _Panel.Children[i] as TextBox;
                if (tx != null)
                {
                    tx.TextChanged -= TextBox_TextChanged;
                    tx.GotFocus -= TextBox_GotFocus;
                    tx.PreviewMouseLeftButtonDown -= TextBox_PreviewMouseLeftButtonDown;
                    tx.PreviewMouseRightButtonUp -= TextBox_PreviewMouseRightButtonUp;
                    tx.PreviewKeyDown -= TextBox_PreviewKeyDown;
                    BindingOperations.ClearAllBindings(tx);
                }
                _Panel.Children.RemoveAt(i);
            }
        }

        private void prepareTextBoxes()
        {
            if (_Panel == null) return;
            var boxes = new List<BoxData>();
            switch (Format)
            {
                case DateBoxFormat.ShortDate:
                    boxes = parseDateFormatString(true);
                    break;
                case DateBoxFormat.ShortDateAndShortTime:
                    boxes = parseDateFormatString(true);
                    boxes.Add(new BoxData { Type = BoxType.Separator, FormatString = " " });
                    boxes.AddRange(parseTimeFormatString(true));
                    break;
                case DateBoxFormat.ShortDateAndLongTime:
                    boxes = parseDateFormatString(true);
                    boxes.Add(new BoxData { Type = BoxType.Separator, FormatString = " " });
                    boxes.AddRange(parseTimeFormatString(false));
                    break;
                case DateBoxFormat.LongDate:
                    boxes = parseDateFormatString(false);
                    break;
                case DateBoxFormat.LongDateAndShortTime:
                    boxes = parseDateFormatString(false);
                    boxes.Add(new BoxData { Type = BoxType.Separator, FormatString = " " });
                    boxes.AddRange(parseTimeFormatString(true));
                    break;
                case DateBoxFormat.LongDateAndLongTime:
                    boxes = parseDateFormatString(false);
                    boxes.Add(new BoxData { Type = BoxType.Separator, FormatString = " " });
                    boxes.AddRange(parseTimeFormatString(false));
                    break;
                case DateBoxFormat.ShortTime:
                    boxes = parseTimeFormatString(true);
                    break;
                case DateBoxFormat.LongTime:
                    boxes = parseTimeFormatString(false);
                    break;
            }

            if (!boxes.Any()) return;

            var backGroundBinding = new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                Mode = BindingMode.OneWay
            };
            var foreGroundBinding = new Binding("Foreground")
            {
                ElementName = ElementPanel,
                Path = new PropertyPath(TextBlock.ForegroundProperty),
                Mode = BindingMode.OneWay
            };

            for (int i = 0, j = 0; i < boxes.Count; i++)
            {
                if (boxes[i].Type != BoxType.Separator)
                {
                    boxes[i].Index = j++;
                }
            }

            if (FlowDirection == System.Windows.FlowDirection.RightToLeft)
                boxes.Reverse();

            foreach (var bx in boxes)
            {
                if (bx.Type == BoxType.Separator)
                {
                    var tb = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = bx.FormatString,
                        Tag = bx
                    };
                    _Panel.Children.Add(tb);
                }
                else
                {
                    var tx = new TextBox
                    {
                        BorderThickness = new Thickness(0),
                        TextAlignment = TextAlignment.Center,
                        MaxLength = bx.FormatString.Length,
                        Cursor = Cursors.Arrow,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Tag = bx
                    };
                    if (bx.FormatString == "h" || bx.FormatString == "H" || bx.FormatString == "m" ||
                        bx.FormatString == "s" || bx.FormatString == "d" || bx.FormatString == "M" ||
                        bx.FormatString == "y")
                        tx.MaxLength = 2;

                    var textBinding = new Binding("DateValue")
                    {
                        Converter = new DateToStringPartConverter(),
                        Mode = BindingMode.OneWay,
                        RelativeSource =
                            new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DateTimePicker), 1),
                        ConverterParameter = bx.FormatString
                    };

                    tx.SetBinding(BackgroundProperty, backGroundBinding);
                    tx.SetBinding(ForegroundProperty, foreGroundBinding);
                    tx.SetBinding(TextBox.TextProperty, textBinding);
                    tx.TextChanged += TextBox_TextChanged;
                    tx.GotFocus += TextBox_GotFocus;
                    tx.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
                    tx.MouseRightButtonUp += TextBox_PreviewMouseRightButtonUp;
                    tx.PreviewKeyDown += TextBox_PreviewKeyDown;
                    _Panel.Children.Add(tx);
                }
            }
        }

        private void setDayValue(int value)
        {
            var currentDate = DateValue;
            if (value < 0 || value > 31)
            {
                applyInvalidDate(currentDate);
                return;
            }

            var sb = new StringBuilder();
            sb.Append(value);
            sb.Append(" ");
            sb.Append(
                Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(currentDate.Month));
            sb.Append(" ");
            sb.Append(currentDate.Year);
            DateTime date;
            if (!DateTime.TryParse(sb.ToString(), out date))
            {
                applyInvalidDate(currentDate);
                return;
            }
            DateValue = new DateTime(date.Year, date.Month, date.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);
            setStatus(DateValue);
        }

        private void setMonthValue(int value)
        {
            var currentDate = DateValue;

            if (value < 1 || value > 12)
            {
                applyInvalidDate(currentDate);
                return;
            }
            var sb = new StringBuilder();
            sb.Append(currentDate.Day);
            sb.Append(" ");
            sb.Append(Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(value));
            sb.Append(" ");
            sb.Append(currentDate.Year);
            DateTime date;
            if (!DateTime.TryParse(sb.ToString(), out date))
            {
                applyInvalidDate(currentDate);
                return;
            }
            DateValue = new DateTime(date.Year, date.Month, date.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);
            setStatus(DateValue);
        }

        private void setYearValue(int value)
        {
            var currentDate = DateValue;

            if (value < 1 || value > Thread.CurrentThread.CurrentCulture.Calendar.MaxSupportedDateTime.Year)
            {
                applyInvalidDate(currentDate);
                return;
            }
            var sb = new StringBuilder();
            sb.Append(currentDate.Day);
            sb.Append(" ");
            sb.Append(Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(currentDate.Month));
            sb.Append(" ");
            sb.Append(value);
            DateTime date;
            if (!DateTime.TryParse(sb.ToString(), out date))
            {
                applyInvalidDate(currentDate);
                return;
            }
            DateValue = new DateTime(date.Year, date.Month, date.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);
            setStatus(DateValue);
        }

        private void setHoursValue(int value, string formatString)
        {
            var currentDate = DateValue;

            if (value < 0 || value > 24)
            {
                applyInvalidDate(currentDate);
                return;
            }
            if (value == 24) value = 0;

            if (formatString.Contains("h"))
            {
                if (value > 12)
                {
                    value -= 12;
                }
                // deal with optional 12-hour format (preserving the AM/PM notation)
                var dateString = currentDate.ToString("dd-MMM-yyyy HH:mm:ss tt");
                var arr1 = dateString.Split(' ');
                var arr2 = arr1[1].Split(':');
                arr2[0] = value.ToString(CultureInfo.InvariantCulture);
                arr1[1] = string.Join(":", arr2);
                dateString = string.Join(" ", arr1);
                DateValue = DateTime.Parse(dateString);
            }
            else
            {
                DateValue = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, value, currentDate.Minute, currentDate.Second);
            }
            setStatus(DateValue);
        }

        private void setMinutesValue(int value)
        {
            var currentDate = DateValue;

            if (value < 0 || value > 60)
            {
                applyInvalidDate(currentDate);
                return;
            }
            if (value == 60) value = 0;
            DateValue = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, value, currentDate.Second);
            setStatus(DateValue);
        }

        private void setSecondsValue(int value)
        {
            var currentDate = DateValue;

            if (value < 0 || value > 60)
            {
                applyInvalidDate(currentDate);
                return;
            }
            if (value == 60) value = 0;
            DateValue = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, value);
            setStatus(DateValue);
        }

        private void applyInvalidDate(DateTime currentDate)
        {
            if (AllowErrorValues)
            {
                Status = DateBoxStatus.Error;
                return;
            }
            DateValue = currentDate.AddMilliseconds(1);
            DateValue = currentDate;
        }

        private void setStatus(DateTime date)
        {
            IsBlackoutDate = false;
            if (Format == DateBoxFormat.LongTime || Format == DateBoxFormat.ShortTime)
            {
                Status = DateBoxStatus.Normal;
                return;
            }
            if (_DaysOff.Any(d => d.Day == date.DayOfWeek && d.Checked))
            {
                Status = DateBoxStatus.DayOff;
                return;
            }
            var sigDate = _SignificantDates.FirstOrDefault(h => h.Date.Date == date.Date);
            if (sigDate != null)
            {
                buildHolidayInfo(sigDate);
                Status = DateBoxStatus.Significant;
                return;
            }
            if (_Calendar != null &&
                _Calendar.BlackoutDates.Any(d => d.End.Date >= date.Date && d.Start.Date <= date.Date))
                IsBlackoutDate = true;

            Status = DateBoxStatus.Normal;
        }

        private void buildHolidayInfo(SignificantDate sigDate)
        {
            if (_InfoGrid == null) return;
            _InfoGrid.Children.Clear();
            _InfoGrid.RowDefinitions.Clear();
            _InfoGrid.ColumnDefinitions.Clear();
            if (sigDate.VisualDetailsList.Count == 0)
            {
                _InfoBorder.Visibility = Visibility.Collapsed;
                return;
            }

            _InfoBorder.Visibility = Visibility.Visible;
            switch (InfoPanelOrientation)
            {
                case Orientation.Vertical:
                    _InfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    for (int i = 0; i < sigDate.VisualDetailsList.Count; i++)
                    {
                        _InfoGrid.RowDefinitions.Add(new RowDefinition());
                    }
                    break;
                case Orientation.Horizontal:
                    _InfoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    for (int i = 0; i < sigDate.VisualDetailsList.Count; i++)
                    {
                        _InfoGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                    break;
            }

            for (int i = 0; i < sigDate.VisualDetailsList.Count; i++)
            {
                switch (InfoPanelOrientation)
                {
                    case Orientation.Vertical:
                        sigDate.VisualDetailsList[i].SetValue(Grid.RowProperty, i);
                        sigDate.VisualDetailsList[i].SetValue(Grid.ColumnProperty, 0);
                        break;
                    case Orientation.Horizontal:
                        sigDate.VisualDetailsList[i].SetValue(Grid.RowProperty, 0);
                        sigDate.VisualDetailsList[i].SetValue(Grid.ColumnProperty, i);
                        break;
                }
                _InfoGrid.Children.Add(sigDate.VisualDetailsList[i]);
            }
        }

        private void jumpRight(BoxData bd)
        {
            var found = false;
            var boxes = _Panel.Children.OfType<TextBox>().ToArray();
            var orderedBoxes = from t in boxes let b = t.Tag as BoxData where b != null orderby b.Index select t;
            foreach (
                var t in
                    from t in orderedBoxes
                    let b = t.Tag as BoxData
                    where b != null
                    where b.Index > bd.Index
                    select t)
            {
                t.Focus();
                found = true;
                break;
            }
            if (!found)
            {
                boxes[0].Focus();
            }
        }

        private void jumpLeft(BoxData bd)
        {
            var found = false;
            var boxes = _Panel.Children.OfType<TextBox>().ToArray();
            var orderedBoxes = from t in boxes
                               let b = t.Tag as BoxData
                               where b != null
                               orderby b.Index descending
                               select t;
            foreach (
                var t in
                    from t in orderedBoxes
                    let b = t.Tag as BoxData
                    where b != null
                    where b.Index < bd.Index
                    select t)
            {
                t.Focus();
                found = true;
                break;
            }
            if (!found)
            {
                boxes[boxes.Length - 1].Focus();
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents significant calendar date
    /// </summary>
    public sealed class SignificantDate
    {
        private readonly List<UIElement> _VisualDetailsList = new List<UIElement>();
        /// <summary>
        /// Gets list of significant date details
        /// </summary>
        public List<UIElement> VisualDetailsList
        {
            get { return _VisualDetailsList; }
        }
        /// <summary>
        /// Gets or sets significant date date
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Creates new instance of SignificantDate
        /// </summary>
        /// <param name="date">Significant date date</param>
        public SignificantDate(DateTime date)
        {
            Date = date;
        }
        /// <summary>
        /// Creates new instance of SignificantDate
        /// </summary>
        /// <param name="date">Significant date date</param>
        /// <param name="content">UIElement with significant date details</param>
        public SignificantDate(DateTime date, UIElement content)
            : this(date)
        {
            _VisualDetailsList.Add(content);
        }
        /// <summary>
        /// Creates new instance of SignificantDate
        /// </summary>
        /// <param name="date">Significant date date</param>
        /// <param name="contentList">List of UIElements with significant date details</param>
        public SignificantDate(DateTime date, IEnumerable<UIElement> contentList)
            : this(date)
        {
            _VisualDetailsList.AddRange(contentList);
        }
    }

    /// <summary>
    /// Represents collection of SignificantDate objects
    /// </summary>
    public sealed class SignificantDatesCollection : ObservableCollection<SignificantDate>
    {
        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert.</param>
        protected override void InsertItem(int index, SignificantDate item)
        {
            var existing = this.FirstOrDefault(h => h.Date.Date == item.Date.Date);
            if (existing != null)
            {
                foreach (
                    var vd in item.VisualDetailsList.Where(vd => !existing.VisualDetailsList.Any(v => v.Equals(vd))))
                {
                    existing.VisualDetailsList.Add(vd);
                }
                return;
            }

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param><param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, SignificantDate item)
        {
            var existing = this.FirstOrDefault(h => h.Date.Date == item.Date.Date);
            if (existing != null)
            {
                foreach (
                    var vd in item.VisualDetailsList.Where(vd => !existing.VisualDetailsList.Any(v => v.Equals(vd))))
                {
                    existing.VisualDetailsList.Add(vd);
                }
                return;
            }

            base.SetItem(index, item);
        }
    }

    ///// <summary>
    ///// Converts DateTime to visibility
    ///// </summary>
    //public class DateToVisibilityConverter : IValueConverter
    //{
    //    /// <summary>
    //    /// Converts a value. 
    //    /// </summary>
    //    /// <returns>
    //    /// A converted value. If the method returns null, the valid null value is used.
    //    /// </returns>
    //    /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var d = value as DateTime?;
    //        return d == null ? Visibility.Visible : Visibility.Collapsed;
    //    }

    //    /// <summary>
    //    /// Converts a value. 
    //    /// </summary>
    //    /// <returns>
    //    /// A converted value. If the method returns null, the valid null value is used.
    //    /// </returns>
    //    /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    /// <summary>
    /// Converts DateTime to formatted string
    /// </summary>
    public class DateToStringPartConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as DateTime?;
            var p = System.Convert.ToString(parameter);
            if (!d.HasValue)
                return "";
            if (p.Length > 1)
                return d.Value.ToString(p);
            switch (p)
            {
                case "d":
                    return d.Value.Day.ToString("#0");
                case "M":
                    return d.Value.Month.ToString("#0");
                case "y":
                    return d.Value.Year.ToString("#0");
                case "h":
                    return d.Value.ToString("%h");
                case "H":
                    return d.Value.ToString("%H");
                case "m":
                    return d.Value.ToString("%m");
                case "s":
                    return d.Value.ToString("%s");
            }
            return "@";
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal static class Extensions
    {
        internal static bool In(this string str, params string[] values)
        {
            return values.Any(s => s == str);
        }

        internal static bool In(this Key key, params Key[] values)
        {
            return values.Any(k => k == key);
        }
    }
}
