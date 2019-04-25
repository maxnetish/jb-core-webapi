using jb_core_webapi.Models;
using jb_core_webapi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace jb_core_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IJellyblogDbUserService _userService;
        private IConfiguration _config;

        public AccountController(IJellyblogDbUserService userService, IConfiguration config)
        {
            this._userService = userService;
            this._config = config;
        }

        [HttpDelete]
        [Route("remove")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Remove(string id)
        {
            await this._userService.Remove(id);
            return new OkResult();
        }

        [HttpPost]
        [Route("password")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> NewPassword(UserNewPassword userNewPassword)
        {
            return await this._userService.NewPassword(userNewPassword);
        }

        [HttpPost]
        [Route("add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(User user)
        {
            await this._userService.Add(user);
            return new OkResult();
        }

        [HttpGet]
        [Route("list")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<UserInfo>>> Users()
        {
            return await _userService.Get();
        }

    }

}