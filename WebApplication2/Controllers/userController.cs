using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class userController : Controller
    {
        private readonly IMongoCollection<User> _userCollection;

        public userController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user object");
            }

            // Check if user with the same email already exists
            var existingUser = await _userCollection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Conflict("User with this email already exists");
            }

            // Set registration timestamp
            user.RegisterAt = DateTime.UtcNow;

            // You should hash the password before saving it to the database
            // For demonstration purposes, let's assume PasswordHash is already hashed
            // user.PasswordHash = HashFunction(user.PasswordHash);

            await _userCollection.InsertOneAsync(user);
            return Redirect("/user/Login");
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest loginRequest)
        {
            var user = await _userCollection.Find(u => u.Email == loginRequest.Email).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Kiểm tra mật khẩu
            if (loginRequest.Password != user.PasswordHash)
            {
                return Unauthorized("Invalid password");
            }

            // Tạo claim chứa thông tin của người dùng
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        // Thêm các thông tin khác của người dùng nếu cần
    };

            // Tạo ClaimsIdentity từ danh sách claim
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Tạo và đặt cookie xác thực
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Chuyển hướng đến trang chính sau khi đăng nhập thành công
            return RedirectToAction("Index", "UserPost");
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Chuyển hướng đến trang đăng nhập hoặc trang chính tùy thuộc vào yêu cầu của bạn
            return RedirectToAction("Login", "user");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
