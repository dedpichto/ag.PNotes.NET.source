using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using PNStaticFonts;
using tom;

namespace PNRichEdit
{
    /// <summary>
    /// Specifies paragraph point for adding/removing space
    /// </summary>
    public enum REParaSpace
    {
        /// <summary>
        /// Space before paragraph
        /// </summary>
        Before,
        /// <summary>
        /// Space after paragraph
        /// </summary>
        After
    }

    /// <summary>
    /// Specifies line spacing type
    /// </summary>
    public enum RELineSpacing
    {
        /// <summary>
        /// Space type undefined
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// Single space
        /// </summary>
        Single = 0,
        /// <summary>
        /// One-and-half space
        /// </summary>
        OneAndHalf = 1,
        /// <summary>
        /// Double space
        /// </summary>
        Double = 2,
        /// <summary>
        /// Triple space
        /// </summary>
        Triple = 5
    }

    /// <summary>
    /// Bullets numbering types
    /// </summary>
    public enum RENumbering
    {
        /// <summary>
        /// no bullets
        /// </summary>
        PFN_NONE = 0,
        /// <summary>
        /// simple bullets
        /// </summary>
        PFN_BULLET = 1,
        /// <summary>
        /// arabic numbers
        /// </summary>
        PFN_ARABIC = 2,
        /// <summary>
        /// lower case letters
        /// </summary>
        PFN_LCLETTER = 3,
        /// <summary>
        /// upper case letters
        /// </summary>
        PFN_UCLETTER = 4,
        /// <summary>
        /// lower case roman numbers
        /// </summary>
        PFN_LCROMAN = 5,
        /// <summary>
        /// upper case roman numbers
        /// </summary>
        PFN_UCROMAN = 6
    }
    /// <summary>
    /// Bullets numbering styles
    /// </summary>
    public enum RENumberingStyle
    {
        /// <summary>
        /// one side parentheses
        /// </summary>
        PFNS_PAREN = 0x000,
        /// <summary>
        /// two sides parentheses
        /// </summary>
        PFNS_PARENS = 0x100,
        /// <summary>
        /// period
        /// </summary>
        PFNS_PERIOD = 0x200,
        /// <summary>
        /// plain
        /// </summary>
        PFNS_PLAIN = 0x300,
        /// <summary>
        /// no number
        /// </summary>
        PFNS_NONUMBER = 0x400
    }
    /// <summary>
    /// Font decoration styles
    /// </summary>
    [Flags]
    public enum REFDecorationStyle : uint
    {
        /// <summary>
        /// no decoration
        /// </summary>
        CFE_NONE = 0x0000,
        /// <summary>
        /// bold
        /// </summary>
        CFE_BOLD = 0x0001,
        /// <summary>
        /// italic
        /// </summary>
        CFE_ITALIC = 0x0002,
        /// <summary>
        /// underline
        /// </summary>
        CFE_UNDERLINE = 0x0004,
        /// <summary>
        /// strikeout
        /// </summary>
        CFE_STRIKEOUT = 0x0008,
        /// <summary>
        /// subscript
        /// </summary>
        CFE_SUBSCRIPT = 0x00010000,
        /// <summary>
        /// superscript
        /// </summary>
        CFE_SUPERSCRIPT = 0x00020000
    }
    /// <summary>
    /// Font decoration masks
    /// </summary>
    [Flags]
    public enum REFDecorationMask : uint
    {
        /// <summary>
        /// bold
        /// </summary>
        CFM_BOLD = 0x00000001,
        /// <summary>
        /// italic
        /// </summary>
        CFM_ITALIC = 0x00000002,
        /// <summary>
        /// underline
        /// </summary>
        CFM_UNDERLINE = 0x00000004,
        /// <summary>
        /// strikeout
        /// </summary>
        CFM_STRIKEOUT = 0x00000008,
        /// <summary>
        /// subscript
        /// </summary>
        CFM_SUBSCRIPT = (REFDecorationStyle.CFE_SUBSCRIPT | REFDecorationStyle.CFE_SUPERSCRIPT),
        /// <summary>
        /// superscript
        /// </summary>
        CFM_SUPERSCRIPT = CFM_SUBSCRIPT
    }

