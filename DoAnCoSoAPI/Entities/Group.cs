using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoAnCoSoAPI.Entities
{
    public class Group
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
        [BsonElement("title"), BsonRepresentation(BsonType.String)]
        public string? title { set; get; }
        [BsonElement("metaTitle"), BsonRepresentation(BsonType.String)]
        public string? metaTitle { set; get; }
        [BsonElement("slug"), BsonRepresentation(BsonType.String)]
        public string? slug { set; get; }
        [BsonElement("summary"), BsonRepresentation(BsonType.String)]
        public string? summary { set; get; }
        [BsonElement("status"), BsonRepresentation(BsonType.String)]
        public string? status { set; get; }
        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? createdAt { set; get; }
        [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? updatedAt { set; get; }
        [BsonElement("profile"), BsonRepresentation(BsonType.String)]
        public string? profile { set; get; }
        [BsonElement("content"), BsonRepresentation(BsonType.String)]
        public string? content { set; get; }
    }
}
