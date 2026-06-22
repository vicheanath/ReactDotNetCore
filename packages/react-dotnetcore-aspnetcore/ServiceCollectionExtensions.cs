using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace ReactDotNetCore;

/// <summary>DI registration for the ReactDotNetCore engine.</summary>
public static class ReactDotNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Register the ReactDotNetCore engine: options, the Vite manifest provider, the typed SSR client,
    /// the page renderer, and (optionally) the managed Node sidecar process.
    /// </summary>
    public static IServiceCollection AddReactDotNetCore(this IServiceCollection services, Action<ReactDotNetCoreOptions>? configure = null)
    {
        var options = new ReactDotNetCoreOptions();
        configure?.Invoke(options);

        // Auto-select a free loopback port up-front so the typed client, renderer (dev origin),
        // and sidecar process all agree on the same value.
        if (options.SidecarPort == 0)
            options.SidecarPort = FindFreeLoopbackPort();

        services.AddSingleton(options);
        services.AddSingleton<ViteManifestProvider>();
        services.AddSingleton<ReactDotNetCoreRenderer>();

        services.AddHttpClient<ReactSsrClient>(client =>
        {
            client.BaseAddress = options.SidecarBaseAddress;
            client.Timeout = options.RenderTimeout;
        });

        if (options.LaunchSidecar)
            services.AddHostedService<SidecarHostedService>();

        return services;
    }

    private static int FindFreeLoopbackPort()
    {
        // Note: TcpListener only implements IDisposable on .NET 8+, so Start/Stop (available on all
        // targets) is used instead of `using` for cross-framework compatibility.
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}
