using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Security.Claims;

namespace WebApplication2.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Notification> _notificationCollection;
        public NotificationController(IMongoClient mongoClient)
        {
            _userCollection = mongoClient.GetDatabase("DoAn").GetCollection<User>("user");
            _notificationCollection = mongoClient.GetDatabase("DoAn").GetCollection<Notification>("notification");

        }
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("Không thể xác định người dùng hiện tại.");
            }

            List<Notification> notificationList;
            try
            {
                // Lấy tất cả các thông báo của người dùng hiện tại từ collection và sắp xếp chúng theo thời gian tạo giảm dần
                notificationList = await _notificationCollection.Find(n => n.UserId == currentUserId && !n.IsRead)
                                                                 .SortByDescending(n => n.CreatedAt)
                                                                 .ToListAsync();
                var unreadCount = await CountUnreadNotifications(currentUserId);

                // Truyền số lượng thông báo chưa đọc vào view thông qua ViewBag hoặc Model
                ViewBag.UnreadCount = unreadCount;
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ khi truy vấn MongoDB
                ModelState.AddModelError(string.Empty, $"An error occurred while fetching notifications from MongoDB: {ex.Message}");
                notificationList = new List<Notification>(); // Khởi tạo danh sách rỗng để tránh lỗi
            }

            return View(notificationList);
        }
        public async Task<int> CountUnreadNotifications(string userId)
        {
            try
            {
                var count = await _notificationCollection.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);
                return (int)count;
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ khi truy vấn MongoDB
                // Trong trường hợp xảy ra lỗi, trả về -1 hoặc làm một xử lý phù hợp với ứng dụng của bạn
                return -1;
            }
        }

        [HttpGet("mark-notification-read/{id}")]
        public async Task<IActionResult> MarkNotificationRead(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid notification ID");
            }

            try
            {
                var filter = Builders<Notification>.Filter.Eq(n => n.id, id);
                var update = Builders<Notification>.Update.Set(n => n.IsRead, true);

                var updateResult = await _notificationCollection.UpdateOneAsync(filter, update);

                if (updateResult.MatchedCount == 0)
                {
                    return NotFound("Notification not found");
                }

                return Ok("Notification marked as read successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Log the exception for debugging
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("Không thể xác định người dùng hiện tại.");
            }

            // Lấy số lượng thông báo chưa đọc từ cơ sở dữ liệu
            var unreadCount = await CountUnreadNotifications(currentUserId);

            return Ok(unreadCount);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
