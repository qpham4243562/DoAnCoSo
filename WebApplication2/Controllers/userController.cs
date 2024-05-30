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
using MimeKit; // Thêm thư viện này
using MailKit.Net.Smtp; // Thêm thư viện này

namespace WebApplication2.Controllers
{
    public class userController : Controller
    {
        private readonly IMongoCollection<User> _userCollection;

        public userController(IMongoClient mongoClient)
        {
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

            var emailRegex = new Regex(@"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$");
            if (!emailRegex.IsMatch(user.eMail))
            {
                return BadRequest("Định dạng email không hợp lệ");
            }
            if (user.PasswordHash.Length < 6)
            {
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự");
            }

            var existingUser = await _userCollection.Find(u => u.eMail == user.eMail).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Conflict("User with this email already exists");
            }
            user.role = "user";
            user.RegisterAt = DateTime.UtcNow;
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.images = new byte[0];
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

            if (!VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid password");
            }

            user.LastLogin = DateTime.UtcNow;
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, $"{user.firstName} {user.lastName}"),
                new Claim(ClaimTypes.Role, user.role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            Response.Cookies.Append("UserName", $"{user.firstName} {user.lastName}");

            if (user.role == "admin")
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
                return RedirectToAction("Index", "Home", new { area = "admin" });
            }

            return RedirectToAction("Index", "UserPost");
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
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
            string hashedInput = HashPassword(password);
            return String.Equals(hashedInput, hashedPassword);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "user");
        }

        [HttpGet]
        public async Task<IActionResult> Update()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var currentUser = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

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
                        }
                    }
                }
            }

            updatedUser.Id = null;

            if (string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                updatedUser.PasswordHash = currentUser.PasswordHash;
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.firstName, updatedUser.firstName)
                .Set(u => u.lastName, updatedUser.lastName)
                .Set(u => u.eMail, updatedUser.eMail)
                .Set(u => u.PasswordHash, updatedUser.PasswordHash);

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deleteResult = await _userCollection.DeleteOneAsync(u => u.Id == userId);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound("Failed to delete user");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "user");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
// ========================================================= ĐỔI MẬT KHẨU =================================================



        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!ModelState.IsValid)
            {
                return View(email);
            }

            var user = await _userCollection.Find(u => u.eMail == email).FirstOrDefaultAsync();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không tồn tại.");
                return View(email);
            }

            string resetToken = GenerateResetToken();
            user.ResetToken = resetToken;
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

            await SendResetPasswordEmail(email, resetToken);

            return RedirectToAction("ResetPassword", "user", new { email = email });
        }
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userCollection.Find(u => u.eMail == model.Email && u.ResetToken == model.ResetToken).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest("Token nhập sai ");
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            user.ResetToken = null;
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

            return BadRequest("Đã thay đổi password thành công");
            
        }
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        private string GenerateResetToken()
        {
            Random random = new Random();
            int randomNumber = random.Next(100000, 999999);
            return randomNumber.ToString();
        }

        private async Task SendResetPasswordEmail(string email, string resetToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Social Media", "tuem1410@gmail.com"));
            message.To.Add(new MailboxAddress("Nguyễn Minh Tú", email));
            message.Subject = "Reset Password";

            message.Body = new TextPart("plain")
            {
                Text = $"Mã xác thực 6 số của bạn là: {resetToken}."
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync("tuem1410@gmail.com", "nhli ntqe vlao epxm");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


    }
}
