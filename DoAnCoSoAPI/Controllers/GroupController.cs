using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IMongoCollection<Group>? _group;
        public GroupController(MongoDbService mongoDbService)
        {
            _group = mongoDbService.Database?.GetCollection<Group>("group");
        }
        [HttpGet]
        public async Task<IEnumerable<Group>> Get()
        {
            return await _group.Find(FilterDefinition<Group>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Group?>> GetById(string id)
        {
            var filter = Builders<Group>.Filter.Eq(x => x.id, id);
            var group = _group.Find(filter).FirstOrDefault();
            return group is not null ? Ok(group) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(Group group)
        {
            await _group.InsertOneAsync(group);
            return CreatedAtAction(nameof(GetById), new { id = group.id }, group);
        }
        [HttpPut]

        public async Task<ActionResult> Update(Group group)
        {
            var filter = Builders<Group>.Filter.Eq(x => x.id, group.id);
            //var update = Builders<Group>.Update
            //    .Set(x => x.FirstName, group.FirstName)
            //    .Set(x => x.LastName, group.LastName)
            //   .Set(x => x.Email, group.Email)
            //    .Set(x => x.Password, group.PasswordHash)
            //    .Set(x => x.RegisterAt, group.RegisterAt)
            //.Set(x => x.LastLogin, group.LastLogin);
            //  await _group.UpdateOneAsync(filter, update);
            await _group.ReplaceOneAsync(filter, group);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(Group group)
        {

            var filter = Builders<Group>.Filter.Eq(x => x.id, group.id);
            await _group.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
