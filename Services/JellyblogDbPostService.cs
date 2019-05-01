using jb_core_webapi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace jb_core_webapi.Services
{
    public interface IJellyblogDbPostService
    {
        Task<PaginationResponse<PostShortInfoEnriched>> Get(PostFindAdminCriteria criteria);
    }

    public class JellyblogDbPostService : IJellyblogDbPostService
    {
        private readonly IJellyblogDbContext _context;
        private readonly IJellyblogDbFileService _fileService;

        public JellyblogDbPostService(IJellyblogDbContext context, IJellyblogDbFileService fileService)
        {
            this._context = context;
            this._fileService = fileService;
        }

        private IMongoCollection<Post> _posts = null;
        public IMongoCollection<Post> Posts
        {
            get
            {
                if (_posts == null)
                {
                    _posts = this._context.Database.GetCollection<Post>("posts");
                    _createCollectionIndexes(_posts);
                }
                return _posts;
            }
        }

        public async Task<PaginationResponse<PostShortInfoEnriched>> Get(PostFindAdminCriteria criteria = null)
        {
            ProjectionDefinition<Post, PostShortInfo> projection = "{_id: 1, status: 1, createDate: 1, updateDate: 1, pubDate: 1, titleImg: 1, title: 1, brief: 1}";
            var filter = _mapPostFindAdminCriteriaToFilterDefinfition(criteria);
            var foundPosts = await this.Posts
                .Find(filter)
                .Project(projection)
                .ToListAsync();

            List<PostShortInfoEnriched> enrichedItems = new List<PostShortInfoEnriched>(foundPosts.Count);
            foreach (var ent in foundPosts)
            {
                enrichedItems.Add(await _enrichPost(ent));
            }

            return new PaginationResponse<PostShortInfoEnriched>()
            {
                HasMore = false,
                Items = enrichedItems
            };
        }

        private async Task<PostShortInfoEnriched> _enrichPost(PostShortInfo post)
        {
            if (post == null)
            {
                return null;
            }
            if (post.TitleImg == null)
            {
                return new PostShortInfoEnriched(post, null);
            }
            var titleImgAsFileInfo = await _fileService.Get(post.TitleImg.Id);
            return new PostShortInfoEnriched(post, titleImgAsFileInfo);
        }

        private static FilterDefinition<Post> _mapPostFindAdminCriteriaToFilterDefinfition(PostFindAdminCriteria criteria)
        {
            var builder = Builders<Post>.Filter;
            var filter = builder.Empty;

            if (criteria == null)
            {
                return filter;
            }

            if (criteria.From.HasValue)
            {
                filter = filter & builder.Gte("createDate", criteria.From);
            }
            if (criteria.Ids.Length > 0)
            {
                filter = filter & builder.In("_id", criteria.Ids);
            }
            if (criteria.Statuses.Length > 0)
            {
                filter = filter & builder.In("status", criteria.Statuses);
            }
            if (!string.IsNullOrEmpty(criteria.Text))
            {
                filter = filter & builder.Text(criteria.Text, new TextSearchOptions
                {
                    CaseSensitive = false,
                    DiacriticSensitive = false
                });
            }
            if (criteria.To.HasValue)
            {
                filter = filter & builder.Lte("createDate", criteria.To);
            }

            return filter;
        }

        private static void _createCollectionIndexes(IMongoCollection<Post> collection)
        {
            CreateIndexModel<Post>[] indexModels = {
                new CreateIndexModel<Post>(
                Builders<Post>.IndexKeys.Ascending(new StringFieldDefinition<Post>("hru")),
                new CreateIndexOptions
                {
                    Unique = true
                }),
                new CreateIndexModel<Post>(
                Builders<Post>.IndexKeys.Text("title").Text("brief").Text("content"),
                new CreateIndexOptions()
                {
                    Name = "My text index",
                    Weights = BsonDocument.Parse("{title: 4, brief: 2, content: 1}")
                })
            };
            collection.Indexes.CreateMany(indexModels);
        }
    }
}
