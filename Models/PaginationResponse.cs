using System.Collections.Generic;
using Newtonsoft.Json;

namespace jb_core_webapi.Models
{
    public class PaginationResponse<T>
    {
        public PaginationResponse(List<T> Items = null, bool HasMore = false)
        {
            this.Items = Items == null ? new List<T>() : Items;
            this.HasMore = HasMore;
        }

        [JsonProperty("items")]
        public List<T> Items;

        [JsonProperty("hasMore")]
        public bool HasMore;
    }
}