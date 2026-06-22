namespace ReactDotNetCore.Sample.Models;

public sealed class HomeViewModel
{
    public string AppName { get; set; } = "";
    public int UserCount { get; set; }
}

public sealed class UsersViewModel
{
    public IReadOnlyList<UserDto> Users { get; set; } = Array.Empty<UserDto>();
}
