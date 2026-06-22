using ReactDotNetCore.Sample.Models;

namespace ReactDotNetCore.Sample.Services;

/// <summary>
/// Stand-in for an existing domain service/repository. The point of ReactDotNetCore is that this layer
/// is untouched during a Razor -> React migration.
/// </summary>
public sealed class UserService
{
    private static readonly string[] Roles = { "Administrator", "Engineer", "Analyst", "Designer" };

    public UserDto GetUser(int id) => new()
    {
        Id = id,
        Name = $"User {id}",
        Email = $"user{id}@example.com",
        Role = Roles[id % Roles.Length],
        JoinedUtc = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(id * 7),
    };

    public IReadOnlyList<UserDto> GetUsers(int count = 8) =>
        Enumerable.Range(1, count).Select(GetUser).ToList();

    public int Count => 8;
}
