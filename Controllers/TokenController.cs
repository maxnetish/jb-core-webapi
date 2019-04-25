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

namespace jb_core_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {

        private IJellyblogDbUserService _userDbService;
        private IJellybolgDbRefreshTokenService _refreshTokenDbService;
        private IConfiguration _config;

        private string authRefreshTokenCookieName;
        // days
        private int authRefreshTokenLifetime;

        public TokenController(IJellyblogDbUserService userDbService, IJellybolgDbRefreshTokenService tokenDbService, IConfiguration config)
        {
            this._userDbService = userDbService;
            this._refreshTokenDbService = tokenDbService;
            this._config = config;
            this.authRefreshTokenCookieName = config.GetValue<string>("AuthRefreshTokenCookieName");
            this.authRefreshTokenLifetime = config.GetValue<int>("AuthRefreshTokenLifetime");
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Token()
        {
            string refreshTokenFromCookie = null;
            if (!this.HttpContext.Request.Cookies.TryGetValue(this.authRefreshTokenCookieName, out refreshTokenFromCookie))
            {
                // no refresh token
                return new UnauthorizedResult();
            }

            var userRefreshTokenInfo = await this._refreshTokenDbService.Find(refreshTokenFromCookie);
            if (userRefreshTokenInfo == null)
            {
                // not our refresh token
                return new UnauthorizedResult();
            }

            var claimsIdentity = await this._userDbService.GetClaimsIdentity(userRefreshTokenInfo.UserName);
            if (claimsIdentity == null)
            {
                // no such user
                return new UnauthorizedResult();
            }

            var newJwtEncodedToken = _generateJwtEncodedToken(claimsIdentity);
            return new ObjectResult(new
            {
                accessToken = newJwtEncodedToken,
                username = claimsIdentity.Name,
                role = claimsIdentity.FindFirst(ClaimsIdentity.DefaultRoleClaimType).Value
            });
        }

        [HttpPost]
        public async Task<IActionResult> Token(UserCredentials credentials)
        {
            ClaimsIdentity claimsIdentity = await _userDbService.GetClaimsIdentity(credentials);
            if (claimsIdentity == null)
            {
                return new UnauthorizedResult();
            }

            var maxAge = TimeSpan.FromDays(this.authRefreshTokenLifetime);
            var encodedJwt = _generateJwtEncodedToken(claimsIdentity);
            var refreshToken = await _generateAndStoreRefreshKey(claimsIdentity, maxAge);

            _writeRefreshToken(refreshToken, maxAge);

            return new ObjectResult(new
            {
                accessToken = encodedJwt,
                username = claimsIdentity.Name,
                role = claimsIdentity.FindFirst(ClaimsIdentity.DefaultRoleClaimType).Value
            });
        }

        private void _writeRefreshToken(string refreshToken, TimeSpan maxAge)
        {
            HttpContext.Response.Cookies.Append(this.authRefreshTokenCookieName, refreshToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                MaxAge = maxAge,
                // FIXME prod - only secure
                Secure = false
            });
        }

        private string _generateJwtEncodedToken(ClaimsIdentity claimsIdentity)
        {
            var now = DateTime.UtcNow;
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetValue<string>("AuthTokenKey")));
            var jwt = new JwtSecurityToken(
                issuer: _config.GetValue<string>("AuthTokenIssuer"),
                audience: _config.GetValue<string>("AuthTokenAudience"),
                notBefore: now,
                claims: claimsIdentity.Claims,
                // minutes
                expires: now.Add(TimeSpan.FromMinutes(_config.GetValue<int>("AuthTokenLifetime"))),
                signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        private async Task<string> _generateAndStoreRefreshKey(ClaimsIdentity claimsIdentity, TimeSpan maxAge)
        {
            var bytes = new byte[20];
            var rnd = new Random();
            var stringBuilder = new StringBuilder();
            rnd.NextBytes(bytes);
            foreach (var b in bytes)
            {
                stringBuilder.Append(b.ToString("X"));
            }
            var refreshToken = stringBuilder.ToString();
            await this._refreshTokenDbService.Add(new UserRefreshTokenInfo
            {
                RefreshToken = refreshToken,
                UserName = claimsIdentity.Name,
                ValidBefore = DateTime.Now.Add(maxAge)
            });
            return refreshToken;
        }
    }
}