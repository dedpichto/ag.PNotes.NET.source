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
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using SQLiteWrapper;

namespace PNCommon
{
    internal class _FieldData
    {
        internal string Name { get; set; }
        internal string Type { get; set; }
        internal bool NotNull { get; set; }
    }

    internal static class PNSyncCommon
    {
        private static CultureInfo _cultureInvariant;

        private static readonly string[] _Tables =
            {
                "NOTES", "GROUPS", "NOTES_SCHEDULE", "LINKED_NOTES", "NOTES_TAGS",
                "CUSTOM_NOTES_SETTINGS"
            };

        private static string createInsert(DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var fieldsList = string.Join(", ", tableData.Value.Select(f => f.Name));
                var sb = new StringBuilder();
                sb.Append("INSERT INTO ");
                sb.Append(tableData.Key);
                sb.Append(" (");
                sb.Append(fieldsList);
                sb.Append(") VALUES(");
                foreach (var f in tableData.Value)
                {
                    if (f.NotNull)
                    {
                        if (f.Type == "TEXT")
                            sb.Append("'");
                        switch (f.Type)
                        {
                            case "BOOLEAN":
                                sb.Append(Convert.ToInt32(r[f.Name]));
                                break;
                            case "TEXT":
                                sb.Append(Convert.ToString(r[f.Name]).Replace("'", "''"));
                                break;
                            case "REAL":
                                sb.Append(Convert.ToString(r[f.Name], _cultureInvariant));
                                break;
                            default:
                                sb.Append(r[f.Name]);
                                break;
                        }
                        if (f.Type == "TEXT")
                            sb.Append("'");
                    }
                    else
                    {
                        if (!DBNull.Value.Equals(r[f.Name]))
                        {
                            if (f.Type == "TEXT")
                                sb.Append("'");
                            switch (f.Type)
                            {
                                case "BOOLEAN":
                                    sb.Append(Convert.ToInt32(r[f.Name]));
                                    break;
                                case "TEXT":
                                    sb.Append(Convert.ToString(r[f.Name]).Replace("'", "''"));
                                    break;
                                case "REAL":
                                    sb.Append(Convert.ToString(r[f.Name], _cultureInvariant));
                                    break;
                                default:
                                    sb.Append(r[f.Name]);
                                    break;
                            }
                            if (f.Type == "TEXT")
                                sb.Append("'");
                        }
                        else
                        {
                            sb.Append("NULL");
                        }
                    }
                    sb.Append(", ");
                }
                sb.Length -= 2;
                sb.Append(")");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                logException(ex);
                return "";
            }
        }

        internal static Dictionary<string, List<_FieldData>> GetTablesData(SQLiteDataObject dbSrc)
        {
            try
            {
                var tablesData = new Dictionary<string, List<_FieldData>>();
                foreach (var tn in _Tables)
                {
                    var td = new List<_FieldData>();
                    var sb = new StringBuilder("pragma table_info('");
                    sb.Append(tn);
                    sb.Append("')");
                    using (var t = dbSrc.FillDataTable(sb.ToString()))
                    {
                        td.AddRange(from DataRow r in t.Rows
                                    select new _FieldData
                                    {
                                        Name = Convert.ToString(r["name"]),
                                        Type = Convert.ToString(r["type"]),
                                        NotNull = Convert.ToBoolean(r["notnull"])
                                    });
                    }
                    tablesData.Add(tn, td);
                }
                return tablesData;
            }
            catch (Exception ex)
            {
                logException(ex);
                return null;
            }
        }

