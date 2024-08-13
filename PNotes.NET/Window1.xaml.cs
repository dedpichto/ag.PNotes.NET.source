using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using PNRichEdit;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            initializeEdit();
        }

        private class Test
        {
            public string Name { get; set; }
            public string Timezone{ get; set; }
            public DateTime Schedule{ get; set; }
            public int Id{ get; set; }
        }

        private bool _Loaded;
        private readonly ObservableCollection<Test> _Tests = new ObservableCollection<Test>();
        private EditControl _EditControl;
        private PNRichEditBox _Edit;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _Loaded = true;
            //var di = new DirectoryInfo(@"D:\VS2013Projects\PNotes - NET - 2013\WPF\PNotes.NET\images\bigimages");
            //var files = di.GetFiles("*.png").OrderBy(f => f.Name);
            //foreach (var f in files)
            //{
            //    var sb = new StringBuilder("<BitmapImage x:Key=\"big_");
            //    sb.Append(Path.GetFileNameWithoutExtension(f.FullName));
            //    sb.Append("\" UriSource=\"pack:");
            //    sb.Append('/');
            //    sb.Append('/');
            //    sb.Append("application:,,,");
            //    sb.Append('/');
            //    sb.Append("images");
            //    sb.Append('/');
            //    sb.Append("bigimages");
            //    sb.Append('/');
            //    sb.Append(f.Name);
            //    sb.Append('"');
            //    sb.Append('/');
            //    sb.Append('>');
            //    Console.WriteLine(sb.ToString());
            //}
        }

        private void initializeEdit()
        {
            try
            {
                //_EditControl = new EditControl(brdHost);
                _Edit = _EditControl.EditBox;
                _Edit.ReadOnly = true;
                var clr = Color.FromArgb(255, 242, 221, 116);
                _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(255, clr.R, clr.G, clr.B);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
