// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

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
using System.IO;
using System.Windows.Forms;

namespace PNotes.NET
{
    internal sealed class PNPaths
    {
        private static readonly Lazy<PNPaths> _Lazy = new Lazy<PNPaths>(() => new PNPaths());

        private PNPaths()
        {
        }

        internal static PNPaths Instance => _Lazy.Value;

        private string _LangDir = Application.StartupPath + @"\lang";
        private string _SkinsDir = Application.StartupPath + @"\skins";
        private string _SoundsDir = Application.StartupPath + @"\sounds";
        private string _SettingsDir = Application.StartupPath;
        private string _DataDir = Application.StartupPath + @"\data";
        private string _BackupDir = Application.StartupPath + @"\backup";
        private string _FontsDir = Application.StartupPath + @"\fonts";
        private string _DictDir = Application.StartupPath + @"\dictionaries";
        private string _PluginsDir = Application.StartupPath + @"\plugins";
        private string _DBPath = Application.StartupPath + @"\data\" + PNStrings.DB_FILE;
        private string _SettingsDBPath = Application.StartupPath + @"\" + PNStrings.SETTINGS_FILE;
        private readonly string _TempDir = Path.Combine(Path.GetTempPath(), "pnotestemp");
        private string _ThemesDir = Application.StartupPath + @"\themes";
        private string _ContactsDBPath = Application.StartupPath + @"\" + PNStrings.CONTACTS_FILE;
        private string _ContactsDir = Application.StartupPath;

        internal string PluginsDir
        {
            get => Instance._PluginsDir;
            set => Instance._PluginsDir = value;
        }
        internal string TempDir => Instance._TempDir;

        internal string SettingsDBPath
        {
            get => Instance._SettingsDBPath;
            set => Instance._SettingsDBPath = value;
        }
        internal string DBPath
        {
            get => Instance._DBPath;
            set => Instance._DBPath = value;
        }
        internal string DictDir
        {
            get => Instance._DictDir;
            set => Instance._DictDir = value;
        }
        internal string FontsDir
        {
            get => Instance._FontsDir;
            set => Instance._FontsDir = value;
        }
        internal string BackupDir
        {
            get => Instance._BackupDir;
            set => Instance._BackupDir = value;
        }
        internal string DataDir
        {
            get => Instance._DataDir;
            set { Instance._DataDir = value; Instance._DBPath = value + @"\" + PNStrings.DB_FILE; }
        }
        internal string SettingsDir
        {
            get => Instance._SettingsDir;
            set { Instance._SettingsDir = value; Instance._SettingsDBPath = value + @"\" + PNStrings.SETTINGS_FILE; }
        }
        internal string SoundsDir
        {
            get => Instance._SoundsDir;
            set => Instance._SoundsDir = value;
        }
        internal string SkinsDir
        {
            get => Instance._SkinsDir;
            set => Instance._SkinsDir = value;
        }
        internal string LangDir
        {
            get => Instance._LangDir;
            set => Instance._LangDir = value;
        }

        internal string ThemesDir
        {
            get => Instance._ThemesDir;
            set => Instance._ThemesDir = value;
        }

        internal string ContactsDBPath
        {
            get => Instance._ContactsDBPath;
            set => Instance._ContactsDBPath = value;
        }

        public string ContactsDir
        {
            get => Instance._ContactsDir;
            set { Instance._ContactsDir = value; Instance._ContactsDBPath = value + @"\" + PNStrings.CONTACTS_FILE; }
        }
    }
}
