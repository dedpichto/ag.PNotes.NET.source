using System.Windows;
using System.Windows.Controls.Primitives;

namespace WPFStandardStyles
{
    /// <summary>
    /// Represents ToolbarToggleButton
    /// </summary>
    public class ToolbarToggleButton : ToggleButton
    {
        static ToolbarToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarToggleButton), new FrameworkPropertyMetadata(typeof(ToolbarToggleButton)));
        }
    }
}
