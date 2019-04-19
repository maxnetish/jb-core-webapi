using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace jb_core_webapi.Models
{
    public class FileMetadata
    {
        [BsonElement("context")]
        public string Context { get; set; }

        [BsonElement("postId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }

        [BsonElement("originalName")]
        public string OriginalName { get; set; }

        [BsonElement("width")]
        public int? Width { get; set; }

        [BsonElement("height")]
        public int? Height { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("srcsetTag")]
        public string SrcSetTag { get; set; }
    }

    // [BsonIgnoreExtraElements]
    public class File
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("filename")]
        public string Filename { get; set; }

        [BsonElement("uploadDate")]
        // [BsonIgnore]
        public System.DateTime UploadDate { get; set; }

        [BsonElement("length")]
        public long Length { get; set; }

        [BsonElement("chunkSize")]
        public long ChunkSize { get; set; }

        [BsonElement("md5")]
        public string Md5 { get; set; }

        [BsonElement("contentType")]
        public string ContentType { get; set; }

        [BsonElement("metadata")]
        public FileMetadata Metadata { get; set; }
    }
}