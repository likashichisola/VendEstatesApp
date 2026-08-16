using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendEstatesApp.ViewModels;

namespace VendEstatesApp.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    [Route("/Home/Error")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("/Home/NotFound")]
    public IActionResult NotFound()
    {
        return View();
    }
}
