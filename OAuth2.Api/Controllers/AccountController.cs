using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OAuth2.Api.Database;
using OAuth2.Api.Models;
using OAuth2.Api.Models.Dto;
using OAuth2.Api.Models.Vm;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OAuth2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private UserManager<User> _userManager;
        public SignInManager<User> _signInManager { get; set; }
        private readonly JwtModel jwtModel;


        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<JwtModel> options
)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            jwtModel = options.Value;

        }


        [HttpPost("Register")]
        public async Task<IActionResult> CreateUserAsync(RegisterDto registerDto)
        {
            User user = new User
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,

            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);


            if (result.Succeeded)
            {
                return Ok();

            }
            return BadRequest();


        }

        [HttpPost("signIn")]
        public async Task<IActionResult> CreateLoginTokenAsync(LoginDto loginDto)
        {
            UserRepository userRepository = new();

            var user = await userRepository.GetUserByUserNameAsync(loginDto.Email);

            var result = await _signInManager.CheckPasswordSignInAsync(user: user, password: loginDto.Password, false);


            if (result.Succeeded)
            {
                // JWT'nin oluşturulması
                var issuer = jwtModel.Issuer;
                var audience = jwtModel.Audience;
                var key = Encoding.ASCII.GetBytes
                (jwtModel.Key);


                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)                
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                var stringToken = tokenHandler.WriteToken(token);




                return Ok(stringToken);
            }
            return BadRequest(result);


        }


    }
}
