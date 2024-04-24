using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class User_MessageController : ControllerBase
    {
        private readonly IMongoCollection<User_Message>? _user_Message;
        public User_MessageController(MongoDbService mongoDbService)
        {
            _user_Message = mongoDbService.Database?.GetCollection<User_Message>("user_Message");
        }
        [HttpGet]
        public async Task<IEnumerable<User_Message>> Get()
        {
            return await _user_Message.Find(FilterDefinition<User_Message>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<User_Message?>> GetById(string id)
        {
            var filter = Builders<User_Message>.Filter.Eq(x => x.id, id);
            var user_Message = _user_Message.Find(filter).FirstOrDefault();
            return user_Message is not null ? Ok(user_Message) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(User_Message user_Message)
        {
            await _user_Message.InsertOneAsync(user_Message);
            return CreatedAtAction(nameof(GetById), new { id = user_Message.id }, user_Message);
        }
        [HttpPut]

        public async Task<ActionResult> Update(User_Message user_Message)
        {
            var filter = Builders<User_Message>.Filter.Eq(x => x.id, user_Message.id);
            //var update = Builders<User_Message>.Update
            //    .Set(x => x.FirstName, user_Message.FirstName)
            //    .Set(x => x.LastName, user_Message.LastName)
            //   .Set(x => x.Email, user_Message.Email)
            //    .Set(x => x.Password, user_Message.PasswordHash)
            //    .Set(x => x.RegisterAt, user_Message.RegisterAt)
            //.Set(x => x.LastLogin, user_Message.LastLogin);
            //  await _user_Message.UpdateOneAsync(filter, update);
            await _user_Message.ReplaceOneAsync(filter, user_Message);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(User_Message user_Message)
        {

            var filter = Builders<User_Message>.Filter.Eq(x => x.id, user_Message.id);
            await _user_Message.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
