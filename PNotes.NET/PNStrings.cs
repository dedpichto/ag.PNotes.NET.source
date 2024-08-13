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

namespace PNotes.NET
{
    internal class PNStrings
    {
        internal const string SHORTCUT_FILE = @"\PNotes.NET.lnk";
        internal const string PROG_NAME = "PNotes.NET";
        internal const string CHM_FILE = "PNotes.NET.chm";
        internal const string PDF_FILE = "PNotes.NET.pdf";
        internal const string REPORT_VIEWER_LIB = "reportviewer.zip";
        internal const string OLD_EDITION_ARCHIVE = "PNOldEditionBackup.zip";
        internal const string PRE_RUN_FILE = "pnotesprerun.xml";
        internal const string ATT_FROM = "from";
        internal const string ATT_TO = "to";
        internal const string ATT_NAME = "name";
        internal const string ATT_DEL_DIR = "deleteDir";
        internal const string ATT_IS_CRITICAL = "isCritical";
        internal const string ELM_COPY_THEMES = "copy_themes";
        internal const string ELM_COPY_PLUGS = "copy_plugins";
        internal const string ELM_COPY_FILES = "copy_files";
        internal const string ELM_COPY = "copy";
        internal const string ELM_PRE_RUN = "pre_run";
        internal const string ELM_REMOVE = "remove";
        internal const string ELM_DIR = "dir";
        internal const string URL_DOWNLOAD_ROOT = "http://downloads.sourceforge.net/pnotes/";
        internal const string URL_HELP = "http://pnotes.sf.net/helpnet/index.html";
        internal const string URL_SKINS = "http://pnotes.sourceforge.net/index.php?page=9";
        internal const string URL_LANGS = "http://pnotes.sourceforge.net/index.php?page=3";
        internal const string URL_THEMES = "http://pnotes.sourceforge.net/index.php?page=11";
        internal const string URL_UPDATE = "http://pnotes.sourceforge.net/versionnet.txt";
        internal const string URL_PLUGINS_UPDATE = "http://pnotes.sourceforge.net/versionplugins.txt";
        internal const string URL_MAIN = "http://pnotes.sourceforge.net/index.php?page=1";
        internal const string URL_DOWNLOAD = "http://pnotes.sourceforge.net/index.php?page=5";
        internal const string URL_PLUGINS_DOWNLOAD = "http://pnotes.sourceforge.net/index.php?page=10";
        internal const string URL_DOWNLOAD_DIR = "http://downloads.sourceforge.net/pnotes/";

        internal const string URL_PAYPAL =
            "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=8YEWS9ZW3VFJS&lc=IL&item_name=PNotes (it is free and will remain free, but your donation wiil definitely help to make it better)&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted";

        internal const string URL_DICTIONARIES = "http://extensions.openoffice.org/";
        internal const string URL_DICT_FTP = "ftp://ftp.snt.utwente.nl/pub/software/openoffice/contrib/dictionaries";
        internal const string URL_CRITICAL_UPDATES = "http://pnotes.sourceforge.net/critical.xml";
        internal const string URL_THEMES_UPDATE = "http://pnotes.sourceforge.net/versionthemes.xml";
        internal const string MAIL_ADDRESS = "andrey.gruber@gmail.com";
        internal const string DEF_THEME = "Dark Gray (default)";
        internal const string TEMP_DB_FILE = "temp.db3";
        internal const string DB_FILE = "notes.db3";
        internal const string SETTINGS_FILE = "settings.db3";
        internal const string CONTACTS_FILE = "contacts.db3";
        internal const string DEF_NOTE_NAME = "Untitled";
        internal const string NOTE_EXTENSION = ".pnote";
        internal const string NOTE_BACK_EXTENSION = ".pnxb";
        internal const string FULL_BACK_EXTENSION = ".pnback";
        internal const string NOTE_AUTO_BACK_EXTENSION = ".pnote~";
        internal const string SKIN_EXTENSION = ".pnskn";
        internal const string THEME_FILE_MASK = "*.pntheme";
        internal const string DEF_CAPTION_FONT = "Segoe UI";
        //internal const string RESOURCE_PREFIX = "pack://application:,,,/images/";
        //internal const string BIG_IMAGES_PREFIX = "pack://application:,,,/images/bigimages/";
        internal const string FOLDERS_PREFIX = "pack://application:,,,/images/folders/";
        internal const string SMILIES_PREFIX = "pack://application:,,,/images/smilies/";
        internal const string PNG_EXT = ".png";
        internal const string CRITICAL_UPDATE_LOG = "pncritical";
        internal const string DATE_TIME_FORMAT = "dd MMM yyyy HH:mm:ss";
        internal const string DATE_FORMAT = "dd MMM yyyy";
        internal const string LINK_PFX = "note:";
        internal const string PLACEHOLDER1 = "%PLACEHOLDER1%";
        internal const string PLACEHOLDER2 = "%PLACEHOLDER2%";
        internal const string PLACEHOLDER3 = "%PLACEHOLDER3%";
        internal const string YEARS = "%YEARS%";
        internal const string MONTHS = "%MONTHS%";
        internal const string WEEKS = "%WEEKS%";
        internal const string DAYS = "%DAYS%";
        internal const string HOURS = "%HOURS%";
        internal const string MINUTES = "%MINUTES%";
        internal const string SECONDS = "%SECONDS%";
        internal const string TEMP_SYNC_DIR = "pnotestempsync";
        internal const string TEMP_THEMES_DIR = "pnotestempthemes";
        internal const string TEMP_DICT_DIR = "pnotestempdict";
        internal const string RESTART = "RESTART";

