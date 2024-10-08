﻿// PNotes.NET - open source desktop notes manager
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
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace PNotes.NET
{
    internal sealed class PNSingleton
    {
        private static readonly Lazy<PNSingleton> _Lazy = new Lazy<PNSingleton>(() => new PNSingleton());

        private PNSingleton()
        {
        }

        internal static PNSingleton Instance => _Lazy.Value;

        private bool _InSkinReload;
        private bool _IsLocked;
        private bool _DoNotAskIfSave;
        private bool _ExitWithoutSaving;
        private string _UpdaterCommandLine;
        private bool _Restart;
        private bool _InOverdueChecking;
        private bool _PluginsDownload;
        private bool _PluginsChecking;
        private bool _ReportViewerDownload;
        private bool _AppClosed;
        private int _MonitorsCount;
        private Cursor _DropperCursor;
        private bool _CriticalChecking;
        private bool _VersionChecking;
        private Rect _ScreenRect;
        private bool _VersionChanged;
        private PNFont _FontUser;
        private IPAddress _IpAddress;
        private bool _ThemesChecking;
        private bool _ThemesDownload;
        private bool _IsMainWindowLoaded;
        private string _NoteFromShortcut;

        public Rect ScreenRect
        {
            get => Instance._ScreenRect;
            set => Instance._ScreenRect = value;
        }

        public bool VersionChecking
        {
            get => Instance._VersionChecking;
            set => Instance._VersionChecking = value;
        }

        public bool CriticalChecking
        {
            get => Instance._CriticalChecking;
            set => Instance._CriticalChecking = value;
        }

        public Cursor DropperCursor
        {
            get => Instance._DropperCursor;
            set
            {
                Instance._DropperCursor?.Dispose();
                Instance._DropperCursor = value;
            }
        }
        public bool AppClosed
        {
            get => Instance._AppClosed;
            set => Instance._AppClosed = value;
        }
        public bool PluginsChecking
        {
            get => Instance._PluginsChecking;
            set => Instance._PluginsChecking = value;
        }
        public bool PluginsDownload
        {
            get => Instance._PluginsDownload;
            set => Instance._PluginsDownload = value;
        }
        public bool InOverdueChecking
        {
            get => Instance._InOverdueChecking;
            set => Instance._InOverdueChecking = value;
        }
        public bool Restart
        {
            get => Instance._Restart;
            set => Instance._Restart = value;
        }
        public string UpdaterCommandLine
        {
            get => Instance._UpdaterCommandLine;
            set => Instance._UpdaterCommandLine = value;
        }
        public bool ExitWithoutSaving
        {
            get => Instance._ExitWithoutSaving;
            set => Instance._ExitWithoutSaving = value;
        }
        public bool DoNotAskIfSave
        {
            get => Instance._DoNotAskIfSave;
            set => Instance._DoNotAskIfSave = value;
        }
        public bool IsLocked
        {
            get => Instance._IsLocked;
            set => Instance._IsLocked = value;
        }

        public bool InSkinReload
        {
            get => Instance._InSkinReload;
            set => Instance._InSkinReload = value;
        }

        public int MonitorsCount
        {
            get => Instance._MonitorsCount;
            set => Instance._MonitorsCount = value;
        }

        public bool PlatformChanged
        {
            get => Instance._VersionChanged;
            set => Instance._VersionChanged = value;
        }

        public PNFont FontUser
        {
            get => Instance._FontUser;
            set => Instance._FontUser = value;
        }

        public IPAddress IpAddress
        {
            get => Instance._IpAddress;
            set => Instance._IpAddress = value;
        }

        public bool ThemesChecking
        {
            get => Instance._ThemesChecking;
            set => Instance._ThemesChecking = value;
        }

        public bool ThemesDownload
        {
            get => Instance._ThemesDownload;
            set => Instance._ThemesDownload = value;
        }

        public bool IsMainWindowLoaded
        {
            get => Instance._IsMainWindowLoaded;
            set => Instance._IsMainWindowLoaded = value;
        }

        public string NoteFromShortcut
        {
            get => Instance._NoteFromShortcut;
            set => Instance._NoteFromShortcut = value;
        }

        public bool ReportViewerDownload
        {
            get => Instance._ReportViewerDownload;
            set => Instance._ReportViewerDownload = value;
        }
    }
}
