using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// An inventory item tracked at the Rocks Agro Field branch (seeds, produce, livestock, equipment, etc.).
/// </summary>
public class AgroInventory : BaseEntity
{
    public string ItemName { get; set; } = string.Empty;

    public AgroItemCategory Category { get; set; }

    public string Unit { get; set; } = "kg";

    public decimal QuantityInStock { get; set; }

    public decimal UnitCost { get; set; }

    public decimal ReorderLevel { get; set; }

    public string? Notes { get; set; }

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public bool IsLowStock => QuantityInStock <= ReorderLevel;

    public ICollection<AgroSale> Sales { get; set; } = new List<AgroSale>();
}
