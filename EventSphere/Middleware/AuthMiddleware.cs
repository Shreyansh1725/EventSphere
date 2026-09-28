namespace EventSphere.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthMiddleware> _logger;

        public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            bool isPublic = path.StartsWith("/account") ||
                            path.StartsWith("/api") ||
                            path == "/" ||
                            path.StartsWith("/home") ||
                            path.StartsWith("/error") ||
                            path.StartsWith("/css") ||
                            path.StartsWith("/js") ||
                            path.StartsWith("/images") ||
                            path.StartsWith("/lib");

            if (isPublic)
            {
                await _next(context);
                return;
            }

            var userId = context.Session.GetInt32("UserId");
            var role = context.Session.GetString("Role");

            if (userId == null)
            {
                _logger.LogInformation("Unauthenticated access to {Path}, redirecting to login", path);
                context.Response.Redirect("/Account/Login?returnUrl=" + Uri.EscapeDataString(context.Request.Path));
                return;
            }

            if (path.StartsWith("/admin") && role != "Admin")
            {
                _logger.LogWarning("Unauthorized access to Admin route: {Path}", path);
                context.Response.Redirect("/Student/Dashboard");
                return;
            }

            if (path.StartsWith("/coordinator") && role != "Coordinator" && role != "Admin")
            {
                _logger.LogWarning("Unauthorized access to Coordinator route: {Path}", path);
                context.Response.Redirect("/Student/Dashboard");
                return;
            }

            await _next(context);
        }
    }

    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}
