using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.Http; // Use HttpClient from System.Net.Http
using System.Threading.Tasks; // Use Task for asynchronous operations
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class UserPostController : Controller
    {

        private readonly IMongoCollection<User_Post> _userPostCollection;

        public UserPostController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Post>("user_Post");
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<User_Post> postList;
            try
            {
                // Lấy tất cả các User_Post từ collection
                postList = await _userPostCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ khi truy vấn MongoDB
                ModelState.AddModelError(string.Empty, $"An error occurred while fetching user posts from MongoDB: {ex.Message}");
                postList = new List<User_Post>(); // Khởi tạo danh sách rỗng để tránh lỗi
            }

            return View(postList);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromForm] User_Post user_Post)
        {
            var files = HttpContext.Request.Form.Files;

            // Handle multiple images efficiently (up to a reasonable limit)
            if (files != null)
            {
                var imageCount = Math.Min(files.Count, 10); // Limit to 10 images (adjust as needed)

                user_Post.images = new List<byte[]>(imageCount);
                for (int i = 0; i < imageCount; i++)
                {
                    using (var ms = new MemoryStream())
                    {
                        await files[i].CopyToAsync(ms);
                        user_Post.images.Add(ms.ToArray());
                    }
                }
            }

            // Insert data with images
            user_Post.createdAt = DateTime.Now;

            // Lưu user post vào database
            await _userPostCollection.InsertOneAsync(user_Post);

            return Ok(user_Post);
        }
    }
}