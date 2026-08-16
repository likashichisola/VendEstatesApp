using VendEstatesApp.Models;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IAgroSaleService
{
    Task<IEnumerable<AgroSale>> GetAllAsync();

    Task<(bool Success, string? Error)> RecordSaleAsync(AgroSale sale);

    Task<IEnumerable<AgroSale>> GetByDateRangeAsync(DateTime start, DateTime end);
}

/// <summary>
/// Records agro produce/livestock sales and decrements inventory stock accordingly.
/// </summary>
public class AgroSaleService : IAgroSaleService
{
    private readonly IAgroSaleRepository _agroSaleRepository;
    private readonly IAgroInventoryRepository _agroInventoryRepository;

    public AgroSaleService(IAgroSaleRepository agroSaleRepository, IAgroInventoryRepository agroInventoryRepository)
    {
        _agroSaleRepository = agroSaleRepository;
        _agroInventoryRepository = agroInventoryRepository;
    }

    public async Task<IEnumerable<AgroSale>> GetAllAsync() => await _agroSaleRepository.GetAllWithDetailsAsync();

    public async Task<(bool Success, string? Error)> RecordSaleAsync(AgroSale sale)
    {
        var item = await _agroInventoryRepository.GetByIdAsync(sale.AgroInventoryId);
        if (item is null)
        {
            return (false, "Inventory item not found.");
        }

        if (sale.QuantitySold > item.QuantityInStock)
        {
            return (false, "Insufficient stock available for this sale.");
        }

        sale.TotalAmount = sale.QuantitySold * sale.UnitPrice;
        await _agroSaleRepository.AddAsync(sale);

        item.QuantityInStock -= sale.QuantitySold;
        await _agroInventoryRepository.UpdateAsync(item);

        return (true, null);
    }

    public async Task<IEnumerable<AgroSale>> GetByDateRangeAsync(DateTime start, DateTime end) =>
        await _agroSaleRepository.GetByDateRangeAsync(start, end);
}
