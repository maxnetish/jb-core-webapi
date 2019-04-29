using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace jb_core_webapi.Models
{
    // TODO Add derived class PostEnriched
    public class Post : PostShortInfo
    {
        [BsonElement("allowRead")]
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        [BsonDefaultValue("FOR_ALL")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PostAllowRead AllowRead { get; set; }

        [BsonElement("author")]
        [BsonRequired]
        [MaxLength(128)]
        public string Author { get; set; }

        [BsonElement("contentType")]
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        [BsonDefaultValue("HTML")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PostContentType ContentType { get; set; }

        [BsonElement("content")]
        [BsonRequired]
        [MaxLength(131072)]
        public string Content { get; set; }

        [BsonElement("tags")]
        public string[] Tags { get; set; }

        [BsonElement("attachments")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string[] Attachments { get; set; }

        [BsonElement("hru")]
        [MaxLength(64)]
        public string Hru { get; set; }
    }
}