        internal static void SetCultureInfo()
        {
            try
            {
                _cultureInvariant = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                _cultureInvariant.NumberFormat.NumberDecimalSeparator = ".";
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        internal static string CreateTempDir(string dirName)
        {
            try
            {
                // create temp directory
                string tempDir = Path.Combine(Path.GetTempPath(), dirName);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);
                return tempDir;
            }
            catch (Exception ex)
            {
                logException(ex);
                return "";
            }
        }

        internal static List<string> DeletedIDs(SQLiteDataObject oData)
        {
            var list = new List<string>();

            try
            {
                using (DataTable t = oData.FillDataTable("SELECT ID FROM NOTES WHERE GROUP_ID = -2"))
                {
                    list.AddRange(from DataRow r in t.Rows select (string)r["ID"]);
                }
                return list;
            }
            catch (Exception ex)
            {
                logException(ex);
                return list;
            }
        }

        private static object getUpdateValue(string tableName, string columnName)
        {
            try
            {
                switch (tableName)
                {
                    case "GROUPS":
                        switch (columnName)
                        {
                            case "IS_DEFAULT_IMAGE":
                                return 1;
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                logException(ex);
                return null;
            }
        }

        internal static bool AreTablesDifferent(SQLiteDataObject dataSrc, SQLiteDataObject dataDest)
        {
            try
            {
                foreach (var tableName in _Tables)
                {
                    var sqlQuery = "pragma table_info('" + tableName + "')";
                    using (var ts = dataSrc.FillDataTable(sqlQuery))
                    {
                        using (var td = dataDest.FillDataTable(sqlQuery))
                        {
                            var upper = ts.Rows.Count == td.Rows.Count
                                ? ts.Rows.Count
                                : Math.Min(ts.Rows.Count, td.Rows.Count);
                            for (var i = 0; i < upper; i++)
                            {
                                if (Convert.ToString(ts.Rows[i]["name"]) != Convert.ToString(td.Rows[i]["name"]) ||
                                    Convert.ToString(ts.Rows[i]["type"]) != Convert.ToString(td.Rows[i]["type"]))
                                {
                                    return true;
                                }
                            }
                            if (upper >= ts.Rows.Count) continue;
                            for (var i = upper; i < ts.Rows.Count; i++)
                            {
                                var r = ts.Rows[i];
                                var sb = new StringBuilder("ALTER TABLE ");
                                sb.Append(tableName);
                                sb.Append(" ADD COLUMN [");
                                sb.Append(r["name"]);
                                sb.Append("] ");
                                sb.Append(r["type"]);
                                if (!DBNull.Value.Equals(r["dflt_value"]))
                                {
                                    sb.Append(" DEFAULT (");
                                    sb.Append(r["dflt_value"]);
                                    sb.Append(")");
                                    sb.Append("; UPDATE ");
                                    sb.Append(tableName);
                                    sb.Append(" SET ");
                                    sb.Append(r["name"]);
                                    sb.Append(" = ");
                                    if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                    {
                                        sb.Append("'");
                                        sb.Append(Convert.ToString(r["dflt_value"]).Replace("'", "''"));
                                    }
                                    else
                                    {
                                        sb.Append(r["dflt_value"]);
                                    }
                                    if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                    {
                                        sb.Append("'");
                                    }
                                }
                                else
                                {
                                    var updValue = getUpdateValue(tableName, Convert.ToString(r["name"]));
                                    if (updValue != null)
                                    {
                                        sb.Append("; UPDATE ");
                                        sb.Append(tableName);
                                        sb.Append(" SET ");
                                        sb.Append(r["name"]);
                                        sb.Append(" = ");
                                        if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                        {
                                            sb.Append("'");
                                            sb.Append(Convert.ToString(updValue).Replace("'", "''"));
                                        }
                                        else
                                        {
                                            sb.Append(updValue);
                                        }
                                        if (Convert.ToString(r["type"]).ToUpper() == "TEXT")
                                        {
                                            sb.Append("'");
                                        }
                                    }
                                }
                                dataDest.Execute(sb.ToString());
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }
        
        internal static DateTime LastSavedTime(SQLiteDataObject oData, string id)
        {
            try
            {
                var sb = new StringBuilder("SELECT DATE_SAVED FROM NOTES WHERE ID = '");
                sb.Append(id);
                sb.Append("'");
                var obj = oData.GetScalar(sb.ToString());
                if (obj == null || DBNull.Value.Equals(obj))
                    return DateTime.MinValue;
                return Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return DateTime.MinValue;
            }
        }


        private static bool executeTransaction(SQLiteDataObject oData, string sqlQuery)
        {
            var inTrans = false;
            try
            {
#if DEBUG
                LogThis(sqlQuery);
#endif
                inTrans = oData.BeginTransaction();
                if (inTrans)
                {
                    oData.ExecuteInTransaction(sqlQuery);
                    oData.CommitTransaction();
                    inTrans = false;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                if (inTrans)
                {
                    oData.RollbackTransaction();
                }
                logException(ex);
                return false;
            }
        }

        internal static bool AreFilesDifferent(string path1, string path2)
        {
            try
            {
                using (var fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read))
                {
                    using (var fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read))
                    {
                        var l1 = fs1.Length;
                        var l2 = fs2.Length;
                        if (l1 != l2)
                        {
                            return true;
                        }
                        var b1 = new byte[l1];
                        var b2 = new byte[l2];
                        fs1.Read(b1, 0, b1.Length);
                        fs2.Read(b2, 0, b2.Length);

                        return b1.Except(b2).Any();
                        //while (fs1.Position < l1 && fs2.Position < l2)
                        //{
                        //    if (fs1.ReadByte() != fs2.ReadByte())
                        //    {
                        //        return true;
                        //    }
                        //}
                    }
                }
                //return false;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        #region Exchange procedures
        internal static bool ExchangeGroups(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (DataTable t1 = dataSrc.FillDataTable("SELECT * FROM GROUPS"))
                {
                    using (DataTable t2 = dataDest.FillDataTable("SELECT * FROM GROUPS"))
                    {
                        DataTable t = t1.Clone();
                        IEnumerable<DataRow> rows1 = t1.AsEnumerable();
                        IEnumerable<DataRow> rows2 = t2.AsEnumerable();
                        foreach (DataRow r1 in rows1)
                        {
                            DataRow r2 = rows2.FirstOrDefault(r => (int)r["GROUP_ID"] == (int)r1["GROUP_ID"]);
                            if (r2 != null)
                            {
                                if (Convert.ToInt64(r1["UPD_DATE"]) >= Convert.ToInt64(r2["UPD_DATE"]))
                                {
                                    t.Rows.Add(r1.ItemArray);
                                }
                                else if (Convert.ToInt64(r2["UPD_DATE"]) > Convert.ToInt64(r1["UPD_DATE"]))
                                {
                                    t.Rows.Add(r2.ItemArray);
                                }
                            }
                            else
                            {
                                t.Rows.Add(r1.ItemArray);
                            }
                        }
                        foreach (DataRow r2 in rows2)
                        {
                            if (rows1.All(r => (int)r["GROUP_ID"] != (int)r2["GROUP_ID"]))
                            {
                                t.Rows.Add(r2.ItemArray);
                            }
                        }

                        var sqlList = new List<string> { "DELETE FROM GROUPS" };
                        sqlList.AddRange(from DataRow r in t.Rows select createInsert(r, tableData));
                        //foreach (string s in sqlList)
                        //{
                        //    System.Diagnostics.Debug.WriteLine(s);
                        //}
                        bool inTrans1 = false;
                        bool inTrans2 = false;
                        try
                        {
                            inTrans1 = dataSrc.BeginTransaction();
                            inTrans2 = dataDest.BeginTransaction();
                            if (inTrans1 && inTrans2)
                            {
                                foreach (string s in sqlList)
                                {
                                    dataSrc.ExecuteInTransaction(s);
                                    dataDest.ExecuteInTransaction(s);
                                }
                                dataSrc.CommitTransaction();
                                inTrans1 = false;
                                dataDest.CommitTransaction();
                                inTrans2 = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (inTrans1)
                            {
                                dataSrc.RollbackTransaction();
                            }
                            if (inTrans2)
                            {
                                dataDest.RollbackTransaction();
                            }
                            logException(ex);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        internal static bool ExchangeData(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, Dictionary<string, List<_FieldData>> tablesData)
        {
            try
            {
                if (!exchangeNotes(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "NOTES")))
                {
                    return false;
                }
                if (!exchangeCustomNotesSettings(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "CUSTOM_NOTES_SETTINGS")))
                {
                    return false;
                }
                if (!exchangeLinkedNotes(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "LINKED_NOTES")))
                {
                    return false;
                }
                if (!exchangeNotesSchedule(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "NOTES_SCHEDULE")))
                {
                    return false;
                }
                if (!exchangeNotesTags(dataSrc, dataDest, id, tablesData.FirstOrDefault(td => td.Key == "NOTES_TAGS")))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool exchangeNotesSchedule(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (DataTable t1 = dataSrc.FillDataTable("SELECT * FROM NOTES_SCHEDULE WHERE NOTE_ID = '" + id + "'"))
                {
                    using (DataTable t2 = dataDest.FillDataTable("SELECT * FROM NOTES_SCHEDULE WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            long d1 = Convert.ToInt64(t1.Rows[0]["UPD_DATE"]);
                            long d2 = Convert.ToInt64(t2.Rows[0]["UPD_DATE"]);
                            if (d1 > d2)
                            {
                                return insertToNotesSchedule(dataDest, t1.Rows[0], tableData);
                            }
                            if (d2 > d1)
                            {
                                return insertToNotesSchedule(dataSrc, t2.Rows[0], tableData);
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            return insertToNotesSchedule(dataDest, t1.Rows[0], tableData);
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            return insertToNotesSchedule(dataSrc, t2.Rows[0], tableData);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool exchangeLinkedNotes(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (DataTable t1 = dataSrc.FillDataTable("SELECT * FROM LINKED_NOTES WHERE NOTE_ID = '" + id + "'"))
                {
                    using (DataTable t2 = dataDest.FillDataTable("SELECT * FROM LINKED_NOTES WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            DataTable t = t1.Clone();

                            foreach (DataRow r1 in t1.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["LINK_ID"] != (string)r1["LINK_ID"]))
                                {
                                    t.Rows.Add(r1.ItemArray);
                                }
                            }
                            foreach (DataRow r2 in t2.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["LINK_ID"] != (string)r2["LINK_ID"]))
                                {
                                    t.Rows.Add(r2.ItemArray);
                                }
                            }
                            foreach (DataRow r in t.Rows)
                            {
                                if (!insertToLinkedNotes(dataDest, r, tableData))
                                {
                                    return false;
                                }
                                if (!insertToLinkedNotes(dataSrc, r, tableData))
                                {
                                    return false;
                                }
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            if (t1.Rows.Cast<DataRow>().Any(r => !insertToLinkedNotes(dataDest, r, tableData)))
                            {
                                return false;
                            }
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            if (t2.Rows.Cast<DataRow>().Any(r => !insertToLinkedNotes(dataSrc, r, tableData)))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool exchangeNotesTags(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (DataTable t1 = dataSrc.FillDataTable("SELECT * FROM NOTES_TAGS WHERE NOTE_ID = '" + id + "'"))
                {
                    using (DataTable t2 = dataDest.FillDataTable("SELECT * FROM NOTES_TAGS WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            DataTable t = t1.Clone();

                            foreach (DataRow r1 in t1.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["TAG"] != (string)r1["TAG"]))
                                {
                                    t.Rows.Add(r1.ItemArray);
                                }
                            }
                            foreach (DataRow r2 in t2.Rows)
                            {
                                if (t.AsEnumerable().All(r => (string)r["TAG"] != (string)r2["TAG"]))
                                {
                                    t.Rows.Add(r2.ItemArray);
                                }
                            }
                            foreach (DataRow r in t.Rows)
                            {
                                if (!insertToNotesTags(dataDest, r, tableData))
                                {
                                    return false;
                                }
                                if (!insertToNotesTags(dataSrc, r, tableData))
                                {
                                    return false;
                                }
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            if (t1.Rows.Cast<DataRow>().Any(r => !insertToNotesTags(dataDest, r, tableData)))
                            {
                                return false;
                            }
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            if (t2.Rows.Cast<DataRow>().Any(r => !insertToNotesTags(dataSrc, r, tableData)))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool exchangeNotes(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (DataTable t1 = dataSrc.FillDataTable("SELECT * FROM NOTES WHERE ID = '" + id + "'"))
                {
                    using (DataTable t2 = dataDest.FillDataTable("SELECT * FROM NOTES WHERE ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            long d1 = Convert.ToInt64(t1.Rows[0]["UPD_DATE"]);
                            long d2 = Convert.ToInt64(t2.Rows[0]["UPD_DATE"]);
                            if (d1 > d2)
                            {
                                return insertToNotes(dataDest, t1.Rows[0], tableData);
                            }
                            if (d2 > d1)
                            {
                                return insertToNotes(dataSrc, t2.Rows[0], tableData);
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            return insertToNotes(dataDest, t1.Rows[0], tableData);
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            return insertToNotes(dataSrc, t2.Rows[0], tableData);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool exchangeCustomNotesSettings(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                using (DataTable t1 = dataSrc.FillDataTable("SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + id + "'"))
                {
                    using (DataTable t2 = dataDest.FillDataTable("SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + id + "'"))
                    {
                        if (t1.Rows.Count > 0 && t2.Rows.Count > 0)
                        {
                            long d1 = Convert.ToInt64(t1.Rows[0]["UPD_DATE"]);
                            long d2 = Convert.ToInt64(t2.Rows[0]["UPD_DATE"]);
                            if (d1 > d2)
                            {
                                return insertToCustomNotesSettings(dataDest, t1.Rows[0], tableData);
                            }
                            if (d2 > d1)
                            {
                                return insertToCustomNotesSettings(dataSrc, t2.Rows[0], tableData);
                            }
                        }
                        else if (t1.Rows.Count > 0)
                        {
                            return insertToCustomNotesSettings(dataDest, t1.Rows[0], tableData);
                        }
                        else if (t2.Rows.Count > 0)
                        {
                            return insertToCustomNotesSettings(dataSrc, t2.Rows[0], tableData);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }
        #endregion

        #region Insert procedures
        private static bool insertToNotesTags(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM NOTES_TAGS WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("' AND TAG = '");
                sb.Append(r["TAG"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool insertToNotes(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM NOTES WHERE ID = '");
                sb.Append(r["ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool insertToCustomNotesSettings(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool insertToLinkedNotes(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM LINKED_NOTES WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("' AND LINK_ID = '");
                sb.Append(r["LINK_ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        internal static bool InsertToAllTables(SQLiteDataObject dataSrc, SQLiteDataObject dataDest, string id, Dictionary<string, List<_FieldData>> tablesData)
        {
            try
            {
                using (DataTable t = dataSrc.FillDataTable("SELECT * FROM NOTES WHERE ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToNotes(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "NOTES")))
                        {
                            return false;
                        }
                    }
                }
                using (DataTable t = dataSrc.FillDataTable("SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToCustomNotesSettings(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "CUSTOM_NOTES_SETTINGS")))
                        {
                            return false;
                        }
                    }
                }
                using (DataTable t = dataSrc.FillDataTable("SELECT * FROM LINKED_NOTES WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToLinkedNotes(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "LINKED_NOTES")))
                        {
                            return false;
                        }
                    }
                }
                using (DataTable t = dataSrc.FillDataTable("SELECT * FROM NOTES_SCHEDULE WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToNotesSchedule(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "NOTES_SCHEDULE")))
                        {
                            return false;
                        }
                    }
                }
                using (DataTable t = dataSrc.FillDataTable("SELECT * FROM NOTES_TAGS WHERE NOTE_ID = '" + id + "'"))
                {
                    if (t.Rows.Count > 0)
                    {
                        if (!insertToNotesTags(dataDest, t.Rows[0], tablesData.FirstOrDefault(td => td.Key == "NOTES_TAGS")))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }

        private static bool insertToNotesSchedule(SQLiteDataObject dataDest, DataRow r, KeyValuePair<string, List<_FieldData>> tableData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("DELETE FROM NOTES_SCHEDULE WHERE NOTE_ID = '");
                sb.Append(r["NOTE_ID"]);
                sb.Append("'; ");

                sb.Append(createInsert(r, tableData));

                return executeTransaction(dataDest, sb.ToString());
            }
            catch (Exception ex)
            {
                logException(ex);
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Returns current directory of plugin DLL
        /// </summary>
        private static string assemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Returns current product name of plugin DLL
        /// </summary>
        private static string assemblyName
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return customAttributes.Length > 0 ? ((AssemblyProductAttribute)customAttributes[0]).Product : "plugin";
            }
        }

        internal static void LogThis(string message, bool showMessage = false)
        {
            try
            {
                using (var w = new StreamWriter(Path.Combine(assemblyDirectory, assemblyName + ".log"), true))
                {
                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    sb.AppendLine();
                    sb.Append(message);
                    sb.AppendLine();
                    sb.Append("***************************");
                    w.WriteLine(sb.ToString());
                    if (showMessage)
                        MessageBox.Show(message);
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        /// <summary>
        /// Logs exception
        /// </summary>
        /// <param name="ex">Exception to log</param>
        private static void logException(Exception ex)
        {
            try
            {
                var type = ex.GetType();
                using (var w = new StreamWriter(Path.Combine(assemblyDirectory, assemblyName + ".log"), true))
                {
                    var stack = new StackTrace(ex, true);
                    var frame = stack.GetFrame(stack.FrameCount - 1);

                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    sb.AppendLine();
                    sb.Append("Type: ");
                    sb.Append(type);
                    sb.AppendLine();
                    sb.Append("Message: ");
                    sb.Append(ex.Message);
                    sb.AppendLine();
                    sb.Append("In: ");
                    sb.Append(frame.GetFileName());
                    sb.Append("; at: ");
                    sb.Append(frame.GetMethod().Name);
                    var line = frame.GetFileLineNumber();
                    var column = frame.GetFileColumnNumber();
                    if (line != 0 || column != 0)
                    {
                        sb.Append("; line: ");
                        sb.Append(line);
                        sb.Append("; column: ");
                        sb.Append(column);
                    }
                    else
                    {
                        sb.Append("; line: undefined; column: undefined");
                    }
                    if (type == typeof(SQLiteDataException))
                    {
                        var dataException = ex as SQLiteDataException;
                        if (dataException != null)
                        {
                            sb.AppendLine();
                            sb.Append("SQL sentence: ");
                            sb.Append(dataException.SQLiteMessage);
                        }
                    }
                    sb.AppendLine();
                    sb.Append("***************************");
                    w.WriteLine(sb.ToString());
                }
                MessageBox.Show(type.ToString() + '\n' + ex.Message);
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}
