using System;
using System.Threading;
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndExport.xaml
    /// </summary>
    public partial class WndExport
    {
        public WndExport()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndExport(string fileName,string extension, string[] dates)
            : this()
        {
            _FileName = fileName;
            _Extension = extension;
            _Dates = dates;
        }

        private readonly string _FileName;
        private readonly string _Extension;
        private readonly string[] _Dates;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("export_in_progress",
                                "Export notes is in progress. The destination file is:");
                message += "\n";
                message += _FileName;
                txtExport.Text = message;
                FlowDirection = PNLang.Instance.GetFlowDirection();
                var t = new Thread(() =>
                {
                    exportNotes();
                    closeWindow();
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void CloseWindowDelegate();
        private void closeWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                CloseWindowDelegate d = closeWindow;
                Dispatcher.Invoke(d);
            }
            else
            {
                DialogResult = true;
            }
        }

        private void exportNotes()
        {
            try
            {
                switch (_Extension)
                {
                    case ".PDF":
                        PNExport.ExportNotes(ReportType.Pdf, _FileName, _Dates);
                        break;
                    case ".TIF":
                        PNExport.ExportNotes(ReportType.Tif, _FileName, _Dates);
                        break;
                    case ".DOC":
                        PNExport.ExportNotes(ReportType.Doc, _FileName, _Dates);
                        break;
                    case ".RTF":
                        PNExport.ExportNotes(ReportType.Rtf, _FileName, _Dates);
                        break;
                    case ".TXT":
                        PNExport.ExportNotes(ReportType.Txt, _FileName, _Dates);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
