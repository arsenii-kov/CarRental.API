using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Settings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using CarRental.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CarRental.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IOptions<JwtSettings> _options;
        public AuthService(AppDbContext context, IOptions<JwtSettings> options)
        {
            _context = context;
            _options = options;
        }

        private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
        {
            var jwtSettings = _options.Value;

            // 1. Полезная нагрузка (Claims) — данные, зашитые в токен
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // 2. Секретный ключ и учетные данные для подписи
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Время окончания действия
            var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryInMinutes);

            // 4. Описание структуры токена
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                SigningCredentials = creds
            };

            // 5. Создание и сериализация токена в строку
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            return (tokenString, expiresAt);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var isExist = await _context.Users.AnyAsync(u => u.Email == request.Email);
            
            if (isExist)
            {
                throw new Exception("User with this email already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = UserRole.Customer
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (token, expiresAt) = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var passwordVerify = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!passwordVerify)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var (token, expiresAt) = GenerateJwtToken(user);
            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }




    }
}
