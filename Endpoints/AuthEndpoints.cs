using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PR3.Models;
using static PR3.DTOs.AuthDtos;

namespace PR3.Endpoints
{
    public class AuthEndpoints
    {
        public static void MapAuth(WebApplication app, PasswordHasher<User> hasher)
        {
            app.MapPost("/api/auth/register", async (RegisterRequest request, ISRPO_PR3Context context) =>
            {
                if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrEmpty(request.Password)) return Results.BadRequest("Одно из полей не заполнено.");

                if (await context.Users.AnyAsync(u => u.Login == request.Login)) return Results.Conflict("Такой пользователь уже существует.");

                var newUser = new User
                {
                    Login = request.Login,
                    PasswordHash = hasher.HashPassword(null, request.Password),
                    Phone = request.Phone,
                    DeliveryAddress = request.Address,
                    Role = "Customer",
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(newUser);
                await context.SaveChangesAsync();
                return Results.Created($"/api/auth/{newUser.Id}", new { newUser.Id, newUser.Login, newUser.Phone, newUser.DeliveryAddress, newUser.Role });
            }).AllowAnonymous();

            app.MapPost("/api/auth/login", async (LoginRequest request, ISRPO_PR3Context context) =>
            {
                if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrEmpty(request.Password)) return Results.BadRequest("Одно из полей не заполнено.");

                var user = await context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);

                if (user == null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Results.BadRequest("Неверный логин или пароль.");

                var token = JwtHelper.CreateToken(user);
                return Results.Ok(new AuthResponse(token, "Bearer"));
            }).AllowAnonymous();
        }
    }
}
