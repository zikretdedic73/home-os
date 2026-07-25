using HomeOS.Models.Common;

namespace HomeOS.Services;

// Manages the specific-member shares of an item (Visibility.SpecificMembers).
// One shared service so every module persists/reads shares the same way rather
// than each re-implementing the join (Docs/02 - shared core service).
public interface IItemSharingService
{
    // Member ids an item is currently shared with - used to pre-select the
    // picker on edit screens.
    Task<List<int>> GetShareMemberIdsAsync(ShareableType type, int itemId);

    // Replaces the item's shares with exactly the given member ids. A no-op set
    // (empty) clears all shares. Called whenever an item is saved.
    Task ReplaceSharesAsync(ShareableType type, int itemId, IEnumerable<int> memberIds);
}
