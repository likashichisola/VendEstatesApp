using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> GetAllAsync();

    Task<Vehicle?> GetByIdAsync(int id);

    Task<(bool Success, string? Error)> CreateAsync(Vehicle vehicle);

    Task<(bool Success, string? Error)> UpdateAsync(Vehicle vehicle);

    Task DeleteAsync(int id);

    Task<IEnumerable<Vehicle>> GetAvailableAsync();
}

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync() => await _vehicleRepository.GetAllWithBranchAsync();

    public async Task<Vehicle?> GetByIdAsync(int id) => await _vehicleRepository.GetByIdWithBranchAsync(id);

    public async Task<(bool Success, string? Error)> CreateAsync(Vehicle vehicle)
    {
        if (await _vehicleRepository.RegistrationNumberExistsAsync(vehicle.RegistrationNumber))
        {
            return (false, "A vehicle with this registration number already exists.");
        }

        await _vehicleRepository.AddAsync(vehicle);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Vehicle vehicle)
    {
        if (await _vehicleRepository.RegistrationNumberExistsAsync(vehicle.RegistrationNumber, vehicle.Id))
        {
            return (false, "A vehicle with this registration number already exists.");
        }

        await _vehicleRepository.UpdateAsync(vehicle);
        return (true, null);
    }

    public async Task DeleteAsync(int id) => await _vehicleRepository.DeleteAsync(id);

    public async Task<IEnumerable<Vehicle>> GetAvailableAsync() => await _vehicleRepository.GetByStatusAsync(VehicleStatus.Available);
}
