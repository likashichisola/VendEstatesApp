namespace VendEstatesApp.Constants;

/// <summary>
/// String constants matching <see cref="Models.Enums.EmployeeRole"/> for use in
/// [Authorize(Roles = "...")] attributes and role-based UI checks.
/// </summary>
public static class Roles
{
    public const string Director = "Director";
    public const string Manager = "Manager";
    public const string Accountant = "Accountant";

    public const string DirectorOrManager = Director + "," + Manager;
    public const string DirectorOrAccountant = Director + "," + Accountant;
    public const string ManagerOrAccountant = Manager + "," + Accountant;
    public const string All = Director + "," + Manager + "," + Accountant;
}
