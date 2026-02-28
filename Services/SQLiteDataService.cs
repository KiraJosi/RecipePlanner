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

        public void AddRecipe(Recipe recipe)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                "INSERT INTO Recipes (Name) VALUES ($name); SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$name", recipe.Name);

            long newID = (long)command.ExecuteScalar();
            recipe.Id = (int)newID;

            foreach (var ingredient in recipe.Ingredients)
            {
                var ingredientCommand = connection.CreateCommand();
                ingredientCommand.Transaction = transaction;

                ingredientCommand.CommandText =
                    "INSERT INTO Ingredients (RecipeID, Name) VALUES ($rid, $name);";

                ingredientCommand.Parameters.AddWithValue("$rid", recipe.Id);
                ingredientCommand.Parameters.AddWithValue("$name", ingredient);

                ingredientCommand.ExecuteNonQuery();
            }
            transaction.Commit();
        }
    }
}
