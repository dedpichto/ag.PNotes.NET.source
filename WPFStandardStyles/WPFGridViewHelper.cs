using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFStandardStyles
{
    /// <summary>
    /// List sort direction
    /// </summary>
    public enum ListSort
    {
        /// <summary>
        /// No direction
        /// </summary>
        None,
        /// <summary>
        /// Ascending direction
        /// </summary>
        Ascending,
        /// <summary>
        /// Descending direction
        /// </summary>
        Descending
    }

    /// <summary>
    /// Represents class with various GridView helpers
    /// </summary>
    public class WPFGridViewHelper
    {
        /// <summary>
        /// Using a DependencyProperty as the backing store for ShowSortArrow
        /// </summary>
        public static readonly DependencyProperty ShowSortArrowProperty =
            DependencyProperty.RegisterAttached("ShowSortArrow", typeof(bool), typeof(WPFGridViewHelper),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Using a DependencyProperty as the backing store for SortDirection
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.RegisterAttached("SortDirection", typeof(ListSort), typeof(WPFGridViewHelper),
                new FrameworkPropertyMetadata(ListSort.None));

        /// <summary>
        /// Using a DependencyProperty as the backing store for ColumnName
        /// </summary>
        public static readonly DependencyProperty ColumnNameProperty = DependencyProperty.RegisterAttached(
            "ColumnName", typeof(string), typeof(WPFGridViewHelper), new FrameworkPropertyMetadata(""));

        /// <summary>
        /// Using a DependencyProperty as the backing store for ColumnTag
        /// </summary>
        public static readonly DependencyProperty ColumnTagProperty = DependencyProperty.RegisterAttached("ColumnTag",
            typeof(object), typeof(WPFGridViewHelper), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(WPFGridViewHelper),
                new UIPropertyMetadata(null)
            );

        /// <summary>
        /// Using a DependencyProperty as the backing store for AutoSort.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached(
                "AutoSort",
                typeof(bool),
                typeof(WPFGridViewHelper),
                new UIPropertyMetadata(
                    false,
                    (o, e) =>
                    {
                        var listView = o as ListView;
                        if (listView == null) return;
                        if (GetCommand(listView) != null) return;
                        var oldValue = (bool)e.OldValue;
                        var newValue = (bool)e.NewValue;
                        if (oldValue && !newValue)
                        {
                            listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                        if (!oldValue && newValue)
                        {
                            listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                    }
                )
            );

        /// <summary>
        /// Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(WPFGridViewHelper),
                new UIPropertyMetadata(
                    null,
                    (o, e) =>
                    {
                        var listView = o as ItemsControl;
                        if (listView == null) return;
                        if (GetAutoSort(listView)) return;
                        if (e.OldValue != null && e.NewValue == null)
                        {
                            listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                        if (e.OldValue == null && e.NewValue != null)
                        {
                            listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                    }
                )
            );

        /// <summary>
        /// Using a DependencyProperty as the backing store for ShowGridLines
        /// </summary>
        public static readonly DependencyProperty ShowGridLinesProperty =
            DependencyProperty.RegisterAttached("ShowGridLines", typeof(bool), typeof(WPFGridViewHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits ));

        /// <summary>
        /// Using a DependencyProperty as the backing store for HeaderDoubleHeight
        /// </summary>
        public static readonly DependencyProperty HeaderDoubleHeightProperty =
            DependencyProperty.RegisterAttached("HeaderDoubleHeight", typeof(bool), typeof(WPFGridViewHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets HeaderDoubleHeight property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetHeaderDoubleHeight(DependencyObject obj)
        {
            return (bool)obj.GetValue(HeaderDoubleHeightProperty);
        }
        /// <summary>
        /// Sets HeaderDoubleHeight property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetHeaderDoubleHeight(DependencyObject obj, bool value)
        {
            obj.SetValue(HeaderDoubleHeightProperty, value);
        }

        /// <summary>
        /// Gets ShowGridLines property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetShowGridLines(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowGridLinesProperty);
        }
        /// <summary>
        /// Sets ShowGridLines property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetShowGridLines(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowGridLinesProperty, value);
        }

        /// <summary>
        /// Gets SortDirection property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ListSort GetSortDirection(DependencyObject obj)
        {
            if (obj == null) return ListSort.None;
            try
            {
                var sort = obj.GetValue(SortDirectionProperty);
                if (sort == null) return ListSort.None;
                return (ListSort)sort;
            }
            catch
            {
                return ListSort.None;
            }
        }
        /// <summary>
        /// Sets SortDirection property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetSortDirection(DependencyObject obj, ListSort value)
        {
            obj.SetValue(SortDirectionProperty, value);
        }

        /// <summary>
        /// Gets ShowSortArrow property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetShowSortArrow(DependencyObject obj)
        {
            if (obj == null) return false;
            return (bool)obj.GetValue(ShowSortArrowProperty);
        }
        /// <summary>
        /// Sets ShowSortArrow property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetShowSortArrow(DependencyObject obj, bool value)
        {
            if (obj == null) return;
            obj.SetValue(ShowSortArrowProperty, value);
        }

        /// <summary>
        /// Gets ColumnTag property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetColumnTag(DependencyObject obj)
        {
            return obj.GetValue(ColumnTagProperty);
        }
        /// <summary>
        /// Sets ColumnTag property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetColumnTag(DependencyObject obj, object value)
        {
            obj.SetValue(ColumnTagProperty, value);
        }

        /// <summary>
        /// Gets ColumnName property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetColumnName(DependencyObject obj)
        {
            return (string)obj.GetValue(ColumnNameProperty);
        }
        /// <summary>
        /// Sets ColumnName property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetColumnName(DependencyObject obj, string value)
        {
            obj.SetValue(ColumnNameProperty, value);
        }

        /// <summary>
        /// Gets Command property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }
        /// <summary>
        /// Sets Command property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Gets AutoSort property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetAutoSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSortProperty);
        }
        /// <summary>
        /// Sets AutoSort property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetAutoSort(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSortProperty, value);
        }

        /// <summary>
        /// Gets PropertyName property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetPropertyName(DependencyObject obj)
        {
            if (obj == null)
                return null;
            try
            {
                var propertyName = (string)obj.GetValue(PropertyNameProperty);
                if (!string.IsNullOrEmpty(propertyName)) return propertyName;
                var viewColumn = obj as GridViewColumn;
                if (viewColumn == null) return propertyName;
                if (viewColumn.DisplayMemberBinding != null)
                {
                    propertyName = ((Binding)viewColumn.DisplayMemberBinding).Path.Path;
                }
                return propertyName;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Sets PropertyName property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked == null || headerClicked.Role == GridViewColumnHeaderRole.Padding ||
                headerClicked.Role == GridViewColumnHeaderRole.Floating) return;

            SetShowSortArrow(headerClicked, GetShowSortArrow(headerClicked.Column));

            var propertyName = GetPropertyName(headerClicked.Column);
            if (string.IsNullOrEmpty(propertyName)) return;
            var listView = GetAncestor<ListView>(headerClicked);
            if (listView == null) return;

            var grid = listView.View as GridView;

            var command = GetCommand(listView);
            if (command != null)
            {
                if (command.CanExecute(propertyName))
                {
                    command.Execute(propertyName);
                }
            }
            else if (GetAutoSort(listView))
            {
                var currentSort = ApplySort(listView.Items, propertyName);
                if (grid != null)
                {
                    foreach (
                        var h in
                            GetVisualChildren<GridViewColumnHeader>(listView)
                                .Where(h => h.Role != GridViewColumnHeaderRole.Padding && !Equals(h, headerClicked)))
                    {
                        SetSortDirection(h, ListSort.None);
                    }
                }

                SetSortDirection(headerClicked, currentSort);
            }
        }

        /// <summary>
        /// Gets ancestor of DependencyObject
        /// </summary>
        /// <typeparam name="T">DependencyObject</typeparam>
        /// <param name="reference">DependencyObject</param>
        /// <returns>The highest parent DependencyObject</returns>
        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                if (parent != null) parent = VisualTreeHelper.GetParent(parent);
            }
            return (T)parent;
        }

        /// <summary>
        /// Gets children of DependencyObject
        /// </summary>
        /// <typeparam name="T">DependencyObject</typeparam>
        /// <param name="parent">DependencyObject</param>
        /// <returns>Children of DependencyObject</returns>
        public static IEnumerable<T> GetVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var kid = child as T;
                if (kid != null)
                    yield return kid;

                foreach (var descendant in GetVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        /// <summary>
        /// Applies sort on ICollectionView
        /// </summary>
        /// <param name="view">ICollectionView</param>
        /// <param name="propertyName">Sorting criteria</param>
        /// <returns>One of <see cref="ListSort"/> members</returns>
        public static ListSort ApplySort(ICollectionView view, string propertyName)
        {
            var direction = ListSortDirection.Ascending;
            if (view.SortDescriptions.Count > 0)
            {
                var currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    direction = currentSort.Direction == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }
                view.SortDescriptions.Clear();
            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
            return direction == ListSortDirection.Ascending ? ListSort.Ascending : ListSort.Descending;
        }
    }

    /// <summary>
    /// Represents GridViewColumn with additional properties
    /// </summary>
    public class FixedWidthColumn : GridViewColumn
    {
        /// <summary>
        /// Fired when GridViewColumn visibility changed
        /// </summary>
        public event EventHandler<GridColumnVisibilityChangedEventArgs> VisibilityChanged;
        /// <summary>
        /// Fired when GridViewColumn width changed
        /// </summary>
        public event EventHandler<GridColumnWidthChangedEventArgs> WidthChanged;

        /// <summary>
        /// FixedWidth DependencyProperty
        /// </summary>
        public static readonly DependencyProperty FixedWidthProperty = DependencyProperty.Register("FixedWidth",
            typeof(double), typeof(FixedWidthColumn),
            new FrameworkPropertyMetadata(double.NaN, OnFixedWidthChanged));
        /// <summary>
        /// AllowResize DependencyProperty
        /// </summary>
        public static readonly DependencyProperty AllowResizeProperty = DependencyProperty.Register("AllowResize",
            typeof(bool), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(false));
        /// <summary>
        /// SavedWidth DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SavedWidthProperty = DependencyProperty.Register("SavedWidth",
            typeof(double), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(double.NaN));
        /// <summary>
        /// DefaultWidth DependencyProperty
        /// </summary>
        public static readonly DependencyProperty DefaultWidthProperty = DependencyProperty.Register("DefaultWidth",
            typeof(double), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(double.NaN));
        /// <summary>
        /// Visibility DependencyProperty
        /// </summary>
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility",
            typeof(Visibility), typeof(FixedWidthColumn),
            new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));
        /// <summary>
        /// DisplayIndex DependencyProperty
        /// </summary>
        public static readonly DependencyProperty DisplayIndexProperty = DependencyProperty.Register("DisplayIndex",
            typeof(int), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(0));
        /// <summary>
        /// OriginalIndex DependencyProperty
        /// </summary>
        public static readonly DependencyProperty OriginalIndexProperty = DependencyProperty.Register("OriginalIndex",
            typeof(int), typeof(FixedWidthColumn), new FrameworkPropertyMetadata(0));

        private static bool _savedAllowResize;

        static FixedWidthColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(null, OnCoerceWidth));
        }

        /// <summary>Raises the <see cref="E:System.Windows.Controls.GridViewColumn.System#ComponentModel#INotifyPropertyChanged#PropertyChanged" /> event.</summary>
        /// <param name="e">The event data.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == "ActualWidth" && Math.Abs(ActualWidth) > double.Epsilon && AllowResize)
            {
                if (WidthChanged != null) WidthChanged(this, new GridColumnWidthChangedEventArgs(ActualWidth));
            }
        }

        /// <summary>
        /// Gets or sets DefaultWidth of GridViewColumn
        /// </summary>
        public double DefaultWidth
        {
            get { return (double)GetValue(DefaultWidthProperty); }
            set { SetValue(DefaultWidthProperty, value); }
        }
        /// <summary>
        /// Gets or sets DisplayIndex of GridViewColumn
        /// </summary>
        public int DisplayIndex
        {
            get { return (int)GetValue(DisplayIndexProperty); }
            set { SetValue(DisplayIndexProperty, value); }
        }
        /// <summary>
        /// Gets or sets OriginalIndex of GridViewColumn
        /// </summary>
        public int OriginalIndex
        {
            get { return (int)GetValue(OriginalIndexProperty); }
            set { SetValue(OriginalIndexProperty, value); }
        }
        /// <summary>
        /// Gets or sets Visibility of GridViewColumn
        /// </summary>
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }
        /// <summary>
        /// Gets or sets SavedWidth of GridViewColumn
        /// </summary>
        public double SavedWidth
        {
            get { return (double)GetValue(SavedWidthProperty); }
            set { SetValue(SavedWidthProperty, value); }
        }
        /// <summary>
        /// Gets or sets AllowResize of GridViewColumn
        /// </summary>
        public bool AllowResize
        {
            get { return (bool)GetValue(AllowResizeProperty); }
            set { SetValue(AllowResizeProperty, value); }
        }
        /// <summary>
        /// Gets or sets FixedWidth of GridViewColumn
        /// </summary>
        public double FixedWidth
        {
            get { return (double)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        private static void OnVisibilityChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc == null) return;
            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                fwc.FixedWidth = fwc.SavedWidth;
                fwc.SavedWidth = fwc.FixedWidth;
                fwc.AllowResize = _savedAllowResize;
            }
            else
            {
                _savedAllowResize = fwc.AllowResize;
                fwc.FixedWidth = 0;
                fwc.AllowResize = false;
            }
            fwc.Width = fwc.ActualWidth;
            if (fwc.VisibilityChanged != null)
                fwc.VisibilityChanged(fwc,
                    new GridColumnVisibilityChangedEventArgs((Visibility)e.OldValue, (Visibility)e.NewValue));
        }

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc == null) return;
            fwc.CoerceValue(WidthProperty);
            fwc.SavedWidth = (double)e.OldValue;
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            var fwc = o as FixedWidthColumn;
            if (fwc != null)
                return !fwc.AllowResize ? fwc.FixedWidth : baseValue;
            return baseValue;
        }
    }

    /// <summary>
    /// Represents GridViewRowPresenter with additional methods and properties
    /// </summary>
    public class GridViewRowPresenterWithGridLines : GridViewRowPresenter
    {
        private readonly List<FrameworkElement> _lines = new List<FrameworkElement>();

        private IEnumerable<FrameworkElement> Children
        {
            get { return LogicalTreeHelper.GetChildren(this).OfType<FrameworkElement>(); }
        }

        /// <summary>Positions the content of a row according to the size of the corresponding <see cref="T:System.Windows.Controls.GridViewColumn" /> objects.</summary>
        /// <returns>The actual <see cref="T:System.Windows.Size" /> that is used to display the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</returns>
        /// <param name="arrangeSize">The area to use to display the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</param>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var size = base.ArrangeOverride(arrangeSize);
            var children = Children.ToList();
            var parent = FindParent<ListView>(this);
            var visibility = parent == null
                ? Visibility.Collapsed
                : (WPFGridViewHelper.GetShowGridLines(parent) ? Visibility.Visible : Visibility.Hidden);
            EnsureLines(children.Count);
            for (var i = 0; i < _lines.Count; i++)
            {
                var child = children[i];
                var x = child.TransformToAncestor(this).Transform(new Point(0, 0)).X - child.Margin.Left;
                var rect = new Rect(x, -Margin.Top, 0.75, size.Height + Margin.Top + Margin.Bottom);
                var line = _lines[i];
                line.Measure(rect.Size);
                line.Arrange(rect);
                line.Visibility = visibility;
            }
            return size;
        }

        private void EnsureLines(int count)
        {
            count = count - _lines.Count;
            var style = (Style)TryFindResource("GridLineVert");
            for (var i = 0; i < count; i++)
            {
                FrameworkElement line = new Rectangle
                {
                    Style = style
                };
                AddVisualChild(line);
                _lines.Add(line);
            }
        }

        /// <summary>Gets the number of visual children for a row. </summary>
        /// <returns>The number of visual children for the current row. </returns>
        protected override int VisualChildrenCount
        {
            get { return base.VisualChildrenCount + _lines.Count; }
        }

        /// <summary>Gets the visual child in the row item at the specified index.</summary>
        /// <returns>A <see cref="T:System.Windows.Media.Visual" /> object that contains the child at the specified index.</returns>
        /// <param name="index">The index of the child.</param>
        protected override Visual GetVisualChild(int index)
        {
            var count = base.VisualChildrenCount;
            return index < count ? base.GetVisualChild(index) : _lines[index - count];
        }

        private static T FindParent<T>(object child) where T : DependencyObject
        {
            if (!(child is DependencyObject)) return null;

            var parent = VisualTreeHelper.GetParent((DependencyObject)child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }

    #region Events
    /// <summary>
    /// Contains GridColumnWidthChanged event data
    /// </summary>
    public class GridColumnWidthChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Actual width of GridColumn
        /// </summary>
        public double ActualWidth { get; private set; }
        /// <summary>
        /// Creates new instance of GridColumnWidthChangedEventArgs
        /// </summary>
        /// <param name="width">Actual width</param>
        public GridColumnWidthChangedEventArgs(double width)
        {
            ActualWidth = width;
        }
    }
    /// <summary>
    /// Contains GridColumnVisibilityChanged event data
    /// </summary>
    public class GridColumnVisibilityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Old visibility
        /// </summary>
        public Visibility OldValue { get; private set; }
        /// <summary>
        /// New visibility
        /// </summary>
        public Visibility NewValue { get; private set; }
        /// <summary>
        /// Creates new instance of GridColumnVisibilityChangedEventArgs
        /// </summary>
        /// <param name="oldValue">Old visibility</param>
        /// <param name="newValue">New visibility</param>
        public GridColumnVisibilityChangedEventArgs(Visibility oldValue, Visibility newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
    #endregion
}
