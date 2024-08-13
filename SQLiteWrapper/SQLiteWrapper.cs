using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace SQLiteWrapper
{
    /// <summary>
    /// Represents error that occurs during SQLite operations
    /// </summary>
    public class SQLiteDataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of SQLiteDataException
        /// </summary>
        /// <param name="baseException">Base exception</param>
        /// <param name="sqliteMessage">Custom error message</param>
        public SQLiteDataException(Exception baseException, string sqliteMessage)
            : base(baseException.Message, baseException)
        {
            SQLiteMessage = sqliteMessage;
        }

        /// <summary>
        /// Gets or sets custom error message
        /// </summary>
        public string SQLiteMessage { get; private set; }
    }

    /// <summary>
    /// Represents SQLite parameter data
    /// </summary>
    public class SQLiteParameterData
    {
        /// <summary>
        /// Describes the version of System.Data.DataRow
        /// </summary>
        public DataRowVersion ParameterRowVersion { get; set; }

        /// <summary>
        /// Gets or set parameter value
        /// </summary>
        public object ParameterValue { get; set; }

        /// <summary>
        /// Gets or set parameter size
        /// </summary>
        public int ParameterSize { get; set; }

        /// <summary>
        /// Gets or set parameter type
        /// </summary>
        public DbType ParameterType { get; set; }

        /// <summary>
        /// Gets or set parameter source column
        /// </summary>
        public string ParameterSourceColumn { get; set; }

        /// <summary>
        /// Gets or set parameter name
        /// </summary>
        public string ParameterName { get; set; }
    }

    /// <summary>
    /// Represents wrapper object for various SQLite operations
    /// </summary>
    public class SQLiteDataObject : IDisposable
    {
        private SQLiteConnection _Connection;
        private SQLiteConnection _TransConnection;
        private SQLiteTransaction _Transaction;

        /// <summary>
        /// Initializes a new instance of SQLiteDataObject object
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public SQLiteDataObject(string connectionString)
        {
            _Connection = new SQLiteConnection(connectionString);
        }

        /// <summary>
        /// Returns connection string of current connection
        /// </summary>
        public string ConnectionString => _Connection != null ? _Connection.ConnectionString : "";

        /// <summary>
        /// Static method which checks whether database file exists and creates it if necessary
        /// </summary>
        /// <param name="dbPath">Database file name</param>
        /// <returns>Connection string for created/existing database</returns>
        public static string CheckAndCreateDatabase(string dbPath)
        {
            try
            {
                if (!File.Exists(dbPath))
                {
                    SQLiteConnection.CreateFile(dbPath);
                }
                var csb = new SQLiteConnectionStringBuilder {DataSource = dbPath};
                return csb.ToString();
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, "Error on database checking/creation");
            }
        }

        /// <summary>
        /// Returns schema for specified collection
        /// </summary>
        /// <param name="collectionName">Collection name</param>
        /// <returns>Schema for specified collection</returns>
        public DataTable GetSchema(string collectionName)
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    _Connection.Open();
                }
                return _Connection.GetSchema(collectionName);
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, collectionName);
            }
            finally
            {
                if (_Connection.State != ConnectionState.Closed)
                {
                    _Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes SQL query and returns scalar value
        /// </summary>
        /// <param name="sqlQuery">SQL query</param>
        /// <returns>Scalar value</returns>
        public object GetScalar(string sqlQuery)
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    _Connection.Open();
                }
                using (var command = _Connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, sqlQuery);
            }
            finally
            {
                if (_Connection.State != ConnectionState.Closed)
                {
                    _Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes SQL query in transaction and returns scalar value
        /// </summary>
        /// <param name="sqlQuery">SQL query</param>
        /// <returns>Scalar value</returns>
        public object GetScalarInTransaction(string sqlQuery)
        {
            try
            {
                using (var command = _TransConnection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    command.Transaction = _Transaction;
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, sqlQuery);
            }
        }

        /// <summary>
        /// Fills DataTable object by executing SQL query and returns it
        /// </summary>
        /// <param name="sqlQuery">SQL query to execute</param>
        /// <returns>DataTable object</returns>
        public DataTable FillDataTable(string sqlQuery)
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    _Connection.Open();
                }
                using (var da = new SQLiteDataAdapter(sqlQuery, _Connection))
                {
                    var t = new DataTable();
                    da.Fill(t);
                    return t;
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, sqlQuery);
            }
            finally
            {
                if (_Connection.State != ConnectionState.Closed)
                {
                    _Connection.Close();
                }
            }
        }

        /// <summary>
        /// Reclaims empty space and reduces the size of the database file
        /// </summary>
        public void CompactDatabase()
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    _Connection.Open();
                }
                using (var command = _Connection.CreateCommand())
                {
                    command.CommandText = "VACUUM";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, "VACUUM");
            }
            finally
            {
                if (_Connection.State != ConnectionState.Closed)
                {
                    _Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes SQL query
        /// </summary>
        /// <param name="sqlQuery">SQL query to execute</param>
        /// <returns>Number of rows affected</returns>
        public int Execute(string sqlQuery)
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    _Connection.Open();
                }
                using (var command = _Connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, sqlQuery);
            }
            finally
            {
                if (_Connection.State != ConnectionState.Closed)
                {
                    _Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes command
        /// </summary>
        /// <param name="command">SQLiteCommand command to execute</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteCommand(SQLiteCommand command)
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    _Connection.Open();
                }
                command.Connection = _Connection;
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, command.CommandText);
            }
            finally
            {
                if (_Connection.State != ConnectionState.Closed)
                {
                    _Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes command
        /// </summary>
        /// <param name="sqlQuery">SQL query to execute</param>
        /// <param name="parameters">List of command's parameters data</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteCommand(string sqlQuery, IEnumerable<SQLiteParameterData> parameters)
        {
            try
            {
                var sqLiteParameterDatas = parameters as SQLiteParameterData[] ?? parameters.ToArray();
                var pars = sqLiteParameterDatas.Select(p => p.ParameterName);
                var values = pars as string[] ?? pars.ToArray();
                if (values.Any(p => !p.StartsWith("@")))
                {
                    throw new ArgumentException("There are one or more parameters without '@' symbol, parameters: " +
                                                string.Join(",", values));
                }
                foreach (var v in values.Where(v => !sqlQuery.Contains(v)))
                {
                    throw new ArgumentException("There is no parameter with name '" + v + "' in query string");
                }


                if (_Connection.State == ConnectionState.Closed)
                {
                    _Connection.Open();
                }
                using (var command = _Connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    foreach (
                        var prm in
                            sqLiteParameterDatas.Select(
                                p =>
                                    new SQLiteParameter(p.ParameterName, p.ParameterType, p.ParameterSize,
                                        p.ParameterSourceColumn, p.ParameterRowVersion) {Value = p.ParameterValue}))
                    {
                        command.Parameters.Add(prm);
                    }
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, sqlQuery);
            }
            finally
            {
                if (_Connection.State != ConnectionState.Closed)
                {
                    _Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes command in transaction
        /// </summary>
        /// <param name="sqlQuery">SQL query to execute</param>
        /// <param name="parameters">List of command's parameters data</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteCommandInTransaction(string sqlQuery, IEnumerable<SQLiteParameterData> parameters)
        {
            try
            {
                var sqLiteParameterDatas = parameters as SQLiteParameterData[] ?? parameters.ToArray();
                var pars = sqLiteParameterDatas.Select(p => p.ParameterName);
                var values = pars as string[] ?? pars.ToArray();
                if (values.Any(p => !p.StartsWith("@")))
                {
                    throw new ArgumentException("There are one or more parameters without '@' symbol, parameters: " +
                                                string.Join(",", values));
                }
                foreach (var v in values.Where(v => !sqlQuery.Contains(v)))
                {
                    throw new ArgumentException("There is no parameter with name '" + v + "' in query string");
                }

                using (var command = _TransConnection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    foreach (var p in sqLiteParameterDatas)
                    {
                        var prm = new SQLiteParameter(p.ParameterName, p.ParameterType, p.ParameterSize,
                            p.ParameterSourceColumn, p.ParameterRowVersion) {Value = p.ParameterValue};
                        command.Parameters.Add(prm);
                    }
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, sqlQuery);
            }
        }

        /// <summary>
        /// Executes SQL query in transaction
        /// </summary>
        /// <param name="sqlQuery">SQL query to execute</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteInTransaction(string sqlQuery)
        {
            try
            {
                using (var command = _TransConnection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    command.Transaction = _Transaction;
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, sqlQuery);
            }
        }

        /// <summary>
        /// Begins transaction
        /// </summary>
        /// <returns>True if transaction started successfully, false otherwise</returns>
        public bool BeginTransaction()
        {
            try
            {
                if (string.IsNullOrEmpty(_Connection.ConnectionString))
                    return false;
                if (_TransConnection == null)
                    _TransConnection = new SQLiteConnection(_Connection.ConnectionString);
                if (_TransConnection.State != ConnectionState.Open)
                    _TransConnection.Open();
                _Transaction = _TransConnection.BeginTransaction();
                return true;
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, "Error on begin transaction");
            }
        }

        /// <summary>
        /// Commits transaction
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                _Transaction.Commit();
                _Transaction.Dispose();
                _Transaction = null;
                _TransConnection.Close();
                _TransConnection.Dispose();
                _TransConnection = null;
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, "Error on commit transaction");
            }
        }

        /// <summary>
        /// Rolls transaction back
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                _Transaction.Rollback();
                _Transaction.Dispose();
                _Transaction = null;
                _TransConnection.Close();
                _TransConnection.Dispose();
                _TransConnection = null;
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, "Error on rollback transaction");
            }
        }

        /// <summary>
        /// Checks whether table with specified name exists in database
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>True if table with specified name exists in database, false otherwise</returns>
        public bool TableExists(string tableName)
        {
            try
            {
                var sqlQuery = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='" + tableName + "'";
                var result = (long) GetScalar(sqlQuery);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new SQLiteDataException(ex, "Error on checking table existence");
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by SQLiteDataObject
        /// </summary>
        public void Dispose()
        {
            if (_Transaction != null)
            {
                _Transaction.Dispose();
                _Transaction = null;
            }
            if (_Connection != null)
            {
                _Connection.Dispose();
                _Connection = null;
            }
            if (_TransConnection != null)
            {
                _TransConnection.Dispose();
                _TransConnection = null;
            }
        }

        #endregion
    }
}