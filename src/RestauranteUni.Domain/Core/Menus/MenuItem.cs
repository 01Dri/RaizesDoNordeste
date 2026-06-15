using RestauranteUni.Domain.Core.Orders;

namespace RestauranteUni.Domain.Core.Menus;

public class MenuItem : BaseDomain<long>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int DisplayOrder { get; set; }

    public int PreparationTimeInMinutes { get; set; }

    public bool IsFeatured { get; set; }

    public long? MenuId { get; set; }
    public virtual Menu? Menu { get; set; }

    public virtual ICollection<MenuItemIngredient> Ingredients { get; set; } = [];
    
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}