using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.Kanban;

// A Kanban board for one area of the household (e.g. "House renovation").
// Inherits BaseEntity so it is household-scoped and respects visibility.
public class Board : BaseEntity
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Column> Columns { get; set; } = new List<Column>();
}
