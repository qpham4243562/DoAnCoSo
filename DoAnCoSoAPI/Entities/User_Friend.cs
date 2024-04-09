using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DoAnCoSoAPI.Entities
{
    public class User_Friend
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
       

        [BsonElement("type"), BsonRepresentation(BsonType.String)]
        public string? type { set; get; }
        [BsonElement("status"), BsonRepresentation(BsonType.String)]
        public string? status { set; get; }
        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? createdAt { set; get; }
        [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? updatedAt { set; get; }
    }
}
