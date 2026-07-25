using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.LifeAdmin;

// An important household contact (plumber, doctor, landlord, ...). Extends
// BaseEntity for household isolation + visibility.
public class Contact : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Role { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
