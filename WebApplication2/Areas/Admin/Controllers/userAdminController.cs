using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication2.Models;

namespace WebApplication2.Areas.Admin.Controllers
{
    [Area("admin")]

    public class userAdminController : Controller
    {
        private readonly IMongoCollection<User> _userCollection;

        public userAdminController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
        }
        [HttpGet("Admin/userAdmin/Register")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost("Admin/userAdmin/Register")]
        public async Task<IActionResult> Register(User user)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user1 = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user1 == null || user1.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }
            if (user == null)
            {
                return BadRequest("Invalid user object");
            }

            // Check if user with the same email already exists
            var existingUser = await _userCollection.Find(u => u.eMail == user.eMail).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Conflict("User with this email already exists");
            }

            // Set registration timestamp
            user.RegisterAt = DateTime.UtcNow;

            // You should hash the password before saving it to the database
            // For demonstration purposes, let's assume PasswordHash is already hashed
            user.PasswordHash = HashPassword(user.PasswordHash);

            await _userCollection.InsertOneAsync(user);
            return Redirect("/admin/userAdmin/Index");
        }
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Băm mật khẩu
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển đổi byte array sang chuỗi hex
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null || user.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }

            var users = await _userCollection.Find(u => true).ToListAsync();
            return View(users);
        }


        [HttpGet("Admin/userAdmin/Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            // Tìm người dùng theo id
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var admin = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (admin == null || admin.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }
            var user = await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost("Admin/userAdmin/Edit/{id}")]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            // Kiểm tra xem người dùng đã cung cấp mật khẩu mới hay không
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                // Băm mật khẩu mới
                user.PasswordHash = HashPassword(user.PasswordHash);
            }

            // Cập nhật thông tin người dùng
            var result = await _userCollection.ReplaceOneAsync(u => u.Id == id, user);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }


        [HttpGet("Admin/userAdmin/Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // Tìm người dùng theo id và hiển thị trang xác nhận xóa
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var admin = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (admin == null || admin.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }
            var user = await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost("Admin/userAdmin/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var admin = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (admin == null || admin.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }
            // Xóa người dùng từ database
            var result = await _userCollection.DeleteOneAsync(u => u.Id == id);
            if (result.IsAcknowledged && result.DeletedCount > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet("Admin/userAdmin/Search")]
        public async Task<IActionResult> Search(string email)
        {
            Console.WriteLine($"Searching for email: {email}");

            // Kiểm tra xem email được cung cấp hay không
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("No email provided, returning all users.");
                var users = await _userCollection.Find(u => true).ToListAsync();
                return View("Index", users);
            }

            Console.WriteLine("Searching for users with email containing the provided value.");
            var usersFound = await _userCollection.Find(u => u.eMail.Contains(email)).ToListAsync();
            Console.WriteLine($"Found {usersFound.Count} users.");

            // Trả về view với danh sách người dùng tìm thấy
            return View("Index", usersFound);
        }
    }
}
