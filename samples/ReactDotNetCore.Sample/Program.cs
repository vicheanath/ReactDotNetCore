using ReactDotNetCore;
using ReactDotNetCore.Sample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Existing domain services are unchanged by the migration to React views.
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<DashboardService>();

// Register the ReactDotNetCore engine (SSR sidecar + Vite manifest + hydration page renderer).
// This sample keeps all of its frontend (entries, config, components, views) under Views/.
builder.Services.AddReactDotNetCore(options =>
{
    options.ViteRoot = "Views";
    options.SidecarScript = "Views/ssr-server.mjs";
    options.ServerBundle = "Views/dist/server/entry-server.js";
    // Avoid the dev flash-of-unstyled-content by linking the CSS as a real stylesheet.
    options.DevStylesheets = new[] { "styles/globals.css" };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
