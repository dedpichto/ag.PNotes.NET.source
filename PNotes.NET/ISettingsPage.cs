using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNotes.NET
{
    internal interface ISettingsPage
    {
        void Init(WndSettings setings);
        void InitPage(bool firstTime);
        ChangesAction DefineChangesAction();
        bool SavePage();
        bool SaveCollections();
        bool CanExecute();
        void RestoreDefaultValues();
        bool InDefClick { get; set; }

        event EventHandler PromptToRestart;
    }
}
