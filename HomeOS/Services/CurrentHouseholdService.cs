using System.Security.Claims;
using HomeOS.Data;
using HomeOS.Models.Households;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Services;

// Resolves the current member/household and handles the invite model
// (Docs/01_Roadmap.md, section 3.4):
//  - existing member -> return it;
//  - a pending invite (Member with matching Email, no Identity account yet)
//    -> link it and e-mail the owner that the person joined;
//  - otherwise -> create a new household with this user as its owner.
public class CurrentHouseholdService : ICurrentHouseholdService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IStringLocalizer<CurrentHouseholdService> _localizer;

    public CurrentHouseholdService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        IStringLocalizer<CurrentHouseholdService> localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _emailSender = emailSender;
        _localizer = localizer;
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

    public async Task<bool> IsCurrentMemberOwnerAsync()
    {
        var member = await GetOrCreateMemberAsync();
        return member.IsOwner;
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

        var email = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        // Invited member registering: link the pending Member to this account.
        var pending = string.IsNullOrEmpty(email)
            ? null
            : await _context.Members.FirstOrDefaultAsync(m =>
                m.IdentityUserId == "" && m.Email != null && m.Email == email);

        if (pending != null)
        {
            pending.IdentityUserId = identityUserId;
            pending.DisplayName = string.IsNullOrEmpty(pending.DisplayName) ? (email ?? "Member") : pending.DisplayName;
            pending.JoinedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await NotifyOwnerOfJoinAsync(pending);
            return pending;
        }

        // No invite - this user starts their own household and owns it.
        var household = new Household { Name = "My Household" };
        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            HouseholdId = household.Id,
            IdentityUserId = identityUserId,
            DisplayName = email ?? "Member",
            Email = email,
            IsOwner = true
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return member;
    }

    private async Task NotifyOwnerOfJoinAsync(Member joined)
    {
        var owner = await _context.Members
            .FirstOrDefaultAsync(m => m.HouseholdId == joined.HouseholdId && m.IsOwner);

        var ownerEmail = owner?.Email;
        if (string.IsNullOrEmpty(ownerEmail) && owner != null && !string.IsNullOrEmpty(owner.IdentityUserId))
        {
            var ownerUser = await _userManager.FindByIdAsync(owner.IdentityUserId);
            ownerEmail = ownerUser?.Email;
        }

        if (string.IsNullOrEmpty(ownerEmail))
            return;

        var who = string.IsNullOrEmpty(joined.Email) ? joined.DisplayName : joined.Email;
        var subject = _localizer["MemberJoinedEmailSubject"].Value;
        var body = string.Format(_localizer["MemberJoinedEmailBody"].Value, who);

        await _emailSender.SendEmailAsync(ownerEmail, subject, body);
    }
}
