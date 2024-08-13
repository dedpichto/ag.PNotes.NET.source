// PNotes.NET - open source desktop notes manager
// Copyright (C) 2017 Andrey Gruber

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
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PNotes.NET
{
    internal sealed class PNRuntimes
    {
        private static readonly Lazy<PNRuntimes> _Lazy = new Lazy<PNRuntimes>(() => new PNRuntimes());

        private PNRuntimes()
        {
        }

        internal static PNRuntimes Instance => _Lazy.Value;

        internal DateTime StartTime
        {
            get => Instance._StartTime;
            set => Instance._StartTime = value;
        }

        internal CultureInfo CultureInvariant
        {
            get => Instance._CultureInvariant;
            set => Instance._CultureInvariant = value;
        }

        internal XDocument Dictionaries
        {
            get => Instance._Dictionaries;
            set => Instance._Dictionaries = value;
        }

        internal SplashTextProvider SpTextProvider => Instance._SpTextProvider;

        internal Thread SplashThread
        {
            get => Instance._SplashThread;
            set => Instance._SplashThread = value;
        }

        internal bool HideSplash
        {
            get => Instance._HideSplash;
            set => Instance._HideSplash = value;
        }

        internal int SymbolsIndex
        {
            get => Instance._SymbolsIndex;
            set => Instance._SymbolsIndex = value;
        }

        internal RichTextBoxFinds FindOptions
        {
            get => Instance._FindOptions;
            set => Instance._FindOptions = value;
        }

        internal SearchMode SearchMode
        {
            get => Instance._SearchMode;
            set => Instance._SearchMode = value;
        }

        internal string FindString
        {
            get => Instance._FindString;
            set => Instance._FindString = value;
        }

        internal string ReplaceString
        {
            get => Instance._ReplaceString;
            set => Instance._ReplaceString = value;
        }

        internal PNGroup Docking => Instance._Docking;

        internal PNSettings Settings
        {
            get => Instance._Settings;
            set => Instance._Settings = value;
        }

        private DateTime _StartTime;
        private  CultureInfo _CultureInvariant;
        private XDocument _Dictionaries;
        private readonly SplashTextProvider _SpTextProvider = new SplashTextProvider();
        private Thread _SplashThread;
        private bool _HideSplash;
        private int _SymbolsIndex;
        private RichTextBoxFinds _FindOptions = RichTextBoxFinds.None;
        private SearchMode _SearchMode = SearchMode.Normal;
        private string _FindString = "";
        private string _ReplaceString = "";
        private readonly PNGroup _Docking = new PNGroup();
        private PNSettings _Settings = new PNSettings();
    }
}
