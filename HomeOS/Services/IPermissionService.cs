namespace HomeOS.Services;

// Central check/grant/revoke for module data-access permissions. Modules call
// HasPermissionAsync before accessing another module's data; the household
// manages grants via the module manager (Docs/00_Specifikacija_Izvor.md,
// "Kontrola i privatnost domaćinstva").
public interface IPermissionService
{
    // Built-in modules default to granted; a household can revoke. A stricter
    // default-deny for third-party modules is the natural extension.
    Task<bool> HasPermissionAsync(int householdId, string moduleKey, string permission);

    Task SetPermissionAsync(int householdId, string moduleKey, string permission, bool granted);
}
