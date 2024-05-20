using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
namespace WebApplication2
{
    public class ChatHub : Hub
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<User_Message> _userMessageCollection;

        public ChatHub(IMongoClient mongoClient)
        {
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
            _userMessageCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Message>("user_Message");
        }

        public async Task SendMessage(string message)
        {
            var userCookie = Context.GetHttpContext().Request.Cookies["UserName"];
            if (string.IsNullOrEmpty(userCookie))
            {
                return;
            }

            string currentTime = DateTime.Now.ToString();
            string messageWithTime = $"[{currentTime}] {userCookie}: {message}";

            // Lưu tin nhắn vào database
            var userMessage = new User_Message
            {
                user_name = userCookie,
                message = message,
                createdAt = DateTime.Now
            };
            await _userMessageCollection.InsertOneAsync(userMessage);

            await Clients.All.SendAsync("ReceiveMessage", userCookie, messageWithTime);
        }
    }


}
