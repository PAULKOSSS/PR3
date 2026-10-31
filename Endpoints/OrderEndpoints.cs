using Microsoft.EntityFrameworkCore;
using PR3.Models;
using System.Security.Claims;
using static PR3.DTOs.OrderDtos;


namespace PR3.Endpoints
{
    public class OrderEndpoints
    {
        public static void MapOrders(WebApplication app)
        {
            app.MapPost("/api/orders", async (ISRPO_PR3Context context, CreateOrderRequest request, ClaimsPrincipal user) =>
            {
                if (request.Items == null || request.Items.Count == 0) return Results.BadRequest("Корзина не может быть пустой.");

                var userIdClaim = user.FindFirst("sub")?.Value;
                if (!int.TryParse(userIdClaim, out int userId)) return Results.Unauthorized();

                var order = new Order
                {
                    UserId = userId,
                    Status = "Created",
                    CreatedAt = DateTime.UtcNow,
                };

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0) return Results.BadRequest("Количество товара должно быть больше 0.");

                    var clothingSize = await context.ClothingSizes.Include(cs => cs.Clothing).FirstOrDefaultAsync(cs => cs.Id == item.ClothingSizeId);

                    if (clothingSize == null) return Results.BadRequest($"Размер с Id {item.ClothingSizeId} не найден.");
                    if (!clothingSize.Clothing.IsAvailable) return Results.BadRequest($"Товар \"{clothingSize.Clothing.Name}\" снят с продажи.");
                    if (clothingSize.QuantityInStock < item.Quantity) return Results.BadRequest($"Недостаточно товара \"{clothingSize.Clothing.Name}\" на складе.");

                    clothingSize.QuantityInStock -= item.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ClothingSizeId = clothingSize.Id,
                        Quantity = item.Quantity,
                        UnitPrice = clothingSize.Clothing.Price 
                    });
                }

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                return Results.Created($"/api/orders/{order.Id}", new { order.Id, order.Status });
            }).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Customer"));

            app.MapGet("/api/orders/my", async (ISRPO_PR3Context context, ClaimsPrincipal user) =>
            {
                var userIdClaim = user.FindFirst("sub")?.Value;
                if (!int.TryParse(userIdClaim, out int userId)) return Results.Unauthorized();

                var orders = await context.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).Select(o => new OrderResponse(
                    o.Id,
                    o.CreatedAt,
                    o.Status,
                    o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice), o.OrderItems.Select(oi => new OrderItemResponse(
                        oi.ClothingSize.Clothing.Name,
                        oi.ClothingSize.Size.LetterSize,
                        oi.Quantity,
                        oi.UnitPrice)).ToList())).ToListAsync();

                return Results.Ok(orders);
            }).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Customer"));

            app.MapGet("/api/orders", async (ISRPO_PR3Context context, string? status, DateOnly? date) =>
            {
                var query = context.Orders.AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(o => o.Status == status);

                if (date.HasValue)
                {
                    var startOfDay = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                    var endOfDay = startOfDay.AddDays(1);
                    query = query.Where(o => o.CreatedAt >= startOfDay && o.CreatedAt < endOfDay);
                }

                query = query.OrderByDescending(o => o.CreatedAt);

                var allOrders = await query.Select(o => new OrderResponse(
                    o.Id,
                    o.CreatedAt,
                    o.Status,
                    o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice), o.OrderItems.Select(oi => new OrderItemResponse(
                        oi.ClothingSize.Clothing.Name,
                        oi.ClothingSize.Size.LetterSize,
                        oi.Quantity,
                        oi.UnitPrice)).ToList())).ToListAsync();

                return Results.Ok(allOrders);
            }).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Manager"));

            app.MapPatch("/api/orders/{id}/status", async (ISRPO_PR3Context context, UpdateOrderStatusRequest request, int id) =>
            {
                if (string.IsNullOrWhiteSpace(request.Status)) return Results.BadRequest("Статус не может быть пустым.");

                var orders = await context.Orders.FirstOrDefaultAsync(o => o.Id == id);
                if (orders == null) return Results.NotFound("Заказ не найден");

                orders.Status = request.Status;

                await context.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Manager"));
        }
    }
}
