using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jb_core_webapi.Models;
using MongoDB.Driver;

namespace jb_core_webapi.Services
{
    public interface IJellyblogDbPostService
    {
        Task<PaginationResponse<PostShortInfo>> Get();
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
        public IMongoCollection<Post> Posts {
            get
            {
                if(_posts==null)
                {
                    _posts = this._context.Database.GetCollection<Post>("posts");
                }
                return _posts;
            }
        }

        public async Task<PaginationResponse<PostShortInfo>> Get()
        {
            ProjectionDefinition<Post, PostShortInfo> projection = "{_id: 1, status: 1, createDate: 1, updateDate: 1, pubDate: 1, titleImg: 1, title: 1, brief: 1}";
            var foundPosts = await this.Posts
                .Find(d => true)
                .Project(projection)
                .ToListAsync();
            return new PaginationResponse<PostShortInfo>()
            {
                HasMore = false,
                Items = foundPosts
            };
        }
    }
}
