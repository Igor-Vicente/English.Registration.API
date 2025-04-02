using Languages.Registration.API.Configuration;
using Languages.Registration.API.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Store.MongoDb.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Languages.Registration.API.Controllers
{
    [ApiController]
    [Route("api/v1/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<MongoUser> _userManager;
        private readonly SignInManager<MongoUser> _signInManager;
        private readonly JwtOptions _jwt;

        public AuthenticationController(UserManager<MongoUser> userManager, SignInManager<MongoUser> signInManager, IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwtOptions.Value;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(SignUpViewModel model)
        {
            if (!ModelState.IsValid) return BadRequestResponse(ModelState);

            var aspNetUser = new MongoUser()
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(aspNetUser, model.Password);

            if (!result.Succeeded)
                return BadRequestResponse(result);

            var at = await GenerateAccessTokenAsync(model.Email);
            var rt = await GenerateRefreshTokenAsync(model.Email);

            return Ok(new JwtResponseViewModel(at, rt));
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInViewModel model)
        {
            if (!ModelState.IsValid) return BadRequestResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);

            if (result.Succeeded)
            {
                var at = await GenerateAccessTokenAsync(model.Email);
                var rt = await GenerateRefreshTokenAsync(model.Email);

                return Ok(new JwtResponseViewModel(at, rt));
            }

            if (result.IsLockedOut)
                BadRequestResponse("Usuário temporariamente bloaqueado por tentativas inválidas");

            return BadRequestResponse("Usuário ou Senha inválido");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenViewModel model)
        {
            if (!ModelState.IsValid) return BadRequestResponse(ModelState);

            if (!ValidRefreshToken(model.RefreshToken, out var validatedToken))
                return BadRequestResponse("Invalid Token");

            var email = validatedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var jti = validatedToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (!claims.Any(c => c.Type == "LastRefreshToken" && c.Value == jti))
                return BadRequestResponse("Expired token");

            if (user.LockoutEnabled)
                if (user.LockoutEnd < DateTime.Now)
                    return BadRequestResponse("User blocked");

            var at = await GenerateAccessTokenAsync(email);
            var rt = await GenerateRefreshTokenAsync(email);

            return Ok(new JwtResponseViewModel(at, rt));
        }

        private bool ValidRefreshToken(string refreshToken, out JwtSecurityToken validatedToken)
        {
            try
            {
                var validationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = _jwt.Issuer,
                    ValidAudience = _jwt.Audience,
                    RequireSignedTokens = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwt.SecretKey)),
                };

                new JwtSecurityTokenHandler().ValidateToken(refreshToken, validationParameters, out var securityToken);
                validatedToken = (JwtSecurityToken)securityToken;
                return true;
            }
            catch
            {
                validatedToken = null;
                return false;
            }
        }

        private async Task<string> GenerateAccessTokenAsync(string? email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                SigningCredentials = GetCurrentSigningCredentials(),
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(60),
                IssuedAt = DateTime.UtcNow,
                TokenType = "at+jwt"
            });

            return tokenHandler.WriteToken(token);
        }

        private async Task<string> GenerateRefreshTokenAsync(string? email)
        {
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            var handler = new JwtSecurityTokenHandler();

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                SigningCredentials = GetCurrentSigningCredentials(),
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.Now,
                Expires = DateTime.Now.AddDays(30),
                TokenType = "rt+jwt"
            });

            await UpdateLastGeneratedRtClaim(email, jti);
            return handler.WriteToken(securityToken);
        }

        private async Task UpdateLastGeneratedRtClaim(string? email, string jti)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var newLastRtClaim = new Claim("LastRefreshToken", jti);

            var claimLastRt = claims.FirstOrDefault(f => f.Type == "LastRefreshToken");
            if (claimLastRt != null)
                await _userManager.ReplaceClaimAsync(user, claimLastRt, newLastRtClaim);
            else
                await _userManager.AddClaimAsync(user, newLastRtClaim);
        }

        private SigningCredentials GetCurrentSigningCredentials()
        {
            var secretKey = Encoding.ASCII.GetBytes(_jwt.SecretKey);
            return new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);
        }

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        private IActionResult BadRequestResponse(ModelStateDictionary modelState) =>
            new BadRequestObjectResult(new { Success = false, Errors = modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });

        private IActionResult BadRequestResponse(IdentityResult identityResult) =>
           new BadRequestObjectResult(new { Success = false, Errors = identityResult.Errors.Select(x => x.Description) });

        private IActionResult BadRequestResponse(string ErrorMessage) =>
           new BadRequestObjectResult(new { Success = false, Errors = new string[] { ErrorMessage } });
    }
}
