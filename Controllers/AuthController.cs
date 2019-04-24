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
    public class AuthController : ControllerBase
    {
        private IJellyblogDbUserService _userService;
        private IConfiguration _config;

        public AuthController(IJellyblogDbUserService userService, IConfiguration config)
        {
            this._userService = userService;
            this._config = config;
        }

        [HttpDelete]
        [Route("users/remove")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Remove(string id)
        {
            await this._userService.Remove(id);
            return new OkResult();
        }

        [HttpPost]
        [Route("users/password")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> NewPassword(UserNewPassword userNewPassword)
        {
            return await this._userService.NewPassword(userNewPassword);
        }

        [HttpPost]
        [Route("users/add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(User user)
        {
            await this._userService.Add(user);
            return new OkResult();
        }

        [HttpGet]
        [Route("users")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<UserInfo>>> Users()
        {
            return await _userService.Get();
        }

        /**
         * We wouldn't use JWT because of not-fixable problem with update tokens
         * or would use...
         */
        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token(UserCredentials credentials)
        {
            ClaimsIdentity claimsIdentity = await _userService.GetClaimsIdentity(credentials);

            if (claimsIdentity == null)
            {
                return new UnauthorizedResult();
            }

            var now = DateTime.UtcNow;
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetValue<string>("AuthTokenKey")));
            var jwt = new JwtSecurityToken(
                issuer: _config.GetValue<string>("AuthTokenIssuer"),
                audience: _config.GetValue<string>("AuthTokenAudience"),
                notBefore: now,
                claims: claimsIdentity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(_config.GetValue<int>("AuthTokenLifetime"))),
                signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new ObjectResult(new
            {
                accessToken = encodedJwt,
                username = claimsIdentity.Name
            });
        }

    }
}