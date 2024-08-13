using System.Windows;

namespace WPFStandardStyles
{
    /// <summary>
    /// Represents a bunch of additional properties and methods
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Attached DependencyProperty for showing at non-clickable menu item as group's header
        /// </summary>
        public static readonly DependencyProperty GroupDataProperty = DependencyProperty.RegisterAttached("GroupData",
            typeof(string), typeof(Utils), new FrameworkPropertyMetadata(""));
        /// <summary>
        /// Attached DependencyProperty, specified whether TreeViewItem is highlighted
        /// </summary>
        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.RegisterAttached("IsHighlighted", typeof(bool), typeof(Utils),
                new FrameworkPropertyMetadata(false));
        /// <summary>
        /// Attached DependencyProperty, specified whether big icons are shown on toolbar buttons
        /// </summary>
        public static readonly DependencyProperty IsBigIconProperty = DependencyProperty.RegisterAttached("IsBigIcon",
            typeof(bool), typeof(Utils), new FrameworkPropertyMetadata(false, null));
        /// <summary>
        /// Using a DependencyProperty as the backing store for LightSelection
        /// </summary>
        public static readonly DependencyProperty LightSelectionProperty =
            DependencyProperty.RegisterAttached("LightSelection", typeof(bool), typeof(Utils),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Attached DependencyProperty, specified whether control should catch its disabled state
        /// </summary>
        public static readonly DependencyProperty CatchDisabledStateProperty =
            DependencyProperty.RegisterAttached("CatchDisabledState", typeof(bool), typeof(Utils),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Gets CatchDisabledState property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetCatchDisabledState(DependencyObject obj)
        {
            return (bool)obj.GetValue(CatchDisabledStateProperty);
        }
        /// <summary>
        /// Sets CatchDisabledState property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetCatchDisabledState(DependencyObject obj, bool value)
        {
            obj.SetValue(CatchDisabledStateProperty, value);
        }

        /// <summary>
        /// Gets LightSelection property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetLightSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(LightSelectionProperty);
        }

        /// <summary>
        /// Sets LightSelection property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetLightSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(LightSelectionProperty, value);
        }

        /// <summary>
        /// Gets GroupData property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetGroupData(DependencyObject obj)
        {
            return (string)obj.GetValue(GroupDataProperty);
        }
        /// <summary>
        /// Sets GroupData property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetGroupData(DependencyObject obj, string value)
        {
            obj.SetValue(GroupDataProperty, value);
        }

        /// <summary>
        /// Gets IsHighlighted property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetIsHighlighted(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHighlightedProperty);
        }
        /// <summary>
        /// Sets IsHighlighted property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetIsHighlighted(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHighlightedProperty, value);
        }

        /// <summary>
        /// Gets IsBigIcon property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetIsBigIcon(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsBigIconProperty);
        }
        /// <summary>
        /// Sets IsBigIcon property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetIsBigIcon(DependencyObject obj, bool value)
        {
            obj.SetValue(IsBigIconProperty, value);
        }
    }
}
