using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.LifeAdmin;

// A household record with metadata only (name, category, optional expiry). The
// actual file upload is out of scope for this test (a FilePath field is left as
// a V2 hook - Docs/01 section 4.2). Extends BaseEntity for isolation/visibility.
// When it has an expiry date, Life Admin publishes an event so Reminders
// schedules a renewal warning - reusing the Reminders module, not duplicating it.
public class Document : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Reserved for a future file upload (V2); metadata-only for now.
    [MaxLength(400)]
    public string? FilePath { get; set; }
}
