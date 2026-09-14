namespace OrderService.DTOs;

public class GetOrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<GetOrderItemDto> Items { get; set; } = new();
}