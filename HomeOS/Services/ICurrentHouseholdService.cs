namespace HomeOS.Services;

// Core service - every module uses this to find out "who am I / which household
// do I belong to" instead of reading Identity/Claims itself. See
// Docs/02_Pravila_Programiranja.md, section 1.3 (core services, do not duplicate per module).
public interface ICurrentHouseholdService
{
    Task<int> GetCurrentMemberIdAsync();
    Task<int> GetCurrentHouseholdIdAsync();
}
