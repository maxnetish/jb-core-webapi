using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace jb_core_webapi.Models
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
    }
}