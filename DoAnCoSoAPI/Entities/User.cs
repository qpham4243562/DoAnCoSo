﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DoAnCoSoAPI.Entities
{
    public class User
    {
        [BsonId]
        [BsonElement("_id"),BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("firstName"), BsonRepresentation(BsonType.String)]
        public string? FirstName { get; set; }
        [BsonElement("lastName"), BsonRepresentation(BsonType.String)]
        public string? LastName { get; set; }
        [BsonElement("eMail"), BsonRepresentation(BsonType.String)]
        public string? Email { get; set; }
        [BsonElement("passWordHash"), BsonRepresentation(BsonType.String)]
        public string? PasswordHash { get; set; }
        [BsonElement("registerAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? RegisterAt { get; set; }
        [BsonElement("lastLogin"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? LastLogin { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("images")]
        [BsonRepresentation(BsonType.Binary)]

        public List<byte[]>? images { set; get; }
    }
}
