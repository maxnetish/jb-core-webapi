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
    public interface IJellyblogDbUserService
    {
        Task<User> Get(string userName);
        Task<List<UserInfo>> Get();
        Task Add(User user);
        Task Remove(string id);
        Task<IActionResult> NewPassword(UserNewPassword userNewPassword);
        Task<ClaimsIdentity> GetClaimsIdentity(UserCredentials credentials);
    }

    public class JellyblogDbUserService : IJellyblogDbUserService
    {
        private IConfiguration _config;
        private IJellyblogDbContext _context;
        private SHA256 _sha256 = SHA256.Create();

        public JellyblogDbUserService(IConfiguration config, IJellyblogDbContext context)
        {
            this._config = config;
            this._context = context;
        }

        private IMongoCollection<User> _users = null;
        protected IMongoCollection<User> Users
        {
            get
            {
                if (_users == null)
                {
                    _users = this._context.Database.GetCollection<User>("users");
                    _createCollectionIndexes(_users);

                }
                return _users;
            }
        }

        public async Task Add(User user)
        {
            user.Password = _textToHash(user.Password);
            await this.Users.InsertOneAsync(user);
        }

        public async Task Remove(string id)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);
            await this.Users.DeleteOneAsync(filter);
        }

        public async Task<IActionResult> NewPassword(UserNewPassword userNewPassword)
        {
            var identity = await this.GetClaimsIdentity(new UserCredentials
            {
                Password = userNewPassword.OldPassword,
                Username = userNewPassword.Username
            });
            if(identity==null)
            {
                return new UnauthorizedResult();
            }
            var filter = Builders<User>.Filter.Eq("username", userNewPassword.Username);
            var update = Builders<User>.Update.Set("password", _textToHash(userNewPassword.NewPassword));
            await this.Users.UpdateOneAsync(filter, update);
            return new OkResult();
        }

        public async Task<User> Get(string username)
        {
            var criteria = Builders<User>.Filter
                .Eq<string>("username", username);
            var foundDoc = await this.Users.Find(criteria).FirstOrDefaultAsync();

            if (foundDoc == null && username == "admin")
            {
                // if no admin user - add it
                var adminPasswordFromConfig = this._config.GetValue<string>("AdminPassword");
                if (!string.IsNullOrEmpty(adminPasswordFromConfig))
                {
                    foundDoc = new User()
                    {
                        Password = adminPasswordFromConfig,
                        Role = UserRole.admin,
                        UserName = "admin"
                    };
                    await this.Users.InsertOneAsync(foundDoc);
                }
            }

            return foundDoc;
        }

        public async Task<List<UserInfo>> Get()
        {
            ProjectionDefinition<User, UserInfo> projection = "{_id: 1, username: 1, role: 1}";
            var foundDocs = await this.Users
                .Find(d => true)
                .Project(projection)
                .ToListAsync();
            return foundDocs;
        }

        public async Task<ClaimsIdentity> GetClaimsIdentity(UserCredentials credentials)
        {
            var user = await this.Get(credentials.Username);
            if (user == null)
            {
                // not found by username
                return null;
            }

            if (user.Password != _textToHash(credentials.Password))
            {
                // wrong password
                return null;
            }


            var claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString("F"))
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }

        private static void _createCollectionIndexes(IMongoCollection<User> collection)
        {
            var indexOptions = new CreateIndexOptions() { Unique = true };
            var fieldForIndex = new StringFieldDefinition<User>("username");
            var indexDefinition = new IndexKeysDefinitionBuilder<User>().Ascending(fieldForIndex);
            var createIndexModel = new CreateIndexModel<User>(indexDefinition, indexOptions);
            collection.Indexes.CreateOne(createIndexModel);
        }

        private static string _textToHash(string plainText, string hashAlgoName = "SHA256")
        {
            var alg = HashAlgorithm.Create(hashAlgoName);
            var inputBytes = Encoding.UTF8.GetBytes(plainText);
            var outputBytes = alg.ComputeHash(inputBytes);
            var stringBuilder = new StringBuilder();
            foreach (var b in outputBytes)
            {
                stringBuilder.Append(b.ToString("X2"));
            }
            return stringBuilder.ToString();
        }

    }
}
