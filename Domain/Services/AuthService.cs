using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Work360.Domain.DTO;
using Work360.Infrastructure.Context;

namespace Work360.Domain.Services
{
    public class AuthService
    {
        private readonly Work360Context _context;
        private readonly IConfiguration _configuration;

        public AuthService(Work360Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDTO?> Authenticate(LoginRequestDTO request)
        {
            // SIMULAÇÃO DE AUTENTICAÇÃO - O BANCO DE DADOS ORACLE NÃO ESTÁ ACESSÍVEL NO AMBIENTE SANDBOX
            if (request.Email != "teste@teste.com" || request.Senha != "123456")
            {
                return null; // Credenciais inválidas
            }

            // Cria um usuário simulado para gerar o token
            var user = new Entity.User
            {
                UserID = Guid.NewGuid(),
                Name = "Usuário Simulado",
                Email = request.Email,
                Password = request.Senha
            };

            // 3. Gerar JWT válido
            var token = GenerateJwtToken(user);

            // 4. Retornar o token e o tempo de expiração
            return new LoginResponseDTO
            {
                Token = token,
                ExpiresIn = 3600 // 1 hora
            };
        }

        private string GenerateJwtToken(Entity.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada."));
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name)
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Expira em 1 hora
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
