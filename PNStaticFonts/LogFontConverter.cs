#region Usings

using System;
using System.Linq;
using System.Reflection;
using System.Text;

#endregion

namespace PNStaticFonts
{
    /// <summary>
    ///     Converts LOGFONT structure to and from string
    /// </summary>
    public class LogFontConverter
    {
        /// <summary>
        ///     Converts LOGFONT structure to string
        /// </summary>
        /// <param name="logFont">LOGFONT structure to convert</param>
        /// <returns>String</returns>
        public string ConvertToString(LOGFONT logFont)
        {
            var sb = new StringBuilder();
            FieldInfo[] fields = logFont.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                sb.Append(f.Name);
                sb.Append("=");
                sb.Append(f.GetValue(logFont));
                sb.Append("^");
            }
            sb.Length -= 1;
            return sb.ToString();
        }

        /// <summary>
        ///     Converts string to LOGFONT structure
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>LOGFONT structure</returns>
        public LOGFONT ConvertFromString(string str)
        {
            var lf = new LOGFONT();
            string[] data = str.Split('^');
            foreach (string[] f in data.Select(s => s.Split('=')))
            {
                switch (f[0])
                {
                    case "lfHeight":
                        lf.lfHeight = Convert.ToInt32(f[1]);
                        break;
                    case "lfWidth":
                        lf.lfWidth = Convert.ToInt32(f[1]);
                        break;
                    case "lfEscapement":
                        lf.lfEscapement = Convert.ToInt32(f[1]);
                        break;
                    case "lfOrientation":
                        lf.lfOrientation = Convert.ToInt32(f[1]);
                        break;
                    case "lfWeight":
                        lf.lfWeight = Convert.ToInt32(f[1]);
                        break;
                    case "lfFaceName":
                        lf.lfFaceName = f[1];
                        break;
                    case "lfItalic":
                        lf.lfItalic = Convert.ToByte(f[1]);
                        break;
                    case "lfUnderline":
                        lf.lfUnderline = Convert.ToByte(f[1]);
                        break;
                    case "lfStrikeOut":
                        lf.lfStrikeOut = Convert.ToByte(f[1]);
                        break;
                    case "lfCharSet":
                        lf.lfCharSet = Convert.ToByte(f[1]);
                        break;
                    case "lfOutPrecision":
                        lf.lfOutPrecision = Convert.ToByte(f[1]);
                        break;
                    case "lfClipPrecision":
                        lf.lfClipPrecision = Convert.ToByte(f[1]);
                        break;
                    case "lfQuality":
                        lf.lfQuality = Convert.ToByte(f[1]);
                        break;
                    case "lfPitchAndFamily":
                        lf.lfPitchAndFamily = Convert.ToByte(f[1]);
                        break;
                }
            }
            return lf;
        }
    }
}