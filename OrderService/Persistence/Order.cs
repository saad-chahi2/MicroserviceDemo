namespace OrderService.Persistence;

public class Order
{
    public int Id { get; set; }           
    public int CustomerId { get; set; }     
    public string ProductName { get; set; }  
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
}
