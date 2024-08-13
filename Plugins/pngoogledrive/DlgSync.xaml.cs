// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace pngoogledrive
{
    /// <summary>
    /// Interaction logic for DlgSync.xaml
    /// </summary>
    public partial class DlgSync
    {
        public DlgSync(string titleText, string syncText)
        {
            InitializeComponent();
            Title = titleText;
            txtSync.Text = syncText + @" (" + titleText + @")";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(3)));
            var rt = new RotateTransform();
            imgSync.RenderTransform = rt;
            imgSync.RenderTransformOrigin = new Point(0.5, 0.5);
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
            var location = new Point(Screen.PrimaryScreen.WorkingArea.Width - ActualWidth - 4,
                                     Screen.PrimaryScreen.WorkingArea.Bottom - ActualHeight);
            Left = location.X;
            Top = location.Y;
        }

        [DllImport("user32.dll")]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        private const int GWL_STYLE = -16;

        private const uint WS_SYSMENU = 0x80000;

        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        //    SetWindowLong(hwnd, GWL_STYLE,
        //        GetWindowLong(hwnd, GWL_STYLE) & (0xFFFFFFFF ^ WS_SYSMENU));

        //    base.OnSourceInitialized(e);
        //}

    }
}
