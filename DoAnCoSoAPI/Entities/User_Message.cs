﻿using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DoAnCoSoAPI.Entities
{
    public class User_Message
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? id { set; get; }
        [BsonElement("user_name"), BsonRepresentation(BsonType.String)]
      
        public string? user_name { set; get; }
        [BsonElement("message"), BsonRepresentation(BsonType.String)]
        public string? message { set; get; }
        [BsonElement("createdAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? createdAt { set; get; }
       
    }
}
