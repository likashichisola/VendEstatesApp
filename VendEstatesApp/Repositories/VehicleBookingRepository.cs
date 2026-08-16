using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface IVehicleBookingRepository : IGenericRepository<VehicleBooking>
{
    Task<IEnumerable<VehicleBooking>> GetAllWithDetailsAsync();

    Task<VehicleBooking?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<VehicleBooking>> GetActiveBookingsForVehicleAsync(int vehicleId, DateTime start, DateTime end, int? excludeBookingId = null);
}

public class VehicleBookingRepository : GenericRepository<VehicleBooking>, IVehicleBookingRepository
{
    public VehicleBookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<VehicleBooking>> GetAllWithDetailsAsync() =>
        await DbSet.Include(v => v.Vehicle).ThenInclude(veh => veh!.Branch)
            .Include(v => v.CreatedByEmployee)
            .AsNoTracking()
            .OrderByDescending(v => v.StartDate)
            .ToListAsync();

    public async Task<VehicleBooking?> GetByIdWithDetailsAsync(int id) =>
        await DbSet.Include(v => v.Vehicle).ThenInclude(veh => veh!.Branch)
            .Include(v => v.CreatedByEmployee)
            .Include(v => v.Payments)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<VehicleBooking>> GetActiveBookingsForVehicleAsync(int vehicleId, DateTime start, DateTime end, int? excludeBookingId = null) =>
        await DbSet.Where(v => v.VehicleId == vehicleId
                && v.Status != Models.Enums.VehicleBookingStatus.Cancelled
                && v.Status != Models.Enums.VehicleBookingStatus.Completed
                && (excludeBookingId == null || v.Id != excludeBookingId)
                && v.StartDate < end
                && start < v.EndDate)
            .ToListAsync();
}
