using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using PR3;
using PR3.Endpoints;
using PR3.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ISRPO_PR3Context>();

var hasher = new PasswordHasher<User>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = JwtHelper.validation;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

AuthEndpoints.MapAuth(app, hasher);
ClothesEndpoints.MapClothes(app);
OrderEndpoints.MapOrders(app);

app.Run();

