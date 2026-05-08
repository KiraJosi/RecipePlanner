using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlanner.Models;

namespace RecipePlanner.Services.Data
{
    public class PantryRepository
    {
        private readonly SQLiteConnectionFactory _factory;

        public PantryRepository(SQLiteConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<string> GetAll()
        {
            var items = new List<string>();
            using var connection = _factory.CreateConnection();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Name FROM PantryItems";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                items.Add(reader.GetString(0));
            return items;
        }

        public void Save(List<string> items)
        {
            using var connection = _factory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            var delete = connection.CreateCommand();
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM PantryItems";
            delete.ExecuteNonQuery();

            foreach (var item in items)
            {
                var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = "INSERT INTO PantryItems (Name) VALUES ($name)";
                insert.Parameters.AddWithValue("$name", item);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

    }
}
