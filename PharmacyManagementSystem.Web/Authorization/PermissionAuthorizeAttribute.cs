using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PharmacyManagementSystem.Web.Authorization
{
    public class PermissionAuthorizeAttribute
        : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionAuthorizeAttribute(
            string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(
            AuthorizationFilterContext context)
        {
            // --------------------------------------------------------
            // USER MUST BE LOGGED IN
            // --------------------------------------------------------

            if (context.HttpContext.User == null ||
                context.HttpContext.User.Identity == null ||
                !context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result =
                    new RedirectToActionResult(
                        "Login",
                        "Account",
                        null);

                return;
            }


            // --------------------------------------------------------
            // CHECK PERMISSION
            // --------------------------------------------------------

            bool hasPermission =
                PermissionChecker.HasPermission(
                    context.HttpContext.User,
                    _permission);


            // --------------------------------------------------------
            // DENY ACCESS
            // --------------------------------------------------------

            if (!hasPermission)
            {
                context.Result =
                    new ForbidResult();

                return;
            }
        }
    }
}