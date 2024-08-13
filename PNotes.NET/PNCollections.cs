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
using System.Collections.Generic;
using System.Drawing.Text;
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    internal sealed class PNCollections
    {
        private static readonly Lazy<PNCollections> _Lazy = new Lazy<PNCollections>(() => new PNCollections());

        private PNCollections()
        {
        }

        internal static PNCollections Instance => _Lazy.Value;

        internal List<string> Voices => Instance._Voices;

        internal List<string> Tags
        {
            get => Instance._Tags;
            set => Instance._Tags = value;
        }

        internal List<string> ActivePostPlugins
        {
            get => Instance._ActivePostPlugins;
            set => Instance._ActivePostPlugins = value;
        }

        internal List<string> ActiveSyncPlugins
        {
            get => Instance._ActiveSyncPlugins;
            set => Instance._ActiveSyncPlugins = value;
        }

        internal List<PNContact> Contacts
        {
            get => Instance._Contacts;
            set => Instance._Contacts = value;
        }

        internal List<PNContactGroup> ContactGroups
        {
            get => Instance._ContactGroups;
            set => Instance._ContactGroups = value;
        }

        internal List<PNExternal> Externals
        {
            get => Instance._Externals;
            set => Instance._Externals = value;
        }

        internal List<PNSearchProvider> SearchProviders
        {
            get => Instance._SearchProviders;
            set => Instance._SearchProviders = value;
        }

        internal List<PNSmtpProfile> SmtpProfiles
        {
            get => Instance._SmtpProfiles;
            set => Instance._SmtpProfiles = value;
        }

        internal List<PNMailContact> MailContacts
        {
            get => Instance._MailContacts;
            set => Instance._MailContacts = value;
        }

        internal List<PNSyncComp> SyncComps
        {
            get => Instance._SyncComps;
            set => Instance._SyncComps = value;
        }

        internal List<PNHotKey> HotKeysMain => Instance._HotKeysMain;

        internal List<PNHotKey> HotKeysNote => Instance._HotKeysNote;

        internal List<PNHotKey> HotKeysEdit => Instance._HotKeysEdit;

        internal List<PNHotKey> HotKeysGroups => Instance._HotKeysGroups;

        internal List<PNHiddenMenu> HiddenMenus => Instance._HiddenMenus;

        internal List<string> CustomFonts => Instance._CustomFonts;

        internal Dictionary<DockStatus, List<PNote>> DockedNotes => Instance._DockedNotes;

        internal Dictionary<ScheduleType, string> ScheduleDescriptions => Instance._ScheduleDescriptions;

        internal List<SettingsPanel> Panels => Instance._Panels;

        internal List<PNGroup> Groups => Instance._Groups;

        internal List<PNote> Notes => Instance._Notes;

        internal Dictionary<string, Tuple<Uri, Uri, BitmapImage, string, Version>> Themes => Instance._Themes;

        internal PrivateFontCollection PrivateFonts => Instance._PrivateFonts;

        internal Dictionary<string, DayOfWeek> DaysOfWeekPairs => Instance._DaysOfWeekPairs;

        private readonly List<string> _Voices = new List<string>();
        private List<string> _Tags = new List<string>();
        private List<string> _ActivePostPlugins = new List<string>();
        private List<string> _ActiveSyncPlugins = new List<string>();
        private List<PNContact> _Contacts = new List<PNContact>();
        private List<PNContactGroup> _ContactGroups = new List<PNContactGroup>();
        private List<PNExternal> _Externals = new List<PNExternal>();
        private List<PNSearchProvider> _SearchProviders = new List<PNSearchProvider>();
        private List<PNSmtpProfile> _SmtpProfiles = new List<PNSmtpProfile>();
        private List<PNMailContact> _MailContacts = new List<PNMailContact>();
        private List<PNSyncComp> _SyncComps = new List<PNSyncComp>();
        private readonly List<PNHotKey> _HotKeysMain = new List<PNHotKey>();
        private readonly List<PNHotKey> _HotKeysNote = new List<PNHotKey>();
        private readonly List<PNHotKey> _HotKeysEdit = new List<PNHotKey>();
        private readonly List<PNHotKey> _HotKeysGroups = new List<PNHotKey>();
        private readonly List<PNHiddenMenu> _HiddenMenus = new List<PNHiddenMenu>();
        private readonly List<string> _CustomFonts = new List<string>();
        private readonly Dictionary<DockStatus, List<PNote>> _DockedNotes = new Dictionary<DockStatus, List<PNote>>();
        private readonly Dictionary<ScheduleType, string> _ScheduleDescriptions = new Dictionary<ScheduleType, string>();
        private readonly List<SettingsPanel> _Panels = new List<SettingsPanel>();
        private readonly Dictionary<string, Tuple<Uri, Uri, BitmapImage, string, Version>> _Themes =
            new Dictionary<string, Tuple<Uri, Uri, BitmapImage, string, Version>>();
        private readonly PrivateFontCollection _PrivateFonts = new PrivateFontCollection();
        private readonly Dictionary<string, DayOfWeek> _DaysOfWeekPairs = new Dictionary<string, DayOfWeek>();
        private readonly List<PNGroup> _Groups = new List<PNGroup>();
        private readonly List<PNote> _Notes = new List<PNote>();
    }
}
