using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace jb_core_webapi.Models
{
    public class User
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
    }
}