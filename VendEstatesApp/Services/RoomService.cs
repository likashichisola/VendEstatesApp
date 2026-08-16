using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetAllAsync();

    Task<Room?> GetByIdAsync(int id);

    Task<(bool Success, string? Error)> CreateAsync(Room room);

    Task<(bool Success, string? Error)> UpdateAsync(Room room);

    Task DeleteAsync(int id);

    Task<IEnumerable<Room>> GetByStatusAsync(RoomStatus status);

    Task<IEnumerable<Room>> GetAvailableRoomsAsync();

    Task<Dictionary<RoomStatus, int>> GetOccupancySummaryAsync();
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<IEnumerable<Room>> GetAllAsync() => await _roomRepository.GetAllWithBranchAsync();

    public async Task<Room?> GetByIdAsync(int id) => await _roomRepository.GetByIdWithBranchAsync(id);

    public async Task<(bool Success, string? Error)> CreateAsync(Room room)
    {
        if (await _roomRepository.RoomNumberExistsAsync(room.BranchId, room.RoomNumber))
        {
            return (false, "A room with this number already exists for the selected branch.");
        }

        await _roomRepository.AddAsync(room);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Room room)
    {
        if (await _roomRepository.RoomNumberExistsAsync(room.BranchId, room.RoomNumber, room.Id))
        {
            return (false, "A room with this number already exists for the selected branch.");
        }

        await _roomRepository.UpdateAsync(room);
        return (true, null);
    }

    public async Task DeleteAsync(int id) => await _roomRepository.DeleteAsync(id);

    public async Task<IEnumerable<Room>> GetByStatusAsync(RoomStatus status) => await _roomRepository.GetByStatusAsync(status);

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync() => await _roomRepository.GetByStatusAsync(RoomStatus.Available);

    public async Task<Dictionary<RoomStatus, int>> GetOccupancySummaryAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        return rooms.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
    }
}
