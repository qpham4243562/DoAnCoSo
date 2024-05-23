using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
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
        private readonly IMongoCollection<Notification> _notificationCollection;
        public UserPostAdminController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Post>("user_Post");
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
            _likedPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<LikedPost>("LikedPosts");
            _notificationCollection = mongoClient.GetDatabase("DoAn").GetCollection<Notification>("notification");
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user1 = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user1 == null || user1.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }
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
        public async Task<IActionResult> DownloadFile(string id, int fileIndex)
        {
            var post = await _userPostCollection.Find(p => p.id == id).FirstOrDefaultAsync();

            if (post == null || fileIndex < 0 || fileIndex >= post.Files.Count)
            {
                return NotFound();
            }

            var fileBytes = post.Files[fileIndex];
            var fileExtension = Path.GetExtension(post.FileNames[fileIndex]); // Sử dụng tên tệp tin gốc

            string contentType;
            switch (fileExtension.ToLower())
            {
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                default:
                    contentType = "application/octet-stream";
                    break;
            }

            return File(fileBytes, contentType, post.FileNames[fileIndex]); // Sử dụng tên tệp tin gốc
        }
        [HttpGet]
        public async Task<IActionResult> ApprovedPosts()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user1 = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user1 == null || user1.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }

            List<User_Post> postList;
            try
            {
                // Lấy tất cả các User_Post từ collection và sắp xếp theo thời gian tạo giảm dần
                postList = await _userPostCollection.Find(post => !post.IsApproved)
                                                     .SortByDescending(post => post.createdAt)
                                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ khi truy vấn MongoDB
                ModelState.AddModelError(string.Empty, $"An error occurred while fetching user posts from MongoDB: {ex.Message}");
                postList = new List<User_Post>(); // Khởi tạo danh sách rỗng để tránh lỗi
                                                  // Trả về một ActionResult hoặc một View để hiển thị thông báo lỗi cho người dùng
                return View("Error"); // Ví dụ: trả về một view để hiển thị thông báo lỗi
            }

            // Trả về danh sách bài viết đã được duyệt
            return View(postList);
        }
        [HttpPost]
        public async Task<IActionResult> ApprovePost(string id)
        {
            // Lấy bài viết cần duyệt từ cơ sở dữ liệu
            var postToApprove = await _userPostCollection.Find(post => post.id == id).FirstOrDefaultAsync();

            // Kiểm tra xem bài viết có tồn tại hay không
            if (postToApprove == null)
            {
                return NotFound();
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
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user1 = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user1 == null || user1.role != "admin")
            {
                return BadRequest("Không phải Admin");
            }
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

            // Lấy thông tin của bài đăng cần xóa
            var postToDelete = await _userPostCollection.Find(post => post.id == id).FirstOrDefaultAsync();

            // Kiểm tra xem bài đăng có tồn tại hay không
            if (postToDelete == null)
            {
                return NotFound();
            }

            // Xóa bài đăng từ cơ sở dữ liệu
            var result = await _userPostCollection.DeleteOneAsync(post => post.id == id);

            // Kiểm tra xem bài đăng đã được xóa thành công hay không
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            // Gửi thông báo cho chủ sở hữu bài viết đã bị xóa
            await SendNotificationToPostOwner(postToDelete);

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
