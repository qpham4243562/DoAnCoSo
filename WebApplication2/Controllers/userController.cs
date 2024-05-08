using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication2.Models;
using DoAnCoSoAPI.Data;

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
            user.role = "user";
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
            if (user.IsOnline==true)
            {
                return Unauthorized("Nguoi dung dang online");
            }
            user.LastLogin = DateTime.UtcNow;
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
            // Tạo claim chứa thông tin của người dùng
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        // Thêm các thông tin khác của người dùng nếu cần
    };

            // Kiểm tra và thêm vai trò của người dùng
            if (user.role == "admin")
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
                return RedirectToAction("Index", "Home", new { area = "admin" });
            }
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
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            // Lấy userId từ cookie đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tìm thông tin người dùng từ userId
            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(User updatedUser, IFormFileCollection images)
        {
            // Lấy userId từ cookie đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tìm người dùng trong cơ sở dữ liệu sử dụng userId
            var userToUpdate = await _userCollection.FindOneAndUpdateAsync(
                Builders<User>.Filter.Eq(u => u.Id, userId), // Điều kiện tìm kiếm
                Builders<User>.Update
                    .Set(u => u.FirstName, updatedUser.FirstName)
                    .Set(u => u.LastName, updatedUser.LastName)
                    .Set(u => u.Email, updatedUser.Email)
                    .Set(u => u.PasswordHash, updatedUser.PasswordHash)
                    
            // Cập nhật các trường khác cần thiết tại đây
            );

            // Kiểm tra xem người dùng đã được cập nhật thành công hay không
            if (userToUpdate == null)
            {
                return NotFound("User not found");
            }
            foreach (var image in images)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    userToUpdate.images = memoryStream.ToArray();
                }
            }

            // Cập nhật người dùng sau khi đã có hình ảnh mới
            await _userCollection.ReplaceOneAsync(u => u.Id == userId, userToUpdate);
            return RedirectToAction("Update", "user");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            // Lấy userId từ cookie đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Xóa người dùng từ cơ sở dữ liệu
            var deleteResult = await _userCollection.DeleteOneAsync(u => u.Id == userId);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound("Failed to delete user");
            }

            // Đăng xuất người dùng
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Chuyển hướng đến trang đăng nhập hoặc trang chính tùy thuộc vào yêu cầu của bạn
            return RedirectToAction("Login", "user");
        }




    }
}
