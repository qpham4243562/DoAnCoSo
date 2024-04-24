using MongoDB.Bson.Serialization.Attributes;

namespace DoAnCoSoAPI.Entities
{
    public class LikedPost
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string PostId { get; set; }
    }
}