    /// <summary>
    /// Represents a class inherited from Windows.RichTextBox
    /// </summary>
    public partial class PNRichEditBox : RichTextBox
    {
        /// <summary>
        /// Occurs when control is activated by mouse
        /// </summary>
        public event PNREActivatedByMouseEventHandler PNREActivatedByMouse;

        /// <summary>
        /// Creates new instance of PNRichEditBox
        /// </summary>
        public PNRichEditBox()
        {
            InitializeComponent();
        }

        private bool m_CheckSpellingAutomatically;

        /// <summary>
        /// Specifies whether spell checking should be performed automatically
        /// </summary>
        [Browsable(false)]
        public bool CheckSpellingAutomatically
        {
            get { return m_CheckSpellingAutomatically; }
            set
            {
                bool temp = m_CheckSpellingAutomatically;
                m_CheckSpellingAutomatically = value;
                if (!temp && Spellchecking.Initialized)
                {
                    using (Graphics g = CreateGraphics())
                    {
                        Spellchecking.CheckRESpellingAutomatically(this, g);
                    }
                }
            }
        }

        /// <summary>
        /// Processes Windows messages
        /// </summary>
        /// <param name="m">The Windows Message to process</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_PAINT:
                    base.WndProc(ref m);
                    if (Spellchecking.Initialized && m_CheckSpellingAutomatically)
                    {
                        using (Graphics g = CreateGraphics())
                        {
                            Spellchecking.CheckRESpellingAutomatically(this, g);
                        }
                    }
                    return;
                case WM_MOUSEACTIVATE:
                    if (PNREActivatedByMouse != null)
                    {
                        PNREActivatedByMouse(this, new PNREActivatedByMouseEventArgs(PointToClient(Cursor.Position)));
                    }
                    m.Result = new IntPtr(MA_ACTIVATE);
                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr LoadLibrary(string lpFileName);
        /// <summary>
        /// Gets the required creation parameters when the control handle is created
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var p = base.CreateParams;
                p.ExStyle |= WS_EX_TRANSPARENT;
                if (!DesignMode)
                {
                    if (LoadLibrary("msftedit.dll") != IntPtr.Zero)
                    {
                        p.ClassName = "RICHEDIT50W";
                    }
                }
                return p;
            }
        }

        /// <summary>
        /// Raises the OnMouseUp event
        /// </summary>
        /// <param name="mevent">A MouseEventArgs that contains the event data</param>
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Right)
            {
                if (Spellchecking.Initialized && m_CheckSpellingAutomatically)
                {
                    if (!Spellchecking.CheckRESpellingOnRightClick(this))
                    {
                        base.OnMouseUp(mevent);
                    }
                }
                else
                {
                    base.OnMouseUp(mevent);
                }
            }
            else
            {
                base.OnMouseUp(mevent);
            }
        }

        /// <summary>
        /// Sets left and right margins size of PNRichEditBox
        /// </summary>
        /// <param name="marginSize">Margin size</param>
        public void SetMargins(short marginSize)
        {
            RichEdit_SetMargins(new HandleRef(this, Handle), EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, MAKELONG(marginSize, marginSize));
        }

        /// <summary>
        /// Sets paragraph spacing of PNRichEditBox
        /// </summary>
        /// <param name="para">Paragraph side</param>
        /// <param name="space">Space applied (in points, will be multiplied by 20 to get twips)</param>
        public void SetParagraphSpacing(REParaSpace para, int space)
        {
            var pf = new PARAFORMAT2 { dwMask = (para == REParaSpace.Before ? PFM_SPACEBEFORE : PFM_SPACEAFTER) };
            if (para == REParaSpace.Before)
                pf.dySpaceBefore = space * 20;
            else
                pf.dySpaceAfter = space * 20;
            RichEdit_SetParaFormat(new HandleRef(this, Handle), EM_SETPARAFORMAT, 0, pf);
        }

        /// <summary>
        /// Sets line spacing of PNRichEditBox
        /// </summary>
        /// <param name="spacing">Line spacing type</param>
        public void SetLineSpacing(RELineSpacing spacing)
        {
            var pf = new PARAFORMAT2 { dwMask = (PFM_LINESPACING), bLineSpacingRule = 5 };
            switch (spacing)
            {
                case RELineSpacing.Single:
                    pf.dyLineSpacing = 20;
                    break;
                case RELineSpacing.OneAndHalf:
                    pf.dyLineSpacing = 30;
                    break;
                case RELineSpacing.Double:
                    pf.dyLineSpacing = 40;
                    break;
                case RELineSpacing.Triple:
                    pf.dyLineSpacing = 60;
                    break;
                default:
                    pf.dyLineSpacing = 20;
                    break;
            }   
            RichEdit_SetParaFormat(new HandleRef(this, Handle), EM_SETPARAFORMAT, 0, pf);
        }

        /// <summary>
        /// Gets line spacing of PNRichEditBox
        /// </summary>
        /// <returns>Line spacing of PNRichEditBox</returns>
        public RELineSpacing GetLineSpacing()
        {
            var pf = new PARAFORMAT2 { dwMask = (PFM_LINESPACING) };
            RichEdit_GetParaFormat(new HandleRef(this, Handle), EM_GETPARAFORMAT, 0, pf);
            switch (pf.dyLineSpacing)
            {
                case 20:
                    return RELineSpacing.Single;
                case 30:
                    return RELineSpacing.OneAndHalf;
                case 40:
                    return RELineSpacing.Double;
                case 60:
                    return pf.dyLineSpacing == 60 ? RELineSpacing.Triple : RELineSpacing.Undefined;
            }
            return RELineSpacing.Undefined;
        }

        /// <summary>
        /// Gets dictionary with values specified whether PNRichEditBox paragraph has space before and/or after
        /// </summary>
        /// <returns>Dictionary with values specified whether PNRichEditBox paragraph has space before and/or after</returns>
        public Dictionary<REParaSpace, bool> GetParagraphSpacing()
        {
            var result = new Dictionary<REParaSpace, bool> { { REParaSpace.Before, false }, { REParaSpace.After, false } };
            var pf = new PARAFORMAT2 { dwMask = (PFM_SPACEAFTER | PFM_SPACEBEFORE) };
            RichEdit_GetParaFormat(new HandleRef(this, Handle), EM_GETPARAFORMAT, 0, pf);
            result[REParaSpace.Before] = pf.dySpaceBefore > 0;
            result[REParaSpace.After] = pf.dySpaceAfter > 0;
            return result;
        }

        /// <summary>
        /// Gets current bullet style of PNRichEditBox
        /// </summary>
        /// <returns>Current bullet style of PNRichEditBox</returns>
        public int CurrentBulletStyle()
        {
            var pf = new PARAFORMAT2 { dwMask = (PFM_NUMBERING | PFM_NUMBERINGSTYLE) };

            RichEdit_GetParaFormat(new HandleRef(this, Handle), EM_GETPARAFORMAT, 0, pf);
            if (pf.wNumbering == (int)RENumbering.PFN_BULLET)
                return 1;
            if (pf.wNumbering == (int)RENumbering.PFN_ARABIC && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PERIOD)
                return 2;
            if (pf.wNumbering == (int)RENumbering.PFN_ARABIC && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PAREN)
                return 3;
            if (pf.wNumbering == (int)RENumbering.PFN_LCLETTER && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PERIOD)
                return 4;
            if (pf.wNumbering == (int)RENumbering.PFN_LCLETTER && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PAREN)
                return 5;
            if (pf.wNumbering == (int)RENumbering.PFN_UCLETTER && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PERIOD)
                return 6;
            if (pf.wNumbering == (int)RENumbering.PFN_UCLETTER && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PAREN)
                return 7;
            if (pf.wNumbering == (int)RENumbering.PFN_LCROMAN && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PERIOD)
                return 8;
            if (pf.wNumbering == (int)RENumbering.PFN_UCROMAN && pf.wNumberingStyle == (int)RENumberingStyle.PFNS_PERIOD)
                return 9;
            return 0;
        }

        /// <summary>
        /// Clears all bullets from PNRichEditBox
        /// </summary>
        public void ClearBullets()
        {
            var pf = new PARAFORMAT2
                {
                    dwMask = PFM_NUMBERING | PFM_NUMBERINGSTYLE | PFM_OFFSET | PFM_NUMBERINGSTART | PFM_NUMBERINGTAB
                };

            RichEdit_SetParaFormat(new HandleRef(this, Handle), EM_SETPARAFORMAT, 0, pf);
        }

        /// <summary>
        /// Sets bullets on PNRichEditBox using selected type, style and default indent (400 twips)
        /// </summary>
        /// <param name="numbering">Numbering type</param>
        /// <param name="style">Bullets style</param>
        public void SetBullets(RENumbering numbering, RENumberingStyle style)
        {
            SetBullets(numbering, style, 400);
        }

        /// <summary>
        /// Sets bullets on PNRichEditBox using selected type, style and indent
        /// </summary>
        /// <param name="numbering">Numbering type</param>
        /// <param name="style">Bullets style</param>
        /// <param name="indent">Bullets indent (in twips)</param>
        public void SetBullets(RENumbering numbering, RENumberingStyle style, short indent)
        {
            var pf = new PARAFORMAT2();
            var set = false;

            pf.dwMask = (PFM_NUMBERING | PFM_NUMBERINGSTYLE);
            RichEdit_GetParaFormat(new HandleRef(this, Handle), EM_GETPARAFORMAT, 0, pf);
            if (pf.wNumbering == (int)numbering && pf.wNumberingStyle == (int)style)
                set = true;
            pf = new PARAFORMAT2();
            if (numbering == RENumbering.PFN_BULLET)
            {
                pf.dwMask = PFM_NUMBERING | PFM_OFFSET | PFM_NUMBERINGTAB;
                if (!set)
                {
                    pf.wNumbering = (short)numbering;
                    pf.wNumberingTab = indent;
                    pf.dxOffset = pf.wNumberingTab;
                }
            }
            else
            {
                pf.dwMask = PFM_NUMBERING | PFM_NUMBERINGSTYLE | PFM_OFFSET | PFM_NUMBERINGSTART | PFM_NUMBERINGTAB;
                if (!set)
                {
                    pf.wNumbering = (short)numbering;
                    pf.wNumberingStyle = (short)style;
                    pf.wNumberingTab = indent;
                    pf.dxOffset = pf.wNumberingTab;
                    pf.wNumberingStart = 1;

                }
            }
            RichEdit_SetParaFormat(new HandleRef(this, Handle), EM_SETPARAFORMAT, 0, pf);
        }

        /// <summary>
        /// Sets paragraph indent
        /// </summary>
        /// <param name="indent">Paragraph indent (negative to decrease)</param>
        public void SetIndent(int indent)
        {
            var pf = new PARAFORMAT2 {dwMask = PFM_OFFSET | PFM_OFFSETINDENT, dxOffset = indent, dxStartIndent = indent};
            RichEdit_SetParaFormat(new HandleRef(this, Handle), EM_SETPARAFORMAT, 0, pf);
        }

        /// <summary>
        /// Sets font size of PNRichEditBox
        /// </summary>
        /// <param name="size">Font size</param>
        public void SetFontSize(int size)
        {
            var cf = new CHARFORMAT2 { dwMask = CFM_SIZE };

            using (Graphics g = CreateGraphics())
            {
                IntPtr hdc = g.GetHdc();
                size = -MulDiv(size, GetDeviceCaps(hdc, LOGPIXELSY), 72);
                cf.yHeight = 20 * -(size * 72) / GetDeviceCaps(hdc, LOGPIXELSY);
                RichEdit_SetCharFormat(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, cf);
                g.ReleaseHdc(hdc);
            }
        }

        /// <summary>
        /// Gets text width for specified positions
        /// </summary>
        /// <param name="start">Starting position of text</param>
        /// <param name="end">Ending position of text</param>
        /// <returns>Text width for specified positions</returns>
        public int GetTextWidth(int start, int end)
        {
            int width = 0;
            IntPtr iRichOle;

            if (RichEdit_GetOleInterface(new HandleRef(this, Handle), EM_GETOLEINTERFACE, 0, out iRichOle))
            {
                var iDoc = (ITextDocument)Marshal.GetTypedObjectForIUnknown(iRichOle, typeof(ITextDocument));
                if (iDoc != null)
                {
                    ITextRange iRange = iDoc.Range(start, end - 1);
                    if (iRange != null)
                    {
                        var pts = new POINT[2];

                        iRange.GetPoint(32 | TA_BASELINE | TA_LEFT, out pts[0].x, out pts[0].y);
                        iRange.GetPoint(0 | TA_BASELINE | TA_RIGHT, out pts[1].x, out pts[1].y);
                        Point pt1 = PointToClient(new Point(pts[0].x, pts[0].y + 1));
                        Point pt2 = PointToClient(new Point(pts[1].x, pts[1].y + 1));
                        if (pt1.X < pt2.X && pt1.X >= 0 && pt1.Y >= 0 && pt2.X >= 0 && pt2.Y >= 0)
                        {
                            width = pt2.X - pt1.X;
                        }
                    }
                }
                Marshal.Release(iRichOle);
            }
            return width;
        }

        /// <summary>
        /// Gets current font size of PNRichEditBox
        /// </summary>
        /// <returns>Current font size</returns>
        public int GetFontSize()
        {
            var cf = new CHARFORMAT2();
            int height;

            cf.dwMask = CFM_SIZE;
            RichEdit_GetCharFormat(new HandleRef(this, Handle), EM_GETCHARFORMAT, SCF_SELECTION, cf);
            using (Graphics g = CreateGraphics())
            {
                IntPtr hdc = g.GetHdc();
                height = -(cf.yHeight * GetDeviceCaps(hdc, LOGPIXELSY)) / (20 * 72);
                height = -MulDiv(height, 72, GetDeviceCaps(hdc, LOGPIXELSY));
                g.ReleaseHdc(hdc);
            }

            return height;
        }

        /// <summary>
        /// Gets current font name of PNRichEditBox
        /// </summary>
        /// <returns>Current font name</returns>
        public string GetFontName()
        {
            var cf = new CHARFORMAT2();
            var sb = new StringBuilder();

            cf.dwMask = CFM_FACE;
            RichEdit_GetCharFormat(new HandleRef(this, Handle), EM_GETCHARFORMAT, SCF_SELECTION, cf);
            sb.Append(cf.szFaceName);
            string result = sb.ToString();
            int index = result.IndexOf('\0');
            if (index >= 0)
                result = result.Substring(0, index);
            return result;
        }

        /// <summary>
        /// Sets PNRichEditBox font using exact copy of logFont parameter
        /// </summary>
        /// <param name="logFont">Fonts.LOGFONT</param>
        public void SetFontByFont(LOGFONT logFont)
        {
            var cf = new CHARFORMAT2();
            IntPtr hdc = Fonts.GetDC(IntPtr.Zero);
            cf.dwMask = CFM_FACE | CFM_CHARSET | CFM_SIZE;
            char[] array = logFont.lfFaceName.ToCharArray();
            Array.Copy(array, cf.szFaceName, array.Length);
            cf.bCharSet = logFont.lfCharSet;
            cf.yHeight = 20 * -(logFont.lfHeight * 72) / GetDeviceCaps(hdc, LOGPIXELSY);
            Fonts.ReleaseDC(IntPtr.Zero, hdc);
            RichEdit_SetCharFormat(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, cf);
        }

        /// <summary>
        /// Sets PNRichEditBox font using font name of logFont parameter
        /// </summary>
        /// <param name="logFont">Fonts.LOGFONT</param>
        public void SetFontByName(LOGFONT logFont)
        {
            var cf = new CHARFORMAT2 { dwMask = CFM_FACE | CFM_CHARSET };

            char[] name = logFont.lfFaceName.ToCharArray();
            Array.Copy(name, cf.szFaceName, name.Length);
            cf.bCharSet = logFont.lfCharSet;
            RichEdit_SetCharFormat(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, cf);
        }

        /// <summary>
        /// Sets PNRichEditBox font decoration
        /// </summary>
        /// <param name="mask">Decoration mask</param>
        /// <param name="style">Decoration style</param>
        public void SetFontDecoration(REFDecorationMask mask, REFDecorationStyle style)
        {
            var cf = new CHARFORMAT2();
            bool isSet = false;

            cf.dwMask = (uint)mask;
            cf.dwEffects = (uint)style;
            RichEdit_GetCharFormat(new HandleRef(this, Handle), EM_GETCHARFORMAT, SCF_SELECTION, cf);
            if ((cf.dwEffects & (int)style) > 0)
                isSet = true;
            cf = new CHARFORMAT2 { dwMask = (uint)mask };
            if (isSet)
                cf.dwEffects = (int)REFDecorationStyle.CFE_NONE;
            else
                cf.dwEffects = (uint)style;
            RichEdit_SetCharFormat(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, cf);
        }

        /// <summary>
        /// Gets PNRichEditBox font decoration
        /// </summary>
        /// <returns>PNRichEditBox font decoration</returns>
        public REFDecorationStyle GetFontDecoration()
        {
            var cf = new CHARFORMAT2
            {
                dwMask = (int)
                    (REFDecorationMask.CFM_BOLD | REFDecorationMask.CFM_ITALIC | REFDecorationMask.CFM_STRIKEOUT |
                     REFDecorationMask.CFM_SUBSCRIPT | REFDecorationMask.CFM_SUPERSCRIPT |
                     REFDecorationMask.CFM_UNDERLINE)
            };
            RichEdit_GetCharFormat(new HandleRef(this, Handle), EM_GETCHARFORMAT, SCF_SELECTION, cf);
            return (REFDecorationStyle)cf.dwEffects;
        }

        /// <summary>
        /// Removes highlighting from PNRichEditBox
        /// </summary>
        public void RemoveHighlightColor()
        {
            var cf = new CHARFORMAT2 { dwMask = CFE_AUTOBACKCOLOR, dwEffects = CFE_AUTOBACKCOLOR };

            RichEdit_SetCharFormat(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, cf);
        }

        /// <summary>
        /// Prints content of PNRichEditBox
        /// </summary>
        /// <param name="name">The name of document</param>
        public void Print(string name)
        {
            var p = new Printing(this);
            p.PrintRichTextContents(name);
        }

        /// <summary>
        /// Prints visible portion of PNRichEditBox to bitmap
        /// </summary>
        /// <param name="bitmap">Bitmap to print to</param>
        public void PrintToBitmap(Bitmap bitmap)
        {
            var p = new Printing(this);
            p.PrintToBitmap(bitmap);
        }

        /// <summary>
        /// Insert image into PNRichEditBox
        /// </summary>
        /// <param name="image">Image to insert</param>
        public void InsertImage(Image image)
        {
            IntPtr iRichOle;

            if (RichEdit_GetOleInterface(new HandleRef(this, Handle), EM_GETOLEINTERFACE, 0, out iRichOle))
            {
                InsertBitmap(iRichOle, ((Bitmap)image).GetHbitmap());
            }

            //Imaging img = new Imaging();
            //img.InsertImage(this, image);
        }

        #region API declarations
        private const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("reimaging.dll", CharSet = CharSet.Auto)]
        private static extern int InsertBitmap(IntPtr pRichEditOle, IntPtr hBitmap);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int MulDiv(int nNumber, int nNumerator, int nDenominator);
        /*
                [DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "GetTextExtentPoint32")]
                private static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int c, ref Size lpSize);
        */
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "GetDeviceCaps")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern int RichEdit_SetCharFormat(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)]CHARFORMAT2 lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern int RichEdit_GetCharFormat(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)]CHARFORMAT2 lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern int RichEdit_SetParaFormat(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)]PARAFORMAT2 lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern int RichEdit_GetParaFormat(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)]PARAFORMAT2 lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern bool RichEdit_GetOleInterface(HandleRef hWnd, int msg, int wParam, out IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern int RichEdit_SetMargins(HandleRef hWnd, int msg, int wParam, int lParam);

        private const int EC_LEFTMARGIN = 0x0001;
        private const int EC_RIGHTMARGIN = 0x0002;
        private const int TA_LEFT = 0;
        private const int TA_RIGHT = 2;
        private const int TA_BASELINE = 24;
        [StructLayout(LayoutKind.Sequential)]
        private class PARAFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wEffects;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public int[] rgxTabs;
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;

            public PARAFORMAT2()
            {
                cbSize = Marshal.SizeOf(typeof(PARAFORMAT2));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private class CHARFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public uint dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LF_FACESIZE)]
            public char[] szFaceName;
            public short wWeight;
            public short sSpacing;
            public int crBackColor;
            public int lcid;
            public int dwReserved;
            public short sStyle;
            public short wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;

            public CHARFORMAT2()
            {
                cbSize = Marshal.SizeOf(typeof(CHARFORMAT2));
                szFaceName = new char[LF_FACESIZE];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        private const uint CFM_BACKCOLOR = 0x04000000;
        private const uint CFM_CHARSET = 0x08000000;
        private const uint CFM_FACE = 0x20000000;
        private const uint CFM_SIZE = 0x80000000;
        private const uint CFE_AUTOBACKCOLOR = CFM_BACKCOLOR;
        private const int SCF_SELECTION = 0x0001;
        private const uint PFM_NUMBERING = 0x00000020;
        private const uint PFM_NUMBERINGSTYLE = 0x00002000;
        private const uint PFM_NUMBERINGTAB = 0x00004000;
        private const uint PFM_OFFSET = 0x00000004;
        private const uint PFM_OFFSETINDENT = 0x80000000;
        private const uint PFM_NUMBERINGSTART = 0x00008000;
        private const uint PFM_STARTINDENT = 0x00000001;
        private const uint PFM_SPACEBEFORE = 0x00000040;
        private const uint PFM_SPACEAFTER = 0x00000080;
        private const uint PFM_LINESPACING = 0x00000100;

        private const int WM_USER = 0x0400;
        private const int WM_PAINT = 0x000F;
        private const int WM_MOUSEACTIVATE = 0x0021;
        /*
                private const int WM_ACTIVATE = 0x0006;
        */
        private const int MA_ACTIVATE = 1;

        private const int EM_SETCHARFORMAT = (WM_USER + 68);
        private const int EM_GETPARAFORMAT = (WM_USER + 61);
        private const int EM_SETPARAFORMAT = (WM_USER + 71);
        private const int EM_GETCHARFORMAT = (WM_USER + 58);
        private const int EM_GETOLEINTERFACE = (WM_USER + 60);
        private const int EM_SETMARGINS = 0x00D3;

        private const int LF_FACESIZE = 32;
        /*
                private const int DEFAULT_CHARSET = 1;
        */
        private const int LOGPIXELSY = 90;

        private int MAKELONG(short lowPart, short highPart)
        {
            return (int)(((ushort)lowPart) | (uint)(highPart << 16));
        }
        #endregion
    }

    /// <summary>
    /// PNREActivatedByMouse event handler
    /// </summary>
    /// <param name="sender">PNRichEditBox control generated the event</param>
    /// <param name="e">PNREActivatedByMouseEventArgs</param>
    public delegate void PNREActivatedByMouseEventHandler(object sender, PNREActivatedByMouseEventArgs e);

    /// <summary>
    /// Represents agrument for PNREActivatedByMouse event
    /// </summary>
    public class PNREActivatedByMouseEventArgs : EventArgs
    {
        private readonly Point m_Position = Point.Empty;

        /// <summary>
        /// Creates new instance of PNREActivatedByMouseEventArgs
        /// </summary>
        /// <param name="position">Cursor position in client coordinates</param>
        public PNREActivatedByMouseEventArgs(Point position)
        {
            m_Position = position;
        }

        /// <summary>
        /// Cursor position in client coordinates
        /// </summary>
        public Point Position
        {
            get { return m_Position; }
        }
    }
}