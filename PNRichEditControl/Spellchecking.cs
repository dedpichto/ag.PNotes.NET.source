using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Xml.Linq;
using NHunspell;
using tom;
using Point = System.Drawing.Point;

namespace PNRichEdit
{
    /// <summary>
    /// Represents static class for spell checking using NHunspell library
    /// </summary>
    public class Spellchecking
    {
        /// <summary>
        /// Delimiters for spell checking and splitting text into tokens
        /// </summary>
        public const string PNRE_DELIMITERS = " -'.,:;`/\"+(){}[]<>*&^%$#@!?~/|\\= \t\n\r";

        private static readonly char[] Delimiters = PNRE_DELIMITERS.ToCharArray();
        //Hunspell obkect
        private static Hunspell _hunspell;
        //value specified whether Huspell has been initialized
        private static bool _initialized;
        //spell checking underlining color
        private static Color _colorUnderlining = Color.Red;
        //temporary variable for holding RichTextBox
        private static System.Windows.Forms.RichTextBox _edit;
        //path to custom dictionary
        private static string _customPath;
        //suggestions menus
        private static readonly ContextMenu SuggestionsMenu = new ContextMenu();

        private static readonly MenuItem MenuAddToDict = new MenuItem {Header = "Add to dictionary"};
        private static readonly MenuItem MenuNoSuggestion = new MenuItem {Header = "No suggestions"};

        //localization strings
        private static XElement _xLang = new XElement("spellchecking",
                                                       new XElement("DlgSpelling", "Spell checking"),
                                                       new XElement("lblNotInDic", "Not in dictionary"),
                                                       new XElement("lblSuggestions", "Suggestions"),
                                                       new XElement("cmdIgnoreOnce", "Ignore once"),
                                                       new XElement("cmdIgnoreAll", "Ignore all"),
                                                       new XElement("cmdAddToDict", "Add to dictionary"),
                                                       new XElement("cmdChange", "Change"),
                                                       new XElement("cmdChangeAll", "Change all"),
                                                       new XElement("cmdCancel", "Cancel"),
                                                       new XElement("msgComplete", "Spell checking complete"),
                                                       new XElement("mnuAddToDict", "Add to dictionary"),
                                                       new XElement("mnuNoSuggestions", "No suggestions"));

        private static bool? _checkComplete;
        /// <summary>
        /// Gets or sets spellchecking underliining color
        /// </summary>
        public static Color ColorUnderlining
        {
            get { return _colorUnderlining; }
            set { _colorUnderlining = value; }
        }

        /// <summary>
        /// Gets value specified whether Hunspell object has been initialized
        /// </summary>
        public static bool Initialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// Sets localization strings
        /// </summary>
        /// <param name="xe">XElement with required strings</param>
        public static void SetLocalization(XElement xe)
        {
            _xLang = xe;
            XElement x = _xLang.Element("mnuAddToDict");
            if (x != null)
            {
                MenuAddToDict.Header = x.Value;
            }
            x = _xLang.Element("mnuNoSuggestions");
            if (x != null)
            {
                MenuNoSuggestion.Header = x.Value;
            }
        }

        /// <summary>
        /// Initializes Hunspell object
        /// </summary>
        /// <param name="pathDic">DIC file path</param>
        /// <param name="pathAff">AFF file path</param>
        public static void HunspellInit(string pathDic, string pathAff)
        {
            if (_hunspell == null)
            {
                _hunspell = new Hunspell(pathAff, pathDic);
                _initialized = true;
                string name = Path.GetFileNameWithoutExtension(pathDic);
                string path = Path.GetDirectoryName(pathDic);
                if (path != null) _customPath = Path.Combine(path, name + ".custom");
                if (File.Exists(_customPath))
                {
                    using (var sr = new StreamReader(_customPath))
                    {
                        while (sr.Peek() != -1)
                        {
                            string word = sr.ReadLine();
                            if (word != null && word.Trim().Length > 0)
                            {
                                _hunspell.Add(word.Trim());
                            }
                        }
                    }
                }
                MenuAddToDict.Click += mnuAddToDict_Click;
                MenuNoSuggestion.IsEnabled = false;
            }
        }

