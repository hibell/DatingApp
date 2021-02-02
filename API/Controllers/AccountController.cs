using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        // Use HTTP POST to add a new resource through API endpoint.

        // Register method is going to need some parameters that will be received in the POST. The data sent in the POST
        // body will be automatically bound with the ApiController's help.

        // Without the ApiController attribute, we'd need to add add an attribute to each parameter to specify where to
        // get the data from (e.g. FromBody, FromForm, FromHeader, FromQuery, FromRoute, etc.).
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {
            // `using` diposes of hmac correctly after we are doing using it by calling the Dispose() method of the object.
            using var hmac = new HMACSHA512();

            if (await UserExists(dto.Username))
            {
                // ActionResult allows us to send back HTTP status codes too (e.g. HTTP 400).
                return BadRequest("Username is taken");
            }

            var user = new AppUser
            {
                UserName = dto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt = hmac.Key
            };

            // Add just tracks user in EF -- it doesn't actually add anything to the DB.
            _context.Users.Add(user);

            // Now actually save the new user into the DB.
            await _context.SaveChangesAsync();

            return new UserDto {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.UserName == dto.Username.ToLower());

            if (user == null)
                return Unauthorized("Username was not found");

            // Check password
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Wrong username or password");
            }

            return new UserDto {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
        }
    }
}