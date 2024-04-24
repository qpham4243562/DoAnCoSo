using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace DoAnCoSoAPI.Entities
{
    public class Group_meta
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
        [BsonElement("key"), BsonRepresentation(BsonType.String)]
        public string? key { set; get; }
        [BsonElement("content"), BsonRepresentation(BsonType.String)]
        public string? content { set; get; }
    }
}
