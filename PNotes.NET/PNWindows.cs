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

namespace PNotes.NET
{
    internal sealed class PNWindows
    {
        private static readonly Lazy<PNWindows> _Lazy = new Lazy<PNWindows>(() => new PNWindows());

        private PNWindows()
        {
        }

        internal static PNWindows Instance => _Lazy.Value;

        internal WndMain FormMain
        {
            get => Instance._FormMain;
            set => Instance._FormMain = value;
        }

        internal WndSettings FormSettings
        {
            get => Instance._FormSettings;
            set => Instance._FormSettings = value;
        }

        internal WndHotkeys FormHotkeys
        {
            get => Instance._FormHotkeys;
            set => Instance._FormHotkeys = value;
        }

        internal WndMenusManager FormMenus
        {
            get => Instance._FormMenus;
            set => Instance._FormMenus = value;
        }

        internal WndCP FormCP
        {
            get => Instance._FormCP;
            set => Instance._FormCP = value;
        }

        internal WndSearchByDates FormSearchByDates
        {
            get => Instance._FormSearchByDates;
            set => Instance._FormSearchByDates = value;
        }

        internal WndSearchByTags FormSearchByTags
        {
            get => Instance._FormSearchByTags;
            set => Instance._FormSearchByTags = value;
        }

        internal WndSearchInNotes FormSearchInNotes
        {
            get => Instance._FormSearchInNotes;
            set => Instance._FormSearchInNotes = value;
        }

        internal WndPanel FormPanel
        {
            get => Instance._FormPanel;
            set => Instance._FormPanel = value;
        }

        internal WndConfigureReport FormConfigReport
        {
            get => Instance._FormConfigReport;
            set => Instance._FormConfigReport = value;
        }

        internal Dictionary<DockArrow, WndArrow> DockArrows => Instance._DockArrows;

        private WndMain _FormMain;
        private WndSettings _FormSettings;
        private WndHotkeys _FormHotkeys;
        private WndMenusManager _FormMenus;
        private WndCP _FormCP;
        private WndSearchByDates _FormSearchByDates;
        private WndSearchByTags _FormSearchByTags;
        private WndSearchInNotes _FormSearchInNotes;
        private WndPanel _FormPanel;
        private WndConfigureReport _FormConfigReport;

        private readonly Dictionary<DockArrow, WndArrow> _DockArrows = new Dictionary<DockArrow, WndArrow>();
    }
}
