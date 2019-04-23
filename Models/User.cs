using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace jb_core_webapi.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        [BsonRequired]
        public string UserName { get; set; }

        [BsonElement("role")]
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        [BsonDefaultValue("reader")]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public UserRole Role { get; set; }

        [BsonElement("password")]
        [BsonRequired]
        public string Password { get; set; }
    }
}