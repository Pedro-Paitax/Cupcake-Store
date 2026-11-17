using System.Security.Claims;
using System.Text;
using CupcakeStore.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CupcakeStore.Api.Services;

public class TokenService(IOptions<JwtOptions> opcoes)
{
    private readonly JwtOptions _jwt = opcoes.Value;

    public static SymmetricSecurityKey Chave(string key) => new(Encoding.UTF8.GetBytes(key));

    public string Gerar(Usuario usuario)
    {
        var descritor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("name", usuario.Nome),
                new Claim("role", usuario.Perfil)
            }),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = DateTime.UtcNow.AddDays(_jwt.ExpiracaoDias),
            SigningCredentials = new SigningCredentials(Chave(_jwt.Key), SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descritor);
    }
}
