using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNIPBox
{
    /// <summary>
    /// Represents the custom control that allows the user to enter ip address
    /// </summary>
    #region Named parts
    [TemplatePart(Name = ElementGrid, Type = typeof(Grid))]
    #endregion
    public class IpBox : Control
    {
        /// <summary>
        /// Occurs when any field of IP address is changed
        /// </summary>
        [Description("Occurs when any field of IP address is changed")]
        public event EventHandler<FieldChangedEventArgs> FieldChanged;

        #region Constants
        private const string ElementGrid = "PART_Grid";
        #endregion

        /// <summary>
        /// The identifier of the <see cref="IsBlank"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBlankProperty;
        /// <summary>
        /// The identifier of the <see cref="IsAnyBlank"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAnyBlankProperty;

        static IpBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IpBox), new FrameworkPropertyMetadata(typeof(IpBox)));
            IsBlankProperty = DependencyProperty.Register("IsBlank", typeof(bool), typeof(IpBox),
                new FrameworkPropertyMetadata(true));
            IsAnyBlankProperty = DependencyProperty.Register("IsAnyBlank", typeof(bool), typeof(IpBox),
                new FrameworkPropertyMetadata(true));
        }

        private readonly List<TextBox> _Boxes = new List<TextBox>();

        #region Elements
        private Grid _Grid;
        #endregion

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_Grid != null)
            {
                foreach (var tb in _Grid.Children.OfType<TextBox>())
                {
                    tb.PreviewKeyDown -= _Text_PreviewKeyDown;
                    tb.TextChanged -= _Text_TextChanged;
                    tb.GotFocus -= _Text_GotFocus;
                    tb.PreviewMouseRightButtonUp -= _Text_PreviewMouseRightButtonUp;
                    tb.PreviewMouseLeftButtonDown -= _Text_PreviewMouseLeftButtonDown;
                }
            }
            _Grid = GetTemplateChild(ElementGrid) as Grid;
            if (_Grid != null)
            {
                _Boxes.Clear();
                foreach (var tb in _Grid.Children.OfType<TextBox>())
                {
                    tb.PreviewKeyDown += _Text_PreviewKeyDown;
                    tb.TextChanged += _Text_TextChanged;
                    tb.GotFocus += _Text_GotFocus;
                    tb.PreviewMouseRightButtonUp += _Text_PreviewMouseRightButtonUp;
                    tb.PreviewMouseLeftButtonDown += _Text_PreviewMouseLeftButtonDown;
                    _Boxes.Add(tb);
                }
            }
        }

        void _Text_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;
            txt.Focus();
            e.Handled = true;
        }

        void _Text_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void _Text_GotFocus(object sender, RoutedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;
            txt.SelectAll();
        }

        void _Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;
            if (!string.IsNullOrWhiteSpace(txt.Text) && Convert.ToInt32(txt.Text) > 255) txt.Text = "255";
            var index = _Boxes.IndexOf(txt);
            IsBlank = !_Boxes.Any(t => t.Text.Trim().Length > 0);
            IsAnyBlank = _Boxes.Any(t => t.Text.Trim().Length == 0);
            if (FieldChanged != null)
            {
                FieldChanged(this, new FieldChangedEventArgs(index, txt.Text));
            }
            if (txt.Text.Length != txt.MaxLength) return;
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                    _Boxes[index + 1].Focus();
                    break;
                case 3:
                    _Boxes[0].Focus();
                    break;
            }
        }

        void _Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;
            switch (e.Key)
            {
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
                case Key.Delete:
                case Key.Back:
                    break;
                case Key.Tab:
                    if (txt.CaretIndex == txt.Text.Length || txt.SelectionLength == txt.Text.Length)
                    {
                        var index = _Boxes.IndexOf(txt);
                        switch (index)
                        {
                            case 0:
                            case 1:
                            case 2:
                                _Boxes[index + 1].Focus();
                                e.Handled = true;
                                break;
                            case 3:
                                break;
                        }
                    }
                    break;
                case Key.Right:
                    if (txt.CaretIndex == txt.Text.Length || txt.SelectionLength == txt.Text.Length)
                    {
                        var index = _Boxes.IndexOf(txt);
                        switch (index)
                        {
                            case 0:
                            case 1:
                            case 2:
                                _Boxes[index + 1].Focus();
                                break;
                            case 3:
                                _Boxes[0].Focus();
                                break;
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Left:
                    if (txt.CaretIndex == 0 || txt.SelectionLength == txt.Text.Length)
                    {
                        var index = _Boxes.IndexOf(txt);
                        switch (index)
                        {
                            case 1:
                            case 2:
                            case 3:
                                _Boxes[index - 1].Focus();
                                break;
                            case 0:
                                _Boxes[3].Focus();
                                break;
                        }
                        e.Handled = true;
                    }
                    break;
                default:
                    e.Handled = true;
                    break;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Clears IP address
        /// </summary>
        public void Clear()
        {
            foreach (var b in _Boxes)
                b.Clear();
        }

        /// <summary>
        /// Gets bytes array representation of IP address
        /// </summary>
        /// <returns>Bytes array representation of IP address</returns>
        public byte[] GetAddressBytes()
        {
            var bytes = new byte[4];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = _Boxes[i].Text.Trim().Length > 0 ? byte.Parse(_Boxes[i].Text.Trim()) : (byte)0;
            return bytes;
        }

        /// <summary>
        /// Sets IP address from bytes array
        /// </summary>
        /// <param name="bytes">Bytes array</param>
        public void SetAddressBytes(byte[] bytes)
        {
            if (bytes.Length != 4) return;
            for (var i = 0; i < bytes.Length; i++)
                _Boxes[i].Text = bytes[i].ToString(CultureInfo.InvariantCulture);
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets value specified whether conrol is blank
        /// </summary>
        [Browsable(false)]
        public bool IsBlank
        {
            get { return Convert.ToBoolean(GetValue(IsBlankProperty)); }
            private set { SetValue(IsBlankProperty, value); }
        }

        /// <summary>
        /// Gets value specified whether one of conrol's field is blank
        /// </summary>
        [Browsable(false)]
        public bool IsAnyBlank
        {
            get { return Convert.ToBoolean(GetValue(IsAnyBlankProperty)); }
            private set { SetValue(IsAnyBlankProperty, value); }
        }

        /// <returns>
        /// The text associated with this control.
        /// </returns>
        [Browsable(false)]
        public string Text
        {
            get
            {
                return _Boxes[0].Text.Trim() + "." + _Boxes[1].Text.Trim() + "." + _Boxes[2].Text.Trim() +
                       "." + _Boxes[3].Text.Trim();
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents event data for FieldChangedEvent
    /// </summary>
    public class FieldChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates new instance of FieldChangedEventArgs
        /// </summary>
        /// <param name="index">Filed index</param>
        /// <param name="text">Field text</param>
        public FieldChangedEventArgs(int index, string text)
        {
            FieldIndex = index;
            Text = text;
        }

        /// <summary>
        /// Changed field index
        /// </summary>
        public int FieldIndex { get; private set; }

        /// <summary>
        /// Changed field text
        /// </summary>
        public string Text { get; private set; }
    }
}
