using HrMangmentSystem_Application.Interfaces.Auth;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Tenants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HrMangmentSystem_Application.Implementation.Auth
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime Expiration) GenerateToken(Employee employee, TenantEntity tenant, IReadOnlyList<string> roles)
        {
            var jwtSection = _configuration.GetSection("JwtSettings");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var m) ? m : 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,employee.Email),
                new Claim("tenantId",tenant.Id.ToString()),
                new Claim(ClaimTypes.Name,$"{employee.FirstName} {employee.LastName}"),
                new Claim(ClaimTypes.NameIdentifier,employee.Id.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expiresAt = DateTime.Now.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                  issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
                );

              var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenString, expiresAt);

        }
    }
}
