#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

#endregion

namespace PNStaticFonts
{

    #region Enumerations

    /// <summary>
    ///     Font weight enumeration
    /// </summary>
    [Flags]
    public enum FontWeight
    {
        /// <summary>
        ///     0
        /// </summary>
        FW_DONTCARE = 0,

        /// <summary>
        ///     100
        /// </summary>
        FW_THIN = 100,

        /// <summary>
        ///     200
        /// </summary>
        FW_EXTRALIGHT = 200,

        /// <summary>
        ///     200
        /// </summary>
        FW_ULTRALIGHT = 200,

        /// <summary>
        ///     300
        /// </summary>
        FW_LIGHT = 300,

        /// <summary>
        ///     400
        /// </summary>
        FW_NORMAL = 400,

        /// <summary>
        ///     400
        /// </summary>
        FW_REGULAR = 400,

        /// <summary>
        ///     500
        /// </summary>
        FW_MEDIUM = 500,

        /// <summary>
        ///     600
        /// </summary>
        FW_SEMIBOLD = 600,

        /// <summary>
        ///     600
        /// </summary>
        FW_DEMIBOLD = 600,

        /// <summary>
        ///     700
        /// </summary>
        FW_BOLD = 700,

        /// <summary>
        ///     800
        /// </summary>
        FW_EXTRABOLD = 800,

        /// <summary>
        ///     800
        /// </summary>
        FW_ULTRABOLD = 800,

        /// <summary>
        ///     900
        /// </summary>
        FW_HEAVY = 900,

        /// <summary>
        ///     900
        /// </summary>
        FW_BLACK = 900
    }

    /// <summary>
    ///     DrawText enumeration
    /// </summary>
    [Flags]
    public enum DTStyles : uint
    {
        /// <summary>
        ///     0x00000000
        /// </summary>
        DT_LEFT = 0x00000000,

        /// <summary>
        ///     0x00000020
        /// </summary>
        DT_SINGLELINE = 0x00000020,

        /// <summary>
        ///     0x00000004
        /// </summary>
        DT_VCENTER = 0x00000004,

        /// <summary>
        ///     0x00000002
        /// </summary>
        DT_RIGHT = 0x00000002,

        /// <summary>
        ///     0x00000001
        /// </summary>
        DT_CENTER = 0x00000001,

        /// <summary>
        ///     0x00008000
        /// </summary>
        DT_END_ELLIPSIS = 0x00008000,

        /// <summary>
        ///     0x00000010
        /// </summary>
        DT_WORDBREAK = 0x00000010
    }

    /// <summary>
    ///     Misc GDI constants
    /// </summary>
    public enum LFParams
    {
        /// <summary>
        ///     32
        /// </summary>
        LF_FACESIZE = 32,

        /// <summary>
        ///     64
        /// </summary>
        LF_FULLFACESIZE = 64,

        /// <summary>
        ///     1
        /// </summary>
        DEFAULT_CHARSET = 1,

        /// <summary>
        ///     2
        /// </summary>
        PROOF_QUALITY = 2,

        /// <summary>
        ///     2
        /// </summary>
        VARIABLE_PITCH = 2,

        /// <summary>
        ///     4
        /// </summary>
        OUT_TT_PRECIS = 4,

        /// <summary>
        ///     0
        /// </summary>
        CLIP_DEFAULT_PRECIS = 0
    }

    /// <summary>
    ///     Variable parameters
    /// </summary>
    public enum VariableParams
    {
        /// <summary>
        ///     90
        /// </summary>
        LOGPIXELSY = 90
    }

    #endregion

    #region Structures

    /// <summary>
    ///     Represents Windows GDI LOGFONT structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1), Serializable]
    public struct LOGFONT
    {
        /// <summary>
        ///     Specifies the height, in logical units, of the font's character cell or character
        /// </summary>
        public int lfHeight;

        /// <summary>
        ///     Specifies the average width, in logical units, of characters in the font
        /// </summary>
        public int lfWidth;

        /// <summary>
        ///     Specifies the angle, in tenths of degrees, between the escapement vector and the x-axis of the device
        /// </summary>
        public int lfEscapement;

        /// <summary>
        ///     Specifies the angle, in tenths of degrees, between each character's base line and the x-axis of the device
        /// </summary>
        public int lfOrientation;

        /// <summary>
        ///     Specifies the weight of the font in the range 0 through 1000
        /// </summary>
        public int lfWeight;

