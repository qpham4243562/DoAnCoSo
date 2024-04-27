using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.Http; // Use HttpClient from System.Net.Http
using System.Security.Claims;
using System.Threading.Tasks; // Use Task for asynchronous operations
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class UserPostController : Controller
    {

        private readonly IMongoCollection<User_Post> _userPostCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<LikedPost> _likedPostCollection;
        private readonly IMongoCollection<Notification> _notificationCollection;
        public UserPostController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Post>("user_Post");
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
            _likedPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<LikedPost>("LikedPosts");
            _notificationCollection = mongoClient.GetDatabase("DoAn").GetCollection<Notification>("notification");
        }
        [HttpGet]
        public async Task<IActionResult> Index(string selectedClass, string selectedSubject)
        {
            List<User_Post> postList;
            var filter = Builders<User_Post>.Filter.Empty;

            if (!string.IsNullOrEmpty(selectedClass))
                filter &= Builders<User_Post>.Filter.Eq(p => p.Class, selectedClass);

            if (!string.IsNullOrEmpty(selectedSubject))
                filter &= Builders<User_Post>.Filter.Eq(p => p.Subject, selectedSubject);

            try
            {
                postList = await _userPostCollection.Find(filter)
                                                    .SortByDescending(post => post.createdAt)
                                                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while fetching user posts from MongoDB: {ex.Message}");
                postList = new List<User_Post>();
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
            user_Post.Likes = 0;
            // Add information about the creator
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            user_Post.CreatorId = userId;
            var creator = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (creator != null)
            {
                user_Post.CreatorName = $"{creator.LastName} {creator.FirstName}";
            }
            else
            {
                // Xử lý trường hợp không tìm thấy thông tin của người tạo
                // Ví dụ: Gán giá trị mặc định hoặc thông báo lỗi
                user_Post.CreatorName = "Unknown";
            }

            // Save user post to the database
            await _userPostCollection.InsertOneAsync(user_Post);

            return RedirectToAction("Index", "UserPost");
        }
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid ID");
            }

            // Tìm bài đăng cần cập nhật trong cơ sở dữ liệu
            var existingPost = await _userPostCollection.Find(post => post.id == id).FirstOrDefaultAsync();
            if (existingPost == null)
            {
                return NotFound("Post not found");
            }

            return View(existingPost);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, [FromForm] User_Post updatedPost)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid ID");
            }

            // Kiểm tra xem bài đăng có tồn tại trong cơ sở dữ liệu không
            var existingPost = await _userPostCollection.Find(post => post.id == id).FirstOrDefaultAsync();
            if (existingPost == null)
            {
                return NotFound("Post not found");
            }

            // Cập nhật chỉ các thuộc tính được chỉ định
            existingPost.title = updatedPost.title ?? existingPost.title;
            existingPost.content = updatedPost.content ?? existingPost.content;
            // Cập nhật ngày cập nhật
            existingPost.updatedAt = DateTime.Now;

            // Thực hiện cập nhật bài đăng trong cơ sở dữ liệu
            var updatedResult = await _userPostCollection.ReplaceOneAsync(post => post.id == id, existingPost);

            // Chuyển hướng về trang chủ hoặc trang danh sách bài đăng
            return RedirectToAction("Index", "UserPost");
        }





        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            // Kiểm tra xem ID của bài đăng có hợp lệ hay không
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            // Tìm bài đăng cần xóa trong cơ sở dữ liệu
            var postToDelete = await _userPostCollection.Find(post => post.id == id).FirstOrDefaultAsync();

            // Nếu không tìm thấy bài đăng, trả về NotFound
            if (postToDelete == null)
            {
                return NotFound();
            }

            // Chuyển dữ liệu của bài đăng cần xóa sang view để hiển thị thông tin
            return View(postToDelete);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // Thêm xác thực CSRF
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            // Kiểm tra xem ID của bài đăng có hợp lệ hay không
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            // Xóa bài đăng từ cơ sở dữ liệu
            var result = await _userPostCollection.DeleteOneAsync(post => post.id == id);

            // Nếu không tìm thấy bài đăng để xóa, trả về NotFound
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            // Chuyển hướng về trang chủ hoặc trang danh sách bài đăng
            return RedirectToAction("Index", "UserPost");
        }
        [HttpPost("like/{postId}")]
        public async Task<IActionResult> LikePost(string postId)
        {
            // Lấy thông tin người dùng từ cookie
            var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Tìm bài đăng theo postId
            var post = await _userPostCollection.Find(p => p.id == postId).FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng đã like bài đăng này chưa
            var userLiked = post.LikedByUsers.Contains(currentUserId);

            if (userLiked)
            {
                // Nếu đã like, xóa like của người dùng khỏi danh sách
                post.LikedByUsers.Remove(currentUserId);
                post.Likes--;
            }
            else
            {
                // Nếu chưa like, thêm userId vào danh sách
                post.LikedByUsers.Add(currentUserId);
                post.Likes++;

                // Tạo notification mới
                var notification = new Notification
                {
                    UserId = post.CreatorId, // Id của người đăng bài
                    Content = $"{HttpContext.User.Identity.Name} đã thích bài đăng của bạn.",
                    Type = "like",
                    PostId = postId,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                // Lấy người đăng bài
                var user = await _userCollection.Find(u => u.Id == post.CreatorId).FirstOrDefaultAsync();

                // Kiểm tra nếu user không null
                if (user != null)
                {
                    // Thêm notification vào collection _notificationCollection
                    await _notificationCollection.InsertOneAsync(notification);
                }
            }

            // Cập nhật bài đăng
            await _userPostCollection.ReplaceOneAsync(p => p.id == postId, post);

            return Ok(new { likes = post.Likes });
        }
    }
}
