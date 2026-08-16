using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Data;

/// <summary>
/// Seeds baseline reference data (branches, default Director account) on application startup.
/// </summary>
public static class DbInitializer
{
    public const string DefaultDirectorUsername = "director";
    public const string DefaultDirectorPassword = "Director@123";

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!context.Branches.Any())
        {
            var lodge = new Branch
            {
                Name = "Vend Estates 8119",
                Type = BranchType.Lodge,
                Location = "Plot 8119, Lusaka",
                ContactPhone = "0977000001",
                ContactEmail = "lodge@vendestates.com"
            };

            var agro = new Branch
            {
                Name = "Rocks Agro Field",
                Type = BranchType.AgroField,
                Location = "Chisamba Farm Block",
                ContactPhone = "0977000002",
                ContactEmail = "agro@vendestates.com"
            };

            var carRental = new Branch
            {
                Name = "Rocks Car Rental",
                Type = BranchType.CarRental,
                Location = "Cairo Road, Lusaka",
                ContactPhone = "0977000003",
                ContactEmail = "carrental@vendestates.com"
            };

            context.Branches.AddRange(lodge, agro, carRental);
            await context.SaveChangesAsync();
        }

        if (!context.Employees.Any())
        {
            var headOfficeBranch = context.Branches.First(b => b.Type == BranchType.Lodge);

            var director = new Employee
            {
                FirstName = "System",
                LastName = "Director",
                Username = DefaultDirectorUsername,
                Email = "director@vendestates.com",
                PhoneNumber = "0977000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultDirectorPassword),
                Role = EmployeeRole.Director,
                JobTitle = "Managing Director",
                BranchId = headOfficeBranch.Id,
                BasicSalary = 25000m,
                HireDate = DateTime.UtcNow,
                IsActive = true
            };

            context.Employees.Add(director);
            await context.SaveChangesAsync();
        }
    }
}
