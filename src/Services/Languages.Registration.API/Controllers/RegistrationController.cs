using FluentValidation.Results;
using Languages.Registration.API.Configuration;
using Languages.Registration.API.Repositories.Contracts;
using Languages.Registration.API.Services;
using Languages.Registration.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.MongoDb.Identity.Models;

namespace Languages.Registration.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/registration")]
    public class RegistrationController : ControllerBase
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly UserManager<MongoUser> _userManager;
        private readonly IBlobStorageService _blobStorage;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(IAppUserRepository applicationUserRepository,
                                      UserManager<MongoUser> userManager,
                                      IBlobStorageService blobStorageService,
                                      ILogger<RegistrationController> logger)
        {
            _appUserRepository = applicationUserRepository;
            _userManager = userManager;
            _blobStorage = blobStorageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            Thread.Sleep(2000);
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

            if (model.Image == null || model.Image.Length == 0)
                return BadRequestResponse("Provide your profile picture");

            var aspNetUser = await _userManager.GetUserAsync(User);
            var appUser = await _appUserRepository.GetAsync(aspNetUser.Id);

            if (appUser != null)
                return BadRequestResponse("User has already been registered");

            appUser = model.ToAppUser(aspNetUser.Id);

            appUser.ImageUrl = "/imgs/default-profile.png";

            if (!appUser.IsValid())
                return BadRequestResponse(appUser.ValidationResult);

            appUser.ImageUrl = await _blobStorage.UploadAsync(model.Image, $"{Guid.NewGuid()}.png", "image/png");

            await _appUserRepository.AddAsync(appUser);

            return Ok(appUser.ToResponseAppUser());
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync(UpdateAppUserViewModel model)
        {
            var aspNetUser = await _userManager.GetUserAsync(User);

            var appUser = await _appUserRepository.GetAsync(aspNetUser.Id);

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

        private IActionResult BadRequestResponse(ValidationResult validationResult) =>
            new BadRequestObjectResult(new { Success = false, Errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        private IActionResult BadRequestResponse(string ErrorMessage) =>
           new BadRequestObjectResult(new { Success = false, Errors = new string[] { ErrorMessage } });
    }
}
