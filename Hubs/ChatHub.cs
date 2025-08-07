using Microsoft.AspNetCore.SignalR;
using StudentApi.Data;
using System.Collections.Concurrent;

namespace StudentApi.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
        private readonly ChatDataAccess _chatDataAccess;

        public ChatHub(ChatDataAccess chatDataAccess)
        {
            _chatDataAccess = chatDataAccess;
        }

        public override Task OnConnectedAsync()
        {
            var userName = Context.GetHttpContext()?.Request.Query["userName"].ToString();
            if (!string.IsNullOrEmpty(userName))
            {
                _userConnections.AddOrUpdate(userName, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var userToRemove = _userConnections.FirstOrDefault(x => x.Value == connectionId);
            if (!userToRemove.Equals(default(KeyValuePair<string, string>)))
            {
                _userConnections.TryRemove(userToRemove.Key, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }





        public async Task CreateChat(string chatType, string[] participantUserNames, string creatorEmail)
        {
            try
            {
                var user = _chatDataAccess.GetUserByEmail(creatorEmail);
                if (user == null || user.Role != "instructor")
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Only instructors can create chats.");
                    return;
                }

                var participantIds = new List<int>();
                foreach (var participantUserName in participantUserNames)
                {
                    var participant = _chatDataAccess.GetUserByEmail(participantUserName);
                    if (participant != null)
                    {
                        participantIds.Add(participant.Id);
                    }
                }

                int chatId = _chatDataAccess.CreateChat(user.Id, chatType, participantIds);
                await Clients.Caller.SendAsync("ReceiveSuccess", $"Chat {chatId} created successfully.");

                // Update chat list for all participants, including the creator
                var allParticipants = participantUserNames.ToList();
                allParticipants.Add(creatorEmail); // Include the creator

                foreach (var participantEmail in allParticipants)
                {
                    if (_userConnections.TryGetValue(participantEmail, out string connectionId))
                    {
                        await RequestChatList(participantEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", $"Error creating chat: {ex.Message}");
            }
        }


        public async Task SendMessage(int chatId, string message, string senderEmail)
        {
            try
            {
                var user = _chatDataAccess.GetUserByEmail(senderEmail);
                if (user == null)
                {
                    await Clients.Caller.SendAsync("ReceiveError", "User not found.");
                    return;
                }

                _chatDataAccess.AddMessage(chatId, user.Id, message);

                var chatMembers = _chatDataAccess.GetChatMembers(chatId);
                foreach (var memberEmail in chatMembers)
                {
                    if (_userConnections.TryGetValue(memberEmail, out string connectionId))
                    {
                        await Clients.Client(connectionId).SendAsync("ReceiveMessage", chatId, senderEmail, message, DateTime.Now.ToString("g"));
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", $"Error sending message: {ex.Message}");
            }
        }





        public async Task RequestChatList(string userEmail)
        {
            try
            {
                var user = _chatDataAccess.GetUserByEmail(userEmail);
                if (user == null) return;

                var chats = _chatDataAccess.GetUserChats(user.Id);
                var chatList = chats.Select(c => new { Id = c.Id, ChatType = c.ChatType }).ToList();
                if (_userConnections.TryGetValue(userEmail, out string connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("UpdateChatList", chatList);
                }
            }
            catch (Exception ex)
            {
                if (_userConnections.TryGetValue(userEmail, out string connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveError", $"Error loading chats: {ex.Message}");
                }
            }
        }



        public async Task RequestChatHistory(int chatId, string userEmail)
        {
            try
            {
                var user = _chatDataAccess.GetUserByEmail(userEmail);
                if (user == null) return;

                var messages = _chatDataAccess.GetChatHistory(chatId);
                // Format timestamps to match SendMessage
                var formattedMessages = messages.Select(m => new
                {
                    m.SenderName,
                    m.MessageText,
                    Timestamp = m.Timestamp.ToString("g") // Format DateTime to string
                }).ToList();
                await Clients.Caller.SendAsync("UpdateChatHistory", formattedMessages);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", $"Error loading chat history: {ex.Message}");
            }
        }
    }
}

