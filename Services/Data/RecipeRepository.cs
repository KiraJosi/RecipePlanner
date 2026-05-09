using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using RecipePlanner.Models;
using System.Collections.ObjectModel;

namespace RecipePlanner.Services.Data
{
    public class RecipeRepository
    {
        private readonly SQLiteConnectionFactory _factory;

        public RecipeRepository(SQLiteConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<Recipe> GetAll()
        {
            var recipes = new List<Recipe>();
            using var connection = _factory.CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Source, Servings FROM Recipes";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var recipe = new Recipe()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Source = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Servings = reader.IsDBNull(3) ? 4 : reader.GetInt32(3),
                };

                recipe.Ingredients = new ObservableCollection<string>(
                    GetIngredientsForRecipe(recipe.Id));
                recipe.Steps = new ObservableCollection<string>(
                    GetStepsForRecipe(recipe.Id));
                recipe.Tags = new ObservableCollection<string>(
                    GetTagsForRecipe(recipe.Id));

                recipes.Add(recipe);
            }
            return recipes;
        }

        public void Add(Recipe recipe)
        {
            using var connection = _factory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO Recipes (Name, Source, Servings) VALUES ($name, $source, $servings); SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$name", recipe.Name);
            command.Parameters.AddWithValue("$source", recipe.Source ?? "");
            command.Parameters.AddWithValue("$servings", recipe.Servings);

            var result = command.ExecuteScalar();
            if (result is long newId)
                recipe.Id = (int)newId;
            else
                throw new InvalidOperationException("Konnte neue Rezept-ID nicht ermitteln.");

            InsertIngredients(connection, transaction, recipe);
            InsertSteps(connection, transaction, recipe);
            InsertTags(connection, transaction, recipe);

            transaction.Commit();
        }

        public void Update(Recipe recipe)
        {
            using var connection = _factory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            var update = connection.CreateCommand();
            update.Transaction = transaction;
            update.CommandText =
                "UPDATE Recipes SET Name = $name, Source = $source, Servings = $servings WHERE ID = $id";
            update.Parameters.AddWithValue("$name", recipe.Name);
            update.Parameters.AddWithValue("$source", recipe.Source ?? "");
            update.Parameters.AddWithValue("$servings", recipe.Servings);
            update.Parameters.AddWithValue("$id", recipe.Id);
            update.ExecuteNonQuery();

            var delIngredients = connection.CreateCommand();
            delIngredients.Transaction = transaction;
            delIngredients.CommandText = "DELETE FROM Ingredients WHERE RecipeID = $id";
            delIngredients.Parameters.AddWithValue("$id", recipe.Id);
            delIngredients.ExecuteNonQuery();

            var delSteps = connection.CreateCommand();
            delSteps.Transaction = transaction;
            delSteps.CommandText = "DELETE FROM Steps WHERE RecipeID = $id";
            delSteps.Parameters.AddWithValue("$id", recipe.Id);
            delSteps.ExecuteNonQuery();

            var delTags = connection.CreateCommand();
            delTags.Transaction = transaction;
            delTags.CommandText = "DELETE FROM Tags WHERE RecipeID = $id";
            delTags.Parameters.AddWithValue("$id", recipe.Id);
            delTags.ExecuteNonQuery();

            InsertIngredients(connection, transaction, recipe);
            InsertSteps(connection, transaction, recipe);
            InsertTags(connection, transaction, recipe);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var connection = _factory.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Recipes WHERE ID = $id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        private void InsertIngredients(SqliteConnection connection, SqliteTransaction transaction, Recipe recipe)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText =
                    "INSERT INTO Ingredients (RecipeID, Name) VALUES ($rid, $name);";
                cmd.Parameters.AddWithValue("$rid", recipe.Id);
                cmd.Parameters.AddWithValue("$name", ingredient);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertSteps(SqliteConnection connection, SqliteTransaction transaction, Recipe recipe)
        {
            int order = 0;
            foreach (var step in recipe.Steps)
            {
                var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText =
                    "INSERT INTO Steps (RecipeID, SortOrder, Text) VALUES ($rid, $order, $text);";
                cmd.Parameters.AddWithValue("$rid", recipe.Id);
                cmd.Parameters.AddWithValue("$order", order++);
                cmd.Parameters.AddWithValue("$text", step);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertTags(SqliteConnection connection, SqliteTransaction transaction, Recipe recipe)
        {
            foreach (var tag in recipe.Tags)
            {
                var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText =
                    "INSERT INTO Tags (RecipeID, Name) VALUES ($rid, $name);";
                cmd.Parameters.AddWithValue("$rid", recipe.Id);
                cmd.Parameters.AddWithValue("$name", tag);
                cmd.ExecuteNonQuery();
            }
        }

        private List<string> GetIngredientsForRecipe(int recipeId)
        {
            var list = new List<string>();
            using var connection = _factory.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Ingredients WHERE RecipeId = $id";
            command.Parameters.AddWithValue("$id", recipeId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

        private List<string> GetStepsForRecipe(int recipeId)
        {
            var list = new List<string>();
            using var connection = _factory.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Text FROM Steps WHERE RecipeId = $id ORDER BY SortOrder";
            command.Parameters.AddWithValue("$id", recipeId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

        private List<string> GetTagsForRecipe(int recipeId)
        {
            var list = new List<string>();
            using var connection = _factory.CreateConnection();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Name FROM Tags WHERE RecipeId = $id";
            cmd.Parameters.AddWithValue("$id", recipeId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(reader.GetString(0));
            return list;
        }
    }
}
