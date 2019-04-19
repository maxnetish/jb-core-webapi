using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using jb_core_webapi.Models;

namespace jb_core_webapi.Services
{
    public class JellyblogDbFileService
    {
        private readonly IMongoCollection<File> _files;
        private IConfiguration _config;
        public JellyblogDbFileService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("JellyblogDb"));
            var database = client.GetDatabase("jellyblog");
            this._files = database.GetCollection<File>("fs.files");
            this._config = config;
        }

        public List<File> Get()
        {
            return _files.Find(f => true).ToList();
        }

        public async Task<PaginationResponse<File>> Get(FileFindCriteria request)
        {
            var filter = JellyblogDbFileService._mapFileFindCriteriaToFilterDefinition(request);
            var itemsPerPage = this._config.GetValue<int>("PaginationItemsPerPage");
            var limit = itemsPerPage + 1;
            var skip = (request.Page - 1) * itemsPerPage;
            // List<File> foundDocs;
            bool hasMore;

            var foundDocs = await _files.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
            hasMore = foundDocs.Count > itemsPerPage;
            if (foundDocs.Count > itemsPerPage)
            {
                foundDocs.RemoveRange(itemsPerPage, foundDocs.Count - itemsPerPage);
            }

            return new PaginationResponse<File>(Items: foundDocs, HasMore: hasMore);
        }

        private static FilterDefinition<File> _mapFileFindCriteriaToFilterDefinition(FileFindCriteria request)
        {
            var builder = Builders<File>.Filter;
            var filter = builder.Empty;

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

            return filter;
        }
    }
}