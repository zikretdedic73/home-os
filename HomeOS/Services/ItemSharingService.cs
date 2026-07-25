using HomeOS.Data;
using HomeOS.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Services;

public class ItemSharingService : IItemSharingService
{
    private readonly ApplicationDbContext _context;

    public ItemSharingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<int>> GetShareMemberIdsAsync(ShareableType type, int itemId)
    {
        return await _context.ItemShares
            .Where(s => s.Type == type && s.ItemId == itemId)
            .Select(s => s.MemberId)
            .ToListAsync();
    }

    public async Task ReplaceSharesAsync(ShareableType type, int itemId, IEnumerable<int> memberIds)
    {
        var existing = await _context.ItemShares
            .Where(s => s.Type == type && s.ItemId == itemId)
            .ToListAsync();

        _context.ItemShares.RemoveRange(existing);

        foreach (var memberId in memberIds.Distinct())
        {
            _context.ItemShares.Add(new ItemShare { Type = type, ItemId = itemId, MemberId = memberId });
        }

        await _context.SaveChangesAsync();
    }
}
