using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace WebApplication2.Controllers
{
    public class userReportController : Controller
    {

        private readonly IMongoCollection<user_report> _userReportCollection;
        private readonly IMongoCollection<User_Post> _userPostCollection;
        public userReportController(IMongoClient mongoClient)
        {
            // Thay thế "your_database_name" và "your_collection_name" với tên database và collection của bạn
            _userReportCollection = mongoClient.GetDatabase("DoAn").GetCollection<user_report>("user_report");
            _userPostCollection = mongoClient.GetDatabase("DoAn").GetCollection<User_Post>("user_Post");
        }
        // Action để hiển thị form tạo báo cáo cho bài viết
        [HttpGet("Create/{postId}")]
        public IActionResult Create(string postId)
        {
            // Trả về view với postId đã được truyền vào
            return View();
        }

        [HttpPost("Create/{postId}")]
        public async Task<IActionResult> Create(user_report report, string postId)
        {
            // Thực hiện kiểm tra và xử lý việc lưu báo cáo vào cơ sở dữ liệu
            try
            {
                // Gán postId cho report
                report.PostId = postId;

                // Thêm báo cáo vào collection
                await _userReportCollection.InsertOneAsync(report);

                // Điều hướng về trang chủ hoặc trang khác tùy bạn
                return RedirectToAction("Index", "UserPost");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");

                // Trả về view tạo báo cáo với dữ liệu đã nhập trước đó
                return View(report);
            }
        }


    }
}
