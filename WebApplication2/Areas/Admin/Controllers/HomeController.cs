using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace WebApplication2.Areas.Admin.Controllers
{
    [Area("admin")]

    public class HomeController : Controller
    {
        private readonly IMongoCollection<User> _userCollection;

        public HomeController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
        }
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user1 = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user1 == null || user1.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }
            return View();
        }
    }
}
