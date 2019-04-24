using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace jb_core_webapi.Models
{
    [BsonIgnoreExtraElements]
    public class User : UserInfo
    {
        [BsonElement("password")]
        [BsonRequired]
        public string Password { get; set; }
    }
}