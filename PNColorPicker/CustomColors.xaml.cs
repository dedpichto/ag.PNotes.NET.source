using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PNColorPicker
{
    /// <summary>
    /// Interaction logic for CustomColors.xaml
    /// </summary>
    public partial class CustomColors
    {
        #region Interop
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern void ColorRGBToHLS(uint color, ref short hue, ref short lum, ref short sat);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern uint ColorHLSToRGB(short hue, short lum, short sat); 
        #endregion

        /// <summary>
        /// The identifier of the <see cref="CustomColor"/> dependency property.
        /// </summary>
        public static DependencyProperty CustomColorProperty = DependencyProperty.Register("CustomColor", typeof(Color), typeof(CustomColors),
                new FrameworkPropertyMetadata(Colors.Silver, OnColorChanged));
        /// <summary>
        /// The identifier of the <see cref="Red"/> dependency property.
        /// </summary>
        public static DependencyProperty RedProperty = DependencyProperty.Register("Red", typeof(byte), typeof(CustomColors),
                new FrameworkPropertyMetadata((byte)192, OnRGBChanged, CoerceColor), IsValidValue);
        /// <summary>
        /// The identifier of the <see cref="Green"/> dependency property.
        /// </summary>
        public static DependencyProperty GreenProperty = DependencyProperty.Register("Green", typeof(byte), typeof(CustomColors),
                new FrameworkPropertyMetadata((byte)192, OnRGBChanged, CoerceColor), IsValidValue);
        /// <summary>
        /// The identifier of the <see cref="Blue"/> dependency property.
        /// </summary>
        public static DependencyProperty BlueProperty = DependencyProperty.Register("Blue", typeof(byte), typeof(CustomColors),
                new FrameworkPropertyMetadata((byte)192, OnRGBChanged, CoerceColor), IsValidValue);
        /// <summary>
        /// The identifier of the <see cref="Hue"/> dependency property.
        /// </summary>
        public static DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(byte), typeof(CustomColors),
                new FrameworkPropertyMetadata((byte)160, OnHSLChanged, CoerceHue));
        /// <summary>
        /// The identifier of the <see cref="Saturation"/> dependency property.
        /// </summary>
        public static DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(byte), typeof(CustomColors),
                new FrameworkPropertyMetadata((byte)0, OnHSLChanged, CoerceSatLum));
        /// <summary>
        /// The identifier of the <see cref="Luminance"/> dependency property.
        /// </summary>
        public static DependencyProperty LuminanceProperty = DependencyProperty.Register("Luminance", typeof(byte), typeof(CustomColors),
                new FrameworkPropertyMetadata((byte)181, OnHSLChanged, CoerceSatLum));

        private static bool _changeColor = true;
        private Cursor _Dropper;

        /// <summary>
        /// Creates new instance of CustomColors control
        /// </summary>
        public CustomColors()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Gets or sets the color selected in control
        /// </summary>
        public Color CustomColor
        {
            get { return (Color)GetValue(CustomColorProperty); }
            set { SetValue(CustomColorProperty, value); }
        }
        /// <summary>
        /// Gets or sets the Red component of color
        /// </summary>
        public byte Red
        {
            get { return (byte)GetValue(RedProperty); }
            set { SetValue(RedProperty, value); }
        }
        /// <summary>
        /// Gets or sets the Green component of color
        /// </summary>
        public byte Green
        {
            get { return (byte)GetValue(GreenProperty); }
            set { SetValue(GreenProperty, value); }
        }
        /// <summary>
        /// Gets or sets the Blue component of color
        /// </summary>
        public byte Blue
        {
            get { return (byte)GetValue(BlueProperty); }
            set { SetValue(BlueProperty, value); }
        }
        /// <summary>
        /// Gets or sets the color hue
        /// </summary>
        public byte Hue
        {
            get { return (byte)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }
        /// <summary>
        /// Gets or sets the color satureation
        /// </summary>
        public byte Saturation
        {
            get { return (byte)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }
        /// <summary>
        /// Gets or sets the color luminance
        /// </summary>
        public byte Luminance
        {
            get { return (byte)GetValue(LuminanceProperty); }
            set { SetValue(LuminanceProperty, value); }
        }

        private static void OnRGBChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var cc = sender as CustomColors;
            if (cc == null) return;
            var clr = cc.CustomColor;
            if (e.Property == RedProperty)
            {
                clr.R = Convert.ToByte(e.NewValue);
            }
            else if (e.Property == GreenProperty)
            {
                clr.G = Convert.ToByte(e.NewValue);
            }
            else if (e.Property == BlueProperty)
            {
                clr.B = Convert.ToByte(e.NewValue);
            }
            cc.CustomColor = clr;
        }

        private static void OnHSLChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var cc = sender as CustomColors;
            if (cc == null) return;
            var clr = cc.CustomColor;
            if (e.Property == HueProperty)
            {
                var colorref = ColorHLSToRGB(Convert.ToInt16(e.NewValue), Convert.ToInt16(cc.Luminance), Convert.ToInt16(cc.Saturation));
                clr = UIntToColor(colorref);
            }
            else if (e.Property == SaturationProperty)
            {
                var colorref = ColorHLSToRGB(Convert.ToInt16(cc.Hue), Convert.ToInt16(cc.Luminance), Convert.ToInt16(e.NewValue));
                clr = UIntToColor(colorref);
            }
            else if (e.Property == LuminanceProperty)
            {
                var colorref = ColorHLSToRGB(Convert.ToInt16(cc.Hue), Convert.ToInt16(e.NewValue), Convert.ToInt16(cc.Saturation));
                clr = UIntToColor(colorref);
            }
            if (_changeColor)
                cc.CustomColor = clr;
        }

        /// <summary>
        /// Invoked just before the <see cref="CustomColorChanged"/> event is raised on NumericUpdown
        /// </summary>
        /// <param name="oldValue">Old color value</param>
        /// <param name="newValue">New color value</param>
        protected virtual void OnCustomColorChanged(Color oldValue, Color newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue)
            {
                RoutedEvent = CustomColorChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var cc = sender as CustomColors;
            if (cc == null) return;
            var clr = (Color)e.NewValue;
            cc.Red = clr.R;
            cc.Green = clr.G;
            cc.Blue = clr.B;
            short h = 0, l = 0, s = 0;
            var colorref = BitConverter.ToUInt32(new[] { clr.B, clr.G, clr.R, clr.A }, 0);
            ColorRGBToHLS(colorref, ref h, ref l, ref s);
            _changeColor = false;
            cc.Hue = Convert.ToByte(h);
            cc.Saturation = Convert.ToByte(s);
            cc.Luminance = Convert.ToByte(l);
            _changeColor = true;
            cc.OnCustomColorChanged((Color)e.OldValue, (Color)e.NewValue);
        }

        private static object CoerceColor(DependencyObject d, object value)
        {
            var b = Convert.ToInt32(value);
            if (b < 0) return (byte)0;
            return b > 255 ? (byte)255 : (byte)b;
        }

        private static object CoerceHue(DependencyObject d, object value)
        {
            var b = Convert.ToInt32(value);
            if (b < 0) return (byte)0;
            return b > 239 ? (byte)0 : (byte)b;
        }

        private static object CoerceSatLum(DependencyObject d, object value)
        {
            var b = Convert.ToInt32(value);
            if (b < 0) return (byte)0;
            return b > 240 ? (byte)0 : (byte)b;
        }
        
        private static bool IsValidValue(object value)
        {
            var newValue = Convert.ToDecimal(value);
            return newValue <= 255;
        }

        private static Color UIntToColor(uint color)
        {
            const byte a = 255;
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Occurs when the <see cref="CustomColor"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Color> CustomColorChanged
        {
            add { AddHandler(CustomColorChangedEvent, value); }
            remove { RemoveHandler(CustomColorChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="CustomColorChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent CustomColorChangedEvent =
            EventManager.RegisterRoutedEvent("CustomColorChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Color>), typeof(CustomColors));

        private void InnerBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            while (parent != null && !(parent is Window) && !(parent is UserControl) && !(parent is Page))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            var inputElement = parent == null ? this : parent as FrameworkElement;
            var visual = parent == null ? this : parent as Visual;

            var pt = Mouse.GetPosition(inputElement);

            var source = PresentationSource.FromVisual(visual);

            double dpiX = 96.0, dpiY = 96.00;
            if (source != null && source.CompositionTarget != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }
            var bmp = new RenderTargetBitmap((int)inputElement.ActualWidth, (int)inputElement.ActualHeight, dpiX, dpiY, PixelFormats.Pbgra32);
            bmp.Render(this);

            var stride = (bmp.PixelWidth * bmp.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[bmp.PixelHeight * stride];
            var rect = new Int32Rect((int)pt.X, (int)pt.Y, 1, 1);
            bmp.CopyPixels(rect, pixels, stride, 0);

            var red = pixels[2];
            var green = pixels[1];
            var blue = pixels[0];
            var alpha = pixels[3];

            CustomColor = Color.FromArgb(alpha, red, green, blue);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var stream = Application.GetResourceStream(new Uri("pack://application:,,,/PNColorPicker;component/cursors/dropper.cur"));
            if (stream == null || stream.Stream == null) return;
            _Dropper = new Cursor(stream.Stream);
            if (_Dropper != null)
                InnerBorder.Cursor = _Dropper;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_Dropper != null)
                _Dropper.Dispose();
        }
    }
}
