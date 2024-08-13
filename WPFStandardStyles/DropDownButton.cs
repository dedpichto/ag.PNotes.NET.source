using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace WPFStandardStyles
{
    /// <summary>
    /// Represents DropDownButton
    /// </summary>
    public class DropDownButton : ToggleButton
    {
        /// <summary>
        /// Fired when DropDown of DropDownButton is opened
        /// </summary>
        public event EventHandler DropDownOpened;
        /// <summary>
        /// Represents DropDownButton's DropDownMenu
        /// </summary>
        public static readonly DependencyProperty DropDownMenuProperty = DependencyProperty.Register("DropDownMenu",
            typeof(ContextMenu), typeof(DropDownButton), new FrameworkPropertyMetadata(null, OnDropDownMenuChanged));

        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        /// <summary>
        /// Gets or sets DropDownButton's DropDownMenu
        /// </summary>
        public ContextMenu DropDownMenu
        {
            get { return (ContextMenu)GetValue(DropDownMenuProperty); }
            set { SetValue(DropDownMenuProperty, value); }
        }

        /// <summary>Called when a <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> raises a <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Checked" /> event.</summary>
        /// <param name="e">The event data for the <see cref="E:System.Windows.Controls.Primitives.ToggleButton.Checked" /> event.</param>
        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            if (IsChecked != null && IsChecked.Value)
            {
                if (DropDownMenu == null || DropDownMenu.Items.Count == 0)
                    IsChecked = false;
                else
                    DropDownMenu.IsOpen = true;
            }
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseRightButtonUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the right mouse button was released.</param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private static void OnDropDownMenuChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var ddb = o as DropDownButton;
            if (ddb == null) return;
            var prevDropDown = e.OldValue as ContextMenu;
            if (prevDropDown != null)
            {
                prevDropDown.Closed -= dropDown_Closed;
                prevDropDown.Opened -= dropDown_Opened;
            }
            var dropDown = e.NewValue as ContextMenu;
            if (dropDown == null) return;
            dropDown.Closed += dropDown_Closed;
            dropDown.Opened += dropDown_Opened;
            dropDown.PlacementTarget = ddb;
            dropDown.Placement = PlacementMode.Bottom;
        }

        static void dropDown_Opened(object sender, RoutedEventArgs e)
        {
            var ctm = sender as ContextMenu;
            if (ctm == null) return;
            var target = ctm.PlacementTarget as DropDownButton;
            if (target == null) return;
            if (target.DropDownOpened == null) return;
            target.DropDownOpened(target, new EventArgs());
        }

        static void dropDown_Closed(object sender, RoutedEventArgs e)
        {
            var ctm = sender as ContextMenu;
            if (ctm == null) return;
            var target = ctm.PlacementTarget as DropDownButton;
            if (target == null) return;
            target.IsChecked = false;
        }
    }
}
