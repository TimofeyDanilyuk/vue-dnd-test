using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using server.Data;
using Microsoft.EntityFrameworkCore;
using server.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace server.Controllers
{
    /// <summary>
    /// Управление аутентификацией пользователя
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>Регистрация нового пользователя</summary>
        /// <param name="dto">Email и пароль</param>
        /// <returns>JWT токен</returns>
        /// <response code="200">Пользователь создан, возвращает токен</response>
        /// <response code="400">Пользователь с таким email уже существует</response>
        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Пользователь с таким email уже существует");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new TokenResponse { Token = GenerateToken(user) });
        }


        /// <summary>Вход в аккаунт</summary>
        /// <param name="dto">Email и пароль</param>
        /// <returns>JWT токен</returns>
        /// <response code="200">Успешный вход, возвращает токен</response>
        /// <response code="401">Неверный email или пароль</response>
        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Неверный email или пароль");

            return Ok(new TokenResponse { Token = GenerateToken(user) });
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /// <summary>Данные для входа или регистрации</summary>
    public class AuthDto
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        /// <example>
        /// user@example.com
        /// </example>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Пароль
        /// </summary>
        /// <example>
        /// qwerty123
        /// </example>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Ответ с JWT токеном</summary>
    public class TokenResponse
    {
        /// <summary>JWT токен для авторизации</summary>
        public string Token { get; set; } = string.Empty;
    }
}