        //tables
        internal const string T_CONTACT_GROUPS = "CONTACT_GROUPS";
        internal const string T_CONTACTS = "CONTACTS";
        internal const string T_MAIL_CONTACTS = "MAIL_CONTACTS";

        internal const string DATE_FORMAT_CHARS =
            "d\tDay of month as digits with no leading zero for single-digit days.\n" +
            "dd\tDay of month as digits with leading zero for single-digit days.\n" +
            "ddd\tDay of week as a three-letter abbreviation.\n" +
            "dddd\tDay of week as its full name.\n" +
            "M\tMonth as digits with no leading zero for single-digit months.\n" +
            "MM\tMonth as digits with leading zero for single-digit months.\n" +
            "MMM\tMonth as a three-letter abbreviation.\n" +
            "MMMM\tMonth as its full name.\n" +
            "y\tYear as last two digits, but with no leading zero for years less than 10.\n" +
            "yy\tYear as last two digits, but with leading zero for years less than 10.\n" +
            "yyyy\tYear represented by full four digits.";

        internal const string TIME_FORMAT_CHARS =
            "h\tHours with no leading zero for single-digit hours; 12-hour clock.\n" +
            "hh\tHours with leading zero for single-digit hours; 12-hour clock.\n" +
            "H\tHours with no leading zero for single-digit hours; 24-hour clock.\n" +
            "HH\tHours with leading zero for single-digit hours; 24-hour clock.\n" +
            "m\tMinutes with no leading zero for single-digit minutes.\n" +
            "mm\tMinutes with leading zero for single-digit minutes.\n" +
            "s\tSeconds with no leading zero for single-digit seconds.\n" +
            "ss\tSeconds with leading zero for single-digit seconds.\n" +
            "t\tOne character time-marker string, such as A or P.\n" +
            "tt\tMulticharacter time-marker string, such as AM or PM.";

        internal const string CREATE_TRIGGERS =
            "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_DELETE] AFTER DELETE ON [NOTES] BEGIN DELETE FROM NOTES_TAGS WHERE NOTE_ID = OLD . ID; DELETE FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = OLD . ID; DELETE FROM NOTES_SCHEDULE WHERE NOTE_ID = OLD . ID; DELETE FROM LINKED_NOTES WHERE NOTE_ID = OLD . ID;  END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_CUSTOM_NOTES_SETTINGS_UPDATE] AFTER UPDATE ON [CUSTOM_NOTES_SETTINGS] BEGIN UPDATE CUSTOM_NOTES_SETTINGS SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_GROUPS_UPDATE] AFTER UPDATE ON [GROUPS] BEGIN UPDATE GROUPS SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE GROUP_ID = OLD . GROUP_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_LINKED_NOTES_UPDATE] AFTER UPDATE ON [LINKED_NOTES] BEGIN UPDATE LINKED_NOTES SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_SCHEDULE_UPDATE] AFTER UPDATE ON [NOTES_SCHEDULE] BEGIN UPDATE NOTES_SCHEDULE SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_TAGS_UPDATE] AFTER UPDATE ON [NOTES_TAGS] BEGIN UPDATE NOTES_TAGS SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE NOTE_ID = OLD . NOTE_ID ; END;" +
            "CREATE TRIGGER IF NOT EXISTS [TRG_NOTES_UPDATE] AFTER UPDATE ON [NOTES] BEGIN UPDATE NOTES SET UPD_DATE = strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' ) WHERE ID = OLD . ID ; END;";

        internal const string DROP_TRIGGERS = "DROP TRIGGER TRG_CUSTOM_NOTES_SETTINGS_UPDATE;" +
                                              "DROP TRIGGER TRG_GROUPS_UPDATE;" +
                                              "DROP TRIGGER TRG_LINKED_NOTES_UPDATE;" +
                                              "DROP TRIGGER TRG_NOTES_SCHEDULE_UPDATE;" +
                                              "DROP TRIGGER TRG_NOTES_TAGS_UPDATE;" +
                                              "DROP TRIGGER TRG_NOTES_UPDATE;";

