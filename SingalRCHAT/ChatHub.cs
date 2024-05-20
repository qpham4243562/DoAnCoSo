using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
namespace SingalRCHAT
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string message)
        {
            // Lấy tên người dùng từ claim
            var userName = Context.User.Identity.Name;

            // Lấy thời gian hiện tại
            string currentTime = DateTime.Now.ToLocalTime().ToString();

            // Tạo tin nhắn với thời gian và tên người dùng
            string messageWithTime = $"[{currentTime}] {userName}: {message}";

            // Gửi tin nhắn đến tất cả các client
            await Clients.All.SendAsync("ReceiveMessage", userName, messageWithTime);
        }
    }
}
