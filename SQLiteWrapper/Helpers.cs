using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace SQLiteWrapper
{
    /// <summary>
    /// Defines custom DBField attribute. Only properties with this attribute will be used in building INSERT/UPDATE commands. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DBField : Attribute
    {
        /// <summary>
        /// Gets or sets DB field name of current property
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// Gets or sets <see cref="DbType"/> of current property
        /// </summary>
        public DbType FieldType { get; set; }
        /// <summary>
        /// Specifies whether default DB value should be used instead of actual property value
        /// </summary>
        public bool UseDefault { get; set; }
    }

    /// <summary>
    /// Represents base abstract class for creating INSERT/UPDATE commands
    /// </summary>
    public abstract class CommandBuilder
    {
        /// <summary>
        /// Creates INSERT command.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="specialCases">Optional list of [column_name]/[column_value] pairs to use instead of actual properties' values (e.g. {UPDATE_TIMESTAMP, GETDATE()} will use GETDATE() function instead of actual value of UpdateTimestamp property)</param>
        /// <returns>INSERT <see cref="SQLiteCommand"/></returns>
        public SQLiteCommand BuildInsertCommand(string tableName, Dictionary<string, string> specialCases = null)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name null or empty", "tableName");
            var sb = new StringBuilder(string.Format("INSERT INTO {0} (", tableName));
            var fields = new List<string>();
            var values = new List<string>();
            var command = new SQLiteCommand();
            var props = GetType().GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(DBField)));

            foreach (var p in props)
            {
                var att = ((DBField[])p.GetCustomAttributes(typeof(DBField), false)).FirstOrDefault();
                if (att == null || (att.UseDefault && p.GetValue(this) == null))
                    continue;
                fields.Add(att.FieldName);
                if (specialCases != null && specialCases.Keys.Any(k => k == p.Name))
                {
                    values.Add(specialCases[p.Name]);
                }
                else
                {
                    values.Add("@" + att.FieldName);
                    var value = p.GetValue(this);
                    command.Parameters.Add(
                        new SQLiteParameter("@" + att.FieldName, att.FieldType)
                        {
                            Value = value == null ? DBNull.Value : value is bool ? Convert.ToInt32(value) : value
                        });
                }
            }

            sb.Append(string.Join(", ", fields));
            sb.Append(") VALUES(");
            sb.Append(string.Join(", ", values));
            sb.Append(")");
            command.CommandText = sb.ToString();

            return command;
        }

        /// <summary>
        /// Creates INSERT command.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="tableColumns">List of properties to use as column list, all other properties will be ignored.</param>
        /// <param name="specialCases">Optional list of [column_name]/[column_value] pairs to use instead of actual properties' values (e.g. {UPDATE_TIMESTAMP, GETDATE()} will use GETDATE() function instead of actual value of UpdateTimestamp property)</param>
        /// <returns>INSERT <see cref="SQLiteCommand"/> for specified list of columns</returns>
        public SQLiteCommand BuildInsertCommand(string tableName, IEnumerable<string> tableColumns,
            Dictionary<string, string> specialCases = null)
        {
            if(string.IsNullOrEmpty( tableName))
                throw new ArgumentException("Table name null or empty", "tableName");
            var sb = new StringBuilder(string.Format("INSERT INTO {0} (", tableName));
            var fields = new List<string>();
            var values = new List<string>();
            var command = new SQLiteCommand();
            var enumerable = tableColumns as string[] ?? tableColumns.ToArray();
            if (tableColumns == null || !enumerable.Any())
                throw new ArgumentException("Columns list cannot be null or empty", "tableColumns");
            var props = GetType().GetProperties().Where(p => enumerable.Any(n => n == p.Name)).ToArray();
            if (props.Any(p => p.CustomAttributes.All(a => a.AttributeType != typeof(DBField))))
                throw new ArgumentException("'DBField' attribute is not assigned to one or more fields");

            foreach (var p in props)
            {
                var att = ((DBField[])p.GetCustomAttributes(typeof(DBField), false)).FirstOrDefault();
                if (att == null || (att.UseDefault && p.GetValue(this) == null))
                    continue;
                fields.Add(att.FieldName);
                if (specialCases != null && specialCases.Keys.Any(k => k == p.Name))
                {
                    values.Add(specialCases[p.Name]);
                }
                else
                {
                    values.Add("@" + att.FieldName);
                    var value = p.GetValue(this);
                    command.Parameters.Add(
                        new SQLiteParameter("@" + att.FieldName, att.FieldType)
                        {
                            Value = value == null ? DBNull.Value : value is bool ? Convert.ToInt32(value) : value
                        });
                }
            }

            sb.Append(string.Join(", ", fields));
            sb.Append(") VALUES(");
            sb.Append(string.Join(", ", values));
            sb.Append(")");
            command.CommandText = sb.ToString();

            return command;
        }

        /// <summary>
        /// Creates UPDATE command.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="tableColumns">Optional list of properties to use as column list, all other properties will be ignored.</param>
        /// <param name="specialCases">Optional list of [column_name]/[column_value] pairs to use instead of actual properties' values (e.g. {UPDATE_TIMESTAMP, GETDATE()} will use GETDATE() function instead of actual value of UpdateTimestamp property)</param>
        /// <param name="whereClause">Optional WHERE clause</param>
        /// <returns>UPDATE <see cref="SQLiteCommand"/></returns>
        public SQLiteCommand BuildUpdateColumn(string tableName, IEnumerable<string> tableColumns = null,
            Dictionary<string, string> specialCases = null, string whereClause = "")
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name null or empty", "tableName");
            var sb = new StringBuilder(string.Format("UPDATE {0} SET ", tableName));
            var command = new SQLiteCommand();
            var enumerable = tableColumns == null ? new string[] { } : tableColumns.ToArray();
            if (tableColumns != null && !enumerable.Any())
                throw new ArgumentException("Columns list cannot be empty", "tableColumns");
            var props = !enumerable.Any()
                ? GetType().GetProperties().Where(p => p.CustomAttributes.Any(a=>a.AttributeType==typeof(DBField))).ToArray()
                : GetType().GetProperties().Where(p => enumerable.Any(n => n == p.Name)).ToArray();
            if (!props.All(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(DBField))))
                throw new ArgumentException("'DBField' attribute is not assigned to one or more fields");

            foreach (var p in props)
            {
                var att = ((DBField[])p.GetCustomAttributes(typeof(DBField), false)).FirstOrDefault();
                if (att == null || (att.UseDefault && p.GetValue(this) == null))
                    continue;
                sb.Append(att.FieldName);
                sb.Append(" = ");
                if (specialCases != null && specialCases.Keys.Any(k => k == p.Name))
                {
                    sb.Append(specialCases[p.Name]);
                }
                else
                {
                    sb.Append("@" + att.FieldName);
                    var value = p.GetValue(this);
                    command.Parameters.Add(
                        new SQLiteParameter("@" + att.FieldName, att.FieldType)
                        {
                            Value = value == null ? DBNull.Value : value is bool ? Convert.ToInt32(value) : value
                        });
                }

                sb.Append(", ");
            }
            if (sb.Length <= 2) return null;
            sb.Length -= 2;
            if (!string.IsNullOrEmpty(whereClause))
            {
                sb.Append(" ");
                sb.Append(whereClause);
            }
            command.CommandText = sb.ToString();

            return command;
        }
    }
}
