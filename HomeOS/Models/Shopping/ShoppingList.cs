using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.Shopping;

public class ShoppingList : BaseEntity
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
}
