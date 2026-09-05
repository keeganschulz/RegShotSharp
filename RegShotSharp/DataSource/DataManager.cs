using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Data;
using RegShotSharp.Tools;
using RegShotSharp.DataTypes;

namespace RegShotSharp.DataSource
{
    public class DataManager
    {
        private SQLiteDataSource _dataSource;
        private SQLiteConnection _connection;

        //variables used for comparing dbs
        private bool _attachedDb = false;
        private const string _attachedDbAlias = "ad";
        private const string _mainDbAlias = "main";

        /// <summary>
        /// Handles the connection to the SQLite3 DB, verifying the path is correct befor connecting or creating the file at connection
        /// </summary>
        /// <param name="a_directoryPath">Path to the directory where DB file will be created</param>
        /// <param name="a_fileName">Path to file of SQLite3 DB</param>
        /// <returns>Path to the SQLite3 DB file</returns>
        /// <exception cref="Exception">Invalid directory, File does not exist, file extension is incorrect, connection error</exception>
        public string InitializeConnection(string a_directoryPath, string? a_fileName = null)
        {
            //check directory exists
            if (!Directory.Exists(a_directoryPath)) {
                throw new Exception($"DataManager::InitializeConnection - {a_directoryPath} is not a valid directory");
            }

            //validate file is correct OR create file name to be created
            if (a_fileName == null)
            {
                a_fileName = $"{MiscTools.GetCurrentTimestamp()}.sqlite3";
            } else
            {
                if (!File.Exists(a_fileName))
                {
                    throw new Exception($"DataManager::InitializeConnection - {a_fileName} does not exist");
                }

                if (!a_fileName.EndsWith(".sqlite3"))
                {
                    throw new Exception($"DataManager::InitializeConnection - {a_fileName} does not have correct extension (.sqlite3)");
                }
            }


            _dataSource = new SQLiteDataSource();
            string path = Path.Combine(a_directoryPath, a_fileName);

            try
            {
                _connection = _dataSource.OpenConnection(path);
            }
            catch (SQLiteException ex)
            {
                throw new Exception("DataManager::InitializeConnection - error connecting to DB" + ex.Message);
            }
            catch (Exception)
            {
                throw;
            }

            return path;
        }

        /// <summary>
        /// Terminate connection to DB
        /// </summary>
        /// <returns>True if successful, otherwise false</returns>
        public bool TerminateConnection()
        {
            bool success = false;
            success = _dataSource.CloseConnection(_connection);
            _dataSource = null;
            return success;
        }

