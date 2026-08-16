using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Repositories;

public interface IVehicleRepository : IGenericRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetAllWithBranchAsync();

    Task<Vehicle?> GetByIdWithBranchAsync(int id);

    Task<IEnumerable<Vehicle>> GetByStatusAsync(VehicleStatus status);

    Task<bool> RegistrationNumberExistsAsync(string registrationNumber, int? excludeId = null);
}

public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Vehicle>> GetAllWithBranchAsync() =>
        await DbSet.Include(v => v.Branch).AsNoTracking().OrderBy(v => v.RegistrationNumber).ToListAsync();

    public async Task<Vehicle?> GetByIdWithBranchAsync(int id) =>
        await DbSet.Include(v => v.Branch).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<Vehicle>> GetByStatusAsync(VehicleStatus status) =>
        await DbSet.Include(v => v.Branch).AsNoTracking().Where(v => v.Status == status).ToListAsync();

    public async Task<bool> RegistrationNumberExistsAsync(string registrationNumber, int? excludeId = null) =>
        await DbSet.AnyAsync(v => v.RegistrationNumber == registrationNumber && (excludeId == null || v.Id != excludeId));
}
