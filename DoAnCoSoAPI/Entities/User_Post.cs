using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver.Core.Servers;

namespace DoAnCoSoAPI.Entities
{
    public class User_Post
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
        [BsonElement("title"), BsonRepresentation(BsonType.String)]
        public string? title { set; get; }
        [BsonElement("content"), BsonRepresentation(BsonType.String)]
        public string? content { set; get; }
        [BsonIgnoreIfNull]
        [BsonElement("images")]
        [BsonRepresentation(BsonType.Binary)]

        public List<byte[]>? images { set; get; }
        [BsonIgnoreIfNull]
        [BsonElement("imageData")]
        [BsonRepresentation(BsonType.Binary)]
        public List<byte[]>? ImageData { get; set; }

        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? createdAt { set; get; }
        [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? updatedAt { set; get; }
    }

}
