using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace jb_core_webapi.Models
{
    public class EntityRefSerializer : SerializerBase<EntityRef>
    {
        public override EntityRef Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.ObjectId:
                    return new EntityRef
                    {
                        Id = context.Reader.ReadObjectId().ToString()
                    };
                case BsonType.Null:
                    // We have to read value from reader else throwns exception
                    // https://stackoverflow.com/questions/43503478/readbsontype-can-only-be-called-when-state-is-type-not-when-state-is-value
                    context.Reader.ReadNull();
                    return null;
                default:
                    return base.Deserialize(context, args);
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, EntityRef value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
            }
            if (ObjectId.TryParse(value.Id, out ObjectId objectId))
            {
                context.Writer.WriteObjectId(objectId);
            }
            else
            {
                base.Serialize(context, args, value);
            }
        }
    }
}
