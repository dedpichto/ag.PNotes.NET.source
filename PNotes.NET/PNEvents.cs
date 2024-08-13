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

#region Using

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Domino;
using PNStaticFonts;
using Image = System.Drawing.Image;

#endregion

namespace PNotes.NET
{
    internal class EditControlSizeChangedEventArgs : EventArgs
    {
        internal Rectangle NewRectangle { get; }

        internal EditControlSizeChangedEventArgs(Rectangle newEditRect)
        {
            NewRectangle = newEditRect;
        }
    }

    internal class ListBoxItemCheckChangedEventArgs : EventArgs
    {
        internal bool State { get; }

        internal ListBoxItemCheckChangedEventArgs(bool state)
        {
            State = state;
        }
    }

    internal class TreeViewItemCheckChangedEventArgs : EventArgs
    {
        internal bool State { get; }
        internal TreeView ParentTreeView { get; }

        internal TreeViewItemCheckChangedEventArgs(bool state, TreeView parent)
        {
            State = state;
            ParentTreeView = parent;
        }
    }

    internal class BaloonClickedEventArgs : EventArgs
    {
        internal BaloonMode Mode { get; }

        internal BaloonClickedEventArgs(BaloonMode mode)
        {
            Mode = mode;
        }
    }

    internal class NewVersionFoundEventArgs : EventArgs
    {
        internal string Version { get; }

        internal NewVersionFoundEventArgs(string version)
        {
            Version = version;
        }
    }

    internal class ThemesUpdateFoundEventArgs : EventArgs
    {
        internal List<ThemesUpdate> ThemesList { get; }

        internal ThemesUpdateFoundEventArgs(List<ThemesUpdate> listThemes)
        {
            ThemesList = listThemes;
        }
    }

    internal class PluginsUpdateFoundEventArgs : EventArgs
    {
        internal List<PluginsUpdate> PluginsList { get; }
        internal PluginsUpdateFoundEventArgs(List<PluginsUpdate> listEx)
        {
            PluginsList = listEx;
        }
    }

    internal class CriticalUpdatesFoundEventArgs : EventArgs
    {
        internal bool Accepted { get; set; }
        internal string ProgramFileName { get; }
        internal List<CriticalPluginUpdate> Plugins { get; }
        internal CriticalUpdatesFoundEventArgs(string progFileName, List<CriticalPluginUpdate> plugins)
        {
            ProgramFileName = progFileName;
            Plugins = plugins;
        }
    }

    internal class PostSelectedEventArgs : EventArgs
    {
        internal string PostText { get; }

        internal PostSelectedEventArgs(string text)
        {
            PostText = text;
        }
    }

    internal class CompSelectedEventArgs : EventArgs
    {
        internal CompSelectedEventArgs(string name)
        {
            CompName = name;
        }

        internal string CompName { get; }
    }

    internal class NotesSendNotificationEventArgs : EventArgs
    {
        internal IEnumerable<string> SentTo { get; }

        internal IEnumerable<PNote> Notes { get; }

        internal SendResult Result { get; }

        internal NotesSendNotificationEventArgs(IEnumerable<PNote> notes, IEnumerable<string> sentTo, SendResult result)
        {
            Notes = notes;
            SentTo = sentTo;
            Result = result;
        }
    }

    internal class PinnedWindowChangedEventArgs : EventArgs
    {
        internal PinnedWindowChangedEventArgs(string pinClass, string pinText)
        {
            PinClass = pinClass;
            PinText = pinText;
        }

        internal string PinText { get; }

        internal string PinClass { get; }
    }

    internal class ExchangeEnableChangedEventArgs : EventArgs
    {
        internal ExchangeEnableChangedEventArgs(bool enabled)
        {
            Enabled = enabled;
        }

        internal bool Enabled { get; }
    }

    internal class ContactsSelectedEventArgs : EventArgs
    {
        internal List<PNContact> Contacts { get; } = new List<PNContact>();
    }

    internal class ContactGroupChangedEventArgs : EventArgs
    {
        internal ContactGroupChangedEventArgs(PNContactGroup cg, AddEditMode mode)
        {
            Group = cg;
            Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode { get; }

        internal PNContactGroup Group { get; }
    }

    internal class ContactChangedEventArgs : EventArgs
    {
        internal ContactChangedEventArgs(PNContact contact, AddEditMode mode)
        {
            Contact = contact;
            Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode { get; }

        internal PNContact Contact { get; }
    }

