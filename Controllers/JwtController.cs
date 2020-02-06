using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using JwtApplication.Data;
using JwtApplication.Models;

namespace JwtApplication.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController, Authorize]
    public class JwtController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _context;
        public JwtController(IConfiguration configuration, DatabaseContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [HttpPost, AllowAnonymous]
        public string GetToken([FromBody] User user)
        {
            var u = _context.Users.FirstOrDefault(x => x.Username.Equals(user.Username));
            if (u == null || !BCrypt.Net.BCrypt.Verify(user.Password, u.Password))
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, u.Email),
                new Claim(JwtRegisteredClaimNames.Sub, u.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:Expire"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}