using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Repositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<IEnumerable<Booking>> GetAllWithDetailsAsync();

    Task<Booking?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<Booking>> GetActiveBookingsForRoomAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);

    Task<IEnumerable<Booking>> GetLongTermBookingsAsync();

    Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status);

    Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime start, DateTime end);
}

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Booking>> GetAllWithDetailsAsync() =>
        await DbSet.Include(b => b.Room).ThenInclude(r => r!.Branch)
            .Include(b => b.CreatedByEmployee)
            .AsNoTracking()
            .OrderByDescending(b => b.CheckInDate)
            .ToListAsync();

    public async Task<Booking?> GetByIdWithDetailsAsync(int id) =>
        await DbSet.Include(b => b.Room).ThenInclude(r => r!.Branch)
            .Include(b => b.CreatedByEmployee)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IEnumerable<Booking>> GetActiveBookingsForRoomAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null) =>
        await DbSet.Where(b => b.RoomId == roomId
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.CheckedOut
                && (excludeBookingId == null || b.Id != excludeBookingId)
                && b.CheckInDate < checkOut
                && checkIn < b.CheckOutDate)
            .ToListAsync();

    public async Task<IEnumerable<Booking>> GetLongTermBookingsAsync() =>
        await DbSet.Include(b => b.Room)
            .AsNoTracking()
            .Where(b => (b.CheckOutDate.Date - b.CheckInDate.Date).Days >= 30)
            .OrderByDescending(b => b.CheckInDate)
            .ToListAsync();

    public async Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status) =>
        await DbSet.Include(b => b.Room).AsNoTracking().Where(b => b.Status == status).ToListAsync();

    public async Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime start, DateTime end) =>
        await DbSet.Include(b => b.Room)
            .AsNoTracking()
            .Where(b => b.CheckInDate <= end && b.CheckOutDate >= start)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
}
