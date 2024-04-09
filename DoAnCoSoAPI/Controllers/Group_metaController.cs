using DoAnCoSoAPI.Data;
using DoAnCoSoAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DoAnCoSoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Group_metaController : ControllerBase
    {
        private readonly IMongoCollection<Group_meta>? _group_meta;
        public Group_metaController(MongoDbService mongoDbService)
        {
            _group_meta = mongoDbService.Database?.GetCollection<Group_meta>("group_meta");
        }
        [HttpGet]
        public async Task<IEnumerable<Group_meta>> Get()
        {
            return await _group_meta.Find(FilterDefinition<Group_meta>.Empty).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Group_meta?>> GetById(string id)
        {
            var filter = Builders<Group_meta>.Filter.Eq(x => x.id, id);
            var group_meta = _group_meta.Find(filter).FirstOrDefault();
            return group_meta is not null ? Ok(group_meta) : NotFound();
        }
        [HttpPost]

        public async Task<ActionResult> Create(Group_meta group_meta)
        {
            await _group_meta.InsertOneAsync(group_meta);
            return CreatedAtAction(nameof(GetById), new { id = group_meta.id }, group_meta);
        }
        [HttpPut]

        public async Task<ActionResult> Update(Group_meta group_meta)
        {
            var filter = Builders<Group_meta>.Filter.Eq(x => x.id, group_meta.id);
            //var update = Builders<Group_meta>.Update
            //    .Set(x => x.FirstName, group_meta.FirstName)
            //    .Set(x => x.LastName, group_meta.LastName)
            //   .Set(x => x.Email, group_meta.Email)
            //    .Set(x => x.Password, group_meta.PasswordHash)
            //    .Set(x => x.RegisterAt, group_meta.RegisterAt)
            //.Set(x => x.LastLogin, group_meta.LastLogin);
            //  await _group_meta.UpdateOneAsync(filter, update);
            await _group_meta.ReplaceOneAsync(filter, group_meta);
            return Ok();
        }
        [HttpDelete]

        public async Task<ActionResult> Delete(Group_meta group_meta)
        {

            var filter = Builders<Group_meta>.Filter.Eq(x => x.id, group_meta.id);
            await _group_meta.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
