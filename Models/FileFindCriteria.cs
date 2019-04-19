using System;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace jb_core_webapi.Models
{
    public class FileFindCriteria : PaginationRequest
    {
        
        [MaxLength(32)]
        public string Context { get; set; } = null;
        
        public bool? WithoutPostId { get; set; } = null;

        // Mongo ObjectId pattern
        [RegularExpression("^[0-9a-fA-F]{24}$")]
        public string PostId { get; set; } = null;

        [MaxLength(64)]
        public string ContentType { get; set; } = null;
        public DateTime? DateTo { get; set; } = null;
        public DateTime? DateFrom { get; set; } = null;
    }
}