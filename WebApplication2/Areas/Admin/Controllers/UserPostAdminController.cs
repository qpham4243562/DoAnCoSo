using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace WebApplication2.Areas.Admin.Controllers
{
    [Area("admin")]
    public class UserPostAdminController : Controller
    {

        private readonly IMongoCollection<User_Post> _userPostCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<LikedPost> _likedPostCollection;
        public UserPostAdminController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Post>("user_Post");
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
            _likedPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<LikedPost>("LikedPosts");
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<User_Post> postList;
            try
            {
                // Lấy tất cả các User_Post từ collection và sắp xếp theo thời gian tạo giảm dần
                postList = await _userPostCollection.Find(_ => true)
                                                     .SortByDescending(post => post.createdAt)
                                                     .ToListAsync();
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

            // Đặt giá trị của trường IsApproved thành true để chỉ định bài viết đã được duyệt
            postToApprove.IsApproved = true;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _userPostCollection.ReplaceOneAsync(post => post.id == id, postToApprove);

            // Chuyển hướng hoặc thực hiện hành động khác sau khi duyệt bài viết
            return RedirectToAction("PendingPosts");
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
            return RedirectToAction("Index", "UserPostAdmin");
        }

        // Phương thức để gửi thông báo cho chủ sở hữu bài viết đã bị xóa
        private async Task SendNotificationToPostOwner(User_Post deletedPost)
        {
            // Lấy thông tin của chủ sở hữu bài viết
            var owner = await _userCollection.Find(u => u.Id == deletedPost.CreatorId).FirstOrDefaultAsync();

            // Kiểm tra xem chủ sở hữu có tồn tại hay không
            if (owner != null)
            {
                // Tạo nội dung thông báo
                var notificationContent = $"Bài viết của bạn có tiêu đề '{deletedPost.title}' đã bị xóa do vi phạm nguyên tắc.";

                // Tạo thông báo
                var notification = new Notification
                {
                    Content = notificationContent,
                    Type = "post_deleted",
                    UserId = deletedPost.CreatorId, // Gửi thông báo cho chủ sở hữu bài viết
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                // Lưu thông báo vào cơ sở dữ liệu
                await _notificationCollection.InsertOneAsync(notification);

                // Đoạn mã để gửi thông báo đến người dùng, ví dụ: gửi email, thông báo trực tiếp trong ứng dụng, v.v.
            }
        }

        [HttpGet("Admin/UserPostAdmin/Search")]
        public async Task<IActionResult> Search(string id)
        {
            Console.WriteLine($"Searching for id: {id}");

            // Kiểm tra xem id được cung cấp hay không
            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("No id provided, returning all posts.");
                var allPosts = await _userPostCollection.Find(_ => true).ToListAsync();
                return View("Index", allPosts);
            }

            Console.WriteLine($"Searching for posts with ID: {id}");
            var postsFound = await _userPostCollection.Find(post => post.id == id).ToListAsync();
            Console.WriteLine($"Found {postsFound.Count} posts.");

            // Trả về view với danh sách bài đăng tìm thấy
            return View("Index", postsFound);
        }

    }
}
