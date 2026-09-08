using System.Security.Claims;
using PharmacyManagementSystem.Web.DataStorage;

namespace PharmacyManagementSystem.Web.Authorization
{
    public static class PermissionChecker
    {
        public static bool HasPermission(
            ClaimsPrincipal user,
            string permission)
        {
            if (user == null)
            {
                return false;
            }

            if (user.Identity == null ||
                !user.Identity.IsAuthenticated)
            {
                return false;
            }

            string? userId =
                user.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(
                userId,
                out int id))
            {
                return false;
            }

            return UserStorage.HasPermission(
                id,
                permission);
        }
    }
}