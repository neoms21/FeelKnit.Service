using System;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace FeelKnitService.Serializers
{
    public sealed class StringOrGuidSerializer : BsonBaseSerializer
    {
        public override object Deserialize(BsonReader bsonReader, Type nominalType,
            Type actualType, IBsonSerializationOptions options)
        {
            var bsonType = bsonReader.CurrentBsonType;
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return Guid.Empty;
                case BsonType.String:
                    var readString = bsonReader.ReadString();
                    return Guid.Parse(readString);
                default:
                    var message = string.Format("Cannot deserialize BsonString from BsonType {0}.", bsonType);
                    throw new BsonSerializationException(message);
            }
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType,
            object value, IBsonSerializationOptions options)
        {
            if (value != null)
            {
                bsonWriter.WriteString(value.ToString());
            }
            else
            {
                bsonWriter.WriteNull();
            }
        }
    }
}