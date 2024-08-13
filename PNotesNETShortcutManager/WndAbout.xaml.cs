using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PNotesNETShortcutManager
{
    /// <summary>
    /// Interaction logic for WndAbout.xaml
    /// </summary>
    public partial class WndAbout : Window
    {
        public WndAbout()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            const string caption = "About";
            Assembly ass = Assembly.GetExecutingAssembly();
            AssemblyName assName = ass.GetName();
            Version assVer = assName.Version;

            Title = caption + @" - " + assName.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append(assName.Name);
            sb.Append(" - ");
            sb.Append(assVer);
            object[] attrs = ass.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attrs.Length > 0)
            {
                sb.AppendLine();
                AssemblyDescriptionAttribute ata = attrs[0] as AssemblyDescriptionAttribute;
                if (ata != null) sb.Append(ata.Description);
            }
            attrs = ass.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attrs.Length > 0)
            {
                sb.AppendLine();
                AssemblyCopyrightAttribute ata = attrs[0] as AssemblyCopyrightAttribute;
                if (ata != null) sb.Append(ata.Copyright);
            }

            tbAbout.Text = sb.ToString();
        }
    }
}