        internal const string NONE = "(none)";
        internal const string NO_GROUP = "(No group)";
        internal const string END_OF_FILE = "<EOF>";
        internal const string END_OF_ADDRESS = "<EOA>";
        internal const string END_OF_TEXT = "<EOT>";
        internal const string END_OF_NOTE = "<EON>";
        internal const string END_OF_OPTIONS = "<EOO>";
        internal const string END_OF_RECEVIER = "<EOR>";
        internal const string REQUEST_HEADER = "<REQ_HEAD>";
        internal const string REQUEST_SPLITTER = "REQ_SPLIT>";
        internal const string SUCCESS = "SUCCESS";
        internal const string NOSPLASH = "nosplash";
        internal const string DEFAULT_FONT_NAME = "Lucida Sans Unicode";

        //internal const string DEFAULT_UI_FONT = "Microsoft Sans Serif, 8.25pt";
        internal const string MENU_SEPARATOR_STRING = "-----------";

        internal const string MAIL_PATTERN =
            @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";

        internal const char DEL_CHAR = '\a';

        internal static readonly string[] RestrictedHotkeys =
        {
            "Ctrl+S",
            "Ctrl+C",
            "Ctrl+X",
            "Ctrl+V",
            "Ctrl+O",
            "Ctrl+P",
            "Ctrl+F",
            "Ctrl+A",
            "Ctrl+Z",
            "Ctrl+Y",
            "Ctrl+G",
            "Ctrl+E",
            "Ctrl+R",
            "Ctrl+L",
            "Ctrl+B",
            "Ctrl+I",
            "Ctrl+K",
            "Ctrl+U",
            "Ctrl+Del",
            "Shift+Del",
            "Ctrl+Backspace",
            "Shift+Ins",
            "F1",
            "F3",
            "F5"
        };

