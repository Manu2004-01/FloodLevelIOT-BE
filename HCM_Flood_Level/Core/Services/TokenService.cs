using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) => _config = config;

        public string CreateToken(User user, string roleName)
        {
            var jwt = _config.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new ArgumentNullException("Jwt:Key"));
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new System.Security.Claims.Claim(ClaimTypes.Role, roleName ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
