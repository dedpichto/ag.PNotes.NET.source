using System.Windows;
using System.Windows.Controls;

namespace WPFStandardStyles
{
    /// <summary>
    /// Represents ToolbarButton
    /// </summary>
    public class ToolbarButton : Button
    {
        static ToolbarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarButton), new FrameworkPropertyMetadata(typeof(ToolbarButton)));
        }
    }
}
