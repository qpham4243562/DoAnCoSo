using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication2.Models;
using DoAnCoSoAPI.Data;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

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
            if (string.IsNullOrEmpty(user.firstName) || string.IsNullOrEmpty(user.lastName) ||
            string.IsNullOrEmpty(user.eMail) || string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest("Vui lòng điền đầy đủ thông tin");
            }

            // Kiểm tra định dạng email, ví dụ: 
            var emailRegex = new Regex(@"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$");
            if (!emailRegex.IsMatch(user.eMail))
            {
                return BadRequest("Định dạng email không hợp lệ");
            }
            if (user.PasswordHash.Length < 6)
            {
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự");
            }
            // Check if user with the same email already exists
            var existingUser = await _userCollection.Find(u => u.eMail == user.eMail).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Conflict("User with this email already exists");
            }
            user.role = "user";
            // Set registration timestamp
            user.RegisterAt = DateTime.UtcNow;

            // You should hash the password before saving it to the database
            // For demonstration purposes, let's assume PasswordHash is already hashed
            user.PasswordHash = HashPassword(user.PasswordHash);

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
            var user = await _userCollection.Find(u => u.eMail == loginRequest.eMail).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Kiểm tra mật khẩu
            if (!VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid password");
            }

            user.LastLogin = DateTime.UtcNow;
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
            // Tạo claim chứa thông tin của người dùng
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, $"{user.firstName} {user.lastName}"),
        new Claim(ClaimTypes.Role, user.role)
        // Thêm các thông tin khác của người dùng nếu cần
    };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            // Kiểm tra và thêm vai trò của người dùng
            if (user.role == "admin")
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
                
                return RedirectToAction("Index", "Home", new { area = "admin" });
            }
            // Tạo ClaimsIdentity từ danh sách claim
           

            // Tạo và đặt cookie xác thực
           

            // Chuyển hướng đến trang chính sau khi đăng nhập thành công
            return RedirectToAction("Index", "UserPost");
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

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // So sánh mật khẩu đã băm với mật khẩu nhập vào
            string hashedInput = HashPassword(password);
            return String.Equals(hashedInput, hashedPassword);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest("User id not found in cookie");
            }

            // Kiểm tra xem có hình nào được tải lên hay không
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await image.CopyToAsync(memoryStream);
                            updatedUser.images = memoryStream.ToArray();
                            // Lưu tên file hoặc các thông tin khác liên quan đến hình ảnh nếu cần
                        }
                    }
                }
            }

            // Đảm bảo rằng bạn đã cập nhật tất cả các trường dữ liệu khác của đối tượng người dùng
            // ở đây trước khi thực hiện cập nhật trong cơ sở dữ liệu

            // Xóa trường _id từ đối tượng updatedUser
            updatedUser.Id = null;

            // Thực hiện cập nhật trong cơ sở dữ liệu
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.firstName, updatedUser.firstName)
                .Set(u => u.lastName, updatedUser.lastName)
                .Set(u => u.eMail, updatedUser.eMail)
                .Set(u => u.PasswordHash, updatedUser.PasswordHash);

            // Nếu có hình mới được tải lên, cập nhật cả hình ảnh
            if (images != null && images.Count > 0)
            {
                update = update.Set(u => u.images, updatedUser.images);
            }

            var result = await _userCollection.UpdateOneAsync(filter, update);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                return RedirectToAction("Update", "user");
            }
            else
            {
                return NotFound();
            }
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
        [HttpGet]
        public async Task<IActionResult> Index()
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

        [HttpGet]
        public async Task<IActionResult> GetUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty");
            }

            // Tìm thông tin người dùng từ userId
            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            List<User> searchResults;
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>();

            // Phân tách chuỗi tìm kiếm thành các từ riêng biệt
            var searchTerms = searchString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Tạo bộ lọc cho mỗi từ tìm kiếm
            foreach (var term in searchTerms)
            {
                var regexFilter = new BsonRegularExpression(term, "i");
                var nameFilter = filterBuilder.Or(
                    filterBuilder.Regex("firstName", regexFilter),
                    filterBuilder.Regex("lastName", regexFilter),
                    filterBuilder.Regex("eMail", regexFilter)
                );
                filters.Add(nameFilter);
            }

            // Kết hợp tất cả các bộ lọc
            var combinedFilter = filterBuilder.And(filters);

            try
            {
                // Thực hiện truy vấn với bộ lọc đã tạo
                searchResults = await _userCollection.Find(combinedFilter).ToListAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while searching: {ex.Message}");
                searchResults = new List<User>();
            }

            return View(searchResults);
        }

    }
}
