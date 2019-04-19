using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using jb_core_webapi.Models;

namespace jb_core_webapi.Services
{
    public class JellyblogDbFileService
    {
        private readonly IMongoCollection<File> _files;
        public JellyblogDbFileService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("JellyblogDb"));
            var database = client.GetDatabase("jellyblog");
            this._files = database.GetCollection<File>("fs.files");
        }

        public List<File> Get()
        {
            // ProjectionDefinition<BsonDocument> projection = "{metadata: 0}";
            return _files.Find(f => true).ToList();
        }

        public PaginationResponse<File> Get(FileFindCriteria request)
        {
            var builder = Builders<File>.Filter;
            var filter = builder.Empty;
            var limit = 10 + 1;
            var skip = (request.Page - 1) * 10;
            List<File> foundDocs;
            bool hasMore;



            if (!string.IsNullOrEmpty(request.Context))
            {
                filter = filter & builder.Eq("metadata.context", request.Context);
            }
            if (!string.IsNullOrEmpty(request.PostId))
            {
                filter = filter & builder.Eq("metadata.postId", request.PostId);
            }
            else if (request.WithoutPostId.HasValue && (bool)request.WithoutPostId)
            {
                filter = filter & builder.Eq<string>("metadata.postId", null);
            }
            if (!string.IsNullOrEmpty(request.ContentType))
            {
                filter = filter & builder.Regex("contentType", new BsonRegularExpression(request.ContentType));
            }
            if (request.DateFrom.HasValue)
            {
                filter = filter & builder.Gte("uploadDate", request.DateFrom);
            }
            if (request.DateTo.HasValue)
            {
                filter = filter & builder.Lte("uploadDate", request.DateTo);
            }

            foundDocs = _files.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .ToList();
            hasMore = foundDocs.Count > 10;
            if (foundDocs.Count > 10)
            {
                foundDocs.RemoveRange(10, foundDocs.Count - 10);
            }

            return new PaginationResponse<File>(Items: foundDocs, HasMore: hasMore);
        }
    }
}