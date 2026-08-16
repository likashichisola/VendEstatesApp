using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Leave;
using LeaveRequestModel = VendEstatesApp.Models.LeaveRequest;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class LeaveController : Controller
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(Roles.Director) || User.IsInRole(Roles.Manager) || User.IsInRole(Roles.Accountant))
        {
            var all = await _leaveRequestService.GetAllAsync();
            return View(all);
        }

        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var mine = await _leaveRequestService.GetByEmployeeAsync(employeeId);
        return View(mine);
    }

    public async Task<IActionResult> Details(int id)
    {
        var leaveRequest = await _leaveRequestService.GetByIdAsync(id);
        if (leaveRequest is null)
        {
            return NotFound();
        }

        return View(leaveRequest);
    }

    public IActionResult Create()
    {
        return View(new LeaveRequestFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequestFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var leaveRequest = new LeaveRequestModel
        {
            EmployeeId = employeeId,
            Type = model.Type,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = model.Reason
        };

        var (success, error) = await _leaveRequestService.ApplyAsync(leaveRequest);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["SuccessMessage"] = "Leave request submitted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? notes)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _leaveRequestService.ApproveAsync(id, employeeId, notes);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Leave request approved." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? notes)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _leaveRequestService.RejectAsync(id, employeeId, notes);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Leave request rejected." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _leaveRequestService.CancelAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Leave request cancelled." : error;
        return RedirectToAction(nameof(Details), new { id });
    }
}