    internal class ContactsImportedEventArgs : EventArgs
    {
        internal IEnumerable<Tuple<string, string>> Contacts { get; }

        internal ContactsImportedEventArgs(IEnumerable<Tuple<string, string>> contacts)
        {
            Contacts = contacts;
        }
    }

    internal class MailContactChangedEventArgs : EventArgs
    {
        internal MailContactChangedEventArgs(PNMailContact contact, AddEditMode mode)
        {
            Contact = contact;
            Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode { get; }

        internal PNMailContact Contact { get; }
    }

    internal class SmtpChangedEventArgs : EventArgs
    {
        internal SmtpChangedEventArgs(PNSmtpProfile profile, AddEditMode mode)
        {
            Profile = profile;
            Mode = mode;
            Accepted = true;
        }

        internal bool Accepted { get; set; }

        internal AddEditMode Mode { get; }

        internal PNSmtpProfile Profile { get; }
    }

    internal class LocalSyncCompleteEventArgs : EventArgs
    {
        internal LocalSyncCompleteEventArgs(LocalSyncResult result)
        {
            Result = result;
        }

        internal LocalSyncResult Result { get; }
    }

    internal class SmilieSelectedEventArgs : EventArgs
    {
        internal SmilieSelectedEventArgs(Image image)
        {
            Image = image;
        }

        internal Image Image { get; }
    }

    internal class NoteAppearanceAdjustedEventArgs : EventArgs
    {
        internal NoteAppearanceAdjustedEventArgs(bool custOpacity, bool custSkinless, bool custSkin, double opacity,
                                                 PNSkinlessDetails skinless, PNSkinDetails skin)
        {
            CustomOpacity = custOpacity;
            CustomSkinless = custSkinless;
            CustomSkin = custSkin;
            Opacity = opacity;
            Skinless = skinless;
            Skin = skin;
        }

        internal PNSkinDetails Skin { get; }

        internal PNSkinlessDetails Skinless { get; }

        internal double Opacity { get; }

        internal bool CustomSkin { get; }

        internal bool CustomSkinless { get; }

        internal bool CustomOpacity { get; }
    }

    internal class NoteDeletedCompletelyEventArgs : EventArgs
    {
        internal NoteDeletedCompletelyEventArgs(string id, int groupId)
        {
            Id = id;
            GroupId = groupId;
        }

        internal int GroupId { get; }

        internal string Id { get; }
    }

    internal class NewNoteCreatedEventArgs : EventArgs
    {
        internal NewNoteCreatedEventArgs(PNote note)
        {
            Note = note;
        }

        internal PNote Note { get; }
    }

    internal class SpellCheckingStatusChangedEventArgs : EventArgs
    {
        internal SpellCheckingStatusChangedEventArgs(bool newStatus)
        {
            Status = newStatus;
        }

        internal bool Status { get; }
    }

    internal class FontSelectedEventArgs : EventArgs
    {
        internal FontSelectedEventArgs(LOGFONT lf)
        {
            LogFont = lf;
        }

        internal LOGFONT LogFont { get; }
    }

    internal class PasswordChangedEventArgs : EventArgs
    {
        internal PasswordChangedEventArgs(string newPwrd)
        {
            NewPassword = newPwrd;
        }

        internal string NewPassword { get; }
    }

    internal class GroupChangedEventArgs : EventArgs
    {
        internal GroupChangedEventArgs(PNGroup group, AddEditMode mode, PNTreeItem treeItem)
        {
            Group = group;
            Mode = mode;
            TreeItem = treeItem;
        }

        internal AddEditMode Mode { get; }

        internal PNGroup Group { get; }

        internal PNTreeItem TreeItem { get; }
    }

    internal class GroupPropertyChangedEventArgs : EventArgs
    {
        internal GroupPropertyChangedEventArgs(object newStateObject, GroupChangeType type)
        {
            NewStateObject = newStateObject;
            Type = type;
        }

        internal GroupChangeType Type { get; }

        internal object NewStateObject { get; }
    }

    internal class NoteSendReceiveStatusChangedEventArgs : EventArgs
    {
        internal NoteSendReceiveStatusChangedEventArgs(SendReceiveStatus newStatus, SendReceiveStatus oldStatus)
        {
            NewStatus = newStatus;
            OldStatus = oldStatus;
        }

        internal SendReceiveStatus OldStatus { get; }

