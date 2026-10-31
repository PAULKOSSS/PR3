using Microsoft.EntityFrameworkCore;
using PR3.Models;
using static PR3.DTOs.ClothesDtos;

namespace PR3.Endpoints
{
    public class ClothesEndpoints
    {
        public static void MapClothes(WebApplication app)
        {
            app.MapGet("/api/clothes", async (ISRPO_PR3Context context, string? search) =>
            {
                var query = context.Clothes.Where(c => c.IsAvailable); 

                if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Name.Contains(search));

                var clothesList = await query.Select(c => new ClothesDTO(c.Id, c.Name, c.Price, c.ClothingSizes.Select(cs => new SizeDTO(cs.Id, cs.Size.LetterSize, cs.QuantityInStock)).ToList())).ToListAsync();

                return Results.Ok(clothesList);

            }).AllowAnonymous();

            app.MapPatch("/api/clothes/{id}/availability", async (int id, UpdateAvailabilityRequest request, ISRPO_PR3Context context) => 
            {
                var clothes = await context.Clothes.FirstOrDefaultAsync(c => c.Id == id);
                if (clothes == null) return Results.NotFound("Товар не найден");

                clothes.IsAvailable = request.IsAvailable;

                await context.SaveChangesAsync();
                return Results.NoContent();

            }).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Manager"));

            app.MapPatch("/api/clothing-sizes/{id:int}", async (int id, UpdateStockRequest request, ISRPO_PR3Context context) =>
            {
                if (request.Quantity < 0) return Results.BadRequest("Количество не может быть отрицательным");

                var clothingSize = await context.ClothingSizes.FirstOrDefaultAsync(cs => cs.Id == id);
                if (clothingSize == null) return Results.NotFound("Запись размера не найдена");

                clothingSize.QuantityInStock = request.Quantity;
                await context.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Manager"));
        }
    }
}
