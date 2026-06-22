using ReactDotNetCore.Sample.Models;

namespace ReactDotNetCore.Sample.Services;

/// <summary>Builds the dashboard view model. Existing domain code; only the view is React.</summary>
public sealed class DashboardService
{
    public DashboardViewModel GetDashboard() => new()
    {
        Stats = new()
        {
            new() { Key = "revenue", Label = "Total Revenue", Value = "$45,231.89", Delta = "+20.1%", Trend = "up" },
            new() { Key = "subscriptions", Label = "Subscriptions", Value = "+2,350", Delta = "+180.1%", Trend = "up" },
            new() { Key = "sales", Label = "Sales", Value = "+12,234", Delta = "+19%", Trend = "up" },
            new() { Key = "active", Label = "Active Now", Value = "573", Delta = "-2.1%", Trend = "down" },
        },
        Months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
        Revenue = new[] { 4200, 3800, 5100, 4700, 6200, 5800, 7300, 6900, 8100, 7600, 9200, 11200 },
        RecentSales = new()
        {
            new() { Name = "Olivia Martin", Email = "olivia.martin@email.com", Amount = "+$1,999.00" },
            new() { Name = "Jackson Lee", Email = "jackson.lee@email.com", Amount = "+$39.00" },
            new() { Name = "Isabella Nguyen", Email = "isabella.nguyen@email.com", Amount = "+$299.00" },
            new() { Name = "William Kim", Email = "will@email.com", Amount = "+$99.00" },
            new() { Name = "Sofia Davis", Email = "sofia.davis@email.com", Amount = "+$39.00" },
        },
    };
}
