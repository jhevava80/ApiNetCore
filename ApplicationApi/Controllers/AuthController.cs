using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApplicationApi.Data;
using ApplicationApi.Dtos;
using ApplicationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthRepository authRepository;
        private readonly IConfiguration config;
        public AuthController(IAuthRepository authRepository, IConfiguration config )
        {
            this.authRepository = authRepository;
            this.config = config;
        }

        //[FromBody] is not necessary because  [ApiController] is used - public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto)
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //Validate the request
            userForRegisterDto.username = userForRegisterDto.username.ToLower();
            
            if(await this.authRepository.UserExists(userForRegisterDto.username))
            {
                return BadRequest("User already exists");
            }

            var userToCreate = new User
            {
                Username = userForRegisterDto.username
            };

            var createdUser = await this.authRepository.Register(userToCreate, userForRegisterDto.password);
            return StatusCode(201);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await this.authRepository.Login(userForLoginDto.username, userForLoginDto.password);

            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config.GetSection("AppSettings:Token").Value ));

            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor 
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(12),
                SigningCredentials =creds                
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { 
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}