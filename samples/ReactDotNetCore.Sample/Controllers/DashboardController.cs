using Microsoft.AspNetCore.Mvc;
using ReactDotNetCore;
using ReactDotNetCore.Sample.Services;

namespace ReactDotNetCore.Sample.Controllers;

public class DashboardController : Controller
{
    private readonly DashboardService _dashboard;

    public DashboardController(DashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    // GET /Dashboard
    public IActionResult Index()
        => this.ReactView<Dashboard>(_dashboard.GetDashboard());
}
