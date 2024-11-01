using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ContosoPizza.Services
{
    public sealed class JwtService(IConfiguration config)
    {
        public string GenerateToken(IdentityUser user)
        {
            var signature = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim("AccessFailedCount", user.AccessFailedCount.ToString("D2")),
                ]),
                IssuedAt = DateTime.Now,
                Expires = DateTime.Now.AddMinutes(double.Parse(config["Jwt:Duration"]!)),
                Audience = config["Jwt:Audience"],
                Issuer = config["Issuer"],
                SigningCredentials = signature
            };

            var token = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

            return token;
        }
    }
}