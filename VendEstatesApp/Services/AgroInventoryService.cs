using VendEstatesApp.Models;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IAgroInventoryService
{
    Task<IEnumerable<AgroInventory>> GetAllAsync();

    Task<AgroInventory?> GetByIdAsync(int id);

    Task<AgroInventory> CreateAsync(AgroInventory item);

    Task UpdateAsync(AgroInventory item);

    Task DeleteAsync(int id);

    Task<IEnumerable<AgroInventory>> GetLowStockAsync();
}

public class AgroInventoryService : IAgroInventoryService
{
    private readonly IAgroInventoryRepository _agroInventoryRepository;

    public AgroInventoryService(IAgroInventoryRepository agroInventoryRepository)
    {
        _agroInventoryRepository = agroInventoryRepository;
    }

    public async Task<IEnumerable<AgroInventory>> GetAllAsync() => await _agroInventoryRepository.GetAllWithBranchAsync();

    public async Task<AgroInventory?> GetByIdAsync(int id) => await _agroInventoryRepository.GetByIdWithBranchAsync(id);

    public async Task<AgroInventory> CreateAsync(AgroInventory item) => await _agroInventoryRepository.AddAsync(item);

    public async Task UpdateAsync(AgroInventory item) => await _agroInventoryRepository.UpdateAsync(item);

    public async Task DeleteAsync(int id) => await _agroInventoryRepository.DeleteAsync(id);

    public async Task<IEnumerable<AgroInventory>> GetLowStockAsync() => await _agroInventoryRepository.GetLowStockAsync();
}
