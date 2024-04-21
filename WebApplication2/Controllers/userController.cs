using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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

            // You should verify the password here using your hash function
            // For demonstration purposes, let's assume PasswordHash is already hashed
            // if (!VerifyPassword(loginRequest.Password, user.PasswordHash))
            // {
            //     return Unauthorized("Invalid password");
            // }

            // For demonstration purposes, let's assume PasswordHash is plaintext
            if (loginRequest.Password != user.PasswordHash)
            {
                return Unauthorized("Invalid password");
            }

            // Update last login timestamp
            user.LastLogin = DateTime.UtcNow;
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

            return RedirectToAction("Index", "UserPost");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
