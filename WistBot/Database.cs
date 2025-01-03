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
            Username TEXT,
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

        public bool UserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
            }

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string query = @"
        SELECT EXISTS(
            SELECT 1 
            FROM Users 
            WHERE Username = @username
        );
    ";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);

            return Convert.ToInt32(command.ExecuteScalar()) == 1;
        }


        public void AddUser(long telegramId, string username)
        {
            if (UserExists(telegramId))
            {
                UpdateUsername(telegramId, username);
                return;
            }

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string insertQuery = @"
        INSERT INTO Users (TelegramId, Username, UserData)
        VALUES (@telegramId, @username, @emptyData);
    ";

            string emptyData = JsonSerializer.Serialize(new UserData(telegramId, username));

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@emptyData", emptyData);

            command.ExecuteNonQuery();
        }

        public void UpdateUsername(long telegramId, string username)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string updateQuery = @"
        UPDATE Users
        SET Username = @username
        WHERE TelegramId = @telegramId;
    ";

            using var command = new SQLiteCommand(updateQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@username", username);

            command.ExecuteNonQuery();
        }


        public UserData GetUserData(long telegramId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = @"
        SELECT UserData, Username
        FROM Users
        WHERE TelegramId = @telegramId;
    ";

            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                string jsonData = reader.GetString(0);
                string username = reader.IsDBNull(1) ? null : reader.GetString(1);

                var userData = JsonSerializer.Deserialize<UserData>(jsonData) ?? new UserData { TelegramId = telegramId };
                userData.Username = username;
                return userData;
            }

            return new UserData { TelegramId = telegramId };
        }

        public string GetUsername(long telegramId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string selectQuery = @"
                SELECT Username
                FROM Users
                WHERE TelegramId = @telegramId;";
            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@telegramId", telegramId);
            return command.ExecuteScalar() as string;
        }




        public void UpdateItem(long telegramId, string listName, WishListItem newItem)
        {
            var currentData = GetUserData(telegramId) ?? new UserData(telegramId, GetUsername(telegramId));

            var currentList = currentData.WishLists.FirstOrDefault(list => list.Name == listName);

            if (currentList == null)
            {
                currentList = new WishList { Name = listName };
                currentList.Items.Add(newItem);
                currentData.UpdateWishList(listName, currentList);
            }
            else
            {
                var existingItem = currentList.Items.FirstOrDefault(item => item.Id == newItem.Id);
                if (existingItem != null)
                {
                    existingItem.Name = newItem.Name;
                    existingItem.Description = newItem.Description;
                    existingItem.Link = newItem.Link;
                    existingItem.Photo = newItem.Photo;
                    existingItem.PerformerId = newItem.PerformerId;
                    existingItem.CurrentState = newItem.CurrentState;
                }
                else
                {
                    currentList.Items.Add(newItem);
                }

                currentData.UpdateWishList(listName, currentList);
            }

            UpdateUserData(telegramId, currentData);
        }

        public void UpdateUserData(long telegramId, UserData userData)
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

        public long GetUserId(string username)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string selectQuery = @"
                SELECT TelegramId
                FROM Users
                WHERE Username = @username;
            ";
            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@username", username);
            return Convert.ToInt64(command.ExecuteScalar());
        }

        public bool AddWishList(long telegramId, string listName)
        {
            var userData = GetUserData(telegramId);

            if (userData.WishLists.Any(list => list.Name == listName))
            {
                return false;
            }

            userData.WishLists.Add(new WishList { Name = listName });
            UpdateUserData(telegramId, userData);
            return true;
        }

        public WishList GetWishList(long telegramId, string listName)
        {
            var userData = GetUserData(telegramId);

            var wishList = userData.WishLists.FirstOrDefault(list => list.Name == listName);
            if (wishList == null)
            {
                Console.WriteLine("No list found with the given name.");
                return new WishList { Name = listName }; // Повертаємо порожній список.
            }

            return wishList;
        }

        public void UpdateWishList(long telegramId, string listName, WishList newItems)
        {
            var userData = GetUserData(telegramId);

            var wishListIndex = userData.WishLists.FindIndex(list => list.Name == listName);

            if (wishListIndex == -1)
            {
                userData.WishLists.Add(new WishList
                {
                    Name = listName,
                    Items = newItems.Items
                });
            }
            else
            {
                userData.WishLists[wishListIndex].Items = newItems.Items;
                userData.WishLists[wishListIndex].IsPublic = newItems.IsPublic;
            }

            UpdateUserData(telegramId, userData);
        }


        public void DeleteWishList(long telegramId, string listName)
        {
            var userData = GetUserData(telegramId);

            var wishList = userData.WishLists.FirstOrDefault(list => list.Name == listName);
            if (wishList != null)
            {
                userData.WishLists.Remove(wishList);
                UpdateUserData(telegramId, userData);
            }
            else
            {
                Console.WriteLine("No list found with the given name.");
            }
        }

        public WishListItem GetItem(long userId, string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            var userData = GetUserData(userId);
            var list = userData.WishLists.FirstOrDefault(list => list.Items.Any(item => item.Name == text));
            if (list == null)
            {
                return null;
            }
            return list.Items.First(item => item.Name == text);
        }
    }
}
