using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    public class CommentController : Controller
    {
        private readonly IMongoCollection<User_comment> _userCommentCollection;
        private readonly IMongoCollection<User_Post> _userPostCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Notification> _notificationCollection;

        public CommentController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userCommentCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_comment>("user_Comment");
            _userPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Post>("user_Post");
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
            _notificationCollection = mongoClient.GetDatabase("DoAn").GetCollection<Notification>("notification");
        }
        [HttpGet]
        public async Task<IActionResult> Index(string postId)
        {
            try
            {
                // Lấy danh sách comment của bài đăng có postId tương ứng
        var comments = await _userCommentCollection.Find(c => c.id == postId).ToListAsync();
        
        // Truyền postId vào view
        ViewBag.PostId = postId;
                // Lấy bài viết từ cơ sở dữ liệu dựa trên postId
                var post = await _userPostCollection.Find(p => p.id == postId).FirstOrDefaultAsync();

                if (post == null)
                {
                    return NotFound(); // Trả về NotFound nếu không tìm thấy bài viết
                }

                // Truyền danh sách comment của bài viết vào view
                return View(post.Comments);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                ModelState.AddModelError(string.Empty, $"An error occurred while fetching comments: {ex.Message}");
                return View(new List<User_comment>()); // Trả về view với danh sách rỗng nếu có lỗi
            }
        }


        [HttpGet("AddComment/{postId}")]
        public async Task<IActionResult> AddComment(string postId)
        {
            var newComment = new User_comment();

            // Truy vấn danh sách các comment của bài viết từ cơ sở dữ liệu
            var post = await _userPostCollection.Find(p => p.id == postId).FirstOrDefaultAsync();
            if (post != null)
            {
                // Truyền danh sách các comment vào view cùng với newComment để người dùng có thể nhập thông tin cho comment mới
                ViewBag.Comments = post.Comments;
                return View(newComment);

            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost("AddComment/{postId}")]
        public async Task<IActionResult> AddComment(string postId, User_comment newComment)
        {
            try
            {
                // Tìm bài viết dựa trên postId
                var post = await _userPostCollection.Find(p => p.id == postId).FirstOrDefaultAsync();

                if (post == null)
                {
                    return NotFound(); // Trả về NotFound nếu không tìm thấy bài viết
                }

                // Lấy thông tin người dùng từ HttpContext
                var userName = HttpContext.User.Identity.Name;
                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                newComment.id = ObjectId.GenerateNewId().ToString();
                // Gán CreatorId cho newComment
                newComment.CreatorId = currentUserId;
                // Thêm thông tin của người dùng vào comment
                newComment.UserName = userName;
                newComment.createdAt = DateTime.UtcNow;

                // Thêm comment mới vào danh sách comment của bài viết
                if (post.Comments == null)
                {
                    post.Comments = new List<User_comment>();
                }
                post.Comments.Add(newComment);
                var notification = new Notification
                {
                    UserId = post.CreatorId, // Id của người đăng bài
                    Content = $"{HttpContext.User.Identity.Name} đã bình luận bài đăng của bạn.",
                    Type = "comment",
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
            
                // Cập nhật bài viết trong cơ sở dữ liệu
                await _userPostCollection.ReplaceOneAsync(p => p.id == postId, post);

                return RedirectToAction("Index", "UserPost"); // Chuyển hướng đến trang chính sau khi thêm comment thành công
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                ModelState.AddModelError(string.Empty, $"An error occurred while adding comment: {ex.Message}");
                return View(); // Trả về view với thông báo lỗi
            }
        }



        [HttpPost("DeleteComment/{postId}/{commentId}")]
        public async Task<IActionResult> DeleteComment(string postId, string commentId)
        {
            try
            {
                // Tìm bài viết dựa trên postId
                var post = await _userPostCollection.Find(p => p.id == postId).FirstOrDefaultAsync();

                if (post == null)
                {
                    return NotFound(); // Trả về NotFound nếu không tìm thấy bài viết
                }

                // Lấy ID của người dùng hiện tại từ HttpContext
                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Tìm comment cần xóa
                var comment = post.Comments.FirstOrDefault(c => c.id == commentId);

                if (comment == null)
                {
                    return NotFound(); // Trả về NotFound nếu không tìm thấy comment
                }

                // Kiểm tra xem người dùng hiện tại có phải là người tạo comment hay không
                if (comment.CreatorId != currentUserId)
                {
                    return Forbid(); // Trả về Forbid nếu người dùng không có quyền xóa comment
                }

                // Xóa comment khỏi danh sách comment
                post.Comments.Remove(comment);

                // Cập nhật bài viết trong cơ sở dữ liệu
                await _userPostCollection.ReplaceOneAsync(p => p.id == postId, post);

                TempData["SuccessMessage"] = "Comment deleted successfully"; // Lưu thông báo xóa thành công vào TempData

                return Ok(); // Trả về 200 OK để JavaScript biết rằng việc xóa comment đã thành công
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
