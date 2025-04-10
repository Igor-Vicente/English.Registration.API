using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace English.Registration.API.Configuration
{
    public class CustomAuthorize
    {
        public class ClaimsAuthorizeAttribute : TypeFilterAttribute
        {
            public ClaimsAuthorizeAttribute(string claimName, string claimType) : base(typeof(RequirementClaimFilter))
            {
                Arguments = new[] { new Claim(claimName, claimType) };
            }
        }

        public class RequirementClaimFilter : IAuthorizationFilter
        {
            private readonly Claim _claim;

            public RequirementClaimFilter(Claim claim)
            {
                _claim = claim;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    context.Result = new StatusCodeResult(401);
                    return;
                }
                if (!ValidateUserAccountClaims(context.HttpContext, _claim.Type, _claim.Value))
                {
                    context.Result = new StatusCodeResult(403);
                }
            }
        }
        public static bool ValidateUserAccountClaims(HttpContext httpContext, string claimName, string claimValue)
        {
            return httpContext.User.Identity.IsAuthenticated &&
                httpContext.User.Claims.Any(c => c.Type == claimName && c.Value == claimValue);
        }
    }
}
