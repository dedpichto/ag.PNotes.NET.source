using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PNScrollPanel
{
    /// <summary>
    /// Represents custom scroll viewer
    /// </summary>
    public class ScrollPanel : ScrollViewer
    {
        /// <summary>
        /// The identifier of the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
            typeof(Orientation), typeof(ScrollPanel), new FrameworkPropertyMetadata(Orientation.Horizontal));
        /// <summary>
        /// The identifier of the <see cref="ScrollButtonFixedSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollButtonFixedSizeProperty =
            DependencyProperty.Register("ScrollButtonFixedSize", typeof(double), typeof(ScrollPanel),
                new FrameworkPropertyMetadata(0.0));
        /// <summary>
        /// The identifier of the <see cref="ScrollButtonVerticalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollButtonVerticalAlignmentProperty =
            DependencyProperty.Register("ScrollButtonVerticalAlignment", typeof(VerticalAlignment), typeof(ScrollPanel),
                new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
        /// <summary>
        /// The identifier of the <see cref="ScrollButtonHorizontalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollButtonHorizontalAlignmentProperty =
            DependencyProperty.Register("ScrollButtonHorizontalAlignment", typeof(HorizontalAlignment), typeof(ScrollPanel),
                new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));

        static ScrollPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollPanel), new FrameworkPropertyMetadata(typeof(ScrollPanel)));
        }

        /// <summary>
        /// Definaes scroll buttons horizontal alignment
        /// </summary>
        public HorizontalAlignment ScrollButtonHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(ScrollButtonHorizontalAlignmentProperty); }
            set { SetValue(ScrollButtonHorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Defines scroll buttons vertical alignment
        /// </summary>
        public VerticalAlignment ScrollButtonVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(ScrollButtonVerticalAlignmentProperty); }
            set { SetValue(ScrollButtonVerticalAlignmentProperty, value); }
        }

        /// <summary>
        /// Defines fixed size of scroll buttons, If 0 - scroll buttons are stretched along container
        /// </summary>
        public double ScrollButtonFixedSize
        {
            get { return (double)GetValue(ScrollButtonFixedSizeProperty); }
            set { SetValue(ScrollButtonFixedSizeProperty, value); }
        }

        /// <summary>
        /// Defines orientation of ScrollPanel
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
    }

    internal class ButtonIsEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sp = value as ScrollPanel;
            if (sp == null) return false;
            var content = sp.Content as FrameworkElement;
            if (content == null) return false;
            switch (sp.Orientation)
            {
                case Orientation.Horizontal:
                    return !(sp.ActualWidth >= content.ActualWidth);
                case Orientation.Vertical:
                    return !(sp.ActualHeight >= content.ActualHeight);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ButtonFixedSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return Math.Abs((double)value) < double.Epsilon ? double.NaN : (double)value;
            }
            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
