using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeelKnitService.Model
{
    public class User
    {
        private double _latitude;
        private double _longitude;
        private string _userName;

        public User()
        {
            Location = new double[2];
        }
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                UserNameLower = value.ToLower();
            }
        }

        public string UserNameLower { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        public double[] Location { get; set; }

        public string EmailAddress { get; set; }

        public string Key { get; set; }

        public string IosKey { get; set; }
        
        public string iosKey { get; set; }

        public string Avatar { get; set; }

        public bool IsTemporary { get; set; }

        public DateTime? PasswordExpiryTime { get; set; }

        public double Latitude
        {
            get { return _latitude; }
            set
            {
                Location[1] = value;
                _latitude = value;
            }
        }

        public double Longitude
        {
            get { return _longitude; }
            set
            {
                Location[0] = value;
                _longitude = value;
            }
        }
    }
}