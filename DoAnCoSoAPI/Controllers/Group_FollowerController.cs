using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Group_FollowerController : ControllerBase
    {
        private readonly IMongoCollection<Group_Follower>? _group_Follower;
        public Group_FollowerController(MongoDbService mongoDbService)
        {
            _group_Follower = mongoDbService.Database?.GetCollection<Group_Follower>("group_Follower");
        }
        [HttpGet]
        public async Task<IEnumerable<Group_Follower>> Get()
        {
            return await _group_Follower.Find(FilterDefinition<Group_Follower>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Group_Follower?>> GetById(string id)
        {
            var filter = Builders<Group_Follower>.Filter.Eq(x => x.id, id);
            var group_Follower = _group_Follower.Find(filter).FirstOrDefault();
            return group_Follower is not null ? Ok(group_Follower) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(Group_Follower group_Follower)
        {
            await _group_Follower.InsertOneAsync(group_Follower);
            return CreatedAtAction(nameof(GetById), new { id = group_Follower.id }, group_Follower);
        }
        [HttpPut]

        public async Task<ActionResult> Update(Group_Follower group_Follower)
        {
            var filter = Builders<Group_Follower>.Filter.Eq(x => x.id, group_Follower.id);
            //var update = Builders<Group_Follower>.Update
            //    .Set(x => x.FirstName, group_Follower.FirstName)
            //    .Set(x => x.LastName, group_Follower.LastName)
            //   .Set(x => x.Email, group_Follower.Email)
            //    .Set(x => x.Password, group_Follower.PasswordHash)
            //    .Set(x => x.RegisterAt, group_Follower.RegisterAt)
            //.Set(x => x.LastLogin, group_Follower.LastLogin);
            //  await _group_Follower.UpdateOneAsync(filter, update);
            await _group_Follower.ReplaceOneAsync(filter, group_Follower);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(Group_Follower group_Follower)
        {

            var filter = Builders<Group_Follower>.Filter.Eq(x => x.id, group_Follower.id);
            await _group_Follower.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
