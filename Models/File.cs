using Newtonsoft.Json;
using System.Collections.Generic;

namespace jb_core_webapi.Models
{
    public class JbFileMetadata
    {
        [JsonProperty("context")]
        public string Context { get; set; }

        [JsonProperty("postId")]
        public string PostId { get; set; }

        [JsonProperty("originalName")]
        public string OriginalName { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("srcsetTag")]
        public string SrcSetTag { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>(7);
            result.Add("context", this.Context);
            result.Add("postId", this.PostId);
            result.Add("originalName", OriginalName);
            result.Add("width", Width);
            result.Add("height", Height);
            result.Add("description", Description);
            result.Add("srcsetTag", SrcSetTag);
            return result;
        }
    }

    // [BsonIgnoreExtraElements]
    public class JbFileInfo
    {

        public string Id { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("uploadDate")]
        public System.DateTime UploadDate { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("chunkSize")]
        public long ChunkSize { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("url")]
        public string Url => $"/fs/{this.Filename}";

        [JsonProperty("metadata")]
        public JbFileMetadata Metadata { get; set; }
    }
}