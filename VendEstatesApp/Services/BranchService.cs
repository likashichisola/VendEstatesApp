using VendEstatesApp.Models;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IBranchService
{
    Task<IEnumerable<Branch>> GetAllAsync();

    Task<IEnumerable<Branch>> GetActiveAsync();

    Task<Branch?> GetByIdAsync(int id);

    Task<Branch> CreateAsync(Branch branch);

    Task UpdateAsync(Branch branch);

    Task DeleteAsync(int id);
}

public class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepository;

    public BranchService(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<IEnumerable<Branch>> GetAllAsync() => await _branchRepository.GetAllAsync();

    public async Task<IEnumerable<Branch>> GetActiveAsync() => await _branchRepository.GetActiveAsync();

    public async Task<Branch?> GetByIdAsync(int id) => await _branchRepository.GetByIdAsync(id);

    public async Task<Branch> CreateAsync(Branch branch) => await _branchRepository.AddAsync(branch);

    public async Task UpdateAsync(Branch branch) => await _branchRepository.UpdateAsync(branch);

    public async Task DeleteAsync(int id) => await _branchRepository.DeleteAsync(id);
}
