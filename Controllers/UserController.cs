using ContosoPizza.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContosoPizza.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public UserController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet("Login")]
        public IActionResult Login(string username, string password)
        {
            var exists = _context.Users.SingleOrDefault(u => u.UserName == username && u.PasswordHash == password) != null;

            if (!exists)
                return NotFound();

            return Ok();
        }

        [HttpPost("Register")]
        public IActionResult Create(IdentityUser user)
        {
            var exists = _context.Users.SingleOrDefault(u => u.UserName == user.UserName || u.Email == user.Email) == null;

            if(exists)
                return BadRequest("User with email/username already exists");

            _context.Add(user);

            try{
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
                return Problem(ex.InnerException?.Message ?? ex.Message);
            }

            return CreatedAtAction(nameof(Create), new{id = user.Id}, user);
        }
    }
}