using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace jb_core_webapi.Models
{
    public class UserRefreshTokenInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("token")]
        [BsonRequired]
        public string RefreshToken { get; set; }

        [BsonElement("username")]
        [BsonRequired]
        public string UserName { get; set; }

        [BsonElement("validBefore")]
        [BsonRequired]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ValidBefore { get; set; }
    }
}
