using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace jb_core_webapi.Models
{
    public class PostFindAdminCriteria : PaginationRequest
    {
        public DateTime? From { get; set; } = null;

        public DateTime? To { get; set; } = null;

        [FromQuery(Name = "q")]
        [MaxLength(64)]
        public string Text { get; set; } = null;

        public PostStatus[] Statuses { get; set; } = new PostStatus[] { PostStatus.PUB };

        public string[] Ids { get; set; } = new string[0];
    }
}
