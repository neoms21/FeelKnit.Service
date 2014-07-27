using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeelKnitService.Model
{
    public class Response
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ResponseText { get; set; }
    }
}