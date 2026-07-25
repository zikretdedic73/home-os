namespace HomeOS.Models.Common;

public enum Visibility
{
    // Only the owner can see it.
    Private,
    // Everyone in the household can see it.
    Household,
    // Only the owner plus the specific members it was shared with (via
    // ItemShare) - the source spec's "dijeljeno ... sa specifičnim osobama".
    SpecificMembers
}
