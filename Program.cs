using PR3.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

AuthEndpoints.MapAuth(app);
ClothesEndpoints.MapClothes(app);
OrderEndpoints.MapOrders(app);