        /// <summary>
        ///     TRUE to specify an italic font
        /// </summary>
        public byte lfItalic;

        /// <summary>
        ///     TRUE to specify an underlined font
        /// </summary>
        public byte lfUnderline;

        /// <summary>
        ///     TRUE to specify a strikeout font
        /// </summary>
        public byte lfStrikeOut;

        /// <summary>
        ///     Specifies the character set
        /// </summary>
        public byte lfCharSet;

        /// <summary>
        ///     Specifies the output precision
        /// </summary>
        public byte lfOutPrecision;

        /// <summary>
        ///     Specifies the clipping precision
        /// </summary>
        public byte lfClipPrecision;

        /// <summary>
        ///     Specifies the output quality
        /// </summary>
        public byte lfQuality;

        /// <summary>
        ///     Specifies the pitch and family of the font
        /// </summary>
        public byte lfPitchAndFamily;

        /// <summary>
        ///     Specifies a null-terminated string that specifies the typeface name of the font
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)LFParams.LF_FACESIZE)]
        public string lfFaceName;

        /// <summary>
        ///     Initializes structure with default values
        /// </summary>
        public void Init()
        {
            IntPtr hdc = Fonts.GetDC(IntPtr.Zero);
            lfHeight = -Fonts.MulDiv(8, Fonts.GetDeviceCaps(hdc, (int)VariableParams.LOGPIXELSY), 72);
            Fonts.ReleaseDC(IntPtr.Zero, hdc);
            lfQuality = (byte)LFParams.PROOF_QUALITY;
            lfPitchAndFamily = (byte)LFParams.VARIABLE_PITCH;
            lfOutPrecision = (byte)LFParams.OUT_TT_PRECIS;
            lfCharSet = (byte)LFParams.DEFAULT_CHARSET;
            lfWeight = (int)FontWeight.FW_REGULAR;
            lfClipPrecision = (byte)LFParams.CLIP_DEFAULT_PRECIS;
            lfWidth = lfEscapement = lfOrientation = 0;
            lfItalic = lfUnderline = lfStrikeOut = 0;
            lfFaceName = Fonts.INIT_FONT;//"Lucida Sans Unicode";
        }

        /// <summary>
        ///     Gets font size in points
        /// </summary>
        /// <returns>Font size in points</returns>
        public int GetFontSize()
        {
            IntPtr hdc = Fonts.GetDC(IntPtr.Zero);
            int size = -Fonts.MulDiv(lfHeight, 72, Fonts.GetDeviceCaps(hdc, (int)VariableParams.LOGPIXELSY));
            Fonts.ReleaseDC(IntPtr.Zero, hdc);
            return size;
        }

        /// <summary>
        ///     Sets font size from points
        /// </summary>
        /// <param name="size">Font size in points</param>
        public void SetFontSize(int size)
        {
            IntPtr hdc = Fonts.GetDC(IntPtr.Zero);
            lfHeight = -Fonts.MulDiv(size, Fonts.GetDeviceCaps(hdc, (int)VariableParams.LOGPIXELSY), 72);
            Fonts.ReleaseDC(IntPtr.Zero, hdc);
        }

        /// <summary>
        ///     Sets new font face name
        /// </summary>
        /// <param name="faceName">New font face name</param>
        public void SetFontFace(string faceName)
        {
            lfFaceName = faceName;
        }

        /// <summary>
        ///     Gets string representation of structure
        /// </summary>
        /// <returns>String representation of structure</returns>
        public override string ToString()
        {
            return lfFaceName;
        }

        /// <summary>
        ///     Compares two LOGFONT structures
        /// </summary>
        /// <param name="a">LOGFONT structure</param>
        /// <param name="b">LOGFONT structure</param>
        /// <returns>True if structures are equal, false otherwise</returns>
        public static bool operator ==(LOGFONT a, LOGFONT b)
        {
            return (a.lfFaceName == b.lfFaceName
                    && a.lfCharSet == b.lfCharSet
                    && a.lfClipPrecision == b.lfClipPrecision
                    && a.lfEscapement == b.lfEscapement
                    && a.lfHeight == b.lfHeight
                    && a.lfItalic == b.lfItalic
                    && a.lfOrientation == b.lfOrientation
                    && a.lfOutPrecision == b.lfOutPrecision
                    && a.lfPitchAndFamily == b.lfPitchAndFamily
                    && a.lfQuality == b.lfQuality
                    && a.lfStrikeOut == b.lfStrikeOut
                    && a.lfUnderline == b.lfUnderline
                    && a.lfWeight == b.lfWeight
                    && a.lfWidth == b.lfWidth);
        }

        /// <summary>
        ///     Compares two LOGFONT structures
        /// </summary>
        /// <param name="a">LOGFONT structure</param>
        /// <param name="b">LOGFONT structure</param>
        /// <returns>True if structures are not equal, false otherwise</returns>
        public static bool operator !=(LOGFONT a, LOGFONT b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is LOGFONT)
            {
                var lf = (LOGFONT)obj;
                return (lfFaceName == lf.lfFaceName
                        && lfCharSet == lf.lfCharSet
                        && lfClipPrecision == lf.lfClipPrecision
                        && lfEscapement == lf.lfEscapement
                        && lfHeight == lf.lfHeight
                        && lfItalic == lf.lfItalic
                        && lfOrientation == lf.lfOrientation
                        && lfOutPrecision == lf.lfOutPrecision
                        && lfPitchAndFamily == lf.lfPitchAndFamily
                        && lfQuality == lf.lfQuality
                        && lfStrikeOut == lf.lfStrikeOut
                        && lfUnderline == lf.lfUnderline
                        && lfWeight == lf.lfWeight
                        && lfWidth == lf.lfWidth);
            }
            return false;
        }

        /// <summary>
        ///     Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="lf" /> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="lf">Another LOGFONT structure to compare to. </param>
        /// <filterpriority>2</filterpriority>
        public bool Equals(LOGFONT lf)
        {
            return (lfFaceName == lf.lfFaceName
                    && lfCharSet == lf.lfCharSet
                    && lfClipPrecision == lf.lfClipPrecision
                    && lfEscapement == lf.lfEscapement
                    && lfHeight == lf.lfHeight
                    && lfItalic == lf.lfItalic
                    && lfOrientation == lf.lfOrientation
                    && lfOutPrecision == lf.lfOutPrecision
                    && lfPitchAndFamily == lf.lfPitchAndFamily
                    && lfQuality == lf.lfQuality
                    && lfStrikeOut == lf.lfStrikeOut
                    && lfUnderline == lf.lfUnderline
                    && lfWeight == lf.lfWeight
                    && lfWidth == lf.lfWidth);
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            int result = 17;
            result *= 37;
            result += lfFaceName.GetHashCode();
            result *= 37;
            result += lfWidth.GetHashCode();
            result *= 37;
            result += lfWeight.GetHashCode();
            result *= 37;
            result += lfUnderline.GetHashCode();
            result *= 37;
            result += lfStrikeOut.GetHashCode();
            result *= 37;
            result += lfQuality.GetHashCode();
            result *= 37;
            result += lfPitchAndFamily.GetHashCode();
            result *= 37;
            result += lfOutPrecision.GetHashCode();
            result *= 37;
            result += lfOrientation.GetHashCode();
            result *= 37;
            result += lfItalic.GetHashCode();
            result *= 37;
            result += lfHeight.GetHashCode();
            result *= 37;
            result += lfEscapement.GetHashCode();
            result *= 37;
            result += lfClipPrecision.GetHashCode();
            result *= 37;
            result += lfCharSet.GetHashCode();
            return result;
        }
    }

    /// <summary>
    ///     Represents Windows GDI NEWTEXTMETRIC structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct NEWTEXTMETRIC
    {
        /// <summary>
        ///     The height (ascent + descent) of characters
        /// </summary>
        public int tmHeight;

        /// <summary>
        ///     The ascent (units above the base line) of characters
        /// </summary>
        public int tmAscent;

        /// <summary>
        ///     The descent (units below the base line) of characters
        /// </summary>
        public int tmDescent;

        /// <summary>
        ///     The amount of leading (space) inside the bounds set by the tmHeight member
        /// </summary>
        public int tmInternalLeading;

        /// <summary>
        ///     The amount of extra leading (space) that the application adds between rows
        /// </summary>
        public int tmExternalLeading;

        /// <summary>
        ///     The average width of characters in the font (generally defined as the width of the letter x)
        /// </summary>
        public int tmAveCharWidth;

        /// <summary>
        ///     The width of the widest character in the font
        /// </summary>
        public int tmMaxCharWidth;

        /// <summary>
        ///     The weight of the font
        /// </summary>
        public int tmWeight;

        /// <summary>
        ///     The extra width per string that may be added to some synthesized fonts. When synthesizing some attributes, such as bold or italic, graphics device interface (GDI) or a device may have to add width to a string on both a per-character and per-string basis
        /// </summary>
        public int tmOverhang;

        /// <summary>
        ///     The horizontal aspect of the device for which the font was designed
        /// </summary>
        public int tmDigitizedAspectX;

        /// <summary>
        ///     The vertical aspect of the device for which the font was designed
        /// </summary>
        public int tmDigitizedAspectY;

        /// <summary>
        ///     The value of the first character defined in the font
        /// </summary>
        public char tmFirstChar;

        /// <summary>
        ///     The value of the last character defined in the font
        /// </summary>
        public char tmLastChar;

        /// <summary>
        ///     The value of the character to be substituted for characters that are not in the font
        /// </summary>
        public char tmDefaultChar;

        /// <summary>
        ///     The value of the character to be used to define word breaks for text justification
        /// </summary>
        public char tmBreakChar;

        /// <summary>
        ///     An italic font if it is nonzero
        /// </summary>
        public byte tmItalic;

        /// <summary>
        ///     An underlined font if it is nonzero
        /// </summary>
        public byte tmUnderlined;

        /// <summary>
        ///     A strikeout font if it is nonzero
        /// </summary>
        public byte tmStruckOut;

        /// <summary>
        ///     The pitch and family of the selected font
        /// </summary>
        public byte tmPitchAndFamily;

        /// <summary>
        ///     The character set of the font
        /// </summary>
        public byte tmCharSet;

        /// <summary>
        ///     Specifies whether the font is italic, underscored, outlined, bold, and so forth
        /// </summary>
        public int ntmFlags;

        /// <summary>
        ///     The size of the em square for the font
        /// </summary>
        public uint ntmSizeEM;

        /// <summary>
        ///     The height, in notional units, of the font
        /// </summary>
        public uint ntmCellHeight;

        /// <summary>
        ///     The average width of characters in the font, in notional units
        /// </summary>
        public uint ntmAvgWidth;
    }

    /// <summary>
    ///     Represents Windows GDI RECT structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class RECT
    {
        /// <summary>
        ///     Specifies the x-coordinate of the upper-left corner of a rectangle
        /// </summary>
        public int left;

        /// <summary>
        ///     Specifies the y-coordinate of the upper-left corner of a rectangle
        /// </summary>
        public int top;

        /// <summary>
        ///     Specifies the x-coordinate of the lower-right corner of a rectangle
        /// </summary>
        public int right;

        /// <summary>
        ///     Specifies the y-coordinate of the lower-right corner of a rectangle
        /// </summary>
        public int bottom;

        /// <summary>
        ///     Creates instance of RECT structure
        /// </summary>
        /// <param name="l">Left</param>
        /// <param name="t">Top</param>
        /// <param name="r">Right</param>
        /// <param name="b">Bottom</param>
        public RECT(int l, int t, int r, int b)
        {
            left = l;
            top = t;
            right = r;
            bottom = b;
        }
    }

    #endregion

    /// <summary>
    ///     Represents class for various GDI fonts operations
    /// </summary>
    public class Fonts
    {
        #region Callback delegates

        private delegate int EnumFontsDelegate(ref LOGFONT lf, ref NEWTEXTMETRIC nm, int fontType, int lParam);

        #endregion

        #region API constants

        private const int FR_PRIVATE = 0x10;
        private const int LOGPIXELSY = 90;
        private const int TRANSPARENT = 1;
        //private const int SYS_COLOR_INDEX_FLAG = 0;
        //private const int COLOR_HIGHLIGHT = (13 | SYS_COLOR_INDEX_FLAG);
        //private const int COLOR_HIGHLIGHTTEXT = (14 | SYS_COLOR_INDEX_FLAG);
        //private const int COLOR_WINDOW = (5 | SYS_COLOR_INDEX_FLAG);
        //private const int COLOR_WINDOWTEXT = (8 | SYS_COLOR_INDEX_FLAG);
        //private const int COLOR_GRAYTEXT = (17 | SYS_COLOR_INDEX_FLAG);
        //private const int CF_SCREENFONTS = 0x00000001;
        //private const int CF_EFFECTS = 0x00000100;
        //private const int CF_INITTOLOGFONTSTRUCT = 0x00000040;
        private const uint GGI_MARK_NONEXISTING_GLYPHS = 0x01;
        private const long GDI_ERROR = (0xFFFFFFFFL);

        #endregion

        #region Drawing and fonts API

        //public functions
        /// <summary>
        ///     Retrieves a handle to a device context (DC) for the client area of a specified window or for the entire screen
        /// </summary>
        /// <param name="hWnd">A handle to the window whose DC is to be retrieved. If this value is NULL, GetDC retrieves the DC for the entire screen</param>
        /// <returns>If the function succeeds, the return value is a handle to the DC for the specified window's client area.If the function fails, the return value is NULL</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        /// <summary>
        ///     Releases a device context (DC), freeing it for use by other applications
        /// </summary>
        /// <param name="hWnd">A handle to the window whose DC is to be released</param>
        /// <param name="hDc">A handle to the DC to be released</param>
        /// <returns>The return value indicates whether the DC was released. If the DC was released, the return value is 1.If the DC was not released, the return value is zero</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        /// <summary>
        ///     The MulDiv function multiplies two 32-bit values and then divides the 64-bit result by a third 32-bit value
        /// </summary>
        /// <param name="nNumber">Multiplicand</param>
        /// <param name="nNumerator">Multiplier</param>
        /// <param name="nDenominator">Number by which the result of the multiplication is to be divided</param>
        /// <returns>If the function succeeds, the return value is the result of the multiplication and division. If either an overflow occurred or nDenominator was 0, the return value is –1</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int MulDiv(int nNumber, int nNumerator, int nDenominator);

        /// <summary>
        ///     Retrieves device-specific information for the specified device
        /// </summary>
        /// <param name="hdc">A handle to the DC</param>
        /// <param name="nIndex">The item to be returned</param>
        /// <returns>The return value specifies the value of the desired item</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "GetDeviceCaps")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        /// <summary>
        ///     Retrieves a handle identifying a logical brush that corresponds to the specified color index
        /// </summary>
        /// <param name="nIndex">A color index. This value corresponds to the color used to paint one of the window elements</param>
        /// <returns>The return value identifies a logical brush if the nIndex parameter is supported by the current platform. Otherwise, it returns NULL</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetSysColorBrush(int nIndex);

        /// <summary>
        ///     Retrieves the current color of the specified display element. Display elements are the parts of a window and the display that appear on the system display screen
        /// </summary>
        /// <param name="nIndex">The display element whose color is to be retrieved</param>
        /// <returns>The function returns the red, green, blue (RGB) color value of the given element</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetSysColor(int nIndex);

        /// <summary>
        ///     Sets the background mix mode of the specified device context
        /// </summary>
        /// <param name="hdc">A handle to the device context</param>
        /// <param name="iBkMode">The background mode</param>
        /// <returns>If the function succeeds, the return value specifies the previous background mode.If the function fails, the return value is zero</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern int SetBkMode(IntPtr hdc, int iBkMode);

        /// <summary>
        ///     Fills a rectangle by using the specified brush
        /// </summary>
        /// <param name="hDc">A handle to the device context</param>
        /// <param name="lprc">A pointer to a RECT structure that contains the logical coordinates of the rectangle to be filled</param>
        /// <param name="hbr">A handle to the brush used to fill the rectangle</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int FillRect(IntPtr hDc, [MarshalAs(UnmanagedType.LPStruct)] RECT lprc, IntPtr hbr);

        /// <summary>
        ///     Sets the text color for the specified device context to the specified color
        /// </summary>
        /// <param name="hdc">A handle to the device context</param>
        /// <param name="crColor">The color of the text</param>
        /// <returns>If the function succeeds, the return value is a color reference for the previous text color as a COLORREF value. If the function fails, the return value is CLR_INVALID</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern int SetTextColor(IntPtr hdc, int crColor);

        /// <summary>
        ///     Draws formatted text in the specified rectangle. It formats the text according to the specified method (expanding tabs, justifying characters, breaking lines, and so forth)
        /// </summary>
        /// <param name="hDc">A handle to the device context</param>
        /// <param name="lpString">A pointer to the string that specifies the text to be drawn. If the nCount parameter is -1, the string must be null-terminated</param>
        /// <param name="nCount">The length, in characters, of the string. If nCount is -1, then the lpchText parameter is assumed to be a pointer to a null-terminated string and DrawText computes the character count automatically</param>
        /// <param name="lpRect">A pointer to a RECT structure that contains the rectangle (in logical coordinates) in which the text is to be formatted</param>
        /// <param name="uFormat">The method of formatting the text</param>
        /// <returns>If the function succeeds, the return value is the height of the text in logical units. If DT_VCENTER or DT_BOTTOM is specified, the return value is the offset from lpRect->top to the bottom of the drawn text If the function fails, the return value is zero</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int DrawText(IntPtr hDc, string lpString, int nCount,
                                          [MarshalAs(UnmanagedType.LPStruct)] RECT lpRect, DTStyles uFormat);

        /// <summary>
        ///     Creates a logical font that has the specified characteristics. The font can subsequently be selected as the current font for any device context
        /// </summary>
        /// <param name="lplf">A pointer to a LOGFONT structure that defines the characteristics of the logical font</param>
        /// <returns>If the function succeeds, the return value is a handle to a logical font. If the function fails, the return value is NULL</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFontIndirect(ref LOGFONT lplf);

        /// <summary>
        ///     selects an object into the specified device context (DC). The new object replaces the previous object of the same type
        /// </summary>
        /// <param name="hdc">A handle to the DC</param>
        /// <param name="hgdiobj">A handle to the object to be selected</param>
        /// <returns>If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        /// <summary>
        ///     Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid
        /// </summary>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the specified handle is not valid or is currently selected into a DC, the return value is zero</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern bool DeleteObject(IntPtr hObject);

        //private functions
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int c, ref Size lpSize);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "AddFontResourceExW")]
        private static extern int AddFontResourceEx(string lpszFilename, int fl, int pdv);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "RemoveFontResourceEx")]
        private static extern bool RemoveFontResourceEx(string lpFileName, int fl, int pdv);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern int EnumFontFamilies(IntPtr hdc, string fontFamily, EnumFontsDelegate callbackProc,
                                                   int lParam);

        [DllImport("gdi32.dll", EntryPoint = "GetGlyphIndicesW")]
        private static extern uint GetGlyphIndices([In] IntPtr hdc, [In] [MarshalAs(UnmanagedType.LPTStr)] string lpsz,
            int c, [Out] ushort[] pgi, uint fl);

        #endregion

        #region Callback functions

        private static int EnumFontFamiliesProc(ref LOGFONT pelf, ref NEWTEXTMETRIC pntm, int fontType, int lParam)
        {
            _fontItems.Add(pelf);
            return 1;
        }

        #endregion

        internal const string INIT_FONT = "MS Sans Serif";

        #region Static functions

        /// <summary>
        ///     Collects installed fonts names and fills List of LOGFONT structures
        /// </summary>
        /// <param name="list">List of LOGFONT structures to fill</param>
        public static void GetFontsList(List<LOGFONT> list)
        {
            _fontItems.Clear();
            list.Clear();
            _fontItems = list;
            var callback = new EnumFontsDelegate(EnumFontFamiliesProc);
            IntPtr hdc = GetDC(IntPtr.Zero);
            EnumFontFamilies(hdc, null, callback, 0);
            ReleaseDC(IntPtr.Zero, hdc);
        }

        /// <summary>
        ///     Measures string accordingly to GDI device context and specified font
        /// </summary>
        /// <param name="g">Graphics object to get device context</param>
        /// <param name="lf">LOGFONT structure</param>
        /// <param name="str">String to measure</param>
        /// <returns>Size of string measures</returns>
        public static Size MeasureString(Graphics g, LOGFONT lf, string str)
        {
            Size sz = Size.Empty;
            IntPtr hdc = g.GetHdc();
            try
            {
                IntPtr hFont = CreateFontIndirect(ref lf);
                IntPtr hOldFont = SelectObject(hdc, hFont);
                GetTextExtentPoint32(hdc, str, str.Length, ref sz);
                SelectObject(hdc, hOldFont);
                DeleteObject(hFont);
            }
            finally
            {
                if (hdc != IntPtr.Zero)
                    g.ReleaseHdc(hdc);
            }
            return sz;
        }

        /// <summary>
        ///     Draw font name on GDI device context
        /// </summary>
        /// <param name="g">Graphics object to draw on</param>
        /// <param name="lf">LOGFONT structure</param>
        /// <param name="bounds">Rectangle to draw within</param>
        /// <param name="color">Color used to draw text</param>
        public static void DrawFontNameGDI(Graphics g, LOGFONT lf, Rectangle bounds, Color color)
        {
            var ff = FontFamily.Families.FirstOrDefault(f => f.Name == lf.lfFaceName);
            if (ff != null)
            {
                var fs = FontStyle.Regular;
                if (!ff.IsStyleAvailable(fs))
                    fs = FontStyle.Bold;
                if (!ff.IsStyleAvailable(fs))
                    fs = FontStyle.Italic;
                var fmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                fmt.FormatFlags |= StringFormatFlags.NoWrap;
                using (var font = new Font(INIT_FONT, 9, FontStyle.Regular, GraphicsUnit.Point))
                {
                    using (var brush = new SolidBrush(color))
                    {
                        g.DrawString(lf.lfFaceName, font, brush, bounds, fmt);
                    }
                }
                fmt.Alignment = StringAlignment.Far;
                using (var font = new Font(lf.lfFaceName, 9, fs, GraphicsUnit.Point))
                {
                    using (var brush = new SolidBrush(color))
                    {
                        g.DrawString("Sample", font, brush, bounds, fmt);
                    }
                }
            }
            else
            {
                var hdc = g.GetHdc();
                try
                {
                    SetBkMode(hdc, TRANSPARENT);
                    var rc = new RECT(bounds.Left, bounds.Top, bounds.Left + bounds.Width, bounds.Top + bounds.Height);
                    var textColor = Color.FromArgb(0, color.B, color.G, color.R).ToArgb();
                    SetTextColor(hdc, textColor);
                    var lfo = new LOGFONT();
                    lfo.Init();
                    lfo.lfHeight = -MulDiv(9, GetDeviceCaps(hdc, LOGPIXELSY), 72);
                    var hFont = CreateFontIndirect(ref lfo);
                    var hOldFont = SelectObject(hdc, hFont);
                    DrawText(hdc, lf.lfFaceName, -1, rc, DTStyles.DT_LEFT | DTStyles.DT_SINGLELINE | DTStyles.DT_VCENTER);
                    SelectObject(hdc, hOldFont);
                    DeleteObject(hFont);
                    rc.right -= 2;
                    var plf = new LOGFONT
                    {
                        lfQuality = (byte)LFParams.PROOF_QUALITY,
                        lfPitchAndFamily = (byte)LFParams.VARIABLE_PITCH,
                        lfOutPrecision = (byte)LFParams.OUT_TT_PRECIS,
                        lfCharSet = lf.lfCharSet,
                        lfFaceName = lf.lfFaceName,
                        lfHeight = -MulDiv(9, GetDeviceCaps(hdc, LOGPIXELSY), 72)
                    };
                    hFont = CreateFontIndirect(ref plf);
                    hOldFont = SelectObject(hdc, hFont);
                    DrawText(hdc, "Sample", -1, rc, DTStyles.DT_RIGHT | DTStyles.DT_SINGLELINE | DTStyles.DT_VCENTER);
                    SelectObject(hdc, hOldFont);
                    DeleteObject(hFont);
                }
                finally
                {
                    if (hdc != IntPtr.Zero)
                        g.ReleaseHdc(hdc);
                }
            }
        }

        /// <summary>
        ///     Draws text on GDI device context
        /// </summary>
        /// <param name="g">Graphics object to draw on</param>
        /// <param name="clr">Text color</param>
        /// <param name="rect">Rectangle to draw within</param>
        /// <param name="lf">LOGFONT structure</param>
        /// <param name="text">Text to draw</param>
        /// <param name="style">DrawText flags</param>
        public static void DrawTextGDI(Graphics g, Color clr, Rectangle rect, LOGFONT lf, string text, DTStyles style)
        {
            var ff = FontFamily.Families.FirstOrDefault(f => f.Name == lf.lfFaceName);
            if (ff != null)
            {
                var fmt = new StringFormat();
                if ((style & DTStyles.DT_CENTER) == DTStyles.DT_CENTER)
                    fmt.Alignment = StringAlignment.Center;
                else if ((style & DTStyles.DT_LEFT) == DTStyles.DT_LEFT)
                    fmt.Alignment = StringAlignment.Near;
                else if ((style & DTStyles.DT_RIGHT) == DTStyles.DT_RIGHT)
                    fmt.Alignment = StringAlignment.Far;

                if ((style & DTStyles.DT_VCENTER) == DTStyles.DT_VCENTER)
                    fmt.LineAlignment = StringAlignment.Center;
                if ((style & DTStyles.DT_SINGLELINE) == DTStyles.DT_SINGLELINE)
                    fmt.FormatFlags |= StringFormatFlags.NoWrap;
                if ((style & DTStyles.DT_END_ELLIPSIS) == DTStyles.DT_END_ELLIPSIS)
                    fmt.Trimming = StringTrimming.EllipsisCharacter;

                var fs = FontStyle.Regular;
                if (lf.lfWeight > (int)FontWeight.FW_REGULAR || !ff.IsStyleAvailable(FontStyle.Regular))
                    fs = FontStyle.Bold;
                if (lf.lfItalic > 0 && ff.IsStyleAvailable((FontStyle.Italic)))
                    fs |= FontStyle.Italic;
                if (lf.lfUnderline > 0 && ff.IsStyleAvailable((FontStyle.Underline)))
                    fs |= FontStyle.Underline;
                if (lf.lfStrikeOut > 0 && ff.IsStyleAvailable((FontStyle.Strikeout)))
                    fs |= FontStyle.Strikeout;

                IntPtr hdc = GetDC(IntPtr.Zero);
                var height = -MulDiv(lf.lfHeight, 72, GetDeviceCaps(hdc, (int)VariableParams.LOGPIXELSY));
                ReleaseDC(IntPtr.Zero, hdc);

                using (var font = new Font(lf.lfFaceName, height, fs, GraphicsUnit.Point))
                {
                    using (var brush = new SolidBrush(clr))
                    {
                        g.DrawString(text, font, brush, rect, fmt);
                    }
                }
            }
            else
            {
                var rc = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
                var hdc = g.GetHdc();
                try
                {
                    SetBkMode(hdc, TRANSPARENT);
                    var hFont = CreateFontIndirect(ref lf);
                    var newColor = Color.FromArgb(0, clr.B, clr.G, clr.R).ToArgb();
                    var oldColor = SetTextColor(hdc, newColor);
                    var hOldFont = SelectObject(hdc, hFont);
                    DrawText(hdc, text, -1, rc, style);
                    SetTextColor(hdc, oldColor);
                    SelectObject(hdc, hOldFont);
                    DeleteObject(hFont);
                }
                finally
                {
                    if (hdc != IntPtr.Zero)
                        g.ReleaseHdc(hdc);
                }
            }
        }

        /// <summary>
        ///     Registers custom fonts
        /// </summary>
        /// <param name="files">FileInfo collection of custom fonts</param>
        /// <returns></returns>
        public static List<string> RegisterCustomFonts(IEnumerable<FileInfo> files)
        {
            return (from f in files where AddFontResourceEx(f.FullName, FR_PRIVATE, 0) != 0 select f.FullName).ToList();
        }

        /// <summary>
        ///     Unregisters custom fonts
        /// </summary>
        /// <param name="list">List of custom fonts</param>
        public static void UnregisterCustomFonts(List<string> list)
        {
            foreach (string s in list)
            {
                RemoveFontResourceEx(s, FR_PRIVATE, 0);
            }
        }

        /// <summary>
        /// Checks whether character is available for specified font
        /// </summary>
        /// <param name="character">Character to check</param>
        /// <param name="fontName">Font name</param>
        /// <returns></returns>
        public static bool IsUnicodeCharAvailable(char character, string fontName)
        {
            var hdc = IntPtr.Zero;
            var hFontOld = IntPtr.Zero;

            try
            {
                var lf = new LOGFONT
                {
                    lfFaceName = fontName,
                    lfWeight = (int)FontWeight.FW_DONTCARE,
                    lfCharSet = (byte)LFParams.DEFAULT_CHARSET,
                    lfPitchAndFamily = (0 << 4)
                };
                var hFontNew = CreateFontIndirect(ref lf);
                if (hFontNew.Equals(IntPtr.Zero)) return false;
                hdc = GetDC(IntPtr.Zero);
                hFontOld = SelectObject(hdc, hFontNew);

                var glyphIndices = new ushort[1];

                return
                    (GetGlyphIndices(hdc, new string(character, 1), 1, glyphIndices, GGI_MARK_NONEXISTING_GLYPHS) !=
                     GDI_ERROR && glyphIndices[0] != 0xffff);
            }
            finally
            {
                if (!hdc.Equals(IntPtr.Zero))
                {
                    if (!hFontOld.Equals(IntPtr.Zero))
                        SelectObject(hdc, hFontOld);
                    ReleaseDC(IntPtr.Zero, hdc);
                }
            }
        }
        #endregion

        private static List<LOGFONT> _fontItems = new List<LOGFONT>();
    }
}