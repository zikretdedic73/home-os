using System.Linq;

namespace HomeOS.Models.Common;

// Core visibility helper (Docs/02_Pravila_Programiranja.md, section 1.3): a
// member sees items shared with the whole household plus their own private
// items. Composes with the existing HouseholdId filter, and is applied
// consistently across every module's list/search/dashboard queries so a
// private item never leaks to other members.
public static class VisibilityQueryExtensions
{
    // Household items + the member's own items. SpecificMembers items are only
    // visible to their owner through this overload - use the overload below to
    // also include items explicitly shared with the member.
    public static IQueryable<T> VisibleTo<T>(this IQueryable<T> query, int memberId) where T : BaseEntity
        => query.Where(e => e.Visibility == Visibility.Household || e.OwnerId == memberId);

    // Same as above, but also surfaces SpecificMembers items shared with this
    // member. `shares` is the ItemShares DbSet; the check translates to an EXISTS
    // subquery, so there is no extra round-trip. `type` scopes the share rows to
    // the entity kind being queried. Callers pass this overload wherever a
    // specific-person share should be honored (list, search, dashboard).
    public static IQueryable<T> VisibleTo<T>(this IQueryable<T> query, int memberId, IQueryable<ItemShare> shares, ShareableType type) where T : BaseEntity
        => query.Where(e =>
            e.Visibility == Visibility.Household
            || e.OwnerId == memberId
            || (e.Visibility == Visibility.SpecificMembers
                && shares.Any(s => s.Type == type && s.ItemId == e.Id && s.MemberId == memberId)));
}
