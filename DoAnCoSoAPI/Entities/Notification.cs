using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DoAnCoSoAPI.Entities
{
    public class Notification
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [BsonElement("UserId"), BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }
        [BsonElement("Content"), BsonRepresentation(BsonType.String)]

        public string Content { get; set; }

        [BsonElement("Type"), BsonRepresentation(BsonType.String)]
        public string Type { get; set; }
        [BsonElement("PostId"), BsonRepresentation(BsonType.String)]
        public string PostId { get; set; }
        [BsonElement("CreatedAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; }
        [BsonElement("IsRead"), BsonRepresentation(BsonType.Boolean)]
        public bool IsRead { get; set; }

    }
}