        /// <summary>
        /// Dispose Hunspell object
        /// </summary>
        public static void HuspellStop()
        {
            if (_hunspell != null && !_hunspell.IsDisposed)
            {
                _hunspell.Dispose();
                _hunspell = null;
                _initialized = false;
            }
        }

        /// <summary>
        /// Gets list of Huspell suggestions
        /// </summary>
        /// <param name="word">Word to get suggestions for</param>
        /// <returns>List of Huspell suggestions</returns>
        internal static List<string> GetSuggestions(string word)
        {
            return _hunspell.Suggest(word);
        }

        /// <summary>
        /// Performs spell checking on right mouse click on RichTextBox control and shows list of suggestions if underlining word is not correct
        /// </summary>
        /// <param name="edit">RichTextBox control to perform spell checking for</param>
        /// <returns>True if underlining word is not correct and list of suggestions is shown, other wise - false</returns>
        public static bool CheckRESpellingOnRightClick(System.Windows.Forms.RichTextBox edit)
        {
            if (edit.TextLength == 0) return false;
            Point pt = edit.PointToClient(System.Windows.Forms.Cursor.Position);
            char c = edit.GetCharFromPosition(pt);
            int index = edit.GetCharIndexFromPosition(pt);
            SpellWord sw = null;
            Rectangle rc = edit.DisplayRectangle;
            int charPos = edit.GetCharIndexFromPosition(new Point(rc.X, rc.Y));
            int lineIndex = edit.GetLineFromCharIndex(charPos);
            int firstChar = edit.GetFirstCharIndexFromLine(lineIndex);
            int lastChar = edit.GetCharIndexFromPosition(new Point(rc.Right, rc.Bottom));

            if (!Delimiters.Contains(c))
            {
                string text = edit.Text.Substring(firstChar, lastChar - firstChar + 1);
                if (text.Length == 0 || index == text.Length - 1)
                {
                    return false;
                }
                string[] tokens = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                int position = 0;
                foreach (string token in tokens)
                {
                    position = text.IndexOf(token, position, StringComparison.Ordinal);
                    if (index >= position + firstChar && index <= position + firstChar + token.Length)
                    {
                        sw = new SpellWord(token, position + firstChar);
                        break;
                    }
                    position += token.Length;
                }
                if (sw != null)
                {
                    if (!_hunspell.Spell(sw.Word))
                    {
                        if (SuggestionsMenu.Items.Count > 2)
                        {
                            for (int i = SuggestionsMenu.Items.Count - 1; i >= 2; i--)
                            {
                                var item = SuggestionsMenu.Items[i] as MenuItem;
                                if (item != null)
                                    item.Click -= suggestion_Click;
                            }
                        }
                        SuggestionsMenu.Items.Clear();
                        MenuAddToDict.Tag = new SpellSuggestion(sw, edit, "");
                        SuggestionsMenu.Items.Add(MenuAddToDict);
                        SuggestionsMenu.Items.Add(new Separator());
                        List<string> suggestions = _hunspell.Suggest(sw.Word);
                        if (suggestions.Count > 0)
                        {
                            foreach (string s in suggestions)
                            {
                                var mi = new MenuItem()
                                {
                                    Header=s,
                                    Tag = new SpellSuggestion(sw, edit, s),
                                    IsEnabled = !edit.ReadOnly
                                };
                                mi.Click += suggestion_Click;
                                SuggestionsMenu.Items.Add(mi);
                            }
                        }
                        else
                        {
                            SuggestionsMenu.Items.Add(MenuNoSuggestion);
                        }
                        SuggestionsMenu.IsOpen = true;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Automatically checks spelling for RichTextBox control and underlines misspelled words
        /// </summary>
        /// <param name="edit">RichTextBox control to perform spell checking for</param>
        /// <param name="g">Graphics object to draw wavy line on (for misspelled words)</param>
        public static void CheckRESpellingAutomatically(System.Windows.Forms.RichTextBox edit, Graphics g)
        {
            IntPtr iRichOle;

            if (edit.TextLength == 0)
            {
                return;
            }
            if (RichEdit_GetOleInterface(new HandleRef(edit, edit.Handle), E_GETOLEINTERFACE, 0, out iRichOle))
            {
                var iDoc = (ITextDocument) Marshal.GetTypedObjectForIUnknown(iRichOle, typeof (ITextDocument));
                if (iDoc != null)
                {
                    Rectangle rc = edit.DisplayRectangle;

                    int charPos = edit.GetCharIndexFromPosition(new Point(rc.X, rc.Y));

                    int lineIndex = edit.GetLineFromCharIndex(charPos);
                    int firstChar = edit.GetFirstCharIndexFromLine(lineIndex);
                    int lastChar = edit.GetCharIndexFromPosition(new Point(rc.Right, rc.Bottom));

                    int position = 0;
                    var words = new List<SpellWord>();
                    string text = edit.Text.Substring(firstChar, lastChar - firstChar + 1);
                    string[] tokens = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string token in tokens)
                    {
                        if (!_hunspell.Spell(token))
                        {
                            position = text.IndexOf(token, position, StringComparison.Ordinal);
                            int posStart = position + firstChar;
                            int posEnd = posStart + token.Length;
                            if (
                                !(edit.SelectionLength == 0 && edit.SelectionStart >= posStart &&
                                  edit.SelectionStart <= posEnd))
                            {
                                words.Add(new SpellWord(token, position + firstChar));
                            }
                            position += token.Length;
                        }
                    }
                    foreach (SpellWord sw in words)
                    {
                        ITextRange iRange = iDoc.Range(sw.Position, sw.Position + sw.Word.Length - 1);
                        if (iRange != null)
                        {
                            var pts = new POINT[2];

                            iRange.GetPoint(32 | TA_BASELINE | TA_LEFT, out pts[0].x, out pts[0].y);
                            iRange.GetPoint(0 | TA_BASELINE | TA_RIGHT, out pts[1].x, out pts[1].y);
                            Point pt1 = edit.PointToClient(new Point(pts[0].x, pts[0].y + 1));
                            Point pt2 = edit.PointToClient(new Point(pts[1].x, pts[1].y + 1));
                            if (pt1.X < pt2.X && pt1.X >= 0 && pt1.Y >= 0 && pt2.X >= 0 && pt2.Y >= 0)
                            {
                                drawWavyLine(g, pt1, pt2);
                            }
                        }
                    }
                }
                Marshal.Release(iRichOle);
            }
        }

        /// <summary>
        /// Draws wavy line on given Graphics object using two points
        /// </summary>
        /// <param name="g">Graphics object to draw on</param>
        /// <param name="pt1">Start point</param>
        /// <param name="pt2">End point</param>
        private static void drawWavyLine(Graphics g, Point pt1, Point pt2)
        {
            int count = ((pt2.X - pt1.X)/4)*2 + 1;
            var points = new Point[count];
            for (int i = 0; i < count; i++)
            {
                points[i].X = pt1.X + 2*i;
                if (i%2 == 0)
                    points[i].Y = pt2.Y + 2;
                else
                    points[i].Y = pt2.Y;
            }
            if (count == 1)
            {
                Array.Resize(ref points, 3);
                points[1].X = points[0].X + 2;
                points[1].Y = points[0].Y - 2;
                points[2].X = points[1].X + 2;
                points[2].Y = points[0].Y;
            }
            using (var pen = new Pen(_colorUnderlining, 1))
            {
                g.DrawLines(pen, points);
            }
        }

        /// <summary>
        /// Checks spelling for given RichTextBox control and shows results in dialog
        /// </summary>
        /// <param name="edit">RichTextBox control to perform spell checking for</param>
        /// <param name="caption">Optional text to show in dialog caption, usually the name of document, may be empty string</param>
        /// <returns>True if the is any word for checking, false otherwise</returns>
        public static bool CheckRESpelling(System.Windows.Forms.RichTextBox edit, string caption)
        {
            _edit = edit;
            var words = new List<SpellWord>();
            int position = 0;
            string text = edit.Text;
            string[] tokens = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                if (!_hunspell.Spell(token))
                {
                    position = text.IndexOf(token, position, StringComparison.Ordinal);
                    words.Add(new SpellWord(token, position));
                    position += token.Length;
                }
            }

            if (words.Count > 0)
            {
                bool hideSelection = edit.HideSelection;
                edit.HideSelection = false;
                var dlgSpelling = new WndSpelling(words, _xLang, caption);
                dlgSpelling.SpellWordSelected += dlgSpelling_SpellWordSelected;
                dlgSpelling.SpellWordChanged += dlgSpelling_SpellWordChanged;
                dlgSpelling.SpellWordChangedAll += dlgSpelling_SpellWordChangedAll;
                _checkComplete = dlgSpelling.ShowDialog();
                edit.HideSelection = hideSelection;
                dlgSpelling.SpellWordSelected -= dlgSpelling_SpellWordSelected;
                dlgSpelling.SpellWordChanged -= dlgSpelling_SpellWordChanged;
                dlgSpelling.SpellWordChangedAll -= dlgSpelling_SpellWordChangedAll;
                return _checkComplete != null && _checkComplete.Value;
            }
            return true;
        }

        /// <summary>
        /// Adds word to custom dictionary
        /// </summary>
        /// <param name="word">Word to add</param>
        internal static void AddToDictionary(string word)
        {
            _hunspell.Add(word);
            using (var sw = new StreamWriter(_customPath, true))
            {
                sw.WriteLine(word.Trim());
            }
        }

        private static void dlgSpelling_SpellWordChangedAll(object sender, SpellWordChangedEventArgs e)
        {
            var pos = _edit.Find(e.SpellWord.Word, 0, System.Windows.Forms.RichTextBoxFinds.MatchCase | System.Windows.Forms.RichTextBoxFinds.WholeWord);
            while (pos >= 0)
            {
                _edit.Select(pos, e.SpellWord.Word.Length);
                _edit.SelectedText = e.NewWord;
                pos = _edit.Find(e.SpellWord.Word, pos, System.Windows.Forms.RichTextBoxFinds.MatchCase | System.Windows.Forms.RichTextBoxFinds.WholeWord);
            }
        }

        private static void dlgSpelling_SpellWordChanged(object sender, SpellWordChangedEventArgs e)
        {
            _edit.Select(e.SpellWord.Position, e.SpellWord.Word.Length);
            _edit.SelectedText = e.NewWord;
        }

        private static void dlgSpelling_SpellWordSelected(object sender, SpellWordSelectedEventArgs e)
        {
            _edit.Select(e.SpellWord.Position, e.SpellWord.Word.Length);
        }

        private static void mnuAddToDict_Click(object sender, RoutedEventArgs e)
        {
            var ss = MenuAddToDict.Tag as SpellSuggestion;
            if (ss == null) return;
            AddToDictionary(ss.SpellWord.Word);
            ss.Edit.Select(ss.SpellWord.Position, ss.SpellWord.Word.Length);
            ss.Edit.SelectedText = ss.SpellWord.Word;
            ss.Edit.Parent.Invalidate(true);
        }

        private static void suggestion_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;
            var ss = mi.Tag as SpellSuggestion;
            if (ss == null) return;
            ss.Edit.Select(ss.SpellWord.Position, ss.SpellWord.Word.Length);
            ss.Edit.SelectedText = ss.Suggestion;
        }

        #region API declarations

        private const int W_USER = 0x0400;
        private const int E_GETOLEINTERFACE = (W_USER + 60);

        private const int TA_LEFT = 0;
        private const int TA_RIGHT = 2;
        private const int TA_BASELINE = 24;

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern bool RichEdit_GetOleInterface(HandleRef hWnd, int msg, int wParam, out IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        #endregion
    }

    internal class SpellWord
    {
        private readonly int _Position;
        private readonly string _Word;

        internal SpellWord(string word, int position)
        {
            _Word = word;
            _Position = position;
        }

        internal string Word
        {
            get { return _Word; }
        }

        internal int Position
        {
            get { return _Position; }
        }

        public override string ToString()
        {
            return _Word;
        }
    }

    internal class SpellSuggestion
    {
        private readonly System.Windows.Forms.RichTextBox _Edit;
        private readonly SpellWord _SpellWord;
        private readonly string _Suggestion;

        internal SpellSuggestion(SpellWord sw, System.Windows.Forms.RichTextBox edit, string suggestion)
        {
            _SpellWord = sw;
            _Edit = edit;
            _Suggestion = suggestion;
        }

        internal string Suggestion
        {
            get { return _Suggestion; }
        }

        internal System.Windows.Forms.RichTextBox Edit
        {
            get { return _Edit; }
        }

        internal SpellWord SpellWord
        {
            get { return _SpellWord; }
        }
    }
}