using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PNColorPicker
{
    /// <summary>
    /// Represents custom control that allows users to choose color
    /// </summary>
    #region Named parts
    [TemplatePart(Name = ElementOuterBorder, Type = typeof(Border))]
    [TemplatePart(Name = ElementInnerBorder, Type = typeof(Border))]
    [TemplatePart(Name = ElementButton, Type = typeof(Button))]
    [TemplatePart(Name = ElementPopup, Type = typeof(Popup))]
    [TemplatePart(Name = ElementBasic, Type = typeof(UniformGrid))]
    [TemplatePart(Name = ElementOk, Type = typeof(Button))]
    [TemplatePart(Name = ElementTab, Type = typeof(TabControl))]
    [TemplatePart(Name = ElementList, Type = typeof(ListBox))]
    [TemplatePart(Name = ElementCustom, Type = typeof(CustomColors))]
    #endregion
    public class ColorPicker : Control
    {
        #region Arrays
        private readonly string[] _WebNames =
            {
                "Black", "DimGray", "Gray", "DarkGray", "Silver", "LightGray", "Gainsboro", "WhiteSmoke", "White",
                "RosyBrown", "IndianRed", "Brown", "FireBrick", "LightCoral", "Maroon", "DarkRed", "Red", "Snow",
                "MistyRose", "Salmon", "Tomato", "DarkSalmon", "Coral", "OrangeRed", "LightSalmon", "Sienna", "Seashell"
                ,
                "Chocolate", "SaddleBrown", "SandyBrown", "PeachPuff", "Peru", "Linen", "Bisque", "DarkOrange",
                "BurlyWood",
                "Tan", "AntiqueWhite", "NavajoWhite", "BlanchedAlmond", "PapayaWhip", "Moccasin", "Orange", "Wheat",
                "OldLace",
                "FloralWhite", "DarkGoldenrod", "Goldenrod", "Cornsilk", "Gold", "Khaki", "LemonChiffon",
                "PaleGoldenrod", "DarkKhaki",
                "Beige", "LightGoldenrodYellow", "Olive", "Yellow", "LightYellow", "Ivory", "OliveDrab", "YellowGreen",
                "DarkOliveGreen",
                "GreenYellow", "Chartreuse", "LawnGreen", "DarkSeaGreen", "ForestGreen", "LimeGreen", "LightGreen",
                "PaleGreen", "DarkGreen", "Green", "Lime", "Honeydew", "SeaGreen", "MediumSeaGreen", "SpringGreen",
                "MintCream", "MediumSpringGreen", "MediumAquamarine", "Aquamarine", "Turquoise", "LightSeaGreen",
                "MediumTurquoise",
                "DarkSlateGray", "PaleTurquoise", "Teal", "DarkCyan", "Aqua", "Cyan", "LightCyan", "Azure",
                "DarkTurquoise",
                "CadetBlue", "PowderBlue", "LightBlue", "DeepSkyBlue", "SkyBlue", "LightSkyBlue", "SteelBlue",
                "AliceBlue", "DodgerBlue",
                "SlateGray", "LightSlateGray", "LightSteelBlue", "CornflowerBlue", "RoyalBlue", "MidnightBlue",
                "Lavender",
                "Navy", "DarkBlue", "MediumBlue", "Blue", "GhostWhite", "SlateBlue", "DarkSlateBlue", "MediumSlateBlue",
                "MediumPurple", "BlueViolet", "Indigo", "DarkOrchid", "DarkViolet", "MediumOrchid", "Thistle", "Plum",
                "Violet",
                "Purple", "DarkMagenta", "Magenta", "Fuchsia", "Orchid", "MediumVioletRed", "DeepPink", "HotPink",
                "LavenderBlush", "PaleVioletRed",
                "Crimson", "Pink", "LightPink"
            };
        #endregion

        #region Constants
        private const string ElementOuterBorder = "PART_OuterBorder";
        private const string ElementInnerBorder = "PART_InnerBorder";
        private const string ElementButton = "PART_Button";
        private const string ElementPopup = "PART_Popup";
        private const string ElementBasic = "PART_Basic";
        private const string ElementOk = "PART_Ok";
        private const string ElementTab = "PART_Tab";
        private const string ElementList = "PART_Web";
        private const string ElementCustom = "PART_Custom";
        #endregion

        #region Elements
        private Button _Button, _OkButton;
        private Popup _Popup;
        private UniformGrid _Basic;
        private TabControl _Tab;
        private ListBox _ListWeb;
        private CustomColors _Custom;
        #endregion

        private bool _AllowPopup = true;
        private readonly List<WebItem> _WebItems = new List<WebItem>();
        private Color _SelectedColor;

        /// <summary>
        /// The identifier of the <see cref="SelectedColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty;

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
            SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker),
                new FrameworkPropertyMetadata(Colors.Silver, OnSelectedColorChanged));
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_Button != null)
            {
                _Button.Click -= _Button_Click;
                _Button.PreviewKeyDown -= _Button_PreviewKeyDown;
            }
            _Button = GetTemplateChild(ElementButton) as Button;
            if (_Button != null)
            {
                _Button.Click += _Button_Click;
                _Button.PreviewKeyDown += _Button_PreviewKeyDown;
            }

            if (_Popup != null)
            {
                _Popup.Opened -= _Popup_Opened;
                _Popup.LostFocus -= _Popup_LostFocus;
                _Popup.Closed -= _Popup_Closed;
                _Popup.PreviewKeyDown -= _Popup_PreviewKeyDown;
            }
            _Popup = GetTemplateChild(ElementPopup) as Popup;
            if (_Popup != null)
            {
                _Popup.Opened += _Popup_Opened;
                _Popup.LostFocus += _Popup_LostFocus;
                _Popup.Closed += _Popup_Closed;
                _Popup.PreviewKeyDown += _Popup_PreviewKeyDown;
            }

            if (_Tab != null)
            {
                _Tab.SelectionChanged -= _Tab_SelectionChanged;
            }
            _Tab = GetTemplateChild(ElementTab) as TabControl;
            if (_Tab != null)
            {
                _Tab.SelectionChanged += _Tab_SelectionChanged;
            }

            if (_Basic != null)
            {
                foreach (var radio in _Basic.Children.OfType<RadioButton>())
                {
                    radio.MouseDoubleClick -= radio_MouseDoubleClick;
                    radio.Checked -= radio_Checked;
                }
            }
            _Basic = GetTemplateChild(ElementBasic) as UniformGrid;
            if (_Basic != null)
            {
                foreach (var radio in _Basic.Children.OfType<RadioButton>())
                {
                    radio.MouseDoubleClick += radio_MouseDoubleClick;
                    radio.Checked += radio_Checked;
                }
            }

            if (_ListWeb != null)
            {
                _ListWeb.SelectionChanged -= _ListWeb_SelectionChanged;
                _ListWeb.MouseDoubleClick -= _ListWeb_MouseDoubleClick;
            }
            _ListWeb = GetTemplateChild(ElementList) as ListBox;
            if (_ListWeb != null)
            {
                _ListWeb.SelectionChanged += _ListWeb_SelectionChanged;
                _ListWeb.MouseDoubleClick += _ListWeb_MouseDoubleClick;
            }

            if (_OkButton != null)
            {
                _OkButton.Click -= _OkButton_Click;
            }
            _OkButton = GetTemplateChild(ElementOk) as Button;
            if (_OkButton != null)
            {
                _OkButton.Click += _OkButton_Click;
            }

            if (_Custom != null)
            {
                _Custom.CustomColorChanged -= _Custom_CustomColorChanged;
            }
            _Custom = GetTemplateChild(ElementCustom) as CustomColors;
            if (_Custom != null)
            {
                _Custom.CustomColorChanged += _Custom_CustomColorChanged;
            }
            fillListWeb();
        }

        void _Custom_CustomColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            _SelectedColor = _Custom.CustomColor;
            setBasicColor();
            setWebColor();
            _Custom.CustomColor = _SelectedColor;
        }

        void _Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_Tab.SelectedIndex)
            {
                case 0:
                    _OkButton.IsEnabled =
                        _Basic.Children.OfType<RadioButton>().Any(r => r.IsChecked.HasValue && r.IsChecked.Value);
                    break;
                case 1:
                    _OkButton.IsEnabled = _ListWeb.SelectedIndex >= 0;
                    break;
                case 2:
                    _OkButton.IsEnabled = true;
                    break;
            }
        }

        void _ListWeb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _OkButton.PerformClick();
        }

        void _ListWeb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var item = e.AddedItems[0] as WebItem;
            if (item == null) return;
            _SelectedColor = item.Color;
            setBasicColor();
            _Custom.CustomColor = _SelectedColor;
        }

        void radio_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio == null) return;
            if (!radio.IsChecked.HasValue || !radio.IsChecked.Value) return;
            var brush = radio.Background as SolidColorBrush;
            if (brush == null) return;
            _SelectedColor = brush.Color;
            setWebColor();
            _Custom.CustomColor = _SelectedColor;
        }

        void radio_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _OkButton.PerformClick();
        }

        void _OkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_Tab.SelectedIndex)
            {
                case 0:
                    if (_Basic.Children.OfType<RadioButton>().Any(r => r.IsChecked.HasValue && r.IsChecked.Value))
                    {
                        SelectedColor = _SelectedColor;
                        _Popup.IsOpen = false;
                    }
                    break;
                case 1:
                    var item = _ListWeb.SelectedItem as WebItem;
                    if (item != null)
                    {
                        SelectedColor = _SelectedColor;
                        _Popup.IsOpen = false;
                    }
                    break;
                case 2:
                    SelectedColor = _Custom.CustomColor;
                    _Popup.IsOpen = false;
                    break;
            }
        }

        void _Popup_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                _Popup.IsOpen = false;
        }

        void _Popup_Closed(object sender, EventArgs e)
        {
            var button = Mouse.Captured as Button;
            if (button != null && button.Name == ElementButton)
            {
                _AllowPopup = false;
            }
        }

        void _Popup_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (_Popup.IsOpen) return;
            //var button = Mouse.Captured as Button;
            //if (button != null && button.Name == ElementButton)
            //{
            //    _AllowPopup = false;
            //}
        }

        void _Popup_Opened(object sender, EventArgs e)
        {
            var foundBasic = setBasicColor();
            var foundWeb = setWebColor();
            _Custom.CustomColor = _SelectedColor;
            if (foundBasic)
                _Tab.SelectedIndex = 0;
            else if (foundWeb)
                _Tab.SelectedIndex = 1;
            else
                _Tab.SelectedIndex = 2;
        }

        void _Button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                _Popup.IsOpen = false;
        }

        void _Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_AllowPopup)
            {
                _AllowPopup = true;
                return;
            }
            _Popup.IsOpen = true;
        }

        /// <summary>
        /// Gets or sets the color selected in control
        /// </summary>
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        /// <summary>
        /// Invoked just before the <see cref="SelectedColorChanged"/> event is raised on NumericUpdown
        /// </summary>
        /// <param name="oldValue">Old color value</param>
        /// <param name="newValue">New color value</param>
        protected virtual void OnSelectedColorChanged(Color oldValue, Color newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue)
            {
                RoutedEvent = SelectedColorChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnSelectedColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var cp = sender as ColorPicker;
            if (cp == null) return;
            cp._SelectedColor = (Color)e.NewValue;
            cp.OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
        }

        /// <summary>
        /// Occurs when the <see cref="SelectedColor"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
        {
            add { AddHandler(SelectedColorChangedEvent, value); }
            remove { RemoveHandler(SelectedColorChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="SelectedColorChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent SelectedColorChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedColorChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ColorPicker));

        private bool setBasicColor()
        {
            foreach (var r in _Basic.Children.OfType<RadioButton>())
                r.IsChecked = false;
            foreach (var r in from rd in _Basic.Children.OfType<RadioButton>()
                              let brush = rd.Background as SolidColorBrush
                              where brush != null && brush.Color == _SelectedColor
                              select rd)
            {
                r.IsChecked = true;
                return true;
            }
            return false;
        }

        private bool setWebColor()
        {
            foreach (var li in _ListWeb.Items.OfType<WebItem>().Where(li => li.Color == _SelectedColor))
            {
                _ListWeb.SelectedItem = li;
                _ListWeb.ScrollIntoView(_ListWeb.SelectedItem);
                return true;
            }
            _ListWeb.SelectedIndex = -1;
            return false;
        }

        private void fillListWeb()
        {
            if (_ListWeb.ItemsSource != null) return;
            foreach (var s in _WebNames)
            {
                _WebItems.Add(new WebItem(s));
            }
            _ListWeb.ItemsSource = _WebItems;
        }
    }

    internal static class Extensions
    {
        internal static void PerformClick(this Button b)
        {
            b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }

    /// <summary>
    /// Represents class for inverting color of solid brush
    /// </summary>
    public class InvertColorConverter : IValueConverter
    {
        /// <summary>
        /// Inverts color of solid brush
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SolidColorBrush)) return value;
            var clr = ((SolidColorBrush)value).Color;
            return Color.FromArgb(255, (byte)~clr.R, (byte)~clr.G, (byte)~clr.B);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents listbox item for web colors
    /// </summary>
    public class WebItem : ListBoxItem
    {
        /// <summary>
        /// Gets the color of item
        /// </summary>
        public Color Color { get; private set; }
        /// <summary>
        /// Creates new instance of WebItem
        /// </summary>
        /// <param name="colorName">Color name</param>
        public WebItem(string colorName)
        {
            var o = ColorConverter.ConvertFromString(colorName);
            if (o != null)
            {
                var clr = (Color)o;
                var st = new StackPanel { Orientation = Orientation.Horizontal };
                var rect = new Rectangle
                {
                    Fill = new SolidColorBrush(clr),
                    Width = 20,
                    Height = 16,
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var tb = new TextBlock
                {
                    Text = colorName,
                    Margin = new Thickness(8, 2, 2, 2),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                st.Children.Add(rect);
                st.Children.Add(tb);
                Color = clr;
                VerticalContentAlignment = VerticalAlignment.Center;
                Content = st;
            }
        }
    }
}
