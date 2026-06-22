using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactDotNetCore.Tests.TestSupport;

namespace ReactDotNetCore.Tests;

public class ServiceRegistrationTests
{
    private static ServiceCollection BaseServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IWebHostEnvironment>(new FakeWebHostEnvironment());
        return services;
    }

    [Fact]
    public void AddReactDotNetCore_registers_the_core_services()
    {
        var services = BaseServices();
        services.AddReactDotNetCore(o => o.LaunchSidecar = false);
        using var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<ReactDotNetCoreOptions>());
        Assert.NotNull(sp.GetService<ViteManifestProvider>());
        Assert.NotNull(sp.GetService<ReactDotNetCoreRenderer>());
        Assert.NotNull(sp.GetService<ReactSsrClient>());
    }

    [Fact]
    public void AddReactDotNetCore_auto_selects_a_free_port_when_zero()
    {
        var services = BaseServices();
        services.AddReactDotNetCore(o => o.LaunchSidecar = false);
        using var sp = services.BuildServiceProvider();

        Assert.NotEqual(0, sp.GetRequiredService<ReactDotNetCoreOptions>().SidecarPort);
    }

    [Fact]
    public void AddReactDotNetCore_keeps_an_explicit_port()
    {
        var services = BaseServices();
        services.AddReactDotNetCore(o => { o.LaunchSidecar = false; o.SidecarPort = 4321; });
        using var sp = services.BuildServiceProvider();

        Assert.Equal(4321, sp.GetRequiredService<ReactDotNetCoreOptions>().SidecarPort);
    }

    [Fact]
    public void AddReactDotNetCore_registers_the_sidecar_when_enabled()
    {
        var services = BaseServices();
        services.AddReactDotNetCore(); // LaunchSidecar defaults to true

        Assert.Contains(services, d =>
            d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(SidecarHostedService));
    }

    [Fact]
    public void AddReactDotNetCore_skips_the_sidecar_when_disabled()
    {
        var services = BaseServices();
        services.AddReactDotNetCore(o => o.LaunchSidecar = false);

        Assert.DoesNotContain(services, d => d.ImplementationType == typeof(SidecarHostedService));
    }
}
