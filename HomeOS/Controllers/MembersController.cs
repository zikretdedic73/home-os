using HomeOS.Data;
using HomeOS.Models.Households;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

// Shell-owned household member management (Docs/01_Roadmap.md, section 3.4).
// Only the household owner may manage members. Inviting by e-mail creates a
// "pending" member and sends an invite e-mail (reusing the platform IEmailSender);
// the person is linked to the household on registration (see CurrentHouseholdService).
[Authorize]
public class MembersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly IEmailSender _emailSender;
    private readonly IStringLocalizer<MembersController> _localizer;

    public MembersController(ApplicationDbContext context, ICurrentHouseholdService household, IEmailSender emailSender, IStringLocalizer<MembersController> localizer)
    {
        _context = context;
        _household = household;
        _emailSender = emailSender;
        _localizer = localizer;
    }

    // GET: /Members
    public async Task<IActionResult> Index()
    {
        if (!await _household.IsCurrentMemberOwnerAsync())
            return Forbid();

        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var members = await _context.Members
            .Where(m => m.HouseholdId == householdId)
            .OrderByDescending(m => m.IsOwner)
            .ThenBy(m => m.DisplayName)
            .ToListAsync();

        return View(members);
    }

    // POST: /Members/Invite
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(string email)
    {
        if (!await _household.IsCurrentMemberOwnerAsync())
            return Forbid();

        var householdId = await _household.GetCurrentHouseholdIdAsync();
        email = email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = _localizer["EmailRequiredMessage"].Value;
            return RedirectToAction(nameof(Index));
        }

        var alreadyMember = await _context.Members
            .AnyAsync(m => m.HouseholdId == householdId && m.Email == email);

        if (alreadyMember)
        {
            TempData["Error"] = _localizer["AlreadyMemberMessage"].Value;
            return RedirectToAction(nameof(Index));
        }

        _context.Members.Add(new Member
        {
            HouseholdId = householdId,
            IdentityUserId = string.Empty,
            Email = email,
            DisplayName = email,
            IsOwner = false
        });
        await _context.SaveChangesAsync();

        await SendInviteEmailAsync(email);

        TempData["Success"] = _localizer["MemberInvitedMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // POST: /Members/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        if (!await _household.IsCurrentMemberOwnerAsync())
            return Forbid();

        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id && m.HouseholdId == householdId);

        // The owner cannot be removed (would leave the household without an admin).
        if (member != null && !member.IsOwner)
        {
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            TempData["Success"] = _localizer["MemberRemovedMessage"].Value;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SendInviteEmailAsync(string email)
    {
        var registerUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/Register";
        var subject = _localizer["InviteEmailSubject"].Value;
        var body = string.Format(_localizer["InviteEmailBody"].Value, email, registerUrl);
        await _emailSender.SendEmailAsync(email, subject, body);
    }
}
