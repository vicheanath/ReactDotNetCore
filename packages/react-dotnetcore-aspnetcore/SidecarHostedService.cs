using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactDotNetCore;

/// <summary>
/// Owns the lifecycle of the Node SSR sidecar process: launches it on application start (in
/// production-bundle or Vite-HMR-dev mode), streams its output to the logger, waits for health,
/// auto-restarts it on unexpected exit, and terminates it on shutdown.
/// </summary>
public sealed class SidecarHostedService : IHostedService
{
    private readonly ReactDotNetCoreOptions _options;
    private readonly IWebHostEnvironment _env;
    private readonly IServiceProvider _services;
    private readonly ILogger<SidecarHostedService> _logger;

    private Process? _process;
    private int _restarts;
    private volatile bool _stopping;

    public SidecarHostedService(
        ReactDotNetCoreOptions options,
        IWebHostEnvironment env,
        IServiceProvider services,
        ILogger<SidecarHostedService> logger)
    {
        _options = options;
        _env = env;
        _services = services;
        _logger = logger;
    }

    private bool IsDev => _options.DevMode ?? _env.IsDevelopment();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Launch();

        var ssr = _services.GetRequiredService<ReactSsrClient>();
        var ready = await ssr.WaitForReadyAsync(_options.SidecarStartupTimeout, cancellationToken);
        if (!ready)
        {
            _logger.LogError("React SSR sidecar did not become healthy within {Timeout}.", _options.SidecarStartupTimeout);
            throw new TimeoutException("React SSR sidecar failed to start within the configured timeout.");
        }
        _logger.LogInformation("React SSR sidecar is ready on port {Port} ({Mode} mode).",
            _options.SidecarPort, IsDev ? "dev/HMR" : "production");
    }

    private void Launch()
    {
        var scriptPath = Path.Combine(_env.ContentRootPath, _options.SidecarScript);
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"SSR sidecar script not found at '{scriptPath}'.", scriptPath);

        // Run the sidecar with its working directory at the Vite root so tooling that resolves
        // config/content relative to cwd (e.g. Tailwind/PostCSS in dev) finds them.
        var workingDir = Path.GetFullPath(Path.Combine(_env.ContentRootPath, _options.ViteRoot));

        var psi = new ProcessStartInfo
        {
            FileName = _options.NodePath,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(scriptPath);
        psi.Environment["REACTDOTNETCORE_PORT"] = _options.SidecarPort.ToString();

        if (IsDev)
        {
            psi.Environment["REACTDOTNETCORE_MODE"] = "dev";
            psi.Environment["REACTDOTNETCORE_ROOT"] = Path.GetFullPath(Path.Combine(_env.ContentRootPath, _options.ViteRoot));
        }
        else
        {
            var bundlePath = Path.Combine(_env.ContentRootPath, _options.ServerBundle);
            if (!File.Exists(bundlePath))
                throw new FileNotFoundException(
                    $"SSR bundle not found at '{bundlePath}'. Run the frontend build (npm run build) first.", bundlePath);
            psi.Environment["REACTDOTNETCORE_MODE"] = "prod";
            psi.Environment["REACTDOTNETCORE_BUNDLE"] = bundlePath;
        }

        _logger.LogInformation("Starting React SSR sidecar: {Node} {Script} (port {Port}, {Mode}).",
            _options.NodePath, scriptPath, _options.SidecarPort, IsDev ? "dev" : "prod");

        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) _logger.LogInformation("[ssr] {Line}", e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) _logger.LogError("[ssr] {Line}", e.Data); };
        process.Exited += OnExited;

        if (!process.Start())
            throw new InvalidOperationException("Failed to start the React SSR sidecar process.");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _process = process;
    }

    private void OnExited(object? sender, EventArgs e)
    {
        if (_stopping) return;
        var code = SafeExitCode(sender as Process);
        if (_restarts >= _options.MaxSidecarRestarts)
        {
            _logger.LogError("React SSR sidecar exited (code {Code}); restart limit ({Max}) reached.",
                code, _options.MaxSidecarRestarts);
            return;
        }
        _restarts++;
        _logger.LogWarning("React SSR sidecar exited (code {Code}); restarting ({Restart}/{Max}).",
            code, _restarts, _options.MaxSidecarRestarts);
        try { Launch(); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to restart React SSR sidecar."); }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stopping = true;
        if (_process is { HasExited: false })
        {
            _logger.LogInformation("Stopping React SSR sidecar.");
            try { _process.Kill(entireProcessTree: true); }
            catch (Exception ex) { _logger.LogWarning(ex, "Error stopping React SSR sidecar."); }
        }
        _process?.Dispose();
        _process = null;
        return Task.CompletedTask;
    }

    private static int SafeExitCode(Process? p)
    {
        try { return p?.ExitCode ?? -1; }
        catch { return -1; }
    }
}