        internal SendReceiveStatus NewStatus { get; }
    }

    internal class NoteDockStatusChangedEventArgs : EventArgs
    {
        internal NoteDockStatusChangedEventArgs(DockStatus newStatus, DockStatus oldStatus)
        {
            NewStatus = newStatus;
            OldStatus = oldStatus;
        }

        internal DockStatus OldStatus { get; }

        internal DockStatus NewStatus { get; }
    }

    internal class NoteDateChangedEventArgs : EventArgs
    {
        internal NoteDateChangedEventArgs(DateTime newDate, DateTime oldDate, NoteDateType type)
        {
            NewDate = newDate;
            OldDate = oldDate;
            Type = type;
        }

        internal DateTime NewDate { get; }

        internal DateTime OldDate { get; }

        internal NoteDateType Type { get; }
    }

    internal class NoteGroupChangedEventArgs : EventArgs
    {
        internal NoteGroupChangedEventArgs(int newGroup, int oldGroup)
        {
            NewGroup = newGroup;
            OldGroup = oldGroup;
        }

        internal int NewGroup { get; }

        internal int OldGroup { get; }
    }

    internal class NoteNameChangedEventArgs : EventArgs
    {
        internal NoteNameChangedEventArgs(string oldName, string newName)
        {
            NewName = newName;
            OldName = oldName;
        }

        internal string NewName { get; }

        internal string OldName { get; }
    }

    internal class SaveAsNoteNameSetEventArgs : EventArgs
    {
        internal SaveAsNoteNameSetEventArgs(string name, int groupId)
        {
            Name = name;
            GroupId = groupId;
        }

        internal int GroupId { get; }

        internal string Name { get; }
    }

    internal class NoteDeletedEventArgs : EventArgs
    {
        internal NoteDeletedEventArgs(NoteDeleteType type)
        {
            Type = type;
        }

        internal NoteDeleteType Type { get; }

        internal bool Processed { get; set; }
    }

    internal class NoteBooleanChangedEventArgs : EventArgs
    {
        internal NoteBooleanChangedEventArgs(bool state, NoteBooleanTypes type, object stateObject)
        {
            State = state;
            Type = type;
            StateObject = stateObject;
        }

        internal bool Processed { get; set; }

        internal NoteBooleanTypes Type { get; }

        internal object StateObject { get; }

        internal bool State { get; }
    }

    internal class NoteMovedEventArgs : EventArgs
    {
        internal NoteMovedEventArgs(Point location)
        {
            NoteLocation = location;
        }

        internal Point NoteLocation { get; }
    }

    internal class NoteResizedEventArgs : EventArgs
    {
        internal NoteResizedEventArgs(Size size)
        {
            NoteSize = size;
        }

        internal Size NoteSize { get; }
    }

    internal class CustomRtfReadyEventArgs : EventArgs
    {
        internal string CustomRtf { get; }
        internal string Text { get; }

        internal CustomRtfReadyEventArgs(string customRtf)
        {
            CustomRtf = customRtf;
        }
        internal CustomRtfReadyEventArgs(string customRtf, string text)
        {
            CustomRtf = customRtf;
            Text = text;
        }
    }

    internal class SpecialSymbolSelectedEventArgs : EventArgs
    {
        internal string Symbol { get; }

        internal SpecialSymbolSelectedEventArgs(string symbol)
        {
            Symbol = symbol;
        }
    }

    internal class MenusOrderChangedEventArgs : EventArgs
    {
        internal bool Main { get; set; }
        internal bool Note { get; set; }
        internal bool Edit { get; set; }
        internal bool ControlPanel { get; set; }
    }

    internal class MailRecipientsChosenEventArgs : EventArgs
    {
        internal IEnumerable<PNMailContact> Recipients { get; }

        internal MailRecipientsChosenEventArgs(IEnumerable<PNMailContact> recipients)
        {
            Recipients = recipients;
        }
    }

    internal class CanvasSavedEventArgs : EventArgs
    {
        internal Image Image { get; }

        internal CanvasSavedEventArgs(Image image)
        {
            Image = image;
        }
    }

    internal class LotusCredentialSetEventArgs : EventArgs
    {
        internal NotesView PeopleView { get; }
        internal NotesView ContactsView { get; }

        internal LotusCredentialSetEventArgs(NotesView peopleView, NotesView contactsView)
        {
            PeopleView = peopleView;
            ContactsView = contactsView;
        }
    }
}