using Microsoft.AspNetCore.Mvc;

namespace ReactDotNetCore.Tests;

public class ControllerExtensionsTests
{
    private sealed class Widget : IReactComponent { }
    private sealed class TestController : Controller { }

    [Fact]
    public void Generic_ReactView_uses_the_type_name_as_component()
    {
        var controller = new TestController();
        var model = new { x = 1 };

        var result = controller.ReactView<Widget>(model);

        Assert.Equal("Widget", result.Component);
        Assert.Same(model, result.Model);
    }

    [Fact]
    public void Generic_ReactView_allows_a_null_model()
    {
        var result = new TestController().ReactView<Widget>();
        Assert.Equal("Widget", result.Component);
        Assert.Null(result.Model);
    }

    [Fact]
    public void String_ReactView_uses_the_given_component_name()
    {
        var model = new { y = 2 };
        var result = new TestController().ReactView("CustomName", model);

        Assert.Equal("CustomName", result.Component);
        Assert.Same(model, result.Model);
    }
}
