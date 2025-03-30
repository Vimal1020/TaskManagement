using System.Security.Claims;

namespace TaskManagement.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            if (user == null)
            {
                throw new UnauthorizedAccessException("User context is not available");
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new UnauthorizedAccessException("User ID claim not found");

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID format");
            }

            return userId;
        }
    }
}