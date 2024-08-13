using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml.Linq;

namespace PNRichEdit
{
    /// <summary>
    /// Interaction logic for WndSpelling.xaml
    /// </summary>
    public partial class WndSpelling
    {
        internal event EventHandler<SpellWordSelectedEventArgs> SpellWordSelected;
        internal event EventHandler<SpellWordChangedEventArgs> SpellWordChanged;
        internal event EventHandler<SpellWordChangedEventArgs> SpellWordChangedAll;

        /// <summary>
        /// Creates new instance of WndSpelling
        /// </summary>
        public WndSpelling()
        {
            InitializeComponent();

        }

        internal WndSpelling(List<SpellWord> words, XElement xe, string caption)
            : this()
        {
            SetResourceReference(StyleProperty, "CustomWindowStyle");
            m_Words = words;
            applyLanguage(xe);
            var x = xe.Element("msgComplete");
            if (x != null)
            {
                m_MessageComplete = x.Value;
            }
            m_Caption = caption;
        }

        private readonly List<SpellWord> m_Words;
        private readonly string m_MessageComplete;
        private readonly string m_Caption = "";

        private void checkWordsCount()
        {
            if (lstNotInDict.Items.Count != 0) return;
            DialogResult = true;
        }

        private void enableButtons()
        {
            cmdIgnoreOnce.IsEnabled = cmdIgnoreAll.IsEnabled = cmdAddToDict.IsEnabled = lstNotInDict.SelectedIndex > -1;
            cmdChange.IsEnabled = cmdChangeAll.IsEnabled = lstNotInDict.SelectedIndex > -1 && lstSuggestions.SelectedIndex > -1;
        }

        private void applyLanguage(XElement xe)
        {
            var xc = xe.Element(lblNotInDic.Name);
            if (xc != null)
                lblNotInDic.Text = xc.Value;
            xc = xe.Element(lblSuggestions.Name);
            if (xc != null)
                lblSuggestions.Text = xc.Value;
            xc = xe.Element(cmdAddToDict.Name);
            if (xc != null)
                cmdAddToDict.Content = xc.Value;
            xc = xe.Element(cmdCancel.Name);
            if (xc != null)
                cmdCancel.Content = xc.Value;
            xc = xe.Element(cmdChange.Name);
            if (xc != null)
                cmdChange.Content = xc.Value;
            xc = xe.Element(cmdChangeAll.Name);
            if (xc != null)
                cmdChangeAll.Content = xc.Value;
            xc = xe.Element(cmdIgnoreAll.Name);
            if (xc != null)
                cmdIgnoreAll.Content = xc.Value;
            xc = xe.Element(cmdIgnoreOnce.Name);
            if (xc != null)
                cmdIgnoreOnce.Content = xc.Value;
        }

        private void cmdIgnoreOnce_Click(object sender, RoutedEventArgs e)
        {
            if (lstNotInDict.SelectedIndex > -1)
            {
                int index = lstNotInDict.SelectedIndex;
                lstNotInDict.Items.RemoveAt(index);
                if (lstNotInDict.Items.Count > 0)
                {
                    if (index == lstNotInDict.Items.Count)
                    {
                        lstNotInDict.SelectedIndex = index - 1;
                    }
                    else
                    {
                        lstNotInDict.SelectedIndex = index;
                    }
                }
                checkWordsCount();
            }
            enableButtons();
        }

        private void cmdIgnoreAll_Click(object sender, RoutedEventArgs e)
        {
            if (lstNotInDict.SelectedIndex > -1)
            {
                int index = lstNotInDict.SelectedIndex;
                string word = lstNotInDict.Items[index].ToString();
                for (int i = lstNotInDict.Items.Count - 1; i >= 0; i--)
                {
                    if (lstNotInDict.Items[i].ToString() == word)
                    {
                        lstNotInDict.Items.RemoveAt(i);
                    }
                }
                if (lstNotInDict.Items.Count > 0)
                {
                    lstNotInDict.SelectedIndex = 0;
                }
                checkWordsCount();
            }
            enableButtons();
        }

