using jb_core_webapi.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Threading.Tasks;
using System.IO;

namespace jb_core_webapi.Services
{
    public interface IJellyblogDbFileService
    {
        Task<PaginationResponse<JbFileInfo>> Get(FileFindCriteria request);
        Task<JbFileInfo> Get(ObjectId id);
        Task<GridFSDownloadStream<ObjectId>> OpenStreamByFilename(string filename);
        Task<JbFileInfo> Upload(Stream stream, string filename, string contentType, JbFileMetadata meta);
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

        public async Task<GridFSDownloadStream<ObjectId>> OpenStreamByFilename(string filename)
        {
            return await Bucket.OpenDownloadStreamByNameAsync(filename);
        }

        public async Task<JbFileInfo> Upload(Stream stream, string fileName, string contentType, JbFileMetadata meta)
        {
            var id = await Bucket.UploadFromStreamAsync(fileName, stream, new GridFSUploadOptions {
                ContentType = contentType,
                Metadata = new BsonDocument(meta.ToDictionary())
            });
            var justUploadedFile = await Get(id);
            return justUploadedFile;
        }

        public async Task<JbFileInfo> Get(ObjectId id)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            using (var cursor = await Bucket.FindAsync(filter))
            {
                var info = await cursor.FirstOrDefaultAsync();
                return GridFsFileInfo2File(info);
            }
        }

        public async Task<PaginationResponse<JbFileInfo>> Get(FileFindCriteria request)
        {
            var filter = _mapFileFindCriteriaToFilterDefinition(request);
            var itemsPerPage = _config.GetValue<int>("PaginationItemsPerPage");
            var limit = itemsPerPage + 1;
            var skip = (request.Page - 1) * itemsPerPage;
            var options = new GridFSFindOptions
            {
                Limit = itemsPerPage + 1,
                Skip = (request.Page - 1) * itemsPerPage
            };

            using (var cursor = await Bucket.FindAsync(filter, options))
            {
                var items = await cursor.ToListAsync();
                var hasMore = items.Count > itemsPerPage;
                if(items.Count > itemsPerPage)
                {
                    items.RemoveRange(itemsPerPage, items.Count - itemsPerPage);
                }
                // We have to convert to custom model because GridFSFileInfo wouldn't serialize correctly
                var mappedItems = items.ConvertAll<JbFileInfo>(GridFsFileInfo2File);
                return new PaginationResponse<JbFileInfo>(Items: mappedItems, HasMore: hasMore);
            }
        }

        private static FilterDefinition<GridFSFileInfo> _mapFileFindCriteriaToFilterDefinition(FileFindCriteria request)
        {
            var builder = Builders<GridFSFileInfo>.Filter;
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

        private static JbFileInfo GridFsFileInfo2File(GridFSFileInfo inp)
        {
            JbFileInfo result = new JbFileInfo
            {
                ChunkSize = inp.ChunkSizeBytes,
                ContentType = inp.ContentType,
                Filename = inp.Filename,
                Id = inp.Id.ToString(),
                Length = inp.Length,
                UploadDate = inp.UploadDateTime,
                Metadata = new JbFileMetadata
                {
                    Context = BsonValueToNullableString(inp.Metadata.GetValue("context")),
                    Description = BsonValueToNullableString(inp.Metadata.GetValue("description")),
                    Height = BsonValueToNullableString(inp.Metadata.GetValue("height")),
                    OriginalName = BsonValueToNullableString(inp.Metadata.GetValue("originalName")),
                    PostId = BsonValueToNullableString(inp.Metadata.GetValue("postId")),
                    SrcSetTag = BsonValueToNullableString(inp.Metadata.GetValue("srcsetTag")),
                    Width = BsonValueToNullableString(inp.Metadata.GetValue("width"))
                }
            };
            return result;
        }

        private static string BsonValueToNullableString(BsonValue inp)
        {
            if (inp.IsBsonNull)
            {
                return null;
            }
            if(inp.IsObjectId)
            {
                return inp.AsObjectId.ToString();
            }
            return inp.AsString;
        }
    }
}