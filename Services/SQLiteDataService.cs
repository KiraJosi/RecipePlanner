using Microsoft.Data.Sqlite;
using RecipePlanner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RecipePlanner.Services
{
    public class SQLiteDataService
    {
        private readonly string _connectionString;

        public SQLiteDataService()
        {
            _connectionString = "Data Source=recipes.db";
            InitializeDatabase();
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
            ";

            command.ExecuteNonQuery();
        }

        private SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();

            return connection;
        }

        public void AddRecipe(Recipe recipe)
        {
            using var connection = CreateConnection();

            using var transaction = connection.BeginTransaction();

            var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                "INSERT INTO Recipes (Name) VALUES ($name); SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$name", recipe.Name);

            var result = command.ExecuteScalar();

            if (result is long newID)
            {
                recipe.Id = (int)newID;
            }
            else
            {
                throw new InvalidOperationException("Konnte neue Rezept - ID nicht ermitteln.");
            }

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

        public void DeleteRecipe(int id)
        {
            using var connection = CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Recipes WHERE ID = $id";
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }

        public void UpdateRecipe(Recipe recipe)
        {
            using var connection = CreateConnection();
            using var transaction = connection.BeginTransaction();

            var updateCommand = connection.CreateCommand();
            updateCommand.Transaction = transaction;
            updateCommand.CommandText =
                "UPDATE Recipes SET Name = $name WHERE ID = $id";

            updateCommand.Parameters.AddWithValue("$name", recipe.Name);
            updateCommand.Parameters.AddWithValue("$id", recipe.Id);
            updateCommand.ExecuteNonQuery();

            var deleteIngredients = connection.CreateCommand();
            deleteIngredients.Transaction = transaction;
            deleteIngredients.CommandText =
                "DELETE FROM Ingredients WHERE RecipeID = $id";
            deleteIngredients.Parameters.AddWithValue("$id", recipe.Id);
            deleteIngredients.ExecuteNonQuery();

            foreach (var ingredient in recipe.Ingredients)
            {
                var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;

                insertCommand.CommandText =
                    "INSERT INTO Ingredients (RecipeID,Name) VALUES ($rid, $name)";
                insertCommand.Parameters.AddWithValue("$rid", recipe.Id);
                insertCommand.Parameters.AddWithValue("$name", ingredient);

                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public void SavePlannedMeals(List<PlannedMeal> meals)
        {
            using var connection = CreateConnection();

            using var transaction = connection.BeginTransaction();

            var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM PlannedMeals";
            deleteCommand.ExecuteNonQuery();

            foreach (var meal in meals)
            {
                if (meal.Recipe == null)
                    continue;

                var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;

                insertCommand.CommandText = @"
                    INSERT INTO PlannedMeals (RecipeID, Date)
                    VALUES ($rid, $date);
                    ";

                insertCommand.Parameters.AddWithValue("$rid", meal.Recipe.Id);
                insertCommand.Parameters.AddWithValue("$date", meal.Date.ToString("yyyy-MM-dd"));

                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public void SavePantryItems(List<string> items)
        {
            using var connection = CreateConnection();

            using var transaction = connection.BeginTransaction();

            var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM PantryItems";
            deleteCommand.ExecuteNonQuery();

            foreach (var item in items)
            {
                var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = "INSERT INTO PantryItems (Name) VALUES ($name)";
                insertCommand.Parameters.AddWithValue("$name", item);
                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public List<Recipe> GetRecipes()
        {
            var recipes = new List<Recipe>();

            using var connection = CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name FROM Recipes";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var recipe = new Recipe
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                };

                recipe.Ingredients = new ObservableCollection<string>(
                    GetIngredientsForRecipe(recipe.Id)
                    );

                recipes.Add(recipe);

            }
            return recipes;
        }

        public List<string> GetPantryItems()
        {
            var items = new List<string>();

            using var connection = CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Name FROM PantryItems";

            using var reader = command.ExecuteReader();
            while(reader.Read())
            {
                items.Add(reader.GetString(0));
            }

            return items;
        }

        public List<PlannedMeal> GetPlannedMeals()
        {
            var meals = new List<PlannedMeal>();

            using var connection = CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT PM.ID, PM.RecipeID, PM.Date, R.Name
                FROM PlannedMeals PM
                JOIN Recipes R ON PM.RecipeID = R.ID
                ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime date;
                try
                {
                    date = DateTime.ParseExact(reader.GetString(2), "yyyy-MM-dd", null);
                }
                catch (FormatException)
                {
                    date = DateTime.Today;
                }
                var meal = new PlannedMeal
                {
                    ID = reader.GetInt32(0),
                    Recipe = new Recipe
                    {
                        Id = reader.GetInt32(1),
                        Name = reader.GetString(3)
                    },
                    Date = date
                };

                meals.Add(meal);
            }

            return meals;
        }

        private List<string> GetIngredientsForRecipe(int recipeId)
        {
            var ingredients = new List<string>();

            using var connection = CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Ingredients WHERE RecipeId = $id";

            command.Parameters.AddWithValue("$id", recipeId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                ingredients.Add(reader.GetString(0));
            }

            return ingredients;

        }
    }
}
