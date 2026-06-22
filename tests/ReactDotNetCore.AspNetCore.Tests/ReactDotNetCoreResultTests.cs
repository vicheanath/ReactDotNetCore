namespace ReactDotNetCore.Tests;

public class ReactDotNetCoreResultTests
{
    [Fact]
    public void Stores_component_model_and_title()
    {
        var model = new { a = 1 };
        var result = new ReactDotNetCoreResult("UserProfile", model) { Title = "Profile" };

        Assert.Equal("UserProfile", result.Component);
        Assert.Same(model, result.Model);
        Assert.Equal("Profile", result.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Throws_when_component_is_missing(string? component)
    {
        Assert.Throws<ArgumentException>(() => new ReactDotNetCoreResult(component!, null));
    }
}
