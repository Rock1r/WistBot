using System.Data.SQLite;
using System.Text.Json;

namespace WistBot
{
    internal class Database
    {
        private readonly string _connectionString;

        public Database(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
            }

            string directory = Path.GetDirectoryName(databasePath);

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Directory {directory} does not exist. Creating...");
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(databasePath))
            {
                Console.WriteLine("Database file not found. Creating a new one.");
                SQLiteConnection.CreateFile(databasePath);
            }

            _connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TelegramId TEXT NOT NULL UNIQUE,
                    UserData TEXT NOT NULL
                );";

            using var command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        public bool UserExists(long telegramId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string checkQuery = @"
                SELECT COUNT(1)
                FROM Users
                WHERE TelegramId = @telegramId;
            ";

            using var command = new SQLiteCommand(checkQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void AddUser(long telegramId)
        {
            if (UserExists(telegramId))
            {
                Console.WriteLine("User already exists.");
                return;
            }

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string insertQuery = @"
                INSERT INTO Users (TelegramId, UserData)
                VALUES (@telegramId, @emptyData);
            ";

            string emptyData = JsonSerializer.Serialize(new List<ListObject>());

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@emptyData", emptyData);

            command.ExecuteNonQuery();
        }

        public List<ListObject> GetUserData(long telegramId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = @"
                SELECT UserData
                FROM Users
                WHERE TelegramId = @telegramId;
            ";

            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                string jsonData = reader.GetString(0);
                return JsonSerializer.Deserialize<List<ListObject>>(jsonData) ?? new List<ListObject>();
            }

            Console.WriteLine("No user found with the given Telegram ID.");
            return new List<ListObject>();
        }

        public void UpdateItem(long telegramId, ListObject newItem)
        {
            var currentData = GetUserData(telegramId);
            bool itemExists = false;
            if (currentData == null)
            {
                //log creating new userdata
                currentData = new List<ListObject>();
            }

            foreach (var item in currentData)
            {
                if (item.Id == newItem.Id)
                {
                    itemExists = true;
                    item.Name = newItem.Name;
                    item.Description = newItem.Description;
                    item.Link = newItem.Link;
                    item.Document = newItem.Document;
                    item.Photo = newItem.Photo;
                    item.Performer = newItem.Performer;
                    item.CurrentState = newItem.CurrentState;
                    UpdateUserData(telegramId, currentData);
                    return;
                }
            }
            if (!itemExists)
            {
                currentData.Add(newItem);
            }
            UpdateUserData(telegramId, currentData);
        }

        public void UpdateUserData(long telegramId, List<ListObject> userData)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string updateQuery = @"
                UPDATE Users
                SET UserData = @userData
                WHERE TelegramId = @telegramId;
            ";

            string jsonData = JsonSerializer.Serialize(userData);

            using var command = new SQLiteCommand(updateQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@userData", jsonData);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                Console.WriteLine("No user found with the given Telegram ID.");
            }
        }

        public void ClearUserData(long telegramId)
        {
            UpdateUserData(telegramId, new List<ListObject>());
        }
    }
}
