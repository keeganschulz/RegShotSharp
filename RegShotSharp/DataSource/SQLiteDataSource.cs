using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace RegShotSharp.DataSource
{
    public class SQLiteDataSource
    {
        public SQLiteConnection OpenConnection(string a_dbPath, bool a_skipSchemaCreate = false)
        {
            SQLiteConnectionStringBuilder connectionBuilder = new SQLiteConnectionStringBuilder();
            connectionBuilder.DataSource = a_dbPath;
            SQLiteConnection connection = null;

            connection = new SQLiteConnection(connectionBuilder.ConnectionString);
            connection.Open();
            SQLitePCL.Batteries.Init();

            if(!a_skipSchemaCreate) CreateSchema(connection); //TODO: check if already exist
            
            return connection;
        }

        public bool CloseConnection(SQLiteConnection a_connection)
        {
            try
            {
                a_connection.Close();
            }
            catch { }
            return true;
        }

        private string[] createTableCommands = {
            "CREATE TABLE \"SUCCESSES\" (\"ID\"	INTEGER NOT NULL UNIQUE, \"KEY\" TEXT NOT NULL, \"PATH\" TEXT NOT NULL, \"VALUE_TYPE\"	TEXT NOT NULL, \"VALUE\" TEXT, PRIMARY KEY(\"ID\" AUTOINCREMENT));",
            "CREATE TABLE \"ERRORS\" (\"ID\" INTEGER NOT NULL UNIQUE, \"KEY\" TEXT NOT NULL, \"PATH\" TEXT NOT NULL, PRIMARY KEY(\"ID\" AUTOINCREMENT));",
            "CREATE TABLE \"KEYS\" (\"KEY\"	TEXT NOT NULL UNIQUE, PRIMARY KEY(\"KEY\"));"
        };

        private string[] createIndeciesCommands = {
            "CREATE INDEX \"SUCCESSES_KEY_PATH\" ON \"SUCCESSES\" (\"KEY\", \"PATH\");",
            "CREATE INDEX \"ERRORS_KEY_PATH\" ON \"ERRORS\" (\"KEY\", \"PATH\");"
        };

        private void CreateSchema(SQLiteConnection connection)
        {
            foreach (string create_table_command in createTableCommands)
            {
                SQLiteCommand createTableCommand = new SQLiteCommand(create_table_command, connection);
                createTableCommand.ExecuteNonQuery();
            }

            foreach (string create_index_command in createIndeciesCommands)
            {
                SQLiteCommand createIndexCommand = new SQLiteCommand(create_index_command, connection);
                createIndexCommand.ExecuteNonQuery();
            }
        }
    }
}
