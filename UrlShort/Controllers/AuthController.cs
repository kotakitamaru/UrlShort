using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UrlShort.Models;

namespace UrlShort.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        // POST: api/Auth
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserDto request)
        {
            CreatePasswordHash(request.Password,out byte[] passwordHash, out byte[] passwordSalt);
    
            await using var context = new CosmosDBContext();
            if (context.Users.ToList().Exists(x => x.Username == request.Username))
            {
                return Conflict();
            }
            
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            context.Users?.Add(newUser);
            await context.SaveChangesAsync();
            var token = CreateToken(newUser);
            return Ok(new
            {
                token = token,
                isAdmin = newUser.IsAdmin
            });
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            using var dbContext = new CosmosDBContext();
            
            if (dbContext.Users.IsNullOrEmpty() || !dbContext.Users.ToList().Exists(x => x.Username == request.Username))
            {
                return BadRequest("User not found");
            }
            User? newUser = dbContext.Users.First(x => x.Username == request.Username);
            
            if (!VerifyPasswordHash(request.Password, newUser.PasswordHash, newUser.PasswordSalt))
            {
                return BadRequest("Wrong password");
            }

            var token = CreateToken(newUser);
            return Ok(new
            {
                token = token,
                isAdmin = newUser.IsAdmin
            });
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("username", user.Username),
                new Claim("isAdmin",user.IsAdmin.ToString())
            };
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings:Key").Value!));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
