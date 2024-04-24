using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class User_FollowerController : ControllerBase
    {
        private readonly IMongoCollection<User_Follower>? _user_Follower;
        public User_FollowerController(MongoDbService mongoDbService)
        {
            _user_Follower = mongoDbService.Database?.GetCollection<User_Follower>("user_Follower");
        }
        [HttpGet]
        public async Task<IEnumerable<User_Follower>> Get()
        {
            return await _user_Follower.Find(FilterDefinition<User_Follower>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<User_Follower?>> GetById(string id)
        {
            var filter = Builders<User_Follower>.Filter.Eq(x => x.id, id);
            var user_Follower = _user_Follower.Find(filter).FirstOrDefault();
            return user_Follower is not null ? Ok(user_Follower) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(User_Follower user_Follower)
        {
            await _user_Follower.InsertOneAsync(user_Follower);
            return CreatedAtAction(nameof(GetById), new { id = user_Follower.id }, user_Follower);
        }
        [HttpPut]

        public async Task<ActionResult> Update(User_Follower user_Follower)
        {
            var filter = Builders<User_Follower>.Filter.Eq(x => x.id, user_Follower.id);
            //var update = Builders<User_Follower>.Update
            //    .Set(x => x.FirstName, user_Follower.FirstName)
            //    .Set(x => x.LastName, user_Follower.LastName)
            //   .Set(x => x.Email, user_Follower.Email)
            //    .Set(x => x.Password, user_Follower.PasswordHash)
            //    .Set(x => x.RegisterAt, user_Follower.RegisterAt)
            //.Set(x => x.LastLogin, user_Follower.LastLogin);
            //  await _user_Follower.UpdateOneAsync(filter, update);
            await _user_Follower.ReplaceOneAsync(filter, user_Follower);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(User_Follower user_Follower)
        {

            var filter = Builders<User_Follower>.Filter.Eq(x => x.id, user_Follower.id);
            await _user_Follower.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
