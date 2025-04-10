using FluentValidation.Results;
using English.Registration.API.Models;
using English.Registration.API.Repositories.Contracts;
using English.Registration.API.Services;
using English.Registration.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Store.MongoDb.Identity.Models;
using English.Registration.API.Extensions;

namespace English.Registration.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/registration")]
    public class RegistrationController : ControllerBase
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly UserManager<MongoUser> _userManager;
        private readonly IBlobStorageService _blobStorage;

        public RegistrationController(IAppUserRepository applicationUserRepository,
                                      UserManager<MongoUser> userManager,
                                      IBlobStorageService blobStorageService)
        {
            _appUserRepository = applicationUserRepository;
            _userManager = userManager;
            _blobStorage = blobStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var aspNetUser = await _userManager.GetUserAsync(User);

            var user = await _appUserRepository.GetAsync(aspNetUser.Id);

            if (user == null) return NotFound();

            return Ok(user.ToResponseAppUser());
        }

        [HttpPost]
        [RequestSizeLimit(1 * 1024 * 1024)] // 1MB
        public async Task<IActionResult> CreateAsync(RegisterAppUserViewModel model)
        {
            if (model == null)
                return BadRequestResponse("1MB limit input size");

            if (model.Image == null)
                return BadRequestResponse("Provide your profile picture");

            var aspNetUser = await _userManager.GetUserAsync(User);
            var appUser = await _appUserRepository.GetAsync(aspNetUser.Id);

            if (appUser != null)
                return BadRequestResponse("User has already been registered");

            appUser = model.ToNewAppUser(aspNetUser.Id);

            appUser.ImageUrl = "/imgs/profile.png";

            if (!appUser.IsValid())
                return BadRequestResponse(appUser.ValidationResult);

            appUser.ImageUrl = await _blobStorage.UploadAsync(model.Image, $"{Guid.NewGuid()}.png", "image/png");

            await _appUserRepository.AddAsync(appUser);

            return Ok(appUser.ToResponseAppUser());
        }

        [HttpPut]
        [RequestSizeLimit(1 * 1024 * 1024)] // 1MB
        public async Task<IActionResult> PutAsync(UpdateAppUserViewModel model)
        {
            if (model == null)
                return BadRequestResponse("1MB limit input size");

            var aspNetUser = await _userManager.GetUserAsync(User);
            var appUser = await _appUserRepository.GetAsync(aspNetUser.Id);

            if (appUser == null)
                return BadRequestResponse("User did not complete registration");

            if (model.Image != null)
            {
                var imageUrl = await _blobStorage.UploadAsync(model.Image, $"{Guid.NewGuid()}.png", "image/png");
                await _blobStorage.DeleteAsync(appUser.ImageUrl);
                appUser.ImageUrl = imageUrl;
            }

            appUser.Name = model.Name;
            appUser.AboutMe = model.AboutMe;
            appUser.BirthDate = model.BirthDate;
            appUser.City = model.City;

            if (!appUser.IsValid())
                return BadRequestResponse(appUser.ValidationResult);

            await _appUserRepository.UpdateAsync(appUser);

            return Ok(appUser.ToResponseAppUser());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync()
        {
            var aspNetUser = await _userManager.GetUserAsync(User);

            var user = await _appUserRepository.GetAsync(aspNetUser.Id);

            if (user == null) return NotFound();

            await _appUserRepository.DeleteAsync(aspNetUser.Id);
            await _userManager.DeleteAsync(aspNetUser);

            return NoContent();
        }

        [HttpGet("users/range")]
        public async Task<IActionResult> GetNearbyUsers([FromQuery] Coordinates coordinates, int range = 50000)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse(ModelState);

            var aspNetUser = await _userManager.GetUserAsync(User);
            var appUser = await _appUserRepository.GetAsync(aspNetUser.Id);

            if (appUser == null)
                return BadRequestResponse("User did not complete registration");

            appUser.Location = new Location("Point", coordinates.Latitude, coordinates.Longitude);

            if (!appUser.IsValid())
                return BadRequestResponse(appUser.ValidationResult);

            await _appUserRepository.UpdateAsync(appUser);

            var users = await _appUserRepository.GetUsersInRange(coordinates, range);

            return Ok(users.ToResponseAppUser());
        }

        private IActionResult BadRequestResponse(ModelStateDictionary modelState) =>
           new BadRequestObjectResult(new { Success = false, Errors = modelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });

        private IActionResult BadRequestResponse(ValidationResult validationResult) =>
            new BadRequestObjectResult(new { Success = false, Errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        private IActionResult BadRequestResponse(string ErrorMessage) =>
           new BadRequestObjectResult(new { Success = false, Errors = new string[] { ErrorMessage } });
    }
}
