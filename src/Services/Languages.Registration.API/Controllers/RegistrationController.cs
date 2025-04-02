﻿using FluentValidation.Results;
using Languages.Registration.API.Configuration;
using Languages.Registration.API.Repositories.Contracts;
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
        private readonly IAppUserRepository _applicationUserRepository;
        private readonly UserManager<MongoUser> _userManager;

        public RegistrationController(IAppUserRepository applicationUserRepository, UserManager<MongoUser> userManager)
        {
            _applicationUserRepository = applicationUserRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var aspNetUser = await _userManager.GetUserAsync(User);

            var user = await _applicationUserRepository.GetAsync(aspNetUser.Id);

            if (user == null) return NotFound();

            return Ok(user.ToResponseAppUser());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(RegisterAppUserViewModel model)
        {
            var aspNetUser = await _userManager.GetUserAsync(User);

            var appUser = model.ToAppUser(aspNetUser.Id);

            if (!appUser.IsValid())
                return BadRequestResponse(appUser.ValidationResult);

            await _applicationUserRepository.AddAsync(appUser);

            return Ok(appUser.ToResponseAppUser());
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync(UpdateAppUserViewModel model)
        {
            var aspNetUser = await _userManager.GetUserAsync(User);

            var appUser = await _applicationUserRepository.GetAsync(aspNetUser.Id);

            appUser.Name = model.Name;
            appUser.AboutMe = model.AboutMe;
            appUser.BirthDate = model.BirthDate;
            appUser.City = model.City;

            if (!appUser.IsValid())
                return BadRequestResponse(appUser.ValidationResult);

            await _applicationUserRepository.UpdateAsync(appUser);

            return Ok(appUser.ToResponseAppUser());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync()
        {
            var aspNetUser = await _userManager.GetUserAsync(User);

            var user = await _applicationUserRepository.GetAsync(aspNetUser.Id);

            if (user == null) return NotFound();

            await _applicationUserRepository.DeleteAsync(aspNetUser.Id);
            await _userManager.DeleteAsync(aspNetUser);

            return NoContent();
        }

        private IActionResult BadRequestResponse(ValidationResult validationResult) =>
            new BadRequestObjectResult(new { Success = false, Errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        //private IActionResult BadRequestResponse(string ErrorMessage) =>
        //   new BadRequestObjectResult(new { Success = false, Errors = new string[] { ErrorMessage } });
    }
}