        private void cmdAddToDict_Click(object sender, RoutedEventArgs e)
        {
            if (lstNotInDict.SelectedIndex > -1)
            {
                string word = lstNotInDict.Items[lstNotInDict.SelectedIndex].ToString();
                for (int i = lstNotInDict.Items.Count - 1; i >= 0; i--)
                {
                    if (lstNotInDict.Items[i].ToString() == word)
                    {
                        lstNotInDict.Items.RemoveAt(i);
                    }
                }
                Spellchecking.AddToDictionary(word);
                if (lstNotInDict.Items.Count > 0)
                {
                    lstNotInDict.SelectedIndex = 0;
                }
                checkWordsCount();
            }
            enableButtons();
        }

        private void cmdChange_Click(object sender, RoutedEventArgs e)
        {
            if (lstNotInDict.SelectedIndex > -1 && lstSuggestions.SelectedIndex > -1)
            {
                int index = lstNotInDict.SelectedIndex;
                var sw = lstNotInDict.Items[index] as SpellWord;
                string newWord = lstSuggestions.Items[lstSuggestions.SelectedIndex].ToString();
                if (SpellWordChanged != null)
                {
                    SpellWordChanged(this, new SpellWordChangedEventArgs(sw, newWord));
                }
                lstNotInDict.Items.RemoveAt(index);
                if (lstNotInDict.Items.Count > 0)
                {
                    if (index == lstNotInDict.Items.Count)
                    {
                        lstNotInDict.SelectedIndex = index - 1;
                    }
                    else
                    {
                        lstNotInDict.SelectedIndex = index;
                    }
                }
                checkWordsCount();
            }
            enableButtons();
        }

        private void cmdChangeAll_Click(object sender, RoutedEventArgs e)
        {
            if (lstNotInDict.SelectedIndex > -1 && lstSuggestions.SelectedIndex > -1)
            {
                int index = lstNotInDict.SelectedIndex;
                var sw = lstNotInDict.Items[index] as SpellWord;
                string newWord = lstSuggestions.Items[lstSuggestions.SelectedIndex].ToString();
                if (SpellWordChangedAll != null)
                {
                    SpellWordChangedAll(this, new SpellWordChangedEventArgs(sw, newWord));
                }
                for (int i = lstNotInDict.Items.Count - 1; i >= 0; i--)
                {
                    if (sw != null && lstNotInDict.Items[i].ToString() == sw.Word)
                    {
                        lstNotInDict.Items.RemoveAt(i);
                    }
                }
                if (lstNotInDict.Items.Count > 0)
                {
                    lstNotInDict.SelectedIndex = 0;
                }
                checkWordsCount();
            }
            enableButtons();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_Caption != "")
            {
                Title += " [" + m_Caption + "]";
            }
            foreach (var sw in m_Words)
            {
                lstNotInDict.Items.Add(sw);
            }
        }

        private void lstNotInDict_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstSuggestions.Items.Clear();
            if (lstNotInDict.SelectedIndex > -1)
            {
                var sw = lstNotInDict.SelectedItem as SpellWord;
                if (SpellWordSelected != null)
                {
                    SpellWordSelected(this, new SpellWordSelectedEventArgs(sw));
                }
                if (sw != null)
                {
                    List<string> suggestions = Spellchecking.GetSuggestions(sw.Word);
                    foreach (string s in suggestions)
                    {
                        lstSuggestions.Items.Add(s);
                    }
                }
            }
            enableButtons();
        }

        private void lstSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            enableButtons();
        }
    }
    
    internal class SpellWordSelectedEventArgs : EventArgs
    {
        private readonly SpellWord m_SpellWord;
        internal SpellWordSelectedEventArgs(SpellWord sw)
        {
            m_SpellWord = sw;
        }
        internal SpellWord SpellWord
        {
            get { return m_SpellWord; }
        }
    }

    internal class SpellWordChangedEventArgs : EventArgs
    {
        private readonly SpellWord m_SpellWord;
        private readonly string m_NewWord = "";
        internal SpellWordChangedEventArgs(SpellWord sw, string nw)
        {
            m_SpellWord = sw;
            m_NewWord = nw;
        }
        internal string NewWord
        {
            get { return m_NewWord; }
        }
        internal SpellWord SpellWord
        {
            get { return m_SpellWord; }
        }
    }
}
