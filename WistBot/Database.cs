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

        public void AddUser(string telegramId, List<ListObject>? userData = null)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string insertQuery = @"
        INSERT INTO Users (TelegramId, UserData)
        VALUES (@telegramId, @userData);
    ";

            string jsonData = userData != null ? JsonSerializer.Serialize(userData) : "[]";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@userData", jsonData);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                if (ex.ErrorCode == (int)SQLiteErrorCode.Constraint)
                {
                    Console.WriteLine("User already exists.");
                }
                else
                {
                    throw;
                }
            }
        }

        public void UpdateUser(string telegramId, List<ListObject> userData)
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


        public List<ListObject> GetUserData(string telegramId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = "SELECT UserData FROM Users WHERE TelegramId = @telegramId;";
            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                string jsonData = reader.GetString(0);
                return JsonSerializer.Deserialize<List<ListObject>>(jsonData) ?? new List<ListObject>();
            }

            return new List<ListObject>();
        }
    }
}
