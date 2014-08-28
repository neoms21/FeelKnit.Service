using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeelKnitService.Model
{
    public class User
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        public string EmailAddress { get; set; } 
        
        public string Key{ get; set; }

        public bool IsTemporary { get; set; }

        public DateTime ExpiryTime { get; set; }
    }
}