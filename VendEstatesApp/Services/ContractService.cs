using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IContractService
{
    Task<IEnumerable<Contract>> GetAllAsync();

    Task<Contract?> GetByIdAsync(int id);

    Task<IEnumerable<Contract>> GetByEmployeeAsync(int employeeId);

    Task<Contract> CreateAsync(Contract contract);

    Task UpdateAsync(Contract contract);

    Task DeleteAsync(int id);

    Task<IEnumerable<Contract>> GetExpiringSoonAsync(int daysAhead = 30);
}

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;

    public ContractService(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<IEnumerable<Contract>> GetAllAsync() => await _contractRepository.GetAllWithEmployeeAsync();

    public async Task<Contract?> GetByIdAsync(int id) => await _contractRepository.GetByIdWithEmployeeAsync(id);

    public async Task<IEnumerable<Contract>> GetByEmployeeAsync(int employeeId) => await _contractRepository.GetByEmployeeAsync(employeeId);

    public async Task<Contract> CreateAsync(Contract contract) => await _contractRepository.AddAsync(contract);

    public async Task UpdateAsync(Contract contract) => await _contractRepository.UpdateAsync(contract);

    public async Task DeleteAsync(int id) => await _contractRepository.DeleteAsync(id);

    public async Task<IEnumerable<Contract>> GetExpiringSoonAsync(int daysAhead = 30)
    {
        var all = await _contractRepository.GetAllWithEmployeeAsync();
        var cutoff = DateTime.UtcNow.AddDays(daysAhead);
        return all.Where(c => c.Status == ContractStatus.Active && c.EndDate.HasValue && c.EndDate.Value <= cutoff);
    }
}
