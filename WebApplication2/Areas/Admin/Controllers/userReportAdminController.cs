using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace WebApplication2.Areas.Admin.Controllers
{
    [Area("admin")]
    public class userReportAdminController : Controller
    {
        private readonly IMongoCollection<user_report> _userReportCollection;
        private readonly IMongoCollection<User_Post> _userPostCollection;
        public userReportAdminController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userReportCollection = mongoClient.GetDatabase("DoAn").GetCollection<user_report>("user_report");
            _userPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Post>("user_Post");
        }
        public async Task<IActionResult> Index()
        {
            List<user_report> userReports;
            try
            {
                // Lấy tất cả các báo cáo người dùng từ collection
                userReports = await _userReportCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ khi truy vấn MongoDB
                ModelState.AddModelError(string.Empty, $"An error occurred while fetching user reports: {ex.Message}");
                userReports = new List<user_report>(); // Khởi tạo danh sách rỗng để tránh lỗi
            }

            return View(userReports);
        }
        [HttpGet("Admin/UserReportAdmin/Search")]
        public async Task<IActionResult> Search(string id)
        {
            Console.WriteLine($"Searching for id: {id}");

            // Kiểm tra xem id được cung cấp hay không
            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("No id provided, returning all posts.");
                var allPosts = await _userReportCollection.Find(_ => true).ToListAsync();
                return View("Index", allPosts);
            }

            Console.WriteLine($"Searching for posts with ID: {id}");
            var postsFound = await _userReportCollection.Find(post => post.PostId == id).ToListAsync();
            Console.WriteLine($"Found {postsFound.Count} posts.");

            // Trả về view với danh sách bài đăng tìm thấy
            return View("Index", postsFound);
        }
        [HttpGet("Admin/userReportAdmin/Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // Tìm người dùng theo id và hiển thị trang xác nhận xóa
            var user = await _userReportCollection.Find(u => u.id== id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost("Admin/userReportAdmin/Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                // Xóa báo cáo người dùng từ collection dựa trên ID
                var result = await _userReportCollection.DeleteOneAsync(report => report.id == id);
                if (result.DeletedCount == 0)
                {
                    // Trả về NotFound nếu không tìm thấy báo cáo
                    return NotFound();
                }
                // Trả về Ok nếu xóa thành công
                return RedirectToAction("Index", "userReportAdmin");
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                return BadRequest($"An error occurred while deleting user report: {ex.Message}");
            }
        }
    }
}
