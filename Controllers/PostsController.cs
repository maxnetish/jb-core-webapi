using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using jb_core_webapi.Services;
using jb_core_webapi.Models;
using Microsoft.AspNetCore.Authorization;

namespace jb_core_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IJellyblogDbPostService _postsService;

        public PostsController(IJellyblogDbPostService postsService)
        {
            this._postsService = postsService;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<PaginationResponse<PostShortInfoEnriched>>> GetForAdmin([FromQuery]PostFindAdminCriteria criteria = null)
        {
            return await _postsService.Get(criteria);
        }

    }
}