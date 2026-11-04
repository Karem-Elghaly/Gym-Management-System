using System.Diagnostics;
using Gym_Management_System.Models;
using GymSystem.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboard;

       
        public HomeController(IDashboardService dashboard) => _dashboard = dashboard;

        public async Task<IActionResult> Index()
        {
            var model = await _dashboard.GetAsync();
            return View(model);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            ViewBag.StatusCode = statusCode;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}