        /// <summary>
        /// Inserts the name of the selected regsitry hives
        /// </summary>
        /// <param name="keys">String array of the hive names</param>
        public void InsertSelectedHives(string[] keys)
        {
            string query = $"INSERT INTO KEYS (KEY) VALUES {String.Join(",", keys.Select(k => $"('{k}')"))}";
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserts list of successful RegistryEntrys to DB
        /// </summary>
        /// <param name="a_entries">List of RegistryEntrys</param>
        public void InsertSuccesses(List<RegistryEntry> a_entries)
        {
            using (SQLiteCommand insertCmd = _connection.CreateCommand())
            {
                insertCmd.CommandText = "INSERT INTO SUCCESSES (KEY, PATH, VALUE, VALUE_TYPE) VALUES (@key, @path, @value, @type)";

                SQLiteParameter keyParam = insertCmd.CreateParameter();
                keyParam.ParameterName = "@key";
                insertCmd.Parameters.Add(keyParam);

                SQLiteParameter pathParam = insertCmd.CreateParameter();
                pathParam.ParameterName = "@path";
                insertCmd.Parameters.Add(pathParam);

                SQLiteParameter valueParam = insertCmd.CreateParameter();
                valueParam.ParameterName = "@value";
                insertCmd.Parameters.Add(valueParam);

                SQLiteParameter typeParam = insertCmd.CreateParameter();
                typeParam.ParameterName = "@type";
                insertCmd.Parameters.Add(typeParam);

                insertCmd.Prepare();

                using(SQLiteTransaction transaction = _connection.BeginTransaction())
                {
                    insertCmd.Transaction = transaction;

                    try
                    {
                        foreach (RegistryEntry rentry in a_entries.Where(ae => ae.SUCCESS))
                        {
                            keyParam.Value = rentry.REGISTRY_KEY;
                            pathParam.Value = rentry.REGISTRY_PATH;
                            valueParam.Value = rentry.REGISTRY_VALUE;
                            typeParam.Value = rentry.REGISTRY_TYPE;

                            insertCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    } catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// [CURRENTLY NOT USED]Inserts list of un-successful RegistryEntrys to DB
        /// </summary>
        /// <param name="a_entries">List of RegistryEntrys</param>
        public void InsertErrors(List<RegistryEntry> a_entries)
        {
            using (SQLiteCommand insertCmd = _connection.CreateCommand())
            {
                insertCmd.CommandText = "INSERT INTO ERRORS (KEY, PATH) VALUES (@key, @path)";

                SQLiteParameter keyParam = insertCmd.CreateParameter();
                keyParam.ParameterName = "@key";
                insertCmd.Parameters.Add(keyParam);

                SQLiteParameter pathParam = insertCmd.CreateParameter();
                pathParam.ParameterName = "@path";
                insertCmd.Parameters.Add(pathParam);

                insertCmd.Prepare();

                using (SQLiteTransaction transaction = _connection.BeginTransaction())
                {
                    insertCmd.Transaction = transaction;

                    try
                    {
                        foreach (RegistryEntry rentry in a_entries.Where(ae => !ae.SUCCESS))
                        {
                            keyParam.Value = rentry.REGISTRY_KEY;
                            pathParam.Value = rentry.REGISTRY_PATH;

                            insertCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    } catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Attach second SQLite3 DB to current connection.
        /// </summary>
        /// <param name="a_databasePath">Path to second DB file</param>
        /// <exception cref="Exception"></exception>
        public void AttachDatabase(string a_databasePath)
        {
            if (!File.Exists(a_databasePath))
            {
                throw new Exception($"DataManager::AttachDatabase - {a_databasePath} does not exist");
            }

            if (!a_databasePath.EndsWith(".sqlite3"))
            {
                throw new Exception($"DataManager::AttachDatabase - {a_databasePath} does not have correct extension (.sqlite3)");
            }

            string attachQuery = $"ATTACH DATABASE '{a_databasePath}' as {_attachedDbAlias}";
            using (SQLiteCommand attachCmd = _connection.CreateCommand())
            {
                try
                {
                    attachCmd.CommandText = attachQuery;
                    attachCmd.ExecuteNonQuery();
                    _attachedDb = true;
                } catch (Exception ex)
                {
                    throw new Exception($"DataManager::AttachDatabase - Could not attach DB at {a_databasePath}.  " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Detach second SQLite3 DB from current connection
        /// </summary>
        public void DetachDatabse()
        {
            if (_attachedDb)
            {
                using(SQLiteCommand detachCmd = _connection.CreateCommand())
                {
                    detachCmd.CommandText = $"DETACH DATABASE {_attachedDbAlias}";
                    detachCmd.ExecuteNonQuery();
                }
                _attachedDb = false;
            }
        }

        /// <summary>
        /// Find all entries that were either removed or added since the first snapshot (the connected DB)
        /// </summary>
        /// <returns>List of all the AlteredEntrys</returns>
        public List<AlteredEntry> FindAlteredEntries()
        {
            List<AlteredEntry> alteredEntries = new List<AlteredEntry>();

            string query = $"""
                SELECT 'REMOVED' AS EVENT, * FROM (
                	SELECT KEY, PATH FROM {_attachedDbAlias}.SUCCESSES EXCEPT SELECT KEY, PATH FROM {_mainDbAlias}.SUCCESSES
                )
                UNION ALL
                SELECT 'ADDED' AS EVENT, * FROM (
                	SELECT KEY, PATH FROM {_mainDbAlias}.SUCCESSES EXCEPT SELECT KEY, PATH FROM {_attachedDbAlias}.SUCCESSES
                );
                """;

            using(SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = query;
                using(SQLiteDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        alteredEntries.Add(new AlteredEntry(reader.GetString(0) == "ADDED", reader.GetString(1), reader.GetString(2)));
                    }
                }
            }

            return alteredEntries;
        }

        /// <summary>
        /// Find all entries that have values that were changed since the first snapshot (the connected DB)
        /// </summary>
        /// <returns></returns>
        public List<EditedEntry> FinedEditedEntries()
        {
            List<EditedEntry> editedEntries = new List<EditedEntry>();

            string query = $"""
                SELECT
                	'EDITED' AS EVENT,
                	s2.VALUE as old_value, s1.VALUE as new_value,
                	s2.VALUE_TYPE as old_value_type, s1.VALUE_TYPE as new_value_type,
                    s2.KEY, s2.PATH
                FROM {_attachedDbAlias}.SUCCESSES s2
                INNER JOIN {_mainDbAlias}.SUCCESSES s1 ON s1.KEY = s2.KEY AND s1.PATH = s2.PATH
                WHERE s1.VALUE IS NOT s2.VALUE OR s1.VALUE_TYPE IS NOT s2.VALUE_TYPE;
                """;

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = query;
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string pre_key = reader.GetString(5);
                        string pre_path = reader.GetString(6);
                        string fullPath = pre_key;
                        if (!string.IsNullOrEmpty(pre_path))
                        {
                            fullPath = $"{pre_key}\\{pre_path}";
                        }
                        editedEntries.Add(new EditedEntry(fullPath, reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4)));
                    }
                }
            }

            return editedEntries;
        }
    }
}
