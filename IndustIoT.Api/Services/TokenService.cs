using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.IdentityModel.Tokens;

namespace IndustIoT.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse? Autenticar(LoginRequest request)
    {
        // Usuários demo vêm do appsettings — em produção seriam um Identity
        // provider; manutenção de usuários está fora do escopo deste desafio.
        var usuarios = _configuration.GetSection("Auth:Usuarios").Get<List<UsuarioConfig>>() ?? [];
        var usuario = usuarios.FirstOrDefault(u =>
            u.Usuario.Equals(request.Usuario, StringComparison.OrdinalIgnoreCase) &&
            u.Senha == request.Senha);

        if (usuario is null)
            return null;

        var expiraEm = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue("Jwt:ExpiracaoMinutos", 60));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, usuario.Usuario),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Chave"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Emissor"],
            audience: _configuration["Jwt:Audiencia"],
            claims: claims,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Usuario = usuario.Usuario,
            Role = usuario.Role,
            ExpiraEm = expiraEm
        };
    }

    private class UsuarioConfig
    {
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
