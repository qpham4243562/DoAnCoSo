using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using System.Diagnostics;
using DoAnCoSoAPI.Entities;
using MongoDB.Driver;

namespace WebApplication2.Controllers
{
    public class ChatController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoCollection<User_Message> _userMessageCollection;
        private readonly IMongoClient _mongoClient;

        public ChatController(ILogger<HomeController> logger, IMongoClient mongoClient)
        {
            _logger = logger;
            _mongoClient = mongoClient;
            _userMessageCollection = _mongoClient.GetDatabase("DoAn").GetCollection<User_Message>("user_Message");
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _userMessageCollection.Find(_ => true).ToListAsync();
            return View(messages);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
