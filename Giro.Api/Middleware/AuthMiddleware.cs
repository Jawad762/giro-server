using Giro.Api.Models;
using Jose;
using System.Text.Json;

namespace Giro.Api.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secretKey;

        public AuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _secretKey = config["JwtSecret"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ")[1];

            if (token != null)
            {
                try
                {
                    var secretKey = Convert.FromBase64String(_secretKey);

                    var payload = JWT.Decode<Dictionary<string, object>>(token, secretKey);

                    if (payload.ContainsKey("issuedAt"))
                    {
                        var issuedAt = Convert.ToInt64(payload["issuedAt"]);
                        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                        var differenceInSeconds = (currentTime - issuedAt);

                        if (differenceInSeconds > 600)
                        {
                            context.Response.StatusCode = 401;
                            var res = new ApiErrorResponse<string>
                            {
                                Status = "error",
                                ErrorMessage = "unauthorized",
                                ErrorDetails = "Token has expired"
                            };
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonSerializer.Serialize(res));
                            return;
                        }
                    }

                    context.Items["User"] = new UserModel
                    {
                        Id = Convert.ToInt32(payload["id"]),
                        Email = payload["email"].ToString(),
                        Role = payload["role"].ToString()
                    };
                }
                catch (Exception)
                {
                    context.Response.StatusCode = 401;
                    var res = new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorMessage = "unauthorized"
                    };
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(res));
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                var res = new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "unauthorized"
                };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(res));
                return;
            }

            await _next(context);
        }
    }
}
