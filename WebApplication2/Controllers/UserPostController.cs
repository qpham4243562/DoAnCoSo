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
            user_Post.count = 0;

            // Add information about the creator
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            user_Post.CreatorId = userId;
            var creator = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (creator != null)
            {
                user_Post.CreatorName = $"{creator.lastName} {creator.firstName}";
                user_Post.CreatorAvatar = creator.images; // Assuming 'Avatar' is the property storing the user's avatar image
            }
            else
            {
                // Handle the case where creator information is not found
                // For example: Set a default value or display an error message
                user_Post.CreatorName = "Unknown";
                user_Post.CreatorAvatar = null; // Set a default avatar image if necessary
            }

            // Save user post to the database
            await _userPostCollection.InsertOneAsync(user_Post);

            // Update the user's list of posts
            var updateDefinition = Builders<User>.Update.AddToSet(u => u.UserPosts, user_Post.id);
            var updateResult = await _userCollection.UpdateOneAsync(u => u.Id == userId && u.UserPosts != null, updateDefinition);

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
            // Lấy userId từ cookie đăng nhập
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra xem ID của bài đăng có hợp lệ hay không
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
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

            // Kiểm tra xem bài đăng có thuộc về người dùng hiện tại không
            if (postToDelete.CreatorId != userId)
            {
                // Nếu không phải bài đăng của người dùng hiện tại, chỉ xóa khỏi UserPosts
                // và chuyển hướng về trang chủ hoặc trang danh sách bài đăng
                var updateResult = await _userCollection.UpdateOneAsync(u => u.Id == userId, Builders<User>.Update.Pull(u => u.UserPosts, id));

                if (updateResult.ModifiedCount == 0)
                {
                    return NotFound();
                }

                return RedirectToAction("Index", "user");
            }

            // Nếu bài đăng là của người dùng hiện tại, hiển thị view xác nhận xóa bài đăng
            return View(postToDelete);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // Thêm xác thực CSRF
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            // Lấy userId từ cookie đăng nhập
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra xem ID của bài đăng có hợp lệ hay không
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
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

            // Kiểm tra xem bài đăng có thuộc về người dùng hiện tại không
            if (postToDelete.CreatorId != userId)
            {
                // Nếu không phải bài đăng của người dùng hiện tại, chỉ xóa khỏi UserPosts
                // và chuyển hướng về trang chủ hoặc trang danh sách bài đăng
                var updateResultUser = await _userCollection.UpdateOneAsync(u => u.Id == userId, Builders<User>.Update.Pull(u => u.UserPosts, id));

                if (updateResultUser.ModifiedCount == 0)
                {
                    return NotFound();
                }

                return RedirectToAction("Index", "user");
            }

            // Xóa bài đăng từ cơ sở dữ liệu
            var deleteResult = await _userPostCollection.DeleteOneAsync(post => post.id == id);

            // Nếu không tìm thấy bài đăng để xóa, trả về NotFound
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }

            // Xóa bài đăng khỏi danh sách UserPosts của người dùng
            var updateResultSelf = await _userCollection.UpdateOneAsync(u => u.Id == userId, Builders<User>.Update.Pull(u => u.UserPosts, id));

            if (updateResultSelf.ModifiedCount == 0)
            {
                return NotFound();
            }

            // Chuyển hướng về trang chủ hoặc trang danh sách bài đăng
            return RedirectToAction("Index", "user");
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
        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            List<User_Post> searchResults;
            var filter = Builders<User_Post>.Filter.Or(
                Builders<User_Post>.Filter.Regex("title", new BsonRegularExpression(searchString, "i")),
                Builders<User_Post>.Filter.Regex("content", new BsonRegularExpression(searchString, "i"))
            );

            try
            {
                searchResults = await _userPostCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while searching: {ex.Message}");
                searchResults = new List<User_Post>();
            }

            return View(searchResults);
        }
        [HttpGet]
        public async Task<IActionResult> PersonalPage()
        {
            // Lấy userId từ cookie đăng nhập
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in cookie");
            }

            // Tìm người dùng trong cơ sở dữ liệu dựa trên userId
            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Kiểm tra user trước khi truy cập UserPosts
            if (user.UserPosts != null)
            {
                // Lấy danh sách postId của người dùng
                var postIds = user.UserPosts;

                // Kiểm tra postIds trước khi truy vấn bài đăng
                if (postIds != null && postIds.Any())
                {
                    // Tìm các bài đăng từ danh sách postId
                    var userPosts = await _userPostCollection.Find(post => postIds.Contains(post.id)).ToListAsync();

                    return View(userPosts);
                }
                else
                {
                    // Xử lý trường hợp postIds không tồn tại hoặc không có bài đăng nào
                    return View(new List<User_Post>()); // Trả về một danh sách trống hoặc xử lý theo nhu cầu của bạn
                }
            }
            else
            {
                // Xử lý trường hợp UserPosts không tồn tại
                return View(new List<User_Post>()); // Trả về một danh sách trống hoặc xử lý theo nhu cầu của bạn
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUserPosts(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }

            // Tìm người dùng trong cơ sở dữ liệu dựa trên userId
            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Kiểm tra user trước khi truy cập UserPosts
            if (user.UserPosts != null)
            {
                // Lấy danh sách postId của người dùng
                var postIds = user.UserPosts;

                // Kiểm tra postIds trước khi truy vấn bài đăng
                if (postIds != null && postIds.Any())
                {
                    // Tìm các bài đăng từ danh sách postId
                    var userPosts = await _userPostCollection.Find(post => postIds.Contains(post.id)).ToListAsync();

                    return View(userPosts);
                }
                else
                {
                    // Xử lý trường hợp postIds không tồn tại hoặc không có bài đăng nào
                    return View(new List<User_Post>()); // Trả về một danh sách trống hoặc xử lý theo nhu cầu của bạn
                }
            }
            else
            {
                // Xử lý trường hợp UserPosts không tồn tại
                return View(new List<User_Post>()); // Trả về một danh sách trống hoặc xử lý theo nhu cầu của bạn
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePost(string postId)
        {
            // Lấy userId từ cookie đăng nhập
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in cookie");
            }

            // Tìm người dùng trong cơ sở dữ liệu dựa trên userId
            var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Kiểm tra xem postId đã được lưu bởi người dùng chưa
            if (!user.UserPosts.Contains(postId))
            {
                // Thêm postId vào danh sách UserPosts của người dùng
                var updateDefinition = Builders<User>.Update.Push(u => u.UserPosts, postId);
                var updateResult = await _userCollection.UpdateOneAsync(u => u.Id == userId, updateDefinition);

                // Kiểm tra kết quả cập nhật
                if (updateResult.ModifiedCount == 1)
                {
                    return Ok("Post saved successfully");
                }
                else
                {
                    return Ok("Failed to save post");
                }
            }
            else
            {
                // Nếu bài đăng đã được lưu trước đó, trả về thông báo tương ứng
                return Ok("Post already saved");
            }
        }
        [HttpGet]
        public async Task<IActionResult> CombinedSearch(string searchString)
        {
            List<User> userSearchResults = new List<User>();
            List<User_Post> postSearchResults = new List<User_Post>();

            // Phân tách chuỗi tìm kiếm thành các từ riêng biệt
            var searchTerms = searchString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Tạo bộ lọc cho mỗi từ tìm kiếm
            var filterBuilder = Builders<User>.Filter;
            var userFilters = new List<FilterDefinition<User>>();
            var postFilters = new List<FilterDefinition<User_Post>>();

            foreach (var term in searchTerms)
            {
                var regexFilter = new BsonRegularExpression(term, "i");
                var userFilter = filterBuilder.Or(
                    filterBuilder.Regex("lastName", regexFilter),
                    filterBuilder.Regex("firstName", regexFilter),
                    filterBuilder.Regex("eMail", regexFilter)
                );
                userFilters.Add(userFilter);

                var postFilter = Builders<User_Post>.Filter.Or(
                    Builders<User_Post>.Filter.Regex("title", regexFilter),
                    Builders<User_Post>.Filter.Regex("content", regexFilter)
                );
                postFilters.Add(postFilter);
            }

            // Kết hợp tất cả các bộ lọc
            var combinedUserFilter = filterBuilder.And(userFilters);
            var combinedPostFilter = Builders<User_Post>.Filter.Or(postFilters);

            try
            {
                // Thực hiện truy vấn với bộ lọc đã tạo
                userSearchResults = await _userCollection.Find(combinedUserFilter).ToListAsync();
                postSearchResults = await _userPostCollection.Find(combinedPostFilter).ToListAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while searching: {ex.Message}");
            }

            // Trả về kết quả tìm kiếm kết hợp
            var combinedResults = new List<CombinedSearchResultsViewModel>
            {
                new CombinedSearchResultsViewModel { Users = userSearchResults, Posts = postSearchResults }
            };

            // Trả về kết quả tìm kiếm kết hợp
            return View("CombinedSearch", combinedResults);
        }

    }
}
