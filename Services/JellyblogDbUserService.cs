using jb_core_webapi.Models;
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
        Task<List<User>> Get();
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

        public async Task<User> Get(string username)
        {
            var criteria = Builders<User>.Filter
                .Eq<string>("username", username);
            var foundDoc = await this.Users.Find(criteria).FirstOrDefaultAsync();

            if (foundDoc == null && username == "admin")
            {
                // if no admin user - use temporaraly admin account with password hash provided from config
                var adminPasswordFromConfig = this._config.GetValue<string>("AdminPassword");
                if (!string.IsNullOrEmpty(adminPasswordFromConfig))
                {
                    foundDoc = new User()
                    {
                        Password = adminPasswordFromConfig,
                        Role = UserRole.admin,
                        UserName = "admin"
                    };
                }
            }

            return foundDoc;
        }

        public async Task<List<User>> Get()
        {
            var foundDocs = await this.Users.Find(d => true).ToListAsync();
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
