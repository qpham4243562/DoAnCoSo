using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace DoAnCoSoAPI.Entities
{
    public class Group_Member
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
        [BsonElement("roleId"), BsonRepresentation(BsonType.String)]
        public string? roleId { set; get; }
        [BsonElement("status"), BsonRepresentation(BsonType.String)]
        public string? status { set; get; }
        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? createdAt { set; get; }
        [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? updatedAt { set; get; }
        [BsonElement("notes"), BsonRepresentation(BsonType.String)]
        public string? notes { set; get; }
    }
}
