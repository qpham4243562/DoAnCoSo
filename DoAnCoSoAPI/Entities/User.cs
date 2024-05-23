using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSoAPI.Entities
{
    public class User
    {
        [BsonId]
        [BsonElement("_id"),BsonRepresentation(BsonType.ObjectId)]
        [Display(Name = "ID_User")]

        public string? Id { get; set; }
        [BsonElement("firstName"), BsonRepresentation(BsonType.String)]
        public string? firstName { get; set; }
        [BsonElement("lastName"), BsonRepresentation(BsonType.String)]
        public string? lastName { get; set; }
        [BsonElement("eMail"), BsonRepresentation(BsonType.String)]
        public string? eMail { get; set; }
        [BsonElement("passWordHash"), BsonRepresentation(BsonType.String)]
        [Display(Name = "Password")]

        public string? PasswordHash { get; set; }
        [BsonElement("registerAt"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? RegisterAt { get; set; }
        [BsonElement("lastLogin"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? LastLogin { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("images")]
        [BsonRepresentation(BsonType.Binary)]
        public byte? images { set; get; }
        [BsonElement("role"), BsonRepresentation(BsonType.String)]
        [Display(Name = "Role")]
        public string? role { get; set; }

    }
}
