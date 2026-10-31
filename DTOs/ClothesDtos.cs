namespace PR3.DTOs
{
    public class ClothesDtos
    {
        public record SizeDTO(int Id, string LetterSize, int QuantityInStock) { }
        public record Clothes(int Id, string Name, decimal Price, List<SizeDTO> Sizes) { }
        public record UpdateStockRequest(int Quantity);
        public record UpdateAvailabilityRequest(bool IsAvailable);
    }
}
