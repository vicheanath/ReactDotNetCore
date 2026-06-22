namespace ReactDotNetCore.Tests;

public class OptionsTests
{
    [Fact]
    public void Defaults_match_the_documented_values()
    {
        var o = new ReactDotNetCoreOptions();

        Assert.Equal(0, o.SidecarPort);                 // 0 = auto-select
        Assert.True(o.LaunchSidecar);
        Assert.Equal("ssr-server.mjs", o.SidecarScript);
        Assert.Equal("dist/server/entry-server.js", o.ServerBundle);
        Assert.Equal("entry-client.tsx", o.ClientEntryKey);
        Assert.Equal("/react-dotnetcore/client", o.PublicBasePath);
        Assert.Equal(5, o.MaxSidecarRestarts);
        Assert.Null(o.DevMode);
    }

    [Fact]
    public void SidecarBaseAddress_is_loopback_with_the_port()
    {
        var o = new ReactDotNetCoreOptions { SidecarPort = 5174 };
        Assert.Equal("http://127.0.0.1:5174/", o.SidecarBaseAddress.ToString());
    }

    [Fact]
    public void DevServerOrigin_uses_host_and_port()
    {
        var o = new ReactDotNetCoreOptions { SidecarPort = 5174, DevServerHost = "localhost" };
        Assert.Equal("http://localhost:5174", o.DevServerOrigin);
    }
}
