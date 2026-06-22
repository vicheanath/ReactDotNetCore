using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReactDotNetCore;
using ReactDotNetCore.Sample.Models;
using ReactDotNetCore.Sample.Services;

namespace ReactDotNetCore.Sample.Controllers;

public class HomeController : Controller
{
    private readonly UserService _userService;

    public HomeController(UserService userService)
    {
        _userService = userService;
    }

    public IActionResult Index()
        => this.ReactView<Home>(new HomeViewModel { AppName = "ReactDotNetCore for ASP.NET Core", UserCount = _userService.Count });

    [Route("/Privacy")]
    public IActionResult Privacy()
        => this.ReactView<Privacy>();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => this.ReactView<Error>(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
