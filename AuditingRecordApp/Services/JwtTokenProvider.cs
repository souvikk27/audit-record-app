using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuditingRecordApp.Entity;
using Microsoft.IdentityModel.Tokens;
#nullable disable

namespace AuditingRecordApp.Services;

public interface IJwtTokenProvider
{
    string GetToken(ApplicationUser user);
}


public sealed class JwtTokenProvider : IJwtTokenProvider
{
    private readonly IConfiguration _configuration;

    public JwtTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthConfiguration:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddSeconds(Convert.ToDouble(_configuration["AuthConfiguration:ExpirationSeconds"]));

        var token = new JwtSecurityToken(
            _configuration["AuthConfiguration:Issuer"],
            _configuration["AuthConfiguration:Audience"],
            claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}