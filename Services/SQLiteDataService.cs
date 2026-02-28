using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using RecipePlanner.Models;
using System.Collections.ObjectModel;

namespace RecipePlanner.Services
{
    internal class SQLiteDataService
    {
        private readonly string _connectionString;

        public SQLiteDataService()
        {
            _connectionString = "Data Source=recipes.db";
            InitializeDatabase();
        }
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

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

                CREATE TABLE IF NOT EXISTS PantryItems (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                );

                CREATE TABLE IF NOT EXISTS PlannedMeals (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    RecipeID INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    FOREIGN KEY (RecipeID) REFERENCES Recipes(ID) ON DELETE CASCADE
                );
            ";

            command.ExecuteNonQuery();
        }
    }
}
