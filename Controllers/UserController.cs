using System.ComponentModel.DataAnnotations;
using ContosoPizza.Models;
using ContosoPizza.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly JwtService _jwtService;

        public UserController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = new JwtService(config);

            _signInManager.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(string username, string password, bool hashed=true)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usr = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == username);

            if(usr == null)
                return Unauthorized("Invalid login attempt");

            if(!hashed)
            {
                // var result = await _signInManager.PasswordSignInAsync(usr, password, false, true);
                var hashedPassword = _userManager.PasswordHasher.HashPassword(usr, password);
                
                if(usr.UserName != username || usr.PasswordHash != hashedPassword)
                    return Unauthorized("Invalid login attempt");
            }
            else
            {
                var exists = await _userManager.Users.SingleAsync(u => u.UserName == username && u.PasswordHash == password) != null;

                if (!exists)
                    return Unauthorized("Invalid login attempt");

                try
                {
                    await _signInManager.SignInAsync(usr, false);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.InnerException?.Message ?? ex.Message);
                }
            }

            var token = _jwtService.GenerateToken(usr);

            return Ok(new{Bearer=token});

            return Ok("Login successful");
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDTO user)
        {
            var exists = _userManager.Users.SingleOrDefault(u => u.UserName == user.Username || u.Email == user.Email) != null;

            if(exists)
                return BadRequest("Username/email already registered");

            if (!ModelState.IsValid)
            return BadRequest(ModelState);

            var usr = new IdentityUser()
            {
                UserName = user.Username,
                Email = user.Email,
                EmailConfirmed = false,
                LockoutEnabled = true,
            };

            var result = await _userManager.CreateAsync(usr, user.Password);

            usr = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == user.Username);

            if (!result.Succeeded || usr == null)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(Register), new{id = usr.Id}, "Registration successful");
        }

        [Authorize]
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            try{
                await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }

            return Ok("Successfuly logged out");
        }


        public class UserRegisterDTO
        {
            public required string Username { get; set;}
            [EmailAddress]
            public required string Email { get; set;}
            public required string Password { get; set;}
            [Phone]
            public string? Phone{ get; set;}
        }
    }
}