        //internal rtf of first welcome note - DO NOT CHANGE ANY CHARATER!!!
        internal const string WELCOME_NOTE_RTF =
            @"{\rtf1\ansi\ansicpg1251\deff0\nouicompat\deflang1049{\fonttbl{\f0\fnil\fcharset0 Lucida Sans Unicode;}{\f1\fnil\fcharset0 Segoe Script;}{\f2\fnil\fcharset204 Microsoft Sans Serif;}{\f3\fnil\fcharset2 Symbol;}}
{\colortbl ;\red0\green0\blue0;\red255\green0\blue0;\red0\green0\blue255;\red255\green0\blue255;\red128\green0\blue0;}
{\*\generator Riched20 10.0.14393}\viewkind4\uc1 
\pard\cf1\f0\fs16\lang1033 Welcome to PNotes.NET!\par
In order to start find the {\pict{\*\picprop}\wmetafile8\picw508\pich508\picwgoal288\pichgoal288 
010009000003980300000000820300000000050000000b0200000000050000000c02fc01fc0182
030000430f2000cc0000001800180000000000fc01fc0100000000280000001800000018000000
0100180000000000c00600007412000074120000000000000000000074ddf274ddf274ddf274dd
f274ddf274ddf274ddf275dbf074dcf074ddf274ddf274ddf274ddf274ddf274ddf274ddf274dd
f274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274dd
f274ddf274ddf274ddf274ddf275dcf174dcf174ddf274ddf274ddf274ddf274ddf274ddf274dd
f274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf275dcf174ddf249cf
ec50cae768cce074ddf274ddf274ddf275dcf174dcf174ddf274ddf274ddf274ddf274ddf274dd
f274ddf274ddf274ddf274ddf274ddf274ddf274ddf275dcf174ddf247cfecaff5ffd6fbff82ed
fd4fd3f255d2eb71d8ed74ddf274ddf275dcf174dcf174ddf274ddf274ddf274ddf274ddf274dd
f274ddf274ddf274ddf274ddf274ddf274ddf266d2e971eafdeafcfad9fafbf3fffeecffffb2f5
ff5fdffc50ceea65d1e974ddf274ddf274ddf274dcf174dcf174ddf274ddf274ddf274ddf274dd
f274ddf274ddf275dcf174dcf144cef1dafefdc1eff47fc1d595d1e0c1e9f0e5fcfafaffffd8fe
ff8eecfd53d4f453ceea70d7eb74ddf274ddf274ddf274dcf174ddf274ddf274ddf274ddf274dc
f174ddf261cbe38cf0ffd5fcfbc3f7f9a6dee984c5d876b8cf7fc0d5a3d7e4cff0f3f2fffef0ff
ffbdf8ff6ae1fb4ecfec64d0e674ddf274ddf274ddf274ddf274ddf274ddf275dcf174ddf245ce
efc9fdfcc7fdfdcbfdfdd3fdfdd0ffffbdf0f59ad5e17ebed375b7ce84c3d7afdde8ddf5f6faff
ffe2ffff9bf0ff57d6f354cfea71d4e974ddf274ddf274dcf174ddf266cfe776e6fdc6faf87fca
db91d6e3b0ecf1c8fcfdd5ffffdaffffd0fcfdb6e8ef90cddd76b8cf74b6ce8ec7d9bfe4ebe5fa
f8fffffe7bebff4fd3ee74ddf274ddf274dcf174ddf249c6e5acfbfeb7f9fa90dae678c4d66dbc
d275c0d68fd3e2ade8efc9fcfdd9ffffdcffffcefafcabdfe987c6d868aec9bbe4eac5f9ff44d0
f074ddf274dcf174ddf274ddf272d9ef49d4f4c0fdfcb7fdfdc0ffffbcfefeadf0f594dbe77ac6
d86bbad071bdd28ccfdface5eecbfcfcdeffffdbffffc8f1f5d4f9fb41d0f274dcf175dcf174dd
f274dcf074ddf264cbe377eafcb1f5f68ee0eaa5f0f2b7fefec2ffffc5ffffc1ffffb1f3f697dc
e87bc5d86ab8cf6eb7ce81c7d9b3e9f0e5fdfd5fddf96bd4eb74ddf274ddf274ddf274dcf074dd
f24dc5e399fbff96e9f05aaec459a7b968b6c884d1df9feaf1b5fbfcc1ffffcaffffc6ffffb7f6
f99cdee975c0d4a4dde6a7f6ff4ccaea74ddf274dcf174ddf274ddf274dcf174ddf245c6e8a5fd
fba2fafc86cccd6eafb666aeb963b5c562bbd265bcd27accdd92dde8acf3f6c0ffffc9fdfdc4fd
fdcbfdfc47cdf173dcf074dcf174ddf274ddf274ddf274ddf271d7ec43d4f3aeffff96f4f46faf
b174b2ab7dbbbd97dfe9a4f9fa8cdfe974c9db64bbd161b7cf6bbcd181d1e0c1faf987f0ff5eca
e374ddf274dcf174ddf274ddf274ddf274ddf26ccbe063e4f899f6f85bb3c2566c643a5781121e
9f070c8d476bb5b3ffffb5ffffabfafb9ae9f082d4e275c9d9b7f9f94bccee74dcf074ddf274dd
f274ddf274ddf274ddf274ddf255c4e177ebf894f7fa76bdc06561760014c70012ce00029d0000
7a5e98c69ce9f2abffffb6ffffb5fdfdb7fcfd9af5fe51c9e574ddf274dcf174ddf274ddf274dd
f274ddf274ddf252c0db8bf6fba2fcfda1fdf84477da0635fd042ee50007a1010ea60000950a15
94539ec576d3df86dde7b6fbfa57def86dd3e874ddf274ddf274ddf274ddf274ddf274ddf274dd
f248bddc5fe7fe84f2fc94ffff4885f4315dfc1d4bf4052de70219ca010cac010295376ab06cd1
dd5cb6d2a5fafb4acbe974ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf273d7ec66c8
dc4fc1dd4fc6de2997e51b49fb2358ff1844f10319c70418bd0303a675b7d8aefcfd9ef7f992f6
fb46c5e674ddf274dcf174ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274dd
f274ddf258a5e24889da1545da001fe9000dd00942c666e6f67ff1fb9cffff82f3fd5fc5dc74dd
f274dbf174ddf274ddf274ddf274ddf274ddf274ddf274ddf274dcf174dcf174dcf174ddf274dd
f274ddf26bc2e23460d14279c96fcae165cbe250c3e14fc0dc29c7ef6dd0e574ddf274ddf274dd
f274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274dbf174dcf174dd
f274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274dd
f274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274dcf174dcf074dc
f074dbf174dcf074dcf074dcf074ddf274ddf274ddf274ddf274ddf274ddf274ddf274ddf274dd
f2030000000000
} icon in the system notification area.\par

\pard{\pntext\f3\'B7\tab}{\*\pn\pnlvlblt\pnf3\pnindent400{\pntxtb\'B7}}\fi-400\li400 Right click on this icon will open main menu. \par
{\pntext\f3\'B7\tab}Double click on this icon will create new note (this behavior can be changed later at the Preferences dialog).\par
{\pntext\f3\'B7\tab}Right click on note's header or footer will bring up note's popup menu. \par
{\pntext\f3\'B7\tab}Right click on note's edit area will bring up edit popup menu with full set of formatting options.\par

\pard\cf2\b\f1\fs24 Thank\cf1  \cf3 you\cf1  \cf4 and\cf1  \cf5 enjoy!\cf0\b0\f2\fs16\lang1049\par
}";

    }
}