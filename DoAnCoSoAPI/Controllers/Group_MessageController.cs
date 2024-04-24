using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Group_MessageController : ControllerBase
    {
        private readonly IMongoCollection<Group_Message>? _group_Message;
        public Group_MessageController(MongoDbService mongoDbService)
        {
            _group_Message = mongoDbService.Database?.GetCollection<Group_Message>("group_Message");
        }
        [HttpGet]
        public async Task<IEnumerable<Group_Message>> Get()
        {
            return await _group_Message.Find(FilterDefinition<Group_Message>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Group_Message?>> GetById(string id)
        {
            var filter = Builders<Group_Message>.Filter.Eq(x => x.id, id);
            var group_Message = _group_Message.Find(filter).FirstOrDefault();
            return group_Message is not null ? Ok(group_Message) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(Group_Message group_Message)
        {
            await _group_Message.InsertOneAsync(group_Message);
            return CreatedAtAction(nameof(GetById), new { id = group_Message.id }, group_Message);
        }
        [HttpPut]

        public async Task<ActionResult> Update(Group_Message group_Message)
        {
            var filter = Builders<Group_Message>.Filter.Eq(x => x.id, group_Message.id);
            //var update = Builders<Group_Message>.Update
            //    .Set(x => x.FirstName, group_Message.FirstName)
            //    .Set(x => x.LastName, group_Message.LastName)
            //   .Set(x => x.Email, group_Message.Email)
            //    .Set(x => x.Password, group_Message.PasswordHash)
            //    .Set(x => x.RegisterAt, group_Message.RegisterAt)
            //.Set(x => x.LastLogin, group_Message.LastLogin);
            //  await _group_Message.UpdateOneAsync(filter, update);
            await _group_Message.ReplaceOneAsync(filter, group_Message);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(Group_Message group_Message)
        {

            var filter = Builders<Group_Message>.Filter.Eq(x => x.id, group_Message.id);
            await _group_Message.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
