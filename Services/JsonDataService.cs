using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipePlanner.Services
{
    public class JsonDataService
    {
        private readonly string _basePath;

        public JsonDataService()
        {
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData");
            Directory.CreateDirectory(_basePath);
        }

        public void Save<T>(string fileName, T data)
        {
            string path = Path.Combine(_basePath, fileName);
            File.WriteAllText(path, JsonSerializer.Serialize(data));
        }

        public T? Load<T>(string fileName)
        {
            string path = Path.Combine(_basePath, fileName);
            if (!File.Exists(path)) return default;

            return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
        }
    }
}
