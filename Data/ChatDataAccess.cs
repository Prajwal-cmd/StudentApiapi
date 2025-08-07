using Microsoft.Data.SqlClient;
using StudentApi.Models;
using System.Data;

namespace StudentApi.Data
{
    public class ChatDataAccess
    {
        private readonly string _connectionString;

        public ChatDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public User GetUserByEmail(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("sp_GetUserByEmail", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32("Id"),
                            Email = reader.GetString("Email"),
                            Name = reader.IsDBNull("Name") ? null : reader.GetString("Name"),
                            Role = reader.IsDBNull("Role") ? "User" : reader.GetString("Role")
                        };
                    }
                }
            }
            return null;
        }

        public int CreateChat(int creatorId, string chatType, List<int> participantIds)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_ChatCreate_1", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CreatorId", creatorId);
                    command.Parameters.AddWithValue("@ChatType", chatType);

                    var participantsTable = new DataTable();
                    participantsTable.Columns.Add("UserId", typeof(int));
                    foreach (var participantId in participantIds)
                    {
                        participantsTable.Rows.Add(participantId);
                    }

                    command.Parameters.Add(new SqlParameter("@Participants", SqlDbType.Structured)
                    {
                        Value = participantsTable,
                        TypeName = "dbo.UserIdTableType"
                    });

                    return (int)command.ExecuteScalar();
                }
            }
        }

        public void AddMessage(int chatId, int senderId, string messageText)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_ChatAddMessage_1", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@SenderId", senderId);
                    command.Parameters.AddWithValue("@MessageText", messageText);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Chat> GetUserChats(int userId)
        {
            var chats = new List<Chat>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"
                    SELECT DISTINCT c.Id, c.ChatType, c.CreatedAt
                    FROM Chats c 
                    INNER JOIN ChatMembers cm ON c.Id = cm.ChatId 
                    WHERE cm.UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chats.Add(new Chat
                            {
                                Id = reader.GetInt32("Id"),
                                ChatType = reader.GetString("ChatType"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            });
                        }
                    }
                }
            }
            return chats;
        }

        public List<Message> GetChatHistory(int chatId)
        {
            var messages = new List<Message>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"
                    SELECT u.Email AS SenderName, m.MessageText, m.Timestamp 
                      FROM Messages m 
                      INNER JOIN Users u ON m.SenderId = u.Id 
                      WHERE m.ChatId = @ChatId 
                      ORDER BY m.Timestamp ASC", connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Message
                            {
                                SenderName = reader.GetString(0),
                                MessageText = reader.GetString(1),
                                Timestamp = reader.GetDateTime("Timestamp")
                            });
                        }
                    }
                }
            }
            return messages;
        }

        public List<string> GetChatMembers(int chatId)
        {
            var members = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"
                    SELECT u.Email 
                    FROM ChatMembers cm 
                    INNER JOIN Users u ON cm.UserId = u.Id 
                    WHERE cm.ChatId = @ChatId", connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(reader.GetString("Email"));
                        }
                    }
                }
            }
            return members;
        }
    }
}