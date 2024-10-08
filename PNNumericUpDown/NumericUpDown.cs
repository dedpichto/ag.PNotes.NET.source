﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PNNumericUpDown
{
    /// <summary>
    /// Represents custom spin control that displays numeric values 
    /// </summary>
    #region Named parts
    [TemplatePart(Name = ElementText, Type = typeof(TextBox))]
    [TemplatePart(Name = ElementButtonUp, Type = typeof(RepeatButton))]
    [TemplatePart(Name = ElementButtonDown, Type = typeof(RepeatButton))]
    #endregion
    public class NumericUpDown : Control
    {
        private enum CurrentKey
        {
            None,
            Number,
            Delete,
            Back
        }

        private struct Currentpos
        {
            public CurrentKey Key;
            public int Offset;
        }

        #region Constants
        private const string ElementText = "PART_Text";
        private const string ElementButtonUp = "PART_Up";
        private const string ElementButtonDown = "PART_Down";
        #endregion

        #region Elements
        private TextBox _Text;
        private RepeatButton _UpButton;
        private RepeatButton _DownButton;
        #endregion

        #region Dependency properties
        /// <summary>
        /// The identifier of the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty;
        /// <summary>
        /// The identifier of the <see cref="MaxValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty;
        /// <summary>
        /// The identifier of the <see cref="MinValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinValueProperty;
        /// <summary>
        /// The identifier of the <see cref="Step"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StepProperty;
        /// <summary>
        /// The identifier of the <see cref="NegativeForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeForegroundProperty;
        /// <summary>
        /// The identifier of the <see cref="IsValueNegative"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsValueNegativeProperty;
        /// <summary>
        /// The identifier of the <see cref="DecimalCount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecimalCountProperty;
        /// <summary>
        /// The identifier of the <see cref="IsReadOnly"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty;
        /// <summary>
        /// The identifier of the <see cref="UseGroupSeparator"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UseGroupSeparatorProperty;
        #endregion

        private Currentpos _Position;

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
            ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0m, OnValueChanged, ConstraintValue));
            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(100m, OnMaxValueChanged, CoerceMaximum));
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0m, OnMinValueChanged));
            StepProperty = DependencyProperty.Register("Step", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(1m, OnStepChanged, CoerceStep));
            NegativeForegroundProperty = DependencyProperty.Register("NegativeForeground", typeof(SolidColorBrush), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(Brushes.Red, OnNegativeForegroundChanged));
            IsValueNegativeProperty = DependencyProperty.Register("IsValueNegative", typeof(bool), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(false, OnIsValueNegativeChanged));
            DecimalCountProperty = DependencyProperty.Register("DecimalCount", typeof(int), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0, OnDecimalCountChanged, CoerceDecimalCount));
            IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(true, OnIsReadOnlyChanged));
            UseGroupSeparatorProperty = DependencyProperty.Register("UseGroupSeparator", typeof(bool), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(true, OnUseGroupSeparatorChanged));
        }

        #region Public properties
        /// <summary>
        /// Gets or sets the value that indicates whether group separator is used for number formatting
        /// </summary>
        public bool UseGroupSeparator
        {
            get { return (bool)GetValue(UseGroupSeparatorProperty); }
            set { SetValue(UseGroupSeparatorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether NumericUpDown is in read-only state
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value that indicates the count of decimal digits shown at NumericUpDown
        /// </summary>
        public int DecimalCount
        {
            get { return (int)GetValue(DecimalCountProperty); }
            set { SetValue(DecimalCountProperty, value); }
        }
        /// <summary>
        /// Gets the value that indicates whether th value of NumericUpDown is negative
        /// </summary>
        public bool IsValueNegative
        {
            get { return (bool)GetValue(IsValueNegativeProperty); }
            private set { SetValue(IsValueNegativeProperty, value); }
        }
        /// <summary>
        /// Gets or sets the Brush to apply to the text contents of NumericUpDown
        /// </summary>
        public SolidColorBrush NegativeForeground
        {
            get { return (SolidColorBrush)GetValue(NegativeForegroundProperty); }
            set { SetValue(NegativeForegroundProperty, value); }
        }
        /// <summary>
        /// Gets or sets the value to increment or decrement NumericUpDown when the up or down buttons are clicked
        /// </summary>
        public decimal Step
        {
            get { return (decimal)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        /// <summary>
        /// Gets or sets the minimum allowed value of NumericUpDown
        /// </summary>
        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        /// <summary>
        /// Gets or sets the maximum allowed value of NumericUpDown
        /// </summary>
        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        /// <summary>
        /// Gets or sets the value of NumericUpDown
        /// </summary>
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        #endregion

        #region Routed events
        /// <summary>
        /// Occurs when the <see cref="UseGroupSeparator"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> UseGroupSeparatorChanged
        {
            add { AddHandler(UseGroupSeparatorChangedEvent, value); }
            remove { RemoveHandler(UseGroupSeparatorChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="UseGroupSeparatorChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent UseGroupSeparatorChangedEvent = EventManager.RegisterRoutedEvent("UseGroupSeparatorChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(NumericUpDown));

        /// <summary>
        /// Occurs when the <see cref="DecimalCount"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<int> DecimalCountChanged
        {
            add { AddHandler(DecimalCountChangedEvent, value); }
            remove { RemoveHandler(DecimalCountChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="DecimalCountChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent DecimalCountChangedEvent = EventManager.RegisterRoutedEvent("DecimalCountChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<int>), typeof(NumericUpDown));

        /// <summary>
        /// Occurs when the <see cref="NegativeForeground"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<SolidColorBrush> NegativeForegroundChanged
        {
            add { AddHandler(NegativeForegroundChangedEvent, value); }
            remove { RemoveHandler(NegativeForegroundChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="NegativeForegroundChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent NegativeForegroundChangedEvent = EventManager.RegisterRoutedEvent("NegativeForegroundChanged",
            RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(NumericUpDown));
        /// <summary>
        /// Occurs when the <see cref="Step"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<decimal> StepChanged
        {
            add { AddHandler(StepChangedEvent, value); }
            remove { RemoveHandler(StepChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="StepChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent StepChangedEvent = EventManager.RegisterRoutedEvent("StepChanged",
            RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(NumericUpDown));
        /// <summary>
        /// Occurs when the <see cref="MinValue"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<decimal> MinValueChanged
        {
            add { AddHandler(MinValueChangedEvent, value); }
            remove { RemoveHandler(MinValueChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="MinValueChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent MinValueChangedEvent = EventManager.RegisterRoutedEvent("MinValueChanged",
            RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(NumericUpDown));
        /// <summary>
        /// Occurs when the <see cref="MaxValueChanged"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<decimal> MaxValueChanged
        {
            add { AddHandler(MaxValueChangedEvent, value); }
            remove { RemoveHandler(MaxValueChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="MaxValueChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent MaxValueChangedEvent = EventManager.RegisterRoutedEvent("MaxValueChanged",
            RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(NumericUpDown));
        /// <summary>
        /// Occurs when the <see cref="Value"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<decimal> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="ValueChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(NumericUpDown));
        #endregion

        #region Callback procedures
        private static void OnIsReadOnlyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        private static void OnIsValueNegativeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        /// <summary>
        /// Invoked just before the <see cref="UseGroupSeparatorChanged"/> event is raised on NumericUpDown
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnUseGroupSeparatorChanged(bool oldValue, bool newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue)
            {
                RoutedEvent = UseGroupSeparatorChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnUseGroupSeparatorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var upd = sender as NumericUpDown;
            if (upd == null) return;
            upd.OnUseGroupSeparatorChanged((bool)e.OldValue, (bool)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="NegativeForegroundChanged"/> event is raised on NumericUpDown
        /// </summary>
        /// <param name="oldValue">Old foreground</param>
        /// <param name="newValue">New foreground</param>
        protected virtual void OnNegativeForegroundChanged(SolidColorBrush oldValue, SolidColorBrush newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(oldValue, newValue)
            {
                RoutedEvent = NegativeForegroundChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnNegativeForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var upd = sender as NumericUpDown;
            if (upd == null) return;
            upd.OnNegativeForegroundChanged((SolidColorBrush)e.OldValue, (SolidColorBrush)e.NewValue);
        }
        /// <summary>
        /// Invoked just before the <see cref="DecimalCountChanged"/> event is raised on NumericUpDown
        /// </summary>
        /// <param name="oldValue">Old decimal digits count</param>
        /// <param name="newValue">New decimal digits count</param>
        protected virtual void OnDecimalCountChanged(int oldValue, int newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue)
            {
                RoutedEvent = DecimalCountChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnDecimalCountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var upd = sender as NumericUpDown;
            if (upd == null) return;
            upd.OnDecimalCountChanged((int)e.OldValue, (int)e.NewValue);
        }

        private static object CoerceDecimalCount(DependencyObject d, object value)
        {
            var upd = d as NumericUpDown;
            if (upd == null) return value;
            var fraction = upd.Step - decimal.Truncate(upd.Step);
            var count = Convert.ToInt32(value);
            count = count < 0 ? Math.Abs(count) : count;
            var arr =
                fraction.ToString("f", CultureInfo.CurrentCulture)
                    .Split(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray());
            if (arr.Length == 2 && arr[1].Length > count)
                count = arr[1].Length;
            return count;
        }
        /// <summary>
        /// Invoked just before the <see cref="StepChanged"/> event is raised on NumericUpDown
        /// </summary>
        /// <param name="oldValue">Old step</param>
        /// <param name="newValue">New step</param>
        protected virtual void OnStepChanged(decimal oldValue, decimal newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<decimal>(oldValue, newValue)
            {
                RoutedEvent = StepChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnStepChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var upd = sender as NumericUpDown;
            if (upd == null) return;
            upd.OnStepChanged(Convert.ToDecimal(e.OldValue), Convert.ToDecimal(e.NewValue));
        }

        private static object CoerceStep(DependencyObject d, object value)
        {
            var upd = d as NumericUpDown;
            if (upd == null) return value;
            var step = Convert.ToDecimal(value);
            step = step < 0 ? Math.Abs(step) : step;
            var fraction = step - decimal.Truncate(step);
            var arr =
                fraction.ToString(CultureInfo.CurrentCulture)
                    .Split(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray());
            if (arr.Length == 2 && arr[1].Length > upd.DecimalCount)
                upd.DecimalCount = arr[1].Length;
            return step;
        }
        /// <summary>
        /// Invoked just before the <see cref="MinValueChanged"/> event is raised on NumericUpDown
        /// </summary>
        /// <param name="oldValue">Old min value</param>
        /// <param name="newValue">New min value</param>
        protected virtual void OnMinValueChanged(decimal oldValue, decimal newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<decimal>(oldValue, newValue)
            {
                RoutedEvent = MinValueChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnMinValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var upd = sender as NumericUpDown;
            if (upd == null) return;
            upd.CoerceValue(MaxValueProperty);
            upd.CoerceValue(ValueProperty);
            upd.OnMinValueChanged(Convert.ToDecimal(e.OldValue), Convert.ToDecimal(e.NewValue));
        }
        /// <summary>
        /// Invoked just before the <see cref="MaxValueChanged"/> event is raised on NumericUpDown
        /// </summary>
        /// <param name="oldValue">Old max value</param>
        /// <param name="newValue">New max value</param>
        protected virtual void OnMaxValueChanged(decimal oldValue, decimal newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<decimal>(oldValue, newValue)
            {
                RoutedEvent = MaxValueChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnMaxValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var upd = sender as NumericUpDown;
            if (upd == null) return;
            upd.CoerceValue(ValueProperty);
            upd.OnMaxValueChanged(Convert.ToDecimal(e.OldValue), Convert.ToDecimal(e.NewValue));
        }

        private static object CoerceMaximum(DependencyObject d, object value)
        {
            var upd = d as NumericUpDown;
            var max = Convert.ToDecimal(value);
            if (upd == null) return value;
            return max < upd.MinValue ? upd.MinValue : value;
        }
        /// <summary>
        /// Invoked just before the <see cref="ValueChanged"/> event is raised on NumericUpDown
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnValueChanged(decimal oldValue, decimal newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<decimal>(oldValue, newValue)
            {
                RoutedEvent = ValueChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var upd = sender as NumericUpDown;
            if (upd == null) return;
            upd.IsValueNegative = (Convert.ToDecimal(e.NewValue) < 0);
            upd.OnValueChanged(Convert.ToDecimal(e.OldValue), Convert.ToDecimal(e.NewValue));
        }

        private static object ConstraintValue(DependencyObject d, object value)
        {
            var upd = d as NumericUpDown;
            var newValue = Convert.ToDecimal(value);
            if (upd == null) return value;
            if (newValue < upd.MinValue) return upd.MinValue;
            return newValue > upd.MaxValue ? upd.MaxValue : value;
        }

        #endregion

        #region Overrides
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call ApplyTemplate
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var bd = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(NumericUpDown), 1),
                Converter = new TextToDecimalConverter(),
                ConverterParameter = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            if (_Text != null)
            {
                _Text.GotFocus -= _Text_GotFocus;
                _Text.PreviewKeyDown -= _Text_PreviewKeyDown;
                _Text.PreviewMouseRightButtonUp -= _Text_PreviewMouseRightButtonUp;
                _Text.TextChanged -= _Text_TextChanged;
                BindingOperations.ClearAllBindings(_Text);
            }
            _Text = GetTemplateChild(ElementText) as TextBox;
            if (_Text != null)
            {
                _Text.GotFocus += _Text_GotFocus;
                _Text.PreviewKeyDown += _Text_PreviewKeyDown;
                _Text.PreviewMouseRightButtonUp += _Text_PreviewMouseRightButtonUp;
                _Text.TextChanged += _Text_TextChanged;
                _Text.SetBinding(TextBox.TextProperty, bd);
            }

            if (_DownButton != null)
            {
                _DownButton.Click -= _DownButton_Click;
            }
            _DownButton = GetTemplateChild(ElementButtonDown) as RepeatButton;
            if (_DownButton != null)
            {
                _DownButton.Click += _DownButton_Click;
            }

            if (_UpButton != null)
            {
                _UpButton.Click -= _UpButton_Click;
            }
            _UpButton = GetTemplateChild(ElementButtonUp) as RepeatButton;
            if (_UpButton != null)
            {
                _UpButton.Click += _UpButton_Click;
            }
        }
        #endregion

        #region Private event handlers
        private void _Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch (_Position.Key)
            {
                case CurrentKey.Number:
                    if (DecimalCount == 0 || _Text.Text.Length == 0) return;
                    var position = _Position.Offset == -1 ? 1 : _Text.Text.Length - _Position.Offset;
                    // ReSharper disable once RedundantCheckBeforeAssignment
                    if (_Text.CaretIndex != position)
                        _Text.CaretIndex = position;
                    break;
            }
        }

        private void _Text_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void _UpButton_Click(object sender, RoutedEventArgs e)
        {
            addStep(true);
            if (!_Text.IsFocused)
                _Text.Focus();
            else
                _Text.SelectAll();
        }

        private void _DownButton_Click(object sender, RoutedEventArgs e)
        {
            addStep(false);
            if (!_Text.IsFocused)
                _Text.Focus();
            else
                _Text.SelectAll();
        }

        private void _Text_GotFocus(object sender, RoutedEventArgs e)
        {
            _Text.SelectAll();
            e.Handled = true;
        }

        private void _Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                e.Handled = true;
                return;
            }
            while (true)
            {
                _Position = new Currentpos();
                switch (e.Key)
                {
                    case Key.Up:
                        addStep(true);
                        _Text.SelectAll();
                        e.Handled = true;
                        break;
                    case Key.Down:
                        addStep(false);
                        _Text.SelectAll();
                        e.Handled = true;
                        break;
                    case Key.Delete:
                        if (IsReadOnly)
                        {
                            e.Handled = true;
                            break;
                        }
                        if ((_Text.SelectionLength == _Text.Text.Length) || (_Text.CaretIndex == 0 && _Text.Text.Length == 1))
                        {
                            Value = MinValue;
                            e.Handled = true;
                            break;
                        }
                        if (_Text.CaretIndex < _Text.Text.Length)
                        {
                            if (DecimalCount > 0 &&
                                _Text.CaretIndex ==
                                _Text.Text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
                                    StringComparison.Ordinal))
                            {
                                _Text.CaretIndex++;
                                continue;
                            }
                            if (_Text.Text[_Text.CaretIndex] == CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0])
                            {
                                _Text.CaretIndex++;
                                continue;
                            }
                        }
                        break;
                    case Key.Back:
                        if (IsReadOnly)
                        {
                            e.Handled = true;
                            break;
                        }
                        if ((_Text.SelectionLength == _Text.Text.Length) || (_Text.CaretIndex == 1 && _Text.Text.Length == 1))
                        {
                            Value = MinValue;
                            e.Handled = true;
                            break;
                        }
                        if (_Text.CaretIndex != 0)
                        {
                            if (DecimalCount > 0 &&
                                _Text.CaretIndex ==
                                _Text.Text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
                                    StringComparison.Ordinal) + 1)
                            {
                                _Text.CaretIndex--;
                                continue;
                            }
                            if (_Text.Text[_Text.CaretIndex - 1] == CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0])
                            {
                                _Text.CaretIndex--;
                                continue;
                            }
                        }
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
                        if (IsReadOnly || Value == MaxValue)
                        {
                            e.Handled = true;
                        }
                        _Position.Key = CurrentKey.Number;
                        //{
                        //    var temp = _Text.Text.ToCharArray().ToList();
                        //    if (_Text.SelectionLength > 0)
                        //    {
                        //        for (var i = _Text.SelectionStart + _Text.SelectionLength - 1; i >= _Text.SelectionStart; i--)
                        //        {
                        //            temp.RemoveAt(i);
                        //        }
                        //    }
                        //    temp.Insert(_Text.SelectionStart, charFromNumberKey(e.Key));
                        //    Value = (decimal)new TextToDecimalConverter().ConvertBack(new string(temp.ToArray()), null, this,
                        //        CultureInfo.CurrentCulture);
                        //    e.Handled = true;
                        //}
                        if (DecimalCount > 0)
                        {
                            var sepPos = _Text.Text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);

                            if ((_Text.SelectionStart + _Text.SelectionLength) <= sepPos)
                            {
                                _Position.Offset = _Text.SelectionLength == _Text.Text.Length
                                    ? -1
                                    : _Text.Text.Length - (_Text.SelectionLength + _Text.SelectionStart);
                            }
                            else if (_Text.SelectionStart < sepPos && (_Text.SelectionStart + _Text.SelectionLength) > sepPos)
                            {
                                _Position.Offset = _Text.SelectionLength == _Text.Text.Length
                                    ? -1
                                    : _Text.Text.Length - (_Text.SelectionLength + _Text.SelectionStart) - 1;
                                _Text.Text = _Text.Text.Remove(_Text.SelectionStart, _Text.SelectionLength).Insert(_Text.SelectionStart, charFromNumberKey(e.Key)).Insert(_Text.SelectionStart + 1, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                e.Handled = true;
                            }
                            else if (_Text.SelectionStart > sepPos && _Text.SelectionStart < _Text.Text.Length)
                            {
                                if (_Text.SelectionLength == 0)
                                {
                                    _Position.Offset = _Text.Text.Length - _Text.SelectionStart - 1;
                                    _Text.Text = _Text.Text.Remove(_Text.SelectionStart, 1).Insert(_Text.SelectionStart, charFromNumberKey(e.Key));
                                    e.Handled = true;
                                }
                            }
                        }
                        break;
                    case Key.Tab:
                    case Key.Right:
                    case Key.Left:
                    case Key.Home:
                    case Key.End:
                    case Key.Escape:
                        break;
                    case Key.OemMinus:
                    case Key.Subtract:
                        if (IsReadOnly)
                        {
                            e.Handled = true;
                            break;
                        }
                        if ((_Text.Text.Any(c => c == '-')) || (_Text.CaretIndex > 0) ||
                            (_Text.SelectionLength == _Text.Text.Length) || (MinValue >= 0))
                        {
                            e.Handled = true;
                        }
                        break;
                    case Key.OemPeriod:
                    case Key.Decimal:
                        if (IsReadOnly)
                        {
                            e.Handled = true;
                            break;
                        }
                        if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "." && DecimalCount > 0 &&
                            _Text.CaretIndex == _Text.Text.IndexOf('.') &&
                            _Text.SelectionLength != _Text.Text.Length)
                        {
                            _Text.Select(_Text.CaretIndex + 1, 0);
                        }
                        e.Handled = true;
                        break;
                    case Key.OemComma:
                        if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == "," && DecimalCount > 0 &&
                            _Text.CaretIndex == _Text.Text.IndexOf(',') &&
                            _Text.SelectionLength != _Text.Text.Length)
                        {
                            _Text.Select(_Text.CaretIndex + 1, 0);
                        }
                        e.Handled = true;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
                break;
            }
        }
        #endregion

        #region Private procedures

        private string charFromNumberKey(Key key)
        {
            switch (key)
            {
                case Key.D0:
                case Key.NumPad0:
                    return "0";
                case Key.D1:
                case Key.NumPad1:
                    return "1";
                case Key.D2:
                case Key.NumPad2:
                    return "2";
                case Key.D3:
                case Key.NumPad3:
                    return "3";
                case Key.D4:
                case Key.NumPad4:
                    return "4";
                case Key.D5:
                case Key.NumPad5:
                    return "5";
                case Key.D6:
                case Key.NumPad6:
                    return "6";
                case Key.D7:
                case Key.NumPad7:
                    return "7";
                case Key.D8:
                case Key.NumPad8:
                    return "8";
                case Key.D9:
                case Key.NumPad9:
                    return "9";
            }
            return "";
        }

        private void addStep(bool plus)
        {
            if (plus)
            {
                if (Value + Step <= MaxValue)
                    Value += Step;
            }
            else if (Value - Step >= MinValue)
                Value -= Step;
        }
        #endregion
    }

    internal class TextToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as decimal?;
            if (d == null) return "";
            var p = parameter as NumericUpDown;
            if (p == null) return "";
            var partInt = decimal.Truncate(d.Value);
            var partFrac = decimal.Truncate((d.Value - partInt) * (int)Math.Pow(10.0, p.DecimalCount));
            var formatInt = p.UseGroupSeparator ? "#" + culture.NumberFormat.NumberGroupSeparator + "##0" : "##0";
            var formatFrac = new string('0', p.DecimalCount);
            //var p = upd != null ? upd.DecimalCount : 0;
            //return d == null ? "" : d.Value.ToString("f" + p, culture);
            return p.DecimalCount > 0 ?
                partInt.ToString(formatInt) + culture.NumberFormat.NumberDecimalSeparator + partFrac.ToString(formatFrac)
                : partInt.ToString(formatInt);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s == null)
                return null;
            if (s == "") s = "0";
            s = s.Replace(culture.NumberFormat.NumberGroupSeparator, "");
            var d = decimal.Parse(s, NumberStyles.Any);
            var upd = parameter as NumericUpDown;
            if (upd == null) return decimal.Parse(s, NumberStyles.Any);
            if (d <= upd.MaxValue)
                return d >= upd.MinValue ? d : upd.MinValue;
            return upd.MaxValue;
        }

    }

}
