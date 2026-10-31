namespace PR3.DTOs
{
    public class OrderDtos
    {
        public record OrderItemRequest(int ClothingSizeId, int Quantity) { }
        public record CreateOrderRequest(List<OrderItemRequest> Items);
        public record UpdateOrderStatusRequest(string Status);

    }
}
