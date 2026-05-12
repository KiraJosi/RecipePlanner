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

        public List<PantryItem> GetAll()
        {
            var items = new List<PantryItem>();
            using var connection = _factory.CreateConnection();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ID, Name, Amount FROM PantryItems";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                items.Add(new PantryItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Amount = reader.IsDBNull(2) ? "" : reader.GetString(2)
                });
            return items;
        }

        public void Save(List<PantryItem> items)
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
                insert.CommandText = "INSERT INTO PantryItems (Name, Amount) VALUES ($name, $amount)";
                insert.Parameters.AddWithValue("$name", item.Name);
                insert.Parameters.AddWithValue("$amount", item.Amount ?? "");
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

    }
}
