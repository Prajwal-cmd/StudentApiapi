namespace StudentApi.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public string ChatType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChatMember
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public bool IsCreator { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }
        public string SenderName { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}