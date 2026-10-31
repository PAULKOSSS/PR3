using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using PR3.Models;

namespace PR3
{
    public class JwtHelper
    {
        public static SymmetricSecurityKey key = new(RandomNumberGenerator.GetBytes(32));

        public const string Issuer = "ClothingStoreApi";
        public const string Audience = "ClothingStoreClient";

        public static TokenValidationParameters validation = new()
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,

            ValidateAudience = true,
            ValidAudience = Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            NameClaimType = "name",
            RoleClaimType = "role"
        };

        public static string CreateToken(User user)
        {
            var claims = new[]
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Login),
                new Claim("role", user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
