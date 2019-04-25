using jb_core_webapi.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Threading.Tasks;

namespace jb_core_webapi.Services
{
    public interface IJellyblogDbFileService
    {
        Task<PaginationResponse<GridFSFileInfo>> Get(FileFindCriteria request);
    }

    public class JellyblogDbFileService : IJellyblogDbFileService
    {
        private readonly IConfiguration _config;
        private readonly IJellyblogDbContext _context;

        public JellyblogDbFileService(IConfiguration config, IJellyblogDbContext context)
        {
            _config = config;
            _context = context;
        }

        private GridFSBucket _bucket;
        protected GridFSBucket Bucket
        {
            get
            {
                if (_bucket == null)
                {
                    _bucket = new GridFSBucket(this._context.Database);
                }
                return _bucket;
            }
        }

        //private IMongoCollection<File> _files = null;
        //protected IMongoCollection<File> Files
        //{
        //    get
        //    {
        //        if (_files == null)
        //        {
        //            _files = this._context.Database.GetCollection<File>("fs.files");
        //        }
        //        return _files;
        //    }
        //}

        public async Task<PaginationResponse<GridFSFileInfo>> Get(FileFindCriteria request)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Empty;

            using (var cursor = await Bucket.FindAsync(filter))
            {
                var items = await cursor.ToListAsync();
                return new PaginationResponse<GridFSFileInfo>(Items: items, HasMore: false);
            }


            //var filter = JellyblogDbFileService._mapFileFindCriteriaToFilterDefinition(request);
            //var itemsPerPage = this._config.GetValue<int>("PaginationItemsPerPage");
            //var limit = itemsPerPage + 1;
            //var skip = (request.Page - 1) * itemsPerPage;
            //// List<File> foundDocs;
            //bool hasMore;

            //var foundDocs = await this.Files.Find(filter)
            //    .Skip(skip)
            //    .Limit(limit)
            //    .ToListAsync();
            //hasMore = foundDocs.Count > itemsPerPage;
            //if (foundDocs.Count > itemsPerPage)
            //{
            //    foundDocs.RemoveRange(itemsPerPage, foundDocs.Count - itemsPerPage);
            //}

            //return new PaginationResponse<File>(Items: foundDocs, HasMore: hasMore);
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