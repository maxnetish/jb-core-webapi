using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MongoDB.Bson.Serialization;

namespace jb_core_webapi.Models
{
    public class PostShortInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("status")]
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        [BsonDefaultValue("DRAFT")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PostStatus Status { get; set; }

        [BsonElement("createDate")]
        [BsonRequired]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreateDate { get; set; }

        [BsonElement("pubDate")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? PubDate { get; set; }

        [BsonElement("updateDate")]
        [BsonRequired]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime UpdateDate { get; set; }

        [BsonElement("titleImg")]
        [BsonSerializer(typeof(EntityRefSerializer))]
        //[BsonRepresentation(BsonType.ObjectId)]
        public EntityRef TitleImg { get; set; }

        [BsonElement("title")]
        [BsonRequired]
        [MaxLength(512)]
        public string Title { get; set; }

        [BsonElement("brief")]
        [MaxLength(1024)]
        public string Brief { get; set; }
    }

    public class EntityRefSerializer : SerializerBase<EntityRef>
    {
        public override EntityRef Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var id = context.Reader.ReadObjectId();
            return new EntityRef
            {
                   Id= id.ToString()
            };
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, EntityRef value)
        {
            context.Writer.WriteObjectId(ObjectId.Parse(value.Id));
        }
    }
}
