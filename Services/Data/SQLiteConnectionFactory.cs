using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlanner.Services.Data
{
    public class SQLiteConnectionFactory
    {
        private readonly string _connectionString;

        public SQLiteConnectionFactory()
        {
            _connectionString = "Data source=recipes.db";
            InitializeDatabase();
        }

        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();

            return connection;
        }

        private void InitializeDatabase()
        {
            using var connection = CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                    PRAGMA foreign_keys = ON;
                
                    CREATE TABLE IF NOT EXISTS Recipes (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Ingredients (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        RecipeID INTEGER NOT NULL,
                        Name TEXT NOT NULL,
                        FOREIGN KEY (RecipeID) REFERENCES Recipes(ID) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS Steps (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        RecipeID INTEGER NOT NULL,
                        SortOrder INTEGER NOT NULL,
                        Text TEXT NOT NULL,
                        FOREIGN KEY (RecipeID) REFERENCES Recipes(ID) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS PantryItems (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS PlannedMeals (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        RecipeID INTEGER NOT NULL,
                        Date TEXT NOT NULL,
                        FOREIGN KEY (RecipeID) REFERENCES Recipes(ID) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS Tags (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        RecipeID INTEGER NOT NULL,
                        Name TEXT NOT NULL,
                        FOREIGN KEY (RecipeID) REFERENCES Recipes(ID) ON DELETE CASCADE
                    );
                ";

            command.ExecuteNonQuery();

            try
            {
                var migration = connection.CreateCommand();
                migration.CommandText =
                    "ALTER TABLE Recipes ADD COLUMN Source TEXT NOT NULL DEFAULT '';";
                migration.ExecuteNonQuery();
            }
            catch
            {

            }

            try
            {
                var createTags = connection.CreateCommand();
                createTags.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tags (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        RecipeID INTEGER NOT NULL,
                        Name TEXT NOT NULL,
                        FOREIGN KEY (RecipeID) REFERENCES Recipes(ID) ON DELETE CASCADE
                    );";
                createTags.ExecuteNonQuery();
            }
            catch
            {

            }
        }
    }
}
