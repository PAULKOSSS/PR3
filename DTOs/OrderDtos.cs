namespace PR3.DTOs
{
    public class OrderDtos
    {
        public record OrderItemRequest(int ClothingSizeId, int Quantity) { }
        public record CreateOrderRequest(List<OrderItemRequest> Items);
        public record OrderItemResponse(string ClothingName, string Size, int Quantity, decimal UnitPrice);
        public record OrderResponse(int Id, DateTime CreatedAt, string Status, decimal TotalPrice, List<OrderItemResponse> Items);
        public record UpdateOrderStatusRequest(string Status);

    }
}
