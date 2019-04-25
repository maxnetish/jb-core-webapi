using jb_core_webapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace jb_core_webapi.Services
{
    public interface IJellybolgDbRefreshTokenService
    {
        Task<UserRefreshTokenInfo> Find(string token);
        Task Add(UserRefreshTokenInfo tokenInfo);
        Task RemoveNotActual();
    }

    public class JellybolgDbRefreshTokenService : IJellybolgDbRefreshTokenService
    {
        private IConfiguration _config;
        private IJellyblogDbContext _context;

        public JellybolgDbRefreshTokenService(IConfiguration config, IJellyblogDbContext context)
        {
            this._config = config;
            this._context = context;
        }

        private IMongoCollection<UserRefreshTokenInfo> _userTokenInfos = null;
        protected IMongoCollection<UserRefreshTokenInfo> UserTokenInfos
        {
            get
            {
                if (_userTokenInfos == null)
                {
                    _userTokenInfos = this._context.Database.GetCollection<UserRefreshTokenInfo>("userRefreshTokens");
                    _createCollectionIndexes(_userTokenInfos);

                }
                return _userTokenInfos;
            }
        }

        public async Task<UserRefreshTokenInfo> Find(string token)
        {
            var filter = Builders<UserRefreshTokenInfo>.Filter.Eq("token", token);
            return await this.UserTokenInfos.Find(filter).FirstOrDefaultAsync();
        }

        public async Task Add(UserRefreshTokenInfo tokenInfo)
        {
            await this.UserTokenInfos.InsertOneAsync(tokenInfo);
        }

        public async Task RemoveNotActual()
        {
            var filter = Builders<UserRefreshTokenInfo>.Filter.Lte("validBefore", System.DateTime.Now);
            await this.UserTokenInfos.DeleteManyAsync(filter);
        }

        private static void _createCollectionIndexes(IMongoCollection<UserRefreshTokenInfo> collection)
        {
            var indexOptions = new CreateIndexOptions() { Unique = true };
            var fieldForIndex = new StringFieldDefinition<UserRefreshTokenInfo>("token");
            var indexDefinition = new IndexKeysDefinitionBuilder<UserRefreshTokenInfo>().Ascending(fieldForIndex);
            var createIndexModel = new CreateIndexModel<UserRefreshTokenInfo>(indexDefinition, indexOptions);
            collection.Indexes.CreateOne(createIndexModel);
        }
    }
}
