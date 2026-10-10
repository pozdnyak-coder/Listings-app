namespace ListingsApi.Data.Entities;

public class Listing
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public int Rooms { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }

    public int DistrictId { get; set; }        // внешний ключ
    public District? District { get; set; }    // навигационное свойство
}
