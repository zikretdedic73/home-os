using System.Linq;

namespace HomeOS.Models.Common;

// Core visibility helper (Docs/02_Pravila_Programiranja.md, section 1.3): a
// member sees items shared with the whole household plus their own private
// items. Composes with the existing HouseholdId filter, and is applied
// consistently across every module's list/search/dashboard queries so a
// private item never leaks to other members.
public static class VisibilityQueryExtensions
{
    public static IQueryable<T> VisibleTo<T>(this IQueryable<T> query, int memberId) where T : BaseEntity
        => query.Where(e => e.Visibility == Visibility.Household || e.OwnerId == memberId);
}
