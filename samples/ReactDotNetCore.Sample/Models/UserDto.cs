namespace ReactDotNetCore.Sample.Models;

/// <summary>The view model passed to the React UserProfile view. Serialized to props as camelCase JSON.</summary>
public sealed class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime JoinedUtc { get; set; }
}
