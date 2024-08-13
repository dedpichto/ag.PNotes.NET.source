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

using PNotes.NET.Annotations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PNotes.NET
{
    internal enum CommandType
    {
        None,
        NewNote,
        NewNoteInGroup,
        LoadNote,
        NoteFromClipboard,
        DuplicateNote,
        SaveAsShortcut,
        Diary,
        Today,
        Preferences,
        ControlPanel,
        Panel,
        ToPanelAll,
        FromPanelAll,
        ShowHide,
        ShowGroups,
        ShowIncoming,
        HideGroups,
        HideIncoming,
        LastModifiedNotes,
        LastToday,
        Last1,
        Last2,
        Last3,
        Last4,
        Last5,
        Last6,
        Last7,
        SaveNote,
        SaveAs,
        SaveAsText,
        RestoreFromBackup,
        Print,
        Adjust,
        AdjustAppearance,
        AdjustSchedule,
        DeleteNote,
        ToPanel,
        SaveAll,
        BackupSync,
        BackupCreate,
        BackupRestore,
        SyncLocal,
        ImportNotes,
        ImportSettings,
        ImportFonts,
        ImportDictionaries,
        ExpPdf,
        ExpTif,
        ExpDoc,
        ExpRtf,
        ExpTxt,
        ExpAll,
        PrintFields,
        ReloadAll,
        Placement,
        DockAll,
        DockNote,
        DockAllNone,
        DockAllLeft,
        DockAllTop,
        DockAllRight,
        DockAllBottom,
        Visibility,
        ShowNote,
        ShowAllOnCp,
        HideNote,
        HideAllOnCp,
        AllToFront,
        Centralize,
        SendAsText,
        SendAsAttachment,
        SendAsZip,
        SendNetwork,
        ContactAdd,
        ContactGroupAdd,
        ContactSelect,
        ContactGroupSelect,
        Reply,
        ToPost,
        FromPost,
        ExportToOffice,
        ExportToOutlookNote,
        Tags,
        TagsCurrent,
        TagsShowBy,
        TagsHideBy,
        Switches,
        OnTop,
        HighPriority,
        ProtectionMode,
        SetNotePassword,
        RemoveNotePassword,
        MarkAsComplete,
        RollUnroll,
        Pin,
        Unpin,
        Scramble,
        Unscramble,
        AddFavorites,
        RemoveFavorites,
        Run,
        EmptyBin,
        RestoreNote,
        Preview,
        PreviewFromMenu,
        PreviewSettings,
        UseCustColor,
        ChooseCustColor,
        PreviewRight,
        PreviewRightFromMenu,
        ShowGroupsOnCp,
        ShowGroupsOnCpFromMenu,
        ColReset,
        HotkeysCP,
        MenusManagementCP,
        Password,
        Search,
        QuickSearch,
        Help,
        Support,
        About,
        PwrdCreate,
        PwrdChange,
        PwrdRemove,
        SearchInNotes,
        SearchByTags,
        SearchByDates,
        IncBinInQSearch,
        ClearQSearch,
        QSearchIgnoreCase,
        GroupAdd,
        GroupAddSubgroup,
        GroupEdit,
        GroupRemove,
        GroupShow,
        GroupShowAll,
        GroupHide,
        GroupHideAll,
        GroupPassAdd,
        GroupPassRemove,
        Ok,
        Cancel,
        Find,
        FindNext,
        Replace,
        ReplaceAll,
        ClearSearchHistory,
        ClearSearchSettings,
        StandardView,
        SkinlessCaptionFont,
        GroupIcon,
        Up,
        Down,
        RestoreOrder,
        ResetAll,
        ResetCurrent,
        BrowseButton,
        LoadImport,
        Download,
        OpenInBrowser,
        LoadFromFile,
        MoveLeft,
        MoveRight,
        Dummy,
        Save,
        Apply,
        DefaultSettings,
        ChangeFont,
        RestoreFont,
        CheckUpdate,
        Question,
        DefaultVoice,
        VoiceSample,
        CheckConnection,
        CheckMessages,
        CheckPluginsUpdate,
        RemovePlugin,
        SyncNow,
        ImportOutlook,
        ImportGmail,
        ImportLotus,
        LinkedNotes,
        LinkedManagement,
        Undo,
        Redo,
        Cut,
        Copy,
        Paste,
        CopyPlain,
        PastePlain,
        Format,
        ToUpper,
        ToLower,
        CapitalSentence,
        CapitalWord,
        ToggleCase,
        Font,
        FontSize,
        FontColor,
        Bold,
        Italic,
        Underline,
        Strikethrough,
        Highlight,
        AlignLeft,
        AlignCenter,
        AlignRight,
        Bullets,
        LineSpacing,
        Space10,
        Space15,
        Space20,
        Space30,
        AddSpaceBefore,
        RemoveSpaceBefore,
        AddSpaceAfter,
        RemoveSpaceAfter,
        Subscript,
        Superscript,
        ClearFormat,
        IncreaseIndent,
        DecreaseIndent,
        Insert,
        InsertPicture,
        InsertSmilie,
        InsertDateTime,
        InsertTable,
        InsertSpecialSymbol,
        InsertDrawing,
        InsertLink,
        Spell,
        CheckSpellNow,
        CheckSpellAuto,
        DownloadDict,
        SearchWeb,
        PostSelectedtOn,
        ReplaceFromPost,
        SelectAll,
        SortAsc,
        SortDesc,
        NoBulletsMenu,
        SimpleBulletsMenu,
        BulletsMenu,
        FontSizeMenu,
        AutomaticColor,
        NoHighlight,
        ColorMenu,
        DropInsertContent,
        DropInsertObject,
        DropInsertLink,
        SwitchOnAll,
        SwitchOnHighPriority,
        SwitchOnProtection,
        SwitchOnComplete,
        SwitchOnRoll,
        SwitchOnOnTop,
        SwitchOffAll,
        SwitchOffHighPriority,
        SwitchOffProtection,
        SwitchOffComplete,
        SwitchOffRoll,
        SwitchOffOnTop,
        SpecialOptions,
        OnScreenKeyboard,
        Magnifier,
        SearchInMain,
        FavoritesMain,
        ShowAllFavorites,
        LockProgram,
        Homepage,
        Exit
    }

    public class DropCommands
    {
        static DropCommands()
        {
            DropInsertContentCommand = new PNRoutedUICommand("Insert File Content", "mnuInsertContent", typeof(DropCommands))
            {
                Section = "insert_menu",
                Type = CommandType.DropInsertContent
            };
            DropInsertObjectCommand = new PNRoutedUICommand("Insert As Object", "mnuInsertObject", typeof(DropCommands))
            {
                Section = "insert_menu",
                Type = CommandType.DropInsertObject
            };
            DropInsertLinkCommand = new PNRoutedUICommand("Insert Link To File", "mnuInsertLink", typeof(DropCommands))
            {
                Section = "insert_menu",
                Type = CommandType.DropInsertLink
            };

        }

        public static PNRoutedUICommand DropInsertLinkCommand { get; }

        public static PNRoutedUICommand DropInsertObjectCommand { get; }

        public static PNRoutedUICommand DropInsertContentCommand { get; }
    }

    public class EditCommands
    {
        static EditCommands()
        {
            SortDescCommand = new PNRoutedUICommand("Sort Descending", "mnuSortDescending", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.SortDesc
            };
            SortAscCommand = new PNRoutedUICommand("Sort Ascending", "mnuSortAscending", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.SortAsc
            };
            PosSelectedtOnCommand = new PNRoutedUICommand("Post Selected On:", "mnuPostOn", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.PostSelectedtOn
            };
            ReplaceFromPostCommand = new PNRoutedUICommand("Insert Post From:", "mnuInsertPost", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.ReplaceFromPost
            };
            SelectAllCommand = new PNRoutedUICommand("Select All", "mnuSelectAll", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.SelectAll
            };
            SearchWebCommand = new PNRoutedUICommand("Search Selected On:", "mnuSearchWeb", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.SearchWeb
            };
            SpellCommand = new PNRoutedUICommand("Spell Checking", "mnuSpell", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Spell
            };
            CheckSpellNowCommand = new PNRoutedUICommand("Check Now", "mnuCheckSpellNow", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.CheckSpellNow
            };
            CheckSpellAutoCommand = new PNRoutedUICommand("Check Automatically", "mnuCheckSpellAuto", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.CheckSpellAuto
            };
            DownloadDictCommand = new PNRoutedUICommand("Download Dictionaries", "mnuDownloadDict", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.DownloadDict
            };
            InsertLinkCommand = new PNRoutedUICommand("Link To Other Note", "mnuInsertLinkToNote", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.InsertLink
            };
            InsertDrawingCommand = new PNRoutedUICommand("Insert Free Drawing", "mnuDrawing", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.InsertDrawing
            };
            InsertSpecialSymbolCommand = new PNRoutedUICommand("Insert Special Symbol", "mnuInsertSpecialSymbol", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.InsertSpecialSymbol
            };
            InsertTableCommand = new PNRoutedUICommand("Insert Table", "mnuInsertTable", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.InsertTable
            };
            InsertDateTimeCommand = new PNRoutedUICommand("Insert Date/Time", "mnuInsertDT", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.InsertDateTime
            };
            InsertSmilieCommand = new PNRoutedUICommand("Insert Smilie", "mnuInsertSmiley", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.InsertSmilie
            };
            InsertPictureCommand = new PNRoutedUICommand("Insert Picture", "mnuInsertPicture", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.InsertPicture
            };
            InsertCommand = new PNRoutedUICommand("Insert", "mnuInsert", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Insert
            };
            DecreaseIndentCommand = new PNRoutedUICommand("Decrease Indent", "mnuDecreaseIndent", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.DecreaseIndent
            };
            IncreaseIndentCommand = new PNRoutedUICommand("Increase Indent", "mnuIncreaseIndent", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.IncreaseIndent
            };
            ClearFormatCommand = new PNRoutedUICommand("Clear All Formatting", "mnuClearFormat", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.ClearFormat
            };
            SuperscriptCommand = new PNRoutedUICommand("Superscript", "mnuSuperscript", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Superscript
            };
            SubscriptCommand = new PNRoutedUICommand("Subscript", "mnuSubscript", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Subscript
            };
            RemoveSpaceAfterCommand = new PNRoutedUICommand("Remove Space After Paragraph", "mnuRemoveSpaceAfter", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.RemoveSpaceAfter
            };
            AddSpaceAfterCommand = new PNRoutedUICommand("Add Space After Paragraph", "mnuAddSpaceAfter", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.AddSpaceAfter
            };
            RemoveSpaceBeforeCommand = new PNRoutedUICommand("Remove Space Before Paragraph", "mnuRemoveSpaceBefore", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.RemoveSpaceBefore
            };
            AddSpaceBeforeCommand = new PNRoutedUICommand("Add Space Before Paragraph", "mnuAddSpaceBefore", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.AddSpaceBefore
            };
            Space30Command = new PNRoutedUICommand("3.0", "mnuSpace30", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Space30
            };
            Space20Command = new PNRoutedUICommand("2.0", "mnuSpace20", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Space20
            };
            Space15Command = new PNRoutedUICommand("1.5", "mnuSpace15", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Space15
            };
            Space10Command = new PNRoutedUICommand("1.0", "mnuSpace10", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Space10
            };
            LineSpacingCommand = new PNRoutedUICommand("Line Spacing", "mnuLineSpacing", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.LineSpacing
            };
            BulletsCommand = new PNRoutedUICommand("Bullets/Numbering", "mnuBulletsNumbering", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Bullets
            };
            AlignRightCommand = new PNRoutedUICommand("Align Right", "mnuAlignRight", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.AlignRight
            };
            AlignCenterCommand = new PNRoutedUICommand("Center", "mnuAlignCenter", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.AlignCenter
            };
            AlignLeftCommand = new PNRoutedUICommand("Align Left", "mnuAlignLeft", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.AlignLeft
            };
            HighlightCommand = new PNRoutedUICommand("Highlight", "mnuHighlight", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Highlight
            };
            StrikethroughCommand = new PNRoutedUICommand("Strikethrough", "mnuStrikethrough", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Strikethrough
            };
            UnderlineCommand = new PNRoutedUICommand("Underline", "mnuUnderline", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Underline
            };
            ItalicCommand = new PNRoutedUICommand("Italic", "mnuItalic", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Italic
            };
            BoldCommand = new PNRoutedUICommand("Bold", "mnuBold", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Bold
            };
            FontColorCommand = new PNRoutedUICommand("Font Color", "mnuFontColor", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.FontColor
            };
            FontSizeCommand = new PNRoutedUICommand("Font Size", "mnuFontSize", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.FontSize
            };
            FontCommand = new PNRoutedUICommand("Font", "mnuFont", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Font
            };
            ToggleCaseCommand = new PNRoutedUICommand("Toggle Case", "mnuToggleCase", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.ToggleCase
            };
            CapitalWordCommand = new PNRoutedUICommand("Capitalize Words", "mnuCapWord", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.CapitalWord
            };
            CapitalSentenceCommand = new PNRoutedUICommand("Capitalize Sentences", "mnuCapSent", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.CapitalSentence
            };
            ToLowerCommand = new PNRoutedUICommand("Convert To Lowercase", "mnuToLower", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.ToLower
            };
            ToUpperCommand = new PNRoutedUICommand("Convert To Uppercase", "mnuToUpper", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.ToUpper
            };
            FormatCommand = new PNRoutedUICommand("Format", "mnuFormat", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Format
            };
            PastePlainCommand = new PNRoutedUICommand("Paste As Plain Text", "mnuPastePlain", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.PastePlain
            };
            CopyPlainCommand = new PNRoutedUICommand("Copy As Plain Text", "mnuCopyPlain", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.CopyPlain
            };
            PasteCommand = new PNRoutedUICommand("Paste", "mnuPaste", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Paste
            };
            CopyCommand = new PNRoutedUICommand("Copy", "mnuCopy", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Copy
            };
            CutCommand = new PNRoutedUICommand("Cut", "mnuCut", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Cut
            };
            RedoCommand = new PNRoutedUICommand("Redo", "mnuRedo", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Redo
            };
            UndoCommand = new PNRoutedUICommand("Undo", "mnuUndo", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.Undo
            };
            NoBulletsCommand = new PNRoutedUICommand("No Bullets/Numbering", "mnuNoBullets", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.NoBulletsMenu
            };
            SimpleBulletsMenuCommand = new PNRoutedUICommand("Simple Bullets", "mnuBullets", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.SimpleBulletsMenu
            };
            BulletsMenuCommand = new PNRoutedUICommand("", "__bullets__", typeof(EditCommands))
            {
                Type = CommandType.BulletsMenu
            };
            FontSizeMenuCommand = new PNRoutedUICommand("", "__font_size__", typeof(EditCommands))
            {
                Type = CommandType.FontSizeMenu
            };
            AutomaticColorCommand = new PNRoutedUICommand("Automatic", "mnuColorAutomatic", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.AutomaticColor
            };
            NoHighlightCommand = new PNRoutedUICommand("No Fill", "mnuNoHighlight", typeof(EditCommands))
            {
                Section = "edit_menu",
                Type = CommandType.NoHighlight
            };
            ColorMenuCommand = new PNRoutedUICommand("", "__color__", typeof(EditCommands))
            {
                Type = CommandType.ColorMenu
            };

        }

        public static PNRoutedUICommand ColorMenuCommand { get; }

        public static PNRoutedUICommand NoHighlightCommand { get; }

        public static PNRoutedUICommand AutomaticColorCommand { get; }

        public static PNRoutedUICommand FontSizeMenuCommand { get; }

        public static PNRoutedUICommand BulletsMenuCommand { get; }

        public static PNRoutedUICommand SimpleBulletsMenuCommand { get; }

        public static PNRoutedUICommand NoBulletsCommand { get; }

        public static PNRoutedUICommand UndoCommand { get; }

        public static PNRoutedUICommand RedoCommand { get; }

        public static PNRoutedUICommand CutCommand { get; }

        public static PNRoutedUICommand CopyCommand { get; }

        public static PNRoutedUICommand PasteCommand { get; }

        public static PNRoutedUICommand CopyPlainCommand { get; }

        public static PNRoutedUICommand PastePlainCommand { get; }

        public static PNRoutedUICommand FormatCommand { get; }

        public static PNRoutedUICommand ToUpperCommand { get; }

        public static PNRoutedUICommand ToLowerCommand { get; }

        public static PNRoutedUICommand CapitalSentenceCommand { get; }

        public static PNRoutedUICommand CapitalWordCommand { get; }

        public static PNRoutedUICommand ToggleCaseCommand { get; }

        public static PNRoutedUICommand FontCommand { get; }

        public static PNRoutedUICommand FontSizeCommand { get; }

        public static PNRoutedUICommand FontColorCommand { get; }

        public static PNRoutedUICommand BoldCommand { get; }

        public static PNRoutedUICommand ItalicCommand { get; }

        public static PNRoutedUICommand UnderlineCommand { get; }

        public static PNRoutedUICommand StrikethroughCommand { get; }

        public static PNRoutedUICommand HighlightCommand { get; }

        public static PNRoutedUICommand AlignLeftCommand { get; }

        public static PNRoutedUICommand AlignCenterCommand { get; }

        public static PNRoutedUICommand AlignRightCommand { get; }

        public static PNRoutedUICommand BulletsCommand { get; }

        public static PNRoutedUICommand LineSpacingCommand { get; }

        public static PNRoutedUICommand Space10Command { get; }

        public static PNRoutedUICommand Space15Command { get; }

        public static PNRoutedUICommand Space20Command { get; }

        public static PNRoutedUICommand Space30Command { get; }

        public static PNRoutedUICommand AddSpaceBeforeCommand { get; }

        public static PNRoutedUICommand RemoveSpaceBeforeCommand { get; }

        public static PNRoutedUICommand AddSpaceAfterCommand { get; }

        public static PNRoutedUICommand RemoveSpaceAfterCommand { get; }

        public static PNRoutedUICommand SubscriptCommand { get; }

        public static PNRoutedUICommand SuperscriptCommand { get; }

        public static PNRoutedUICommand ClearFormatCommand { get; }

        public static PNRoutedUICommand IncreaseIndentCommand { get; }

        public static PNRoutedUICommand DecreaseIndentCommand { get; }

        public static PNRoutedUICommand InsertCommand { get; }

        public static PNRoutedUICommand InsertPictureCommand { get; }

        public static PNRoutedUICommand InsertSmilieCommand { get; }

        public static PNRoutedUICommand InsertDateTimeCommand { get; }

        public static PNRoutedUICommand InsertTableCommand { get; }

        public static PNRoutedUICommand InsertSpecialSymbolCommand { get; }

        public static PNRoutedUICommand InsertDrawingCommand { get; }

        public static PNRoutedUICommand InsertLinkCommand { get; }

        public static PNRoutedUICommand SpellCommand { get; }

        public static PNRoutedUICommand CheckSpellNowCommand { get; }

        public static PNRoutedUICommand CheckSpellAutoCommand { get; }

        public static PNRoutedUICommand DownloadDictCommand { get; }

        public static PNRoutedUICommand SearchWebCommand { get; }

        public static PNRoutedUICommand PosSelectedtOnCommand { get; }

        public static PNRoutedUICommand ReplaceFromPostCommand { get; }

        public static PNRoutedUICommand SelectAllCommand { get; }

        public static PNRoutedUICommand SortAscCommand { get; }

        public static PNRoutedUICommand SortDescCommand { get; }
    }

    public class OkCancelCommands
    {
        static OkCancelCommands()
        {
            OkCommand = new PNRoutedUICommand("OK", "cmdOK", typeof(OkCancelCommands)) { Type = CommandType.Ok };
            CancelCommand =
                new PNRoutedUICommand("Cancel", "cmdCancel", typeof(OkCancelCommands)) { Type = CommandType.Cancel };
        }

        public static PNRoutedUICommand OkCommand { get; }

        public static PNRoutedUICommand CancelCommand { get; }
    }

    public class SettingsCommands
    {
        static SettingsCommands()
        {
            SaveCommand = new PNRoutedUICommand("Save", "cmdSave", typeof(SettingsCommands)) { Type = CommandType.Save };
            ApplyCommand =
                new PNRoutedUICommand("Apply", "cmdApply", typeof(SettingsCommands)) { Type = CommandType.Apply };
            DefaultSettingsCommand = new PNRoutedUICommand("Default settings", "cmdDef", typeof(SettingsCommands))
            {
                Type = CommandType.DefaultSettings
            };
            ChangeFontCommand = new PNRoutedUICommand("Change", "cmdSetFontUI", typeof(SettingsCommands))
            {
                Type = CommandType.ChangeFont,
                Section = "settings_menu"
            };
            RestoreFontCommand = new PNRoutedUICommand("Restore", "cmdResetFontUI", typeof(SettingsCommands))
            {
                Type = CommandType.RestoreFont,
                Section = "settings_menu"
            };
            CheckUpdateCommand = new PNRoutedUICommand("Check now", "cmdNewVersion", typeof(SettingsCommands))
            {
                Type = CommandType.CheckUpdate
            };
            DefaultVoiceCommand = new PNRoutedUICommand("Default voice", "cmdDefVoice", typeof(SettingsCommands))
            {
                Type = CommandType.DefaultVoice
            };
            VoiceSampleCommand = new PNRoutedUICommand("Listen sample", "cmdVoiceSample", typeof(SettingsCommands))
            {
                Type = CommandType.VoiceSample
            };
            HotkeysCommand = new PNRoutedUICommand("Hot keys management", "cmdHotkeys", typeof(SettingsCommands))
            {
                Type = CommandType.HotkeysCP
            };
            MenusManagementCommand = new PNRoutedUICommand("Menus management", "cmdMenus", typeof(SettingsCommands))
            {
                Type = CommandType.MenusManagementCP
            };
            CheckConnectionCommand = new PNRoutedUICommand("Test connection", "cmdCheckConnection", typeof(SettingsCommands))
            {
                Type = CommandType.CheckConnection
            };
            CheckMessagesCommand = new PNRoutedUICommand("Check for messages", "cmdCheckForNotes", typeof(SettingsCommands))
            {
                Type = CommandType.CheckConnection
            };
            CheckPluginsCommand = new PNRoutedUICommand("Check for new versions and new plugins", "cmdCheckSocPlugUpdate", typeof(SettingsCommands))
            {
                Type = CommandType.CheckPluginsUpdate
            };
            RemovePluginCommand = new PNRoutedUICommand("Remove plugin", "cmdRemovePostPlugin", typeof(SettingsCommands))
            {
                Type = CommandType.RemovePlugin
            };
            SyncNowCommand = new PNRoutedUICommand("Synchronize now", "cmdSyncNow", typeof(SettingsCommands))
            {
                Type = CommandType.SyncNow
            };
            ImportOutlookCommand = new PNRoutedUICommand("MS Outlook", "mnuImpOutlook", typeof(SettingsCommands))
            {
                Type = CommandType.ImportOutlook
            };
            ImportGMailCommand = new PNRoutedUICommand("GMail", "mnuImpGmail", typeof(SettingsCommands))
            {
                Type = CommandType.ImportGmail
            };
            ImportLotusCommand = new PNRoutedUICommand("IBM Notes", "mnuImpLotus", typeof(SettingsCommands))
            {
                Type = CommandType.ImportLotus
            };

        }

        public static PNRoutedUICommand SaveCommand { get; }

        public static PNRoutedUICommand ApplyCommand { get; }

        public static PNRoutedUICommand DefaultSettingsCommand { get; }

        public static PNRoutedUICommand ChangeFontCommand { get; }

        public static PNRoutedUICommand RestoreFontCommand { get; }

        public static PNRoutedUICommand CheckUpdateCommand { get; }

        public static PNRoutedUICommand DefaultVoiceCommand { get; }

        public static PNRoutedUICommand VoiceSampleCommand { get; }

        public static PNRoutedUICommand HotkeysCommand { get; }

        public static PNRoutedUICommand MenusManagementCommand { get; }

        public static PNRoutedUICommand CheckConnectionCommand { get; }

        public static PNRoutedUICommand CheckMessagesCommand { get; }

        public static PNRoutedUICommand SyncNowCommand { get; }

        public static PNRoutedUICommand CheckPluginsCommand { get; }

        public static PNRoutedUICommand RemovePluginCommand { get; }

        public static PNRoutedUICommand ImportOutlookCommand { get; }

        public static PNRoutedUICommand ImportGMailCommand { get; }

        public static PNRoutedUICommand ImportLotusCommand { get; }

    }

    public class MenusCommands
    {
        static MenusCommands()
        {
            UpCommand = new PNRoutedUICommand("Up", "cmdUp", typeof(MenusCommands)) { Type = CommandType.Up };
            DownCommand = new PNRoutedUICommand("Down", "cmdDown", typeof(MenusCommands)) { Type = CommandType.Down };
            RestoreOrderCommand = new PNRoutedUICommand("Restore", "cmdRestoreOrder", typeof(MenusCommands)) { Type = CommandType.RestoreOrder };
            ResetAllCommand = new PNRoutedUICommand("Reset all", "cmdResetAll", typeof(MenusCommands)) { Type = CommandType.ResetAll };
            ResetCurrentCommand = new PNRoutedUICommand("Reset current", "cmdResetCurrent", typeof(MenusCommands)) { Type = CommandType.ResetCurrent };
        }

        public static PNRoutedUICommand UpCommand { get; }

        public static PNRoutedUICommand DownCommand { get; }

        public static PNRoutedUICommand RestoreOrderCommand { get; }

        public static PNRoutedUICommand ResetAllCommand { get; }

        public static PNRoutedUICommand ResetCurrentCommand { get; }
    }

    public class SearchCommands
    {
        static SearchCommands()
        {
            FindNextCommand =
                new PNRoutedUICommand("Find next", "cmdFindNext", typeof(SearchCommands)) { Type = CommandType.FindNext };
            ReplaceCommand =
                new PNRoutedUICommand("Replace", "cmdReplace", typeof(SearchCommands)) { Type = CommandType.Replace };
            ReplaceAllCommand =
                new PNRoutedUICommand("Replace all", "cmdReplaceAll", typeof(SearchCommands))
                {
                    Type = CommandType.ReplaceAll
                };
            FindCommand = new PNRoutedUICommand("Find", "cmdFind", typeof(SearchCommands)) { Type = CommandType.Find };
            ClearSearchHistoryCommand =
                new PNRoutedUICommand("Clear search history", "cmdClearHistory", typeof(SearchCommands))
                {
                    Type = CommandType.ClearSearchHistory
                };
            ClearSearchSettingsCommand =
                new PNRoutedUICommand("Reset search settings", "cmdClearSettings", typeof(SearchCommands))
                {
                    Type = CommandType.ClearSearchSettings
                };
        }

        public static PNRoutedUICommand ClearSearchHistoryCommand { get; }

        public static PNRoutedUICommand ClearSearchSettingsCommand { get; }

        public static PNRoutedUICommand FindCommand { get; }

        public static PNRoutedUICommand FindNextCommand { get; }

        public static PNRoutedUICommand ReplaceCommand { get; }

        public static PNRoutedUICommand ReplaceAllCommand { get; }
    }

    public class GroupViewCommands
    {
        static GroupViewCommands()
        {
            StandardViewCommand =
                new PNRoutedUICommand("Standard view", "cmdStandard", typeof(GroupViewCommands))
                {
                    Type = CommandType.StandardView
                };
            SkinlessCaptionFontCommand =
                new PNRoutedUICommand("Caption font", "cmdFontSknls", typeof(GroupViewCommands))
                {
                    Type = CommandType.SkinlessCaptionFont
                };
            GroupIcontCommand =
                new PNRoutedUICommand("Group icon", "cmdGroupIcon", typeof(GroupViewCommands))
                {
                    Type = CommandType.GroupIcon
                };
        }

        public static PNRoutedUICommand GroupIcontCommand { get; }

        public static PNRoutedUICommand SkinlessCaptionFontCommand { get; }

        public static PNRoutedUICommand StandardViewCommand { get; }
    }

    public class ControlGroupsCommands
    {
        static ControlGroupsCommands()
        {
            GroupAddCommand = new PNRoutedUICommand("Add top level group", "cmdAddTopGroup", typeof(ControlGroupsCommands))
            {
                Type = CommandType.GroupAdd
            };
            GroupAddSubgroupCommand = new PNRoutedUICommand("Add subgroup to selected group", "cmdAddSubgroup",
                    typeof(ControlGroupsCommands))
            { Type = CommandType.GroupAddSubgroup };
            GroupEditCommand = new PNRoutedUICommand("Edit group", "cmdEditGroup", typeof(ControlGroupsCommands))
            {
                Type = CommandType.GroupEdit
            };
            GroupRemoveCommand = new PNRoutedUICommand("Delete group", "cmdRemoveGroup", typeof(ControlGroupsCommands))
            {
                Type = CommandType.GroupRemove
            };
            GroupShowCommand = new PNRoutedUICommand("Show all notes from selected group", "cmdShowAllFromGroup",
                    typeof(ControlGroupsCommands))
            { Type = CommandType.GroupShow };
            GroupShowAllCommand = new PNRoutedUICommand("Show group (include subgroups)", "cmdShowAllIncSubgroups",
                    typeof(ControlGroupsCommands))
            { Type = CommandType.GroupShowAll };
            GroupHideCommand = new PNRoutedUICommand("Hide all notes from selected group", "cmdHideAllFromGroup",
                    typeof(ControlGroupsCommands))
            { Type = CommandType.GroupHide };
            GroupHideAllCommand = new PNRoutedUICommand("Hide group (include subgroups)", "cmdHideAllIncSubgroups",
                    typeof(ControlGroupsCommands))
            { Type = CommandType.GroupHideAll };
            GroupPassAddCommand = new PNRoutedUICommand("Set group password", "cmdSetGroupPassword", typeof(ControlGroupsCommands))
            {
                Type = CommandType.GroupPassAdd
            };
            GroupPassRemoveCommand = new PNRoutedUICommand("Remove group password", "cmdRemoveGroupPassword",
                    typeof(ControlGroupsCommands))
            { Type = CommandType.GroupPassRemove };
        }

        public static PNRoutedUICommand GroupAddCommand { get; }

        public static PNRoutedUICommand GroupAddSubgroupCommand { get; }

        public static PNRoutedUICommand GroupEditCommand { get; }

        public static PNRoutedUICommand GroupRemoveCommand { get; }

        public static PNRoutedUICommand GroupShowCommand { get; }

        public static PNRoutedUICommand GroupShowAllCommand { get; }

        public static PNRoutedUICommand GroupHideCommand { get; }

        public static PNRoutedUICommand GroupHideAllCommand { get; }

        public static PNRoutedUICommand GroupPassAddCommand { get; }

        public static PNRoutedUICommand GroupPassRemoveCommand { get; }
    }

    public class MainCommands
    {
        static MainCommands()
        {
            NewNoteCommand = new PNRoutedUICommand("New Note", "cmdNewNote", typeof(MainCommands)) { Type = CommandType.NewNote };
            LoadNoteCommand = new PNRoutedUICommand("Load Note", "cmdLoadNotes", typeof(MainCommands))
            {
                Type = CommandType.LoadNote
            };
            NoteFromClipboardCommand = new PNRoutedUICommand("New Note From Clipboard", "cmdNoteFromCB", typeof(MainCommands))
            {
                Type = CommandType.NoteFromClipboard
            };
            NewNoteInGroupCommand = new PNRoutedUICommand("New Note In Group", "mnuNewNoteInGroup", typeof(MainCommands))
            {
                Type = CommandType.NewNoteInGroup,
                Section = "main_menu"
            };
            DiaryCommand = new PNRoutedUICommand("Diary", "cmdDiary", typeof(MainCommands)) { Type = CommandType.Diary };
            TodayCommand = new PNRoutedUICommand("Today", "mnuTodayDiary", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Today
            };
            SaveAllCommand = new PNRoutedUICommand("Save All", "cmdSaveAll", typeof(MainCommands)) { Type = CommandType.SaveAll };
            BackupSyncCommand = new PNRoutedUICommand("Backup/Synchronization", "cmdBackup", typeof(MainCommands))
            {
                Type = CommandType.BackupSync
            };
            BackupCreateCommand = new PNRoutedUICommand("Create Full Backup", "mnuBackupCreate", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.BackupCreate
            };
            BackupRestoreCommand = new PNRoutedUICommand("Restore From Full Backup", "mnuBackupRestore", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.BackupRestore
            };
            SyncLocalCommand = new PNRoutedUICommand("Manual Local Synchronization", "mnuSyncLocal", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SyncLocal
            };
            ImportNotesCommand = new PNRoutedUICommand("Import Notes From PNotes", "mnuImportNotes", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportNotes
            };
            ImportSettingsCommand = new PNRoutedUICommand("Import Settings From PNotes", "mnuImportSettings", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportSettings
            };
            ImportFontsCommand = new PNRoutedUICommand("Import Fonts From PNotes", "mnuImportFonts", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportFonts
            };
            ImportDictionariesCommand = new PNRoutedUICommand("Import Dictionaries From PNotes", "mnuImportDictionaries", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ImportDictionaries
            };
            ExpTiffCommand = new PNRoutedUICommand("TIF image", "mnuExpTif", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ExpTif
            };
            ExpPdfCommand = new PNRoutedUICommand("PDF File", "mnuExpPdf", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ExpPdf
            };
            ExpDocCommand = new PNRoutedUICommand("Word document", "mnuExpDoc", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ExpDoc
            };
            ExpRtfCommand = new PNRoutedUICommand("RTF document", "mnuExpRtf", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ExpRtf
            };
            ExpTxtCommand = new PNRoutedUICommand("Text file", "mnuExpTxt", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ExpTxt
            };
            ExpAllCommand = new PNRoutedUICommand("Export All Notes To", "mnuExportAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ExpAll
            };
            PrintFieldsCommand = new PNRoutedUICommand("Configure report", "mnuPrintFields", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.PrintFields
            };
            DockAllCommand = new PNRoutedUICommand("Docking (All Notes)", "mnuDockAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAll
            };
            ShowAllCommand = new PNRoutedUICommand("Show All", "mnuShowAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowAllOnCp
            };
            HideAllCommand = new PNRoutedUICommand("Hide All", "mnuHideAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.HideAllOnCp
            };
            AllToFrontCommand = new PNRoutedUICommand("Bring All To Front", "mnuAllToFront", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.AllToFront
            };
            HotkeysCommand = new PNRoutedUICommand("Hot Keys Management", "mnuHotkeys", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.HotkeysCP
            };
            MenusManagementCommand = new PNRoutedUICommand("Menus Management", "mnuMenusManagement", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.MenusManagementCP
            };
            RunCommand = new PNRoutedUICommand("Run", "cmdRun", typeof(MainCommands))
            {
                Type = CommandType.Run
            };
            HelpCommand = new PNRoutedUICommand("Help", "cmdHelp", typeof(MainCommands))
            {
                Type = CommandType.Help
            };
            SupportCommand = new PNRoutedUICommand("Support PNotes.NET Project", "cmdSupport", typeof(MainCommands))
            {
                Type = CommandType.Support
            };
            AboutCommand = new PNRoutedUICommand("About", "mnuAbout", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.About
            };
            TagsShowByCommand = new PNRoutedUICommand("Show By Tag", "mnuShowByTag", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.TagsShowBy
            };
            TagsHideByCommand = new PNRoutedUICommand("Hide By Tag", "mnuHideByTag", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.TagsHideBy
            };
            SearchInNotesCommand = new PNRoutedUICommand("Search In Notes", "mnuSearchInNotes", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SearchInNotes
            };
            SearchByTagsCommand = new PNRoutedUICommand("Search By Tags", "mnuSearchByTags", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SearchByTags
            };
            SearchByDatesCommand = new PNRoutedUICommand("Search By Dates", "mnuSearchByDates", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SearchByDates
            };
            PreferencesCommand = new PNRoutedUICommand("Preferences", "cmdPreferences", typeof(MainCommands))
            {
                Type = CommandType.Preferences
            };
            ControlPanelCommand = new PNRoutedUICommand("Control Panel", "mnuCP", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ControlPanel
            };
            PanelCommand = new PNRoutedUICommand("Panel", "mnuPanelAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Panel
            };
            ToPanelAllCommand = new PNRoutedUICommand("Move To Panel (All Notes)", "mnuToPanelAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ToPanelAll
            };
            FromPanelAllCommand = new PNRoutedUICommand("Remove From Panel (All Notes)", "mnuFromPanelAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.FromPanelAll
            };
            ShowHideCommand = new PNRoutedUICommand("Show/Hide", "mnuShowHide", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowHide
            };
            ShowGroupsCommand = new PNRoutedUICommand("Show Groups", "mnuShowGroups", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowGroups
            };
            ShowIncomingCommand = new PNRoutedUICommand("Show Incoming", "mnuShowIncoming", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowIncoming
            };
            HideGroupsCommand = new PNRoutedUICommand("Hide Groups", "mnuHideGroups", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.HideGroups
            };
            HideIncomingCommand = new PNRoutedUICommand("Hide Incoming", "mnuHideIncoming", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.HideIncoming
            };
            LastModifiedNotesCommand = new PNRoutedUICommand("Last Modified Notes", "mnuLastModified", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.LastModifiedNotes
            };
            LastTodayCommand = new PNRoutedUICommand("Today", "mnuTodayLast", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.LastToday
            };
            Last1Command = new PNRoutedUICommand("1 Day Ago", "mnu1DayAgo", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Last1
            };
            Last2Command = new PNRoutedUICommand("2 Days Ago", "mnu2DaysAgo", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Last2
            };
            Last3Command = new PNRoutedUICommand("3 Days Ago", "mnu3DaysAgo", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Last3
            };
            Last4Command = new PNRoutedUICommand("4 Days Ago", "mnu4DaysAgo", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Last4
            };
            Last5Command = new PNRoutedUICommand("5 Days Ago", "mnu5DaysAgo", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Last5
            };
            Last6Command = new PNRoutedUICommand("6 Days Ago", "mnu6DaysAgo", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Last6
            };
            Last7Command = new PNRoutedUICommand("7 Days Ago", "mnu7DaysAgo", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Last7
            };
            ReloadAllCommand = new PNRoutedUICommand("Reload All", "mnuReloadAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ReloadAll
            };
            SwitchOnAllCommand = new PNRoutedUICommand("Switch On (All Notes)", "mnuSwitchOnAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOnAll
            };
            SwitchOnHighPriorityCommand = new PNRoutedUICommand("Set High Priority", "mnuSOnHighPriority", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOnHighPriority
            };
            SwitchOnProtectionCommand = new PNRoutedUICommand("Switch On Protection Mode", "mnuSOnProtection", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOnProtection
            };
            SwitchOnCompleteCommand = new PNRoutedUICommand("Set Complete Mark", "mnuSOnComplete", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOnComplete
            };
            SwitchOnRollCommand = new PNRoutedUICommand("Roll", "mnuSOnRoll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOnRoll
            };
            SwitchOnOnTopCommand = new PNRoutedUICommand("'On Top' Status", "mnuSOnOnTop", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOnOnTop
            };
            SwitchOffAllCommand = new PNRoutedUICommand("Switch Off (All Notes)", "mnuSwitchOffAll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOffAll
            };
            SwitchOffHighPriorityCommand = new PNRoutedUICommand("Remove High Priority", "mnuSOffHighPriority", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOffHighPriority
            };
            SwitchOffProtectionCommand = new PNRoutedUICommand("Switch Off Protection Mode", "mnuSOffProtection", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOffProtection
            };
            SwitchOffCompleteCommand = new PNRoutedUICommand("Remove Complete Mark", "mnuSOffComplete", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOffComplete
            };
            SwitchOffRollCommand = new PNRoutedUICommand("Unroll", "mnuSOffUnroll", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOffRoll
            };
            SwitchOffOnTopCommand = new PNRoutedUICommand("'On Top' Status", "mnuSOffOnTop", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SwitchOffOnTop
            };
            SpecialOptionsCommand = new PNRoutedUICommand("Special Options", "mnuSpecialOptions", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SpecialOptions
            };
            OnScreenKeyboardCommand = new PNRoutedUICommand("Toggle On-Screen Keyboard", "mnuOnScreenKbrd", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.OnScreenKeyboard
            };
            MagnifierCommand = new PNRoutedUICommand("Toggle Magnifier", "mnuMagnifier", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Magnifier
            };
            SearchInMainCommand = new PNRoutedUICommand("Search", "mnuSearch", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.SearchInMain
            };
            FavoritesMainCommand = new PNRoutedUICommand("Favorites", "mnuFavorites", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.FavoritesMain
            };
            ShowAllFavoritesCommand = new PNRoutedUICommand("Show All Favorites", "mnuShowAllFavorites", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowAllFavorites
            };
            LockProgramCommand = new PNRoutedUICommand("Lock Program", "mnuLockProg", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.LockProgram
            };
            HomepageCommand = new PNRoutedUICommand("PNotes.NET Homepage", "mnuHomepage", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Homepage
            };
            ExitCommand = new PNRoutedUICommand("Exit", "mnuExit", typeof(MainCommands))
            {
                Section = "main_menu",
                Type = CommandType.Exit
            };

        }

        public static PNRoutedUICommand ExitCommand { get; }

        public static PNRoutedUICommand HomepageCommand { get; }

        public static PNRoutedUICommand LockProgramCommand { get; }

        public static PNRoutedUICommand ShowAllFavoritesCommand { get; }

        public static PNRoutedUICommand FavoritesMainCommand { get; }

        public static PNRoutedUICommand SearchInMainCommand { get; }

        public static PNRoutedUICommand MagnifierCommand { get; }

        public static PNRoutedUICommand OnScreenKeyboardCommand { get; }

        public static PNRoutedUICommand SpecialOptionsCommand { get; }

        public static PNRoutedUICommand SwitchOffOnTopCommand { get; }

        public static PNRoutedUICommand SwitchOffRollCommand { get; }

        public static PNRoutedUICommand SwitchOffCompleteCommand { get; }

        public static PNRoutedUICommand SwitchOffProtectionCommand { get; }

        public static PNRoutedUICommand SwitchOffHighPriorityCommand { get; }

        public static PNRoutedUICommand SwitchOffAllCommand { get; }

        public static PNRoutedUICommand SwitchOnOnTopCommand { get; }

        public static PNRoutedUICommand SwitchOnRollCommand { get; }

        public static PNRoutedUICommand SwitchOnCompleteCommand { get; }

        public static PNRoutedUICommand SwitchOnProtectionCommand { get; }

        public static PNRoutedUICommand SwitchOnHighPriorityCommand { get; }

        public static PNRoutedUICommand SwitchOnAllCommand { get; }

        public static PNRoutedUICommand ReloadAllCommand { get; }

        public static PNRoutedUICommand Last7Command { get; }

        public static PNRoutedUICommand Last6Command { get; }

        public static PNRoutedUICommand Last5Command { get; }

        public static PNRoutedUICommand Last4Command { get; }

        public static PNRoutedUICommand Last3Command { get; }

        public static PNRoutedUICommand Last2Command { get; }

        public static PNRoutedUICommand Last1Command { get; }

        public static PNRoutedUICommand LastTodayCommand { get; }

        public static PNRoutedUICommand LastModifiedNotesCommand { get; }

        public static PNRoutedUICommand HideIncomingCommand { get; }

        public static PNRoutedUICommand HideGroupsCommand { get; }

        public static PNRoutedUICommand ShowIncomingCommand { get; }

        public static PNRoutedUICommand ShowGroupsCommand { get; }

        public static PNRoutedUICommand ShowHideCommand { get; }

        public static PNRoutedUICommand FromPanelAllCommand { get; }

        public static PNRoutedUICommand ToPanelAllCommand { get; }

        public static PNRoutedUICommand PanelCommand { get; }

        public static PNRoutedUICommand ControlPanelCommand { get; }

        public static PNRoutedUICommand NewNoteCommand { get; }

        public static PNRoutedUICommand LoadNoteCommand { get; }

        public static PNRoutedUICommand NoteFromClipboardCommand { get; }

        public static PNRoutedUICommand NewNoteInGroupCommand { get; }

        public static PNRoutedUICommand DiaryCommand { get; }

        public static PNRoutedUICommand TodayCommand { get; }

        public static PNRoutedUICommand SaveAllCommand { get; }

        public static PNRoutedUICommand BackupSyncCommand { get; }

        public static PNRoutedUICommand BackupCreateCommand { get; }

        public static PNRoutedUICommand BackupRestoreCommand { get; }

        public static PNRoutedUICommand SyncLocalCommand { get; }

        public static PNRoutedUICommand ImportNotesCommand { get; }

        public static PNRoutedUICommand ImportSettingsCommand { get; }

        public static PNRoutedUICommand ImportFontsCommand { get; }

        public static PNRoutedUICommand ImportDictionariesCommand { get; }

        public static PNRoutedUICommand ExpTiffCommand { get; }

        public static PNRoutedUICommand ExpDocCommand { get; }

        public static PNRoutedUICommand ExpPdfCommand { get; }

        public static PNRoutedUICommand ExpRtfCommand { get; }

        public static PNRoutedUICommand ExpTxtCommand { get; }

        public static PNRoutedUICommand ExpAllCommand { get; }

        public static PNRoutedUICommand PrintFieldsCommand { get; }

        public static PNRoutedUICommand DockAllCommand { get; }

        public static PNRoutedUICommand ShowAllCommand { get; }

        public static PNRoutedUICommand HideAllCommand { get; }

        public static PNRoutedUICommand AllToFrontCommand { get; }

        public static PNRoutedUICommand HotkeysCommand { get; }

        public static PNRoutedUICommand MenusManagementCommand { get; }

        public static PNRoutedUICommand HelpCommand { get; }

        public static PNRoutedUICommand SupportCommand { get; }

        public static PNRoutedUICommand AboutCommand { get; }

        public static PNRoutedUICommand RunCommand { get; }

        public static PNRoutedUICommand TagsShowByCommand { get; }

        public static PNRoutedUICommand TagsHideByCommand { get; }

        public static PNRoutedUICommand SearchInNotesCommand { get; }

        public static PNRoutedUICommand SearchByTagsCommand { get; }

        public static PNRoutedUICommand SearchByDatesCommand { get; }

        public static PNRoutedUICommand PreferencesCommand { get; }
    }

    public class CommonCommands
    {
        static CommonCommands()
        {
            PwrdCreateCommand = new PNRoutedUICommand("Create Password", "mnuPwrdCreate", typeof(CommonCommands))
            {
                Section = "cp_menu",
                Type = CommandType.PwrdCreate
            };
            PwrdChangeCommand = new PNRoutedUICommand("Change Password", "mnuPwrdChange", typeof(CommonCommands))
            {
                Section = "cp_menu",
                Type = CommandType.PwrdChange
            };
            PwrdRemoveCommand = new PNRoutedUICommand("Remove Password", "mnuPwrdRemove", typeof(CommonCommands))
            {
                Section = "cp_menu",
                Type = CommandType.PwrdRemove
            };
            BrowseButtonCommand =
                new PNRoutedUICommand("...", "__browse_button__", typeof(CommonCommands)) { Type = CommandType.BrowseButton };
            LoadImportCommand =
                new PNRoutedUICommand("Load", "cmdLoadImport", typeof(CommonCommands)) { Type = CommandType.LoadImport };
            DownloadCommand =
                new PNRoutedUICommand("Download", "cmdDownload", typeof(CommonCommands)) { Type = CommandType.Download };
            OpenInBrowserCommand =
                new PNRoutedUICommand("Open in browser", "cmdBrowser", typeof(CommonCommands)) { Type = CommandType.OpenInBrowser };
            LoadFromFileCommand =
                new PNRoutedUICommand("Load from file", "cmdFromFile", typeof(CommonCommands)) { Type = CommandType.LoadFromFile };
            MoveLeftCommand =
                new PNRoutedUICommand("<<", "__move_left__", typeof(CommonCommands)) { Type = CommandType.MoveLeft };
            MoveRightCommand =
                new PNRoutedUICommand(">>", "__move_right__", typeof(CommonCommands)) { Type = CommandType.MoveRight };
            QuestionCommand =
                new PNRoutedUICommand("?", "__question__", typeof(CommonCommands)) { Type = CommandType.Question };
            DummyCommand =
                new PNRoutedUICommand("", "__dummy__", typeof(CommonCommands)) { Type = CommandType.Dummy };
            DockNoneCommand = new PNRoutedUICommand("None", "mnuDAllNone", typeof(CommonCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllNone
            };
            DockLeftCommand = new PNRoutedUICommand("Left", "mnuDAllLeft", typeof(CommonCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllLeft
            };
            DockTopCommand = new PNRoutedUICommand("Top", "mnuDAllTop", typeof(CommonCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllTop
            };
            DockRightCommand = new PNRoutedUICommand("Right", "mnuDAllRight", typeof(CommonCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllRight
            };
            DockBottomCommand = new PNRoutedUICommand("Bottom", "mnuDAllBottom", typeof(CommonCommands))
            {
                Section = "main_menu",
                Type = CommandType.DockAllBottom
            };

        }

        public static PNRoutedUICommand DummyCommand { get; }

        public static PNRoutedUICommand PwrdCreateCommand { get; }

        public static PNRoutedUICommand PwrdChangeCommand { get; }

        public static PNRoutedUICommand PwrdRemoveCommand { get; }

        public static PNRoutedUICommand BrowseButtonCommand { get; }

        public static PNRoutedUICommand LoadImportCommand { get; }

        public static PNRoutedUICommand DownloadCommand { get; }

        public static PNRoutedUICommand OpenInBrowserCommand { get; }

        public static PNRoutedUICommand LoadFromFileCommand { get; }

        public static PNRoutedUICommand MoveLeftCommand { get; }

        public static PNRoutedUICommand MoveRightCommand { get; }

        public static PNRoutedUICommand QuestionCommand { get; }

        public static PNRoutedUICommand DockNoneCommand { get; }

        public static PNRoutedUICommand DockLeftCommand { get; }

        public static PNRoutedUICommand DockTopCommand { get; }

        public static PNRoutedUICommand DockRightCommand { get; }

        public static PNRoutedUICommand DockBottomCommand { get; }
    }


    public class NoteCommands
    {
        static NoteCommands()
        {
            DuplicateNoteCommand = new PNRoutedUICommand("Duplicate Note", "cmdDuplicate", typeof(NoteCommands))
            {
                Type = CommandType.DuplicateNote
            };
            SaveAsShortcutCommand = new PNRoutedUICommand("Save As Desktop Shortcut", "mnuSaveAsShortcut", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.SaveAsShortcut
            };
            SaveNoteCommand = new PNRoutedUICommand("Save", "cmdSave", typeof(NoteCommands)) { Type = CommandType.SaveNote };
            SaveAsCommand = new PNRoutedUICommand("Rename/Change Group", "cmdSaveAs", typeof(NoteCommands)) { Type = CommandType.SaveAs };
            SaveAsTextCommand = new PNRoutedUICommand("Save As Text File", "cmdSaveAsText", typeof(NoteCommands))
            {
                Type = CommandType.SaveAsText
            };
            RestoreFromBackupCommand = new PNRoutedUICommand("Restore From Backup", "cmdRestoreFromBackup",
                    typeof(NoteCommands))
            { Type = CommandType.RestoreFromBackup };
            PrintCommand = new PNRoutedUICommand("Print", "cmdPrint", typeof(NoteCommands)) { Type = CommandType.Print };
            AdjustAppearanceCommand = new PNRoutedUICommand("Adjust Appearance", "mnuAdjustAppearance", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.AdjustAppearance
            };
            AdjustScheduleCommand = new PNRoutedUICommand("Adjust Schedule", "mnuAdjustSchedule", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.AdjustSchedule
            };
            DeleteNoteCommand = new PNRoutedUICommand("Delete Note", "cmdDelete", typeof(NoteCommands)) { Type = CommandType.DeleteNote };
            HideNoteCommand = new PNRoutedUICommand("Hide", "mnuHideNote", typeof(NoteCommands))
            {
                Section = "main_menu",
                Type = CommandType.HideNote
            };
            SendAsTextCommand = new PNRoutedUICommand("Send As Text", "cmdSendAsText", typeof(NoteCommands))
            {
                Type = CommandType.SendAsText
            };
            SendAsAttachmentCommand = new PNRoutedUICommand("Send As Attachment", "cmdSendAsAttachment", typeof(NoteCommands))
            {
                Type = CommandType.SendAsAttachment
            };
            SendAsZipCommand = new PNRoutedUICommand("Send In ZIP Archive", "cmdSendZip", typeof(NoteCommands))
            {
                Type = CommandType.SendAsZip
            };
            SendNetworkCommand = new PNRoutedUICommand("Send Via Network", "cmdSendNetwork", typeof(NoteCommands))
            {
                Type = CommandType.SendNetwork
            };
            OnTopCommand = new PNRoutedUICommand("On Top", "mnuOnTop", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.OnTop
            };
            HighPriorityCommand = new PNRoutedUICommand("Toggle High Priority", "mnuToggleHighPriority", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.HighPriority
            };
            ProtectionModeCommand = new PNRoutedUICommand("Toggle Protection Mode", "mnuToggleProtectionMode", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ProtectionMode
            };
            SetNotePasswordCommand = new PNRoutedUICommand("Set Note Password", "mnuSetPassword", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.SetNotePassword
            };
            RemoveNotePasswordCommand = new PNRoutedUICommand("Remove Note Password", "mnuRemovePassword", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.RemoveNotePassword
            };
            MarkAsCompleteCommand = new PNRoutedUICommand("Mark As Complete", "mnuMarkAsComplete", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.MarkAsComplete
            };
            RollUnrollCommand = new PNRoutedUICommand("Roll/Unroll", "mnuRollUnroll", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.RollUnroll
            };
            PinCommand = new PNRoutedUICommand("Pin To Window", "mnuPin", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.Pin
            };
            UnpinCommand = new PNRoutedUICommand("Unpin", "mnuUnpin", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.Unpin
            };
            ScrambleCommand = new PNRoutedUICommand("Encrypt text", "mnuScramble", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.Scramble
            };
            UnscrambleCommand = new PNRoutedUICommand("Decrypt text", "mnuUnscramble", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.Unscramble
            };
            AddFavoritesCommand = new PNRoutedUICommand("Add To Favorites", "cmdFavorites", typeof(NoteCommands))
            {
                Type = CommandType.AddFavorites
            };
            RemoveFavoritesCommand = new PNRoutedUICommand("Remove From Favorites", "cmdRemoveFromFavorites", typeof(NoteCommands))
            {
                Type = CommandType.RemoveFavorites
            };
            ContactAddCommand = new PNRoutedUICommand("Add Contact", "mnuAddContact", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactAdd
            };
            ContactGroupAddCommand = new PNRoutedUICommand("Add Group Of Contacts", "mnuAddGroup", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactGroupAdd
            };
            ContactSelectCommand = new PNRoutedUICommand("Select Contacts", "mnuSelectContact", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactSelect
            };
            ContactGroupSelectCommand = new PNRoutedUICommand("Select Groups Of Contacts", "mnuSelectGroup", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ContactGroupSelect
            };
            TagsCommand = new PNRoutedUICommand("Tags", "cmdTags", typeof(NoteCommands))
            {
                Type = CommandType.Tags
            };
            ToPanelCommand = new PNRoutedUICommand("Put In Panel", "mnuPanel", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ToPanel
            };
            DockCommand = new PNRoutedUICommand("Dock", "mnuDock", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.DockNote
            };
            ReplyCommand = new PNRoutedUICommand("Reply", "mnuReply", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.Reply
            };
            PostToCommand = new PNRoutedUICommand("Post Entire Note On:", "mnuPostNote", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ToPost
            };
            FromPostCommand = new PNRoutedUICommand("Replace Note Text By Post From:", "mnuReplacePost", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.FromPost
            };
            ExportToOfficeCommand = new PNRoutedUICommand("Export to MS Office As:", "mnuExportToOffice", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ExportToOffice
            };
            ExportToOutlookCommand = new PNRoutedUICommand("Outlook Note", "mnuExportOutlookNote", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.ExportToOutlookNote
            };
            LinkedNotesCommand = new PNRoutedUICommand("Linked Notes", "mnuLinked", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.LinkedNotes
            };
            LinkedManagementCommand = new PNRoutedUICommand("Manage Linked Notes", "mnuManageLinks", typeof(NoteCommands))
            {
                Section = "note_menu",
                Type = CommandType.LinkedManagement
            };

        }

        public static PNRoutedUICommand DuplicateNoteCommand { get; }

        public static PNRoutedUICommand SaveAsShortcutCommand { get; }

        public static PNRoutedUICommand SaveNoteCommand { get; }

        public static PNRoutedUICommand SaveAsCommand { get; }

        public static PNRoutedUICommand SaveAsTextCommand { get; }

        public static PNRoutedUICommand RestoreFromBackupCommand { get; }

        public static PNRoutedUICommand PrintCommand { get; }

        public static PNRoutedUICommand AdjustAppearanceCommand { get; }

        public static PNRoutedUICommand AdjustScheduleCommand { get; }

        public static PNRoutedUICommand DeleteNoteCommand { get; }

        public static PNRoutedUICommand HideNoteCommand { get; }

        public static PNRoutedUICommand SendAsTextCommand { get; }

        public static PNRoutedUICommand ToPanelCommand { get; }

        public static PNRoutedUICommand SendAsAttachmentCommand { get; }

        public static PNRoutedUICommand SendAsZipCommand { get; }

        public static PNRoutedUICommand SendNetworkCommand { get; }

        public static PNRoutedUICommand OnTopCommand { get; }

        public static PNRoutedUICommand HighPriorityCommand { get; }

        public static PNRoutedUICommand ProtectionModeCommand { get; }

        public static PNRoutedUICommand SetNotePasswordCommand { get; }

        public static PNRoutedUICommand RemoveNotePasswordCommand { get; }

        public static PNRoutedUICommand MarkAsCompleteCommand { get; }

        public static PNRoutedUICommand RollUnrollCommand { get; }

        public static PNRoutedUICommand PinCommand { get; }

        public static PNRoutedUICommand UnpinCommand { get; }

        public static PNRoutedUICommand ScrambleCommand { get; }

        public static PNRoutedUICommand UnscrambleCommand { get; }

        public static PNRoutedUICommand AddFavoritesCommand { get; }

        public static PNRoutedUICommand RemoveFavoritesCommand { get; }

        public static PNRoutedUICommand ContactAddCommand { get; }

        public static PNRoutedUICommand ContactGroupAddCommand { get; }

        public static PNRoutedUICommand ContactSelectCommand { get; }

        public static PNRoutedUICommand ContactGroupSelectCommand { get; }

        public static PNRoutedUICommand TagsCommand { get; }

        public static PNRoutedUICommand DockCommand { get; }

        public static PNRoutedUICommand ReplyCommand { get; }

        public static PNRoutedUICommand PostToCommand { get; }

        public static PNRoutedUICommand FromPostCommand { get; }

        public static PNRoutedUICommand ExportToOfficeCommand { get; }

        public static PNRoutedUICommand ExportToOutlookCommand { get; }

        public static PNRoutedUICommand LinkedNotesCommand { get; }

        public static PNRoutedUICommand LinkedManagementCommand { get; }

    }

    public class ControlPanelCommands
    {
        static ControlPanelCommands()
        {
            Adjust = new PNRoutedUICommand("Adjust", "cmdAdjust", typeof(ControlPanelCommands)) { Type = CommandType.Adjust };
            Placement = new PNRoutedUICommand("Placement/Visibility", "cmdPlacement", typeof(ControlPanelCommands))
            {
                Type = CommandType.Placement
            };
            Visibility = new PNRoutedUICommand("Visibility", "mnuVisibility", typeof(ControlPanelCommands))
            {
                Section = "main_menu",
                Type = CommandType.Visibility
            };
            ShowNote = new PNRoutedUICommand("Show", "mnuShowNote", typeof(ControlPanelCommands))
            {
                Section = "main_menu",
                Type = CommandType.ShowNote
            };
            Centralize = new PNRoutedUICommand("Centralize", "mnuCentralize", typeof(ControlPanelCommands))
            {
                Section = "main_menu",
                Type = CommandType.Centralize
            };
            TagsCurrent = new PNRoutedUICommand("Tags (current note)", "mnuTagsCurrent", typeof(ControlPanelCommands))
            {
                Section = "main_menu",
                Type = CommandType.TagsCurrent
            };
            Switches = new PNRoutedUICommand("Switches", "cmdSwitches", typeof(ControlPanelCommands))
            {
                Type = CommandType.Switches
            };
            EmptyBin = new PNRoutedUICommand("Empty Recycle Bin", "cmdEmptyBin", typeof(ControlPanelCommands))
            {
                Type = CommandType.EmptyBin
            };
            RestoreNote = new PNRoutedUICommand("Restore Note", "cmdRestoreNote", typeof(ControlPanelCommands))
            {
                Type = CommandType.RestoreNote
            };
            Preview = new PNRoutedUICommand("Preview", "cmdPreview", typeof(ControlPanelCommands))
            {
                Type = CommandType.Preview
            };
            PreviewFromMenu = new PNRoutedUICommand("Preview", "cmdPreview", typeof(ControlPanelCommands))
            {
                Type = CommandType.PreviewFromMenu
            };
            PreviewSettings = new PNRoutedUICommand("Preview Window Background Settings", "cmdPreviewSettings", typeof(ControlPanelCommands))
            {
                Type = CommandType.PreviewSettings
            };
            UseCustColor = new PNRoutedUICommand("Use Custom Color", "mnuUseCustColor", typeof(ControlPanelCommands))
            {
                Section = "cp_menu",
                Type = CommandType.UseCustColor
            };
            ChooseCustColor = new PNRoutedUICommand("Choose Custom Color", "mnuChooseCustColor", typeof(ControlPanelCommands))
            {
                Section = "cp_menu",
                Type = CommandType.ChooseCustColor
            };
            PreviewRight = new PNRoutedUICommand("Preview Window On The Right", "cmdPreviewRight", typeof(ControlPanelCommands))
            {
                Type = CommandType.PreviewRight
            };
            PreviewRightFromMenu = new PNRoutedUICommand("Preview Window On The Right", "cmdPreviewRight", typeof(ControlPanelCommands))
            {
                Type = CommandType.PreviewRightFromMenu
            };
            ShowGroupsOnCp = new PNRoutedUICommand("Show Groups", "cmdShowGroups", typeof(ControlPanelCommands))
            {
                Type = CommandType.ShowGroupsOnCp
            };
            ShowGroupsOnCpFromMenu = new PNRoutedUICommand("Show Groups", "cmdShowGroups", typeof(ControlPanelCommands))
            {
                Type = CommandType.ShowGroupsOnCpFromMenu
            };
            ColReset = new PNRoutedUICommand("Reset Columns Width/Visibility", "cmdColReset", typeof(ControlPanelCommands))
            {
                Type = CommandType.ColReset
            };
            Password = new PNRoutedUICommand("Password", "cmdPassword", typeof(ControlPanelCommands))
            {
                Type = CommandType.Password
            };
            Search = new PNRoutedUICommand("Search", "cmdSearch", typeof(ControlPanelCommands))
            {
                Type = CommandType.Search
            };
            QuickSearch = new PNRoutedUICommand("Quick Search", "cmdQuickSearch", typeof(ControlPanelCommands))
            {
                Type = CommandType.QuickSearch
            };
            IncBinInQSearch = new PNRoutedUICommand("Include Notes From Recycle Bin in 'Quick Search'", "mnuIncBinInQSearch", typeof(ControlPanelCommands))
            {
                Section = "main_menu",
                Type = CommandType.IncBinInQSearch
            };
            ClearQSearch = new PNRoutedUICommand("Clear 'Quick Search'", "mnuClearQSearch", typeof(ControlPanelCommands))
            {
                Section = "main_menu",
                Type = CommandType.ClearQSearch
            };
            QSearchIgnoreCase = new PNRoutedUICommand("Ignore case'", "mnuQSearchIgnoreCase", typeof(ControlPanelCommands))
            {
                Section = "main_menu",
                Type = CommandType.QSearchIgnoreCase
            };
        }

        public static PNRoutedUICommand Adjust { get; }

        public static PNRoutedUICommand Placement { get; }

        public static PNRoutedUICommand Visibility { get; }

        public static PNRoutedUICommand ShowNote { get; }

        public static PNRoutedUICommand TagsCurrent { get; }

        public static PNRoutedUICommand Switches { get; }

        public static PNRoutedUICommand EmptyBin { get; }

        public static PNRoutedUICommand RestoreNote { get; }

        public static PNRoutedUICommand Preview { get; }

        public static PNRoutedUICommand PreviewSettings { get; }

        public static PNRoutedUICommand UseCustColor { get; }

        public static PNRoutedUICommand ChooseCustColor { get; }

        public static PNRoutedUICommand PreviewRight { get; }

        public static PNRoutedUICommand ShowGroupsOnCp { get; }

        public static PNRoutedUICommand ColReset { get; }

        public static PNRoutedUICommand Password { get; }

        public static PNRoutedUICommand Search { get; }

        public static PNRoutedUICommand QuickSearch { get; }

        public static PNRoutedUICommand IncBinInQSearch { get; }

        public static PNRoutedUICommand ClearQSearch { get; }

        public static PNRoutedUICommand QSearchIgnoreCase { get; }

        public static PNRoutedUICommand Centralize { get; }

        public static PNRoutedUICommand PreviewFromMenu { get; }

        public static PNRoutedUICommand ShowGroupsOnCpFromMenu { get; }

        public static PNRoutedUICommand PreviewRightFromMenu { get; }
    }

    public class PNRoutedUICommand : RoutedUICommand, INotifyPropertyChanged
    {
        public PNRoutedUICommand(string text, string name, Type ownerType)
            : base(text, name, ownerType)
        {
        }

        public PNRoutedUICommand(string text, string name, Type ownerType, InputGestureCollection inputGestures)
            : base(text, name, ownerType, inputGestures)
        {
        }

        public string Section { get; set; }

        internal CommandType Type { get; set; }

        public new string Text
        {
            get => base.Text;
            set
            {
                if (value == base.Text) return;
                base.Text = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
