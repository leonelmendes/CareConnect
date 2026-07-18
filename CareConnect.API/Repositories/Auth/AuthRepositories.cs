using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareConnect.API.Repositories.Users;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace CareConnect.API.Repositories.Auth
{
    public class AuthRepositories : IAuthRepositories
    {
        private readonly IUserRepositories _repository;
        private readonly IConfiguration _configuration;

        public AuthRepositories(IUserRepositories repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _repository.GetByEmailAsync(dto.Email);

            if (user == null)
            {
                return new AuthResponseDto 
                { 
                    Sucesso = false, 
                    MensagemErro = "Utilizador não encontrado. Verifique o seu e-mail." 
                };
            }

            bool passwordValida = false;
            try
            {
                passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            }
            catch
            {
                // Fallback para caso a password ainda esteja em texto plano em testes antigos
                passwordValida = (dto.Password == user.PasswordHash);
            }

            if (!passwordValida)
            {
                return new AuthResponseDto 
                { 
                    Sucesso = false, 
                    MensagemErro = "Email ou Palavra-passe incorreta." 
                };
            }

            var token = GerarTokenJwt(user);

            // ⚠️ SUCESSO: Devolvemos o DTO completo com todos os dados que a UI precisa!
            return new AuthResponseDto
            {
                Sucesso = true,
                Token = token,
                DataExpiracao = DateTime.UtcNow.AddDays(7), // Alinha com o tempo de expiração do teu JWT
                UserId = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Perfil = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl ?? string.Empty,
                MensagemErro = string.Empty
            };
        }

        public async Task<(bool Sucesso, string Token, string Perfil, string MensagemErro)> SyncFirebaseAsync(string firebaseUid, string email, string nome)
        {
            // Verifica se o utilizador já existe pelo UID do Firebase
            var existingUser = await _repository.GetByFirebaseUidAsync(firebaseUid);

            if (existingUser != null)
            {
                var tokenExistente = GerarTokenJwt(existingUser);
                return (true, tokenExistente, existingUser.Role.ToString(), string.Empty);
            }

            // Se não existir, verifica se já existe uma conta com esse E-mail (para ligar as contas)
            if (!string.IsNullOrWhiteSpace(email))
            {
                var userPorEmail = await _repository.GetByEmailAsync(email);
                if (userPorEmail != null)
                {
                    // Atualiza o utilizador existente adicionando o FirebaseUid
                    userPorEmail.FirebaseUid = firebaseUid;
                    // Como o EF Core rastreia a entidade, basta guardar (ou criar um método UpdateAsync no repo se preferires)
                    await _repository.AddAsync(userPorEmail); 
                    var tokenEmail = GerarTokenJwt(userPorEmail);
                    return (true, tokenEmail, userPorEmail.Role.ToString(), string.Empty);
                }
            }

            // É um utilizador 100% novo via Firebase! Criamos no PostgreSQL através do Repositório.
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = firebaseUid,
                Email = email ?? string.Empty,
                Nome = string.IsNullOrWhiteSpace(nome) ? "Novo Utilizador" : nome,
                Role = UserRole.Gestor, // Valor padrão 
                CreatedAt = DateTime.UtcNow,
                PasswordHash = string.Empty // O Firebase gere a password
            };

            var createdUser = await _repository.AddAsync(newUser);
            var novoToken = GerarTokenJwt(createdUser);

            return (true, novoToken, createdUser.Role.ToString(), string.Empty);
        }
        public string GerarTokenJwt(User user)
        {
            var chaveSecreta = _configuration["Jwt:Key"] ?? "CareConnect_Super_Secret_Key_1234567890_!@#$";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
            var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role.ToString()),
                new Claim("firebaseUid", user.FirebaseUid ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: "CareConnectAPI",
                audience: "CareConnectMobile",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // 7 Dias exatos para persistência no Mobile
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}