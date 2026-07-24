namespace HomeOS.Models.Households;

public class Member
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Household? Household { get; set; }

    // Link to the ASP.NET Core Identity account (AspNetUsers.Id) - Identity remains
    // the single source for login/password, Member holds everything Home OS specific.
    // Empty while a member is "pending" (invited by e-mail but not yet registered).
    public string IdentityUserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    // E-mail the member was invited with / registered with. Used to link a
    // pending invite to the Identity account on first login, and for the
    // invite / owner-notification e-mails (Docs/01_Roadmap.md, section 3.4).
    public string? Email { get; set; }

    // First member of a household is its owner - only the owner manages
    // members and per-member module access.
    public bool IsOwner { get; set; }

    public string? PreferredCulture { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

    // A member invited but not yet registered has no Identity account yet.
    public bool IsPending => string.IsNullOrEmpty(IdentityUserId);
}
