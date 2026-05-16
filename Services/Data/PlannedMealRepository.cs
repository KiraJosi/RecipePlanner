using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Models;

namespace RecipePlanner.Services.Data
{
    public class PlannedMealRepository
    {
        private readonly SQLiteConnectionFactory _factory;

        public PlannedMealRepository(SQLiteConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<PlannedMeal> GetAll()
        {
            var meals = new List<PlannedMeal>();
            using var connection = _factory.CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT PM.ID, PM.RecipeID, PM.Date, R.Name
                FROM PlannedMeals PM
                JOIN Recipes R ON PM.RecipeID = R.ID";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime date;
                try
                {
                    date = DateTime.SpecifyKind(
                        DateTime.ParseExact(reader.GetString(2), "yyyy-MM-dd", null),
                        DateTimeKind.Local);
                }
                catch (FormatException)
                {
                    date = DateTime.Today;
                }

                meals.Add(new PlannedMeal
                {
                    ID = reader.GetInt32(0),
                    Recipe = new Recipe
                    {
                        Id = reader.GetInt32(1),
                        Name = reader.GetString(3)
                    },
                    Date = date
                });
            }
                return meals;
            }

        public void Save(List<PlannedMeal> meals)
        {
            using var connection = _factory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM PlannedMeals";
            deleteCommand.ExecuteNonQuery();

            foreach (var meal in meals)
            {
                if (meal.Recipe == null) continue;

                var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = @"
                    INSERT INTO PlannedMeals (RecipeID, Date)
                    VALUES ($rid, $date);";
                insert.Parameters.AddWithValue("$rid", meal.Recipe.Id);
                insert.Parameters.AddWithValue("$date", meal.Date.Date.ToString("yyyy-MM-dd"));
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

    }
}

