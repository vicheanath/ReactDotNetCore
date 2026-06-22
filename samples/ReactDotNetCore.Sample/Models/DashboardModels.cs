namespace ReactDotNetCore.Sample.Models;

public sealed class DashboardStat
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Delta { get; set; } = "";
    public string Trend { get; set; } = "up"; // "up" | "down"
}

public sealed class RecentSale
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Amount { get; set; } = "";
}

public sealed class DashboardViewModel
{
    public List<DashboardStat> Stats { get; set; } = new();
    public int[] Revenue { get; set; } = Array.Empty<int>();
    public string[] Months { get; set; } = Array.Empty<string>();
    public List<RecentSale> RecentSales { get; set; } = new();
}
