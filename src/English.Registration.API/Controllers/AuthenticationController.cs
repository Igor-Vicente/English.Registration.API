using English.Registration.API.Configuration;
using English.Registration.API.Extensions;
using English.Registration.API.Repositories.Contracts;
using English.Registration.API.Services;
using English.Registration.API.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Store.MongoDb.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace English.Registration.API.Controllers
{
    [ApiController]
    [Route("api/v1/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<MongoUser> _userManager;
        private readonly SignInManager<MongoUser> _signInManager;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IEmailSender _emailSender;
        private readonly JwtOptions _jwt;

        public AuthenticationController(UserManager<MongoUser> userManager,
                                        SignInManager<MongoUser> signInManager,
                                        IAppUserRepository appUserRepository,
                                        IOptions<JwtOptions> jwtOptions,
                                        IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appUserRepository = appUserRepository;
            _jwt = jwtOptions.Value;
            _emailSender = emailSender;
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

            var at = await GenerateAccessTokenAsync(aspNetUser);
            var rt = await GenerateRefreshTokenAsync(aspNetUser);
            var appUser = await _appUserRepository.GetAsync(aspNetUser.Id);

            return Ok(new JwtResponseViewModel(at, rt, appUser.ToResponseAppUser()));
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInViewModel model)
        {
            if (!ModelState.IsValid) return BadRequestResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);

            if (result.Succeeded)
            {
                var aspNetUser = await _userManager.FindByEmailAsync(model.Email);
                var at = await GenerateAccessTokenAsync(aspNetUser);
                var rt = await GenerateRefreshTokenAsync(aspNetUser);
                var appUser = await _appUserRepository.GetAsync(aspNetUser.Id);
                return Ok(new JwtResponseViewModel(at, rt, appUser.ToResponseAppUser()));
            }

            if (result.IsLockedOut)
                return BadRequestResponse("User temporarily blocked due to invalid attempts");

            return BadRequestResponse("Invalid username or password");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenViewModel model)
        {
            if (!ModelState.IsValid) return BadRequestResponse(ModelState);

            if (!ValidRefreshToken(model.RefreshToken, out var validatedToken))
                return BadRequestResponse("Invalid Token");

            var email = validatedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            var aspNetUser = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(aspNetUser);
            var jti = validatedToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (!claims.Any(c => c.Type == "LastRefreshToken" && c.Value == jti))
                return BadRequestResponse("Expired token");

            if (aspNetUser.LockoutEnabled)
                if (aspNetUser.LockoutEnd < DateTime.Now)
                    return BadRequestResponse("User blocked");

            var at = await GenerateAccessTokenAsync(aspNetUser);
            var rt = await GenerateRefreshTokenAsync(aspNetUser);

            return Ok(new JwtResponseViewModel(at, rt));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) // Don't reveal that the user does not exist
                return Ok();

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var userEncoded = WebUtility.UrlEncode(user.Id.ToString());
            var codeEncoded = WebUtility.UrlEncode(code);
            var callbackUrl = $"{_jwt.Audience}/new-password?code={codeEncoded}&user={userEncoded}";

            await _emailSender.SendEmailResetPasswordAsync(model.Email, callbackUrl);

            return Ok();
        }

        [HttpPost("new-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse(ModelState);

            if (!ObjectId.TryParse(model.UserId, out var userId))
                return BadRequestResponse("The link used is not valid");

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return BadRequestResponse("The link used is not valid");

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (!result.Succeeded)
                return BadRequestResponse(result);

            return Ok();
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

        private async Task<string> GenerateAccessTokenAsync(MongoUser aspNetUser)
        {
            var claims = await _userManager.GetClaimsAsync(aspNetUser);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, aspNetUser.Id.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, aspNetUser.Email));
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

        private async Task<string> GenerateRefreshTokenAsync(MongoUser aspNetUser)
        {
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, aspNetUser.Email),
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

            await UpdateLastGeneratedRtClaim(aspNetUser.Email, jti);
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
