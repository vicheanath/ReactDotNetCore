using Microsoft.AspNetCore.Mvc;
using ReactDotNetCore;
using ReactDotNetCore.Sample.Models;
using ReactDotNetCore.Sample.Services;

namespace ReactDotNetCore.Sample.Controllers;

public class UsersController : Controller
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    // GET /Users
    public IActionResult Index()
    {
        var model = new UsersViewModel { Users = _userService.GetUsers() };
        return this.ReactView<Users>(model);
    }

    // GET /Users/Detail/5
    // Existing controller/service/model stay the same; only the view becomes React.
    public IActionResult Detail(int id = 1)
    {
        var model = _userService.GetUser(id);
        return this.ReactView<UserProfile>(model);
    }
}
