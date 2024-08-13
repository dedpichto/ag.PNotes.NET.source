using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PNPalette
{
    /// <summary>
    /// Represents color shape
    /// </summary>
    public enum ColorShape
    {
        /// <summary>
        /// Ellipse
        /// </summary>
        Ellipse,
        /// <summary>
        /// Rectangle
        /// </summary>
        Rectangle
    }

    /// <summary>
    /// Represents a custom control that allows user to select one of 16 basic colors
    /// </summary>
    #region Named parts
    [TemplatePart(Name = ElementPanel, Type = typeof(WrapPanel))]
    #endregion
    public class Palette : Control
    {
        #region Constants
        private const string ElementPanel = "PART_Wrap";
        #endregion

        #region Elements
        private WrapPanel _Panel;
        #endregion

        private Cursor _Dropper;
        private MultiBinding _WidthBinding;

        /// <summary>
        /// The identifier of the <see cref="SelectedBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register("SelectedBrush",
            typeof(SolidColorBrush), typeof(Palette), new FrameworkPropertyMetadata(Brushes.Black, OnSelectedBrushChanged));
        /// <summary>
        /// The identifier of the <see cref="ColorShape"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorShapeProperty = DependencyProperty.Register("ColorShape",
            typeof(ColorShape), typeof(Palette), new FrameworkPropertyMetadata(ColorShape.Ellipse));
        /// <summary>
        /// Creates new instance of Palette
        /// </summary>
        static Palette()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Palette), new FrameworkPropertyMetadata(typeof(Palette)));
        }

        /// <summary>
        /// Gets or sets color shape
        /// </summary>
        public ColorShape ColorShape
        {
            get { return (ColorShape)GetValue(ColorShapeProperty); }
            set { SetValue(ColorShapeProperty, value); }
        }

        /// <summary>
        /// Gets or sets currently selected brush
        /// </summary>
        public SolidColorBrush SelectedBrush
        {
            get { return (SolidColorBrush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_Dropper == null)
            {
                var stream = Application.GetResourceStream(new Uri("pack://application:,,,/PNPalette;component/cursors/dropper.cur"));
                if (stream == null || stream.Stream == null) return;
                _Dropper = new Cursor(stream.Stream);
                Unloaded += Palette_Unloaded;
            }
            _Panel = GetTemplateChild(ElementPanel) as WrapPanel;
            if (_Panel != null && _Panel.Children.Count == 0)
            {
                setWidthBinding();
                _Panel.Children.Add(addChild(Colors.Black));
                _Panel.Children.Add(addChild(Colors.Navy));
                _Panel.Children.Add(addChild(Colors.Green));
                _Panel.Children.Add(addChild(Colors.Teal));
                _Panel.Children.Add(addChild(Colors.Maroon));
                _Panel.Children.Add(addChild(Colors.Purple));
                _Panel.Children.Add(addChild(Colors.Olive));
                _Panel.Children.Add(addChild(Colors.Silver));
                _Panel.Children.Add(addChild(Colors.Gray));
                _Panel.Children.Add(addChild(Colors.Blue));
                _Panel.Children.Add(addChild(Colors.Lime));
                _Panel.Children.Add(addChild(Colors.Cyan));
                _Panel.Children.Add(addChild(Colors.Red));
                _Panel.Children.Add(addChild(Colors.Magenta));
                _Panel.Children.Add(addChild(Colors.Yellow));
                _Panel.Children.Add(addChild(Color.FromRgb(255, 255, 254)));
            }
        }

        private void Palette_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_Dropper == null) return;
            _Dropper.Dispose();
        }

        /// <summary>
        /// Invoked just before the <see cref="SelectedBrushChangedEvent"/> event is raised on control
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        protected virtual void OnSelectedBrushChanged(SolidColorBrush oldValue, SolidColorBrush newValue)
        {
            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(oldValue, newValue)
            {
                RoutedEvent = SelectedBrushChangedEvent
            };
            RaiseEvent(e);
        }

        private static void OnSelectedBrushChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var p = sender as Palette;
            if (p == null) return;
            p.OnSelectedBrushChanged((SolidColorBrush)e.OldValue, (SolidColorBrush)e.NewValue);
        }

        /// <summary>
        /// Occurs when the <see cref="SelectedBrush"/> property has been changed in some way
        /// </summary>
        public event RoutedPropertyChangedEventHandler<SolidColorBrush> SelectedBrushChanged
        {
            add { AddHandler(SelectedBrushChangedEvent, value); }
            remove { RemoveHandler(SelectedBrushChangedEvent, value); }
        }
        /// <summary>
        /// Identifies the <see cref="SelectedBrushChanged"/> routed event
        /// </summary>
        public static readonly RoutedEvent SelectedBrushChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedBrushChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(Palette));

        private void setWidthBinding()
        {
            _WidthBinding = new MultiBinding { Converter = new ColorSpotWidthConverter(), Mode = BindingMode.OneWay };

            _WidthBinding.Bindings.Add(new Binding("ActualWidth")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(Palette) }
            });
            _WidthBinding.Bindings.Add(new Binding("BorderThickness")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(Palette) }
            });
        }

        private Shape addChild(Color clr)
        {
            var r = ColorShape == ColorShape.Ellipse
                ? new Ellipse
                {
                    Height = 16,
                    Margin = new Thickness(2),
                    Fill = new SolidColorBrush(clr),
                    Cursor = _Dropper
                }
                : new Rectangle
                {
                    Height = 16,
                    Margin = new Thickness(2),
                    Fill = new SolidColorBrush(clr),
                    Cursor = _Dropper
                } as Shape;
            r.SetBinding(Shape.StrokeProperty,
                new Binding("BorderBrush")
                {
                    RelativeSource =
                        new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(Palette) },
                    Mode = BindingMode.OneWay
                });
            r.SetBinding(WidthProperty, _WidthBinding);
            r.MouseLeftButtonDown += rectangle_MouseLeftButtonDown;
            return r;
        }

        private void rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var r = sender as Shape;
            if (r == null) return;
            SelectedBrush = r.Fill as SolidColorBrush;
            e.Handled = true;
        }
    }

    internal class ColorSpotWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is double) || !(values[1] is Thickness)) return 16;
            var lw = ((Thickness)values[1]).Left;
            var rw = ((Thickness)values[1]).Right;
            var result = ((double)values[0] - (lw + rw) - 8) / 2;
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
