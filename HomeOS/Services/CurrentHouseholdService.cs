using System.Security.Claims;
using HomeOS.Data;
using HomeOS.Models.Households;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Services;

// Simplified for Day 1 / this test scope: the first time a signed-in Identity
// user shows up, a Member is created automatically (and a Household too, if
// none exists yet - a single household for this scope). A full "invite" flow
// for adding members is V2, per Docs/01_Roadmap.md.
public class CurrentHouseholdService : ICurrentHouseholdService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentHouseholdService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> GetCurrentMemberIdAsync()
    {
        var member = await GetOrCreateMemberAsync();
        return member.Id;
    }

    public async Task<int> GetCurrentHouseholdIdAsync()
    {
        var member = await GetOrCreateMemberAsync();
        return member.HouseholdId;
    }

    private async Task<Member> GetOrCreateMemberAsync()
    {
        var identityUserId = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(identityUserId))
            throw new InvalidOperationException(
                "No signed-in user - this method is only called from within [Authorize] actions.");

        var existing = await _context.Members
            .FirstOrDefaultAsync(m => m.IdentityUserId == identityUserId);

        if (existing != null)
            return existing;

        // Prva prijava ovog korisnika - kreiraj domaćinstvo ako ne postoji, pa člana.
        var household = await _context.Households.FirstOrDefaultAsync();
        if (household == null)
        {
            household = new Household { Name = "My Household" };
            _context.Households.Add(household);
            await _context.SaveChangesAsync();
        }

        var displayName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Member";

        var member = new Member
        {
            HouseholdId = household.Id,
            IdentityUserId = identityUserId,
            DisplayName = displayName
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return member;
    }
}
