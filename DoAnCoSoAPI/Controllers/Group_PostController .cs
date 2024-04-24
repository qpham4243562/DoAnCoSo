using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Group_PostController : ControllerBase
    {
        private readonly IMongoCollection<Group_Post>? _group_Post;
        public Group_PostController(MongoDbService mongoDbService)
        {
            _group_Post = mongoDbService.Database?.GetCollection<Group_Post>("group_Post");
        }
        [HttpGet]
        public async Task<IEnumerable<Group_Post>> Get()
        {
            return await _group_Post.Find(FilterDefinition<Group_Post>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Group_Post?>> GetById(string id)
        {
            var filter = Builders<Group_Post>.Filter.Eq(x => x.id, id);
            var group_Post = _group_Post.Find(filter).FirstOrDefault();
            return group_Post is not null ? Ok(group_Post) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(Group_Post group_Post)
        {
            await _group_Post.InsertOneAsync(group_Post);
            return CreatedAtAction(nameof(GetById), new { id = group_Post.id }, group_Post);
        }
        [HttpPut]

        public async Task<ActionResult> Update(Group_Post group_Post)
        {
            var filter = Builders<Group_Post>.Filter.Eq(x => x.id, group_Post.id);
            //var update = Builders<Group_Post>.Update
            //    .Set(x => x.FirstName, group_Post.FirstName)
            //    .Set(x => x.LastName, group_Post.LastName)
            //   .Set(x => x.Email, group_Post.Email)
            //    .Set(x => x.Password, group_Post.PasswordHash)
            //    .Set(x => x.RegisterAt, group_Post.RegisterAt)
            //.Set(x => x.LastLogin, group_Post.LastLogin);
            //  await _group_Post.UpdateOneAsync(filter, update);
            await _group_Post.ReplaceOneAsync(filter, group_Post);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(Group_Post group_Post)
        {

            var filter = Builders<Group_Post>.Filter.Eq(x => x.id, group_Post.id);
            await _group_Post.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
