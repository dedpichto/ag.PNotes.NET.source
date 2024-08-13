using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace PNUpdater
{
    internal enum UpdateType
    {
        Program//,
        //PostPlugin
    }

    internal sealed class Params
    {
        private static readonly Lazy<Params> _Lazy = new Lazy<Params>(() => new Params());
        private Params()
        {
        }

        internal static Params Instance
        {
            get { return _Lazy.Value; }
        }

        private UpdateType _UpdateType;
        private string _UpdateUrl;
        private string _ProgramToRun;
        private string _TargetDir;
        private string _ProgramDir;
        //private readonly List<string> _PluginsList = new List<string>();
        private readonly List<string> _Captions = new List<string>();
        private string _SelfPath;
        private readonly List<string> _DirectoriesToDelete = new List<string>();
        private string _UpdateZip;
        internal List<string> DirectoriesToDelete
        {
            get { return Instance._DirectoriesToDelete; }
        } 
        internal string SelfPath
        {
            get { return Instance._SelfPath; }
            set { Instance._SelfPath = value; }
        }
        internal string ProgramDir
        {
            get { return Instance._ProgramDir; }
            set { Instance._ProgramDir = value; }
        }
        internal List<string> Captions
        {
            get { return Instance._Captions; }
        } 
        //internal List<string> PluginsList
        //{
        //    get { return Instance._PluginsList; }
        //}
        internal string TargetDir
        {
            get { return Instance._TargetDir; }
            set { Instance._TargetDir = value; }
        }
        internal string ProgramToRun
        {
            get { return Instance._ProgramToRun; }
            set { Instance._ProgramToRun = value; }
        }
        internal string UpdateUrl
        {
            get { return Instance._UpdateUrl; }
            set { Instance._UpdateUrl = value; }
        }
        internal UpdateType UpdateType
        {
            get { return Instance._UpdateType; }
            set { Instance._UpdateType = value; }
        }
        internal string UpdateZip
        {
            get { return Instance._UpdateZip; }
            set { Instance._UpdateZip = value; }
        }
    }
}
