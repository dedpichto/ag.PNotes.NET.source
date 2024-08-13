using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PNRichEdit
{
    /// <summary>
    /// Represents class for printing content of PNRichEditBox
    /// </summary>
    internal class Printing
    {
        private const Int32 WM_USER = 0x400;
        private const Int32 EM_FORMATRANGE = WM_USER + 57;
        private readonly PNRichEditBox m_Edit;

        // variable to trace text to print for pagination
        private int m_FirstCharOnPage;
        // page number
        private int m_PageNumber;
        // PNRichEditBox control
        // print range
        private PrintRange m_PrintRange = PrintRange.AllPages;

        /// <summary>
        /// Creates new instance of Printing class
        /// </summary>
        /// <param name="edit">PNRichEditBox to print</param>
        internal Printing(PNRichEditBox edit)
        {
            m_Edit = edit;
        }

        [DllImport("user32.dll")]
        private static extern Int32 SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, IntPtr lParam);
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth,
           int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, RasterOperations dwRop); 

        /// <summary>
        /// Prints visible portion of PNRichEditBox to bitmap
        /// </summary>
        /// <param name="bitmap">Bitmap to print to</param>
        internal void PrintToBitmap(Bitmap bitmap)
        {
            var grBitmap = Graphics.FromImage(bitmap);
            var hdcBitmap = grBitmap.GetHdc();
            var grEdit = m_Edit.CreateGraphics();
            var hdcEdit = grEdit.GetHdc();

            try
            {
                BitBlt(hdcBitmap, 0, 0, bitmap.Width, bitmap.Height, hdcEdit, 0, 0, RasterOperations.SRCCOPY);
            }
            finally
            {
                // Clean up
                if (hdcBitmap != IntPtr.Zero)
                    grBitmap.ReleaseHdc(hdcBitmap);
                if (hdcEdit != IntPtr.Zero)
                    grEdit.ReleaseHdc(hdcEdit);
                grEdit.Dispose();
            }

        }

        /// <summary>
        /// Prints content of PNRichEditBox
        /// </summary>
        internal void PrintRichTextContents(string documentName)
        {
            var printDoc = new PrintDocument();
            printDoc.BeginPrint += printDoc_BeginPrint;
            printDoc.PrintPage += printDoc_PrintPage;
            printDoc.EndPrint += printDoc_EndPrint;
            printDoc.DocumentName = documentName;

            var dlg = new PrintDialog { Document = printDoc };
            if (m_Edit.SelectionLength > 0)
            {
                dlg.AllowSelection = true;
            }
            if (dlg.ShowDialog(m_Edit) == DialogResult.OK)
            {
                if (dlg.PrinterSettings.PrintRange == PrintRange.Selection)
                {
                    m_PrintRange = PrintRange.Selection;
                }
                for (int i = 0; i < dlg.PrinterSettings.Copies; i++)
                {
                    // Start printing process
                    printDoc.Print();
                }
            }
        }

        private void printDoc_BeginPrint(object sender, PrintEventArgs e)
        {
            // Start at the beginning of the text
            m_FirstCharOnPage = 0;
            m_PageNumber = 0;
        }

        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            // make the PNRichEditBox calculate and render as much text as will
            // fit on the page and remember the last character printed for the
            // beginning of the next page
            if (m_PrintRange == PrintRange.AllPages)
            {
                m_FirstCharOnPage = FormatRange(false, e, m_FirstCharOnPage, m_Edit.TextLength);

                // check if there are more pages to print
                e.HasMorePages = m_FirstCharOnPage < m_Edit.TextLength;
            }
            else
            {
                m_FirstCharOnPage = FormatRange(false, e, m_Edit.SelectionStart,
                                                m_Edit.SelectionStart + m_Edit.SelectionLength);

                // check if there are more pages to print
                e.HasMorePages = m_FirstCharOnPage < m_Edit.SelectionStart + m_Edit.SelectionLength;
            }
            using (var f = new Font("Microsoft San Serif", 8, FontStyle.Regular))
            {
                // draw page number
                var fmt = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Far
                    };
                var document = sender as PrintDocument;
                if (document != null)
                {
                    SizeF szf = e.Graphics.MeasureString(document.DocumentName, f);
                    RectangleF rcf = RectangleF.FromLTRB(e.MarginBounds.Left, e.MarginBounds.Top - szf.Height - 10,
                                                         e.MarginBounds.Right, e.MarginBounds.Bottom + szf.Height + 10);
                    e.Graphics.DrawString((++m_PageNumber).ToString("0"), f, SystemBrushes.WindowText, rcf, fmt);
                    // draw document name
                    fmt.LineAlignment = StringAlignment.Near;
                    PrintDocument printDocument = document;
                    e.Graphics.DrawString(printDocument.DocumentName, f, SystemBrushes.WindowText, rcf, fmt);
                }
            }
        }

        private void printDoc_EndPrint(object sender, PrintEventArgs e)
        {
            // Clean up cached information
            FormatRangeDone();
        }

        /// <summary>
        /// Calculate or render the contents of our RichTextBox for printing
        /// </summary>
        /// <param name="measureOnly">If true, only the calculation is performed,
        /// otherwise the text is rendered as well</param>
        /// <param name="e">The PrintPageEventArgs object from the
        /// PrintPage event</param>
        /// <param name="charFrom">Index of first character to be printed</param>
        /// <param name="charTo">Index of last character to be printed</param>
        /// <returns>(Index of last character that fitted on the
        /// page) + 1</returns>
        private int FormatRange(bool measureOnly, PrintPageEventArgs e, int charFrom, int charTo)
        {
            IntPtr hdc = IntPtr.Zero;
            IntPtr lParam = IntPtr.Zero;
            // Non-Zero wParam means render, Zero means measure
            Int32 wParam = (measureOnly ? 0 : 1);

            try
            {
                // Specify which characters to print
                CHARRANGE cr;
                cr.cpMin = charFrom;
                cr.cpMax = charTo;

                // Specify the area inside page margins
                RECT rc;
                rc.top = HundredthInchToTwips(e.MarginBounds.Top);
                rc.bottom = HundredthInchToTwips(e.MarginBounds.Bottom);
                rc.left = HundredthInchToTwips(e.MarginBounds.Left);
                rc.right = HundredthInchToTwips(e.MarginBounds.Right);

                // Specify the page area
                RECT rcPage;
                rcPage.top = HundredthInchToTwips(e.PageBounds.Top);
                rcPage.bottom = HundredthInchToTwips(e.PageBounds.Bottom);
                rcPage.left = HundredthInchToTwips(e.PageBounds.Left);
                rcPage.right = HundredthInchToTwips(e.PageBounds.Right);

                // Get device context of output device
                hdc = e.Graphics.GetHdc();

                // Fill in the FORMATRANGE struct
                FORMATRANGE fr;
                fr.chrg = cr;
                fr.hdc = hdc;
                fr.hdcTarget = hdc;
                fr.rc = rc;
                fr.rcPage = rcPage;

                // Allocate memory for the FORMATRANGE struct and
                // copy the contents of our struct to this memory
                lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr));

                Marshal.StructureToPtr(fr, lParam, false);

                // Send the actual Win32 message
                int res = SendMessage(m_Edit.Handle, EM_FORMATRANGE, wParam, lParam);

                return res;
            }
            finally
            {
                SendMessage(m_Edit.Handle, EM_FORMATRANGE, wParam, lParam);
                if (lParam != IntPtr.Zero)
                {
                    // Free allocated memory
                    Marshal.FreeCoTaskMem(lParam);
                }
                if (hdc != IntPtr.Zero)
                {
                    // Release the device context
                    e.Graphics.ReleaseHdc(hdc);
                }
            }
        }

        /// <summary>
        /// Convert between 1/100 inch (unit used by the .NET framework)
        /// and twips (1/1440 inch, used by Win32 API calls)
        /// </summary>
        /// <param name="n">Value in 1/100 inch</param>
        /// <returns>Value in twips</returns>
        private Int32 HundredthInchToTwips(int n)
        {
            return (Int32)(n * 14.4);
        }

        /// <summary>
        /// Free cached data from rich edit control after printing
        /// </summary>
        private void FormatRangeDone()
        {
            var lParam = new IntPtr(0);
            SendMessage(m_Edit.Handle, EM_FORMATRANGE, 0, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHARRANGE
        {
            public Int32 cpMin;
            public Int32 cpMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FORMATRANGE
        {
            public IntPtr hdc;
            public IntPtr hdcTarget;
            public RECT rc;
            public RECT rcPage;
            public CHARRANGE chrg;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }

        public enum RasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
        }
    }
}