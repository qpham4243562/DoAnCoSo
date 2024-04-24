using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DoAnCoSoAPI.Entities
{
    public class User_comment
    {

        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
        [BsonElement("creatorId")]
        public string CreatorId { get; set; }
        [BsonElement("UserName"), BsonRepresentation(BsonType.String)]
        public string UserName { get; set; } // ID của người dùng đăng comment
        [BsonElement("comment"), BsonRepresentation(BsonType.String)]
        public string? comment { set; get; }
        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? createdAt { set; get; }
       
    }
}
