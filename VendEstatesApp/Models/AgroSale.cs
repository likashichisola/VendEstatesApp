namespace VendEstatesApp.Models;

/// <summary>
/// A sales transaction of produce/livestock/other agro inventory items.
/// </summary>
public class AgroSale : BaseEntity
{
    public int AgroInventoryId { get; set; }

    public AgroInventory? AgroInventory { get; set; }

    public string? CustomerName { get; set; }

    public decimal QuantitySold { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public int SoldByEmployeeId { get; set; }

    public Employee? SoldByEmployee { get; set; }

    public string? Notes { get; set; }
}
