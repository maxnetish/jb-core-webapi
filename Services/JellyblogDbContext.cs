using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace jb_core_webapi.Services
{
    public interface IJellyblogDbContext
    {
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
    }

    public class JellyblogDbContext : IJellyblogDbContext
    {
        public JellyblogDbContext(IConfiguration config)
        {
            Client = new MongoClient(config.GetConnectionString("MongoUrl"));
            Database = this.Client.GetDatabase(config.GetValue<string>("DatabaseName"));
        }
        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
    }
}
