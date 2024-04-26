using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
namespace SingalRCHAT
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            string currentTime = DateTime.Now.ToLocalTime().ToString(); // Lấy thời gian hiện tại
            string messageWithTime = $"[{currentTime}] {user}: {message}"; // Thêm thời gian vào tin nhắn
            await Clients.All.SendAsync("ReceiveMessage", user, messageWithTime);

        }
    }
}
