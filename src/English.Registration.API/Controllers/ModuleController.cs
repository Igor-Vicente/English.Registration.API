using FluentValidation.Results;
using English.Registration.API.Repositories.Contracts;
using English.Registration.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static English.Registration.API.Configuration.CustomAuthorize;
using English.Registration.API.Extensions;

namespace English.Registration.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/modules")]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleRepository _moduleRepository;

        public ModuleController(IModuleRepository moduleRepository)
        {
            _moduleRepository = moduleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetModulesAsync()
        {
            var modules = await _moduleRepository.GetAsync();
            return Ok(modules.ToResponseModule());
        }

        [HttpPost]
        [ClaimsAuthorize("isAdmin", "true")]
        public async Task<IActionResult> PostModuleAsync(AddModuleViewModel model)
        {
            var module = model.ToNewModule();

            if (!module.IsValid())
                return BadRequestResponse(module.ValidationResult);

            await _moduleRepository.AddAsync(module);

            return Ok(module.ToResponseModule());
        }


        private IActionResult BadRequestResponse(ValidationResult validationResult) =>
          new BadRequestObjectResult(new { Success = false, Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
    }
}
