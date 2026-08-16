using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Repositories;

public interface IRoomRepository : IGenericRepository<Room>
{
    Task<IEnumerable<Room>> GetAllWithBranchAsync();

    Task<Room?> GetByIdWithBranchAsync(int id);

    Task<IEnumerable<Room>> GetByStatusAsync(RoomStatus status);

    Task<bool> RoomNumberExistsAsync(int branchId, string roomNumber, int? excludeId = null);
}

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Room>> GetAllWithBranchAsync() =>
        await DbSet.Include(r => r.Branch).AsNoTracking().OrderBy(r => r.RoomNumber).ToListAsync();

    public async Task<Room?> GetByIdWithBranchAsync(int id) =>
        await DbSet.Include(r => r.Branch).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Room>> GetByStatusAsync(RoomStatus status) =>
        await DbSet.Include(r => r.Branch).AsNoTracking().Where(r => r.Status == status).ToListAsync();

    public async Task<bool> RoomNumberExistsAsync(int branchId, string roomNumber, int? excludeId = null) =>
        await DbSet.AnyAsync(r => r.BranchId == branchId && r.RoomNumber == roomNumber && (excludeId == null || r.Id != excludeId));
}
