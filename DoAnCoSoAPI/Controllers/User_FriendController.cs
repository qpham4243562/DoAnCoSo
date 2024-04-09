using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class User_FriendController : ControllerBase
    {
        private readonly IMongoCollection<User_Friend>? _user_Friend;
        public User_FriendController(MongoDbService mongoDbService)
        {
            _user_Friend = mongoDbService.Database?.GetCollection<User_Friend>("user_Friend");
        }
        [HttpGet]
        public async Task<IEnumerable<User_Friend>> Get()
        {
            return await _user_Friend.Find(FilterDefinition<User_Friend>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<User_Friend?>> GetById(string id)
        {
            var filter = Builders<User_Friend>.Filter.Eq(x => x.id, id);
            var user_Friend = _user_Friend.Find(filter).FirstOrDefault();
            return user_Friend is not null ? Ok(user_Friend) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(User_Friend user_Friend)
        {
            await _user_Friend.InsertOneAsync(user_Friend);
            return CreatedAtAction(nameof(GetById), new { id = user_Friend.id }, user_Friend);
        }
        [HttpPut]

        public async Task<ActionResult> Update(User_Friend user_Friend)
        {
            var filter = Builders<User_Friend>.Filter.Eq(x => x.id, user_Friend.id);
            //var update = Builders<User_Friend>.Update
            //    .Set(x => x.FirstName, user_Friend.FirstName)
            //    .Set(x => x.LastName, user_Friend.LastName)
            //   .Set(x => x.Email, user_Friend.Email)
            //    .Set(x => x.Password, user_Friend.PasswordHash)
            //    .Set(x => x.RegisterAt, user_Friend.RegisterAt)
            //.Set(x => x.LastLogin, user_Friend.LastLogin);
            //  await _user_Friend.UpdateOneAsync(filter, update);
            await _user_Friend.ReplaceOneAsync(filter, user_Friend);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(User_Friend user_Friend)
        {

            var filter = Builders<User_Friend>.Filter.Eq(x => x.id, user_Friend.id);
            await _user_Friend.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
