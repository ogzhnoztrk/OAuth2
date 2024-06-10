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

        [HttpPost("login")]
        public async Task<IActionResult> CreateLoginTokenAsync(LoginDto loginDto)
        {
            //kullanıcı kontrolu
            UserRepository userRepository = new();
            var user = await userRepository.GetUserByUserNameAsync(loginDto.Email);


            if (user != null)
            {

                var result = await _signInManager.CheckPasswordSignInAsync(user: user, password: loginDto.Password, false);


                if (result.Succeeded)
                {
            

                    var token = GenerateAccessToken(user.UserName, user.Id.ToString());
                    return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });


                }
                return BadRequest(result);

            }


            return BadRequest("Böyle Bir Kullanıcı Yok");

        }



        #region
        // Generating token based on user information
        private JwtSecurityToken GenerateAccessToken(string userName, string userId)
        {
            // Create user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userId),
            };

            // JWT'nin oluşturulması
            var issuer = jwtModel.Issuer;
            var audience = jwtModel.Audience;
            var key = Encoding.ASCII.GetBytes(jwtModel.Key);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
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



            return token as JwtSecurityToken;
        }
        #endregion

    }
}
