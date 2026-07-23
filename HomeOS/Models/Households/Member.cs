namespace HomeOS.Models.Households;

public class Member
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Household? Household { get; set; }

    // Link to the ASP.NET Core Identity account (AspNetUsers.Id) - Identity remains
    // the single source for login/password, Member holds everything Home OS specific.
    public string IdentityUserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string? PreferredCulture { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
