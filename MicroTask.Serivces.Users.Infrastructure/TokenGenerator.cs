using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MicroTask.Services.Users.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroTask.Serivces.Users.Infrastructure;

public class TokenGenerator
{
    private readonly IConfiguration _configuration;

    public TokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GenerateToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenKey = _configuration["JwtSettings:SecretKey"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey!));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
