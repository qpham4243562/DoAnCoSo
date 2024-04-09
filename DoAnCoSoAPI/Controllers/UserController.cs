using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User>? _user;
        public UserController(MongoDbService mongoDbService)
        {
            _user = mongoDbService.Database?.GetCollection<User>("user");
        }
        [HttpGet]
        public async Task<IEnumerable<User>> Get()
        {
            return await _user.Find(FilterDefinition<User>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<User?>> GetById(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var user = _user.Find(filter).FirstOrDefault();
            return user is not null ? Ok(user) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(User user)
        {
            await _user.InsertOneAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        [HttpPut]

        public async Task<ActionResult> Update(User user)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);
            //var update = Builders<User>.Update
            //    .Set(x => x.FirstName, user.FirstName)
            //    .Set(x => x.LastName, user.LastName)
             //   .Set(x => x.Email, user.Email)
            //    .Set(x => x.Password, user.PasswordHash)
            //    .Set(x => x.RegisterAt, user.RegisterAt)
            //.Set(x => x.LastLogin, user.LastLogin);
          //  await _user.UpdateOneAsync(filter, update);
          await _user.ReplaceOneAsync(filter, user);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(User user)
        {

            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);
            await _user.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
