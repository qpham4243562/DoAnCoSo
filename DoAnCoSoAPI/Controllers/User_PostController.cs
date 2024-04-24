using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class User_PostController : ControllerBase
    {
        private readonly IMongoCollection<User_Post>? _user_Post;
        public User_PostController(MongoDbService mongoDbService)
        {
            _user_Post = mongoDbService.Database?.GetCollection<User_Post>("user_Post");
        }
        [HttpGet]
        public async Task<IEnumerable<User_Post>> Get()
        {
            return await _user_Post.Find(FilterDefinition<User_Post>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<User_Post?>> GetById(string id)
        {
            var filter = Builders<User_Post>.Filter.Eq(x => x.id, id);
            var user_Post = _user_Post.Find(filter).FirstOrDefault();
            return user_Post is not null ? Ok(user_Post) : NotFound();
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
            await _user_Post.InsertOneAsync(user_Post);

            return CreatedAtAction(nameof(GetById), new { id = user_Post.id }, user_Post);
        }

        [HttpPut]

        public async Task<ActionResult> Update(User_Post user_Post)
        {
            var filter = Builders<User_Post>.Filter.Eq(x => x.id, user_Post.id);
            //var update = Builders<User_Post>.Update
            //    .Set(x => x.FirstName, user_Post.FirstName)
            //    .Set(x => x.LastName, user_Post.LastName)
            //   .Set(x => x.Email, user_Post.Email)
            //    .Set(x => x.Password, user_Post.PasswordHash)
            //    .Set(x => x.RegisterAt, user_Post.RegisterAt)
            //.Set(x => x.LastLogin, user_Post.LastLogin);
            //  await _user_Post.UpdateOneAsync(filter, update);
            await _user_Post.ReplaceOneAsync(filter, user_Post);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(User_Post user_Post)
        {

            var filter = Builders<User_Post>.Filter.Eq(x => x.id, user_Post.id);
            await _user_Post.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
