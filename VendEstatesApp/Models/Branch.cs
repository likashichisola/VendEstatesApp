using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// One of the three business branches: Vend Estates 8119 (Lodge), Rocks Agro Field, Rocks Car Rental.
/// </summary>
public class Branch : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public BranchType Type { get; set; }

    public string? Location { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public ICollection<AgroInventory> AgroInventoryItems { get; set; } = new List<AgroInventory>();

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
