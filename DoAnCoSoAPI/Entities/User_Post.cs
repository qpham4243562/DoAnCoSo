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
        [BsonElement("Class"), BsonRepresentation(BsonType.String)]
        public string? Class { set; get; }
        [BsonElement("Subject"), BsonRepresentation(BsonType.String)]
        public string? Subject { set; get; }
        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? createdAt { set; get; }
        [BsonElement("updatedAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? updatedAt { set; get; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public byte[] CreatorAvatar { set; get; }
        [BsonElement("likes")]
        public int Likes { get; set; }
        [BsonElement("likedByUsers")]
        public List<string> LikedByUsers { get; set; } = new List<string>();
        public int? count { get; set; }
        public List<User_comment> Comments { get; set; } // Thêm thuộc tính Comments
        public bool IsApproved { get; set; } // True nếu bài viết đã được duyệt, false nếu chưa
        [BsonElement("files")]
        [BsonRepresentation(BsonType.Binary)]
        public List<byte[]> Files { get; set; }
        [BsonElement("fileNames")]
        public List<string> FileNames { get; set; }
    }

}
