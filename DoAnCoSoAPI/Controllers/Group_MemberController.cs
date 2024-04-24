using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Group_MemberController : ControllerBase
    {
        private readonly IMongoCollection<Group_Member>? _group_Member;
        public Group_MemberController(MongoDbService mongoDbService)
        {
            _group_Member = mongoDbService.Database?.GetCollection<Group_Member>("group_Member");
        }
        [HttpGet]
        public async Task<IEnumerable<Group_Member>> Get()
        {
            return await _group_Member.Find(FilterDefinition<Group_Member>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Group_Member?>> GetById(string id)
        {
            var filter = Builders<Group_Member>.Filter.Eq(x => x.id, id);
            var group_Member = _group_Member.Find(filter).FirstOrDefault();
            return group_Member is not null ? Ok(group_Member) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(Group_Member group_Member)
        {
            await _group_Member.InsertOneAsync(group_Member);
            return CreatedAtAction(nameof(GetById), new { id = group_Member.id }, group_Member);
        }
        [HttpPut]

        public async Task<ActionResult> Update(Group_Member group_Member)
        {
            var filter = Builders<Group_Member>.Filter.Eq(x => x.id, group_Member.id);
            //var update = Builders<Group_Member>.Update
            //    .Set(x => x.FirstName, group_Member.FirstName)
            //    .Set(x => x.LastName, group_Member.LastName)
            //   .Set(x => x.Email, group_Member.Email)
            //    .Set(x => x.Password, group_Member.PasswordHash)
            //    .Set(x => x.RegisterAt, group_Member.RegisterAt)
            //.Set(x => x.LastLogin, group_Member.LastLogin);
            //  await _group_Member.UpdateOneAsync(filter, update);
            await _group_Member.ReplaceOneAsync(filter, group_Member);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(Group_Member group_Member)
        {

            var filter = Builders<Group_Member>.Filter.Eq(x => x.id, group_Member.id);
            await _group_Member.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
