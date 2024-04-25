using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DoAnCoSoAPI.Entities
{
    public class user_report
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
        [BsonElement("PostId"), BsonRepresentation(BsonType.String)]
        public string? PostId { set; get; }
        [BsonElement("Reason"), BsonRepresentation(BsonType.String)]
        public string? Reason { set; get; }
    }
}
