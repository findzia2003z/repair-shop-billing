using RepairShopBilling.Models;

namespace RepairShopBilling.Services;

/// <summary>
/// Service interface for handling application startup operations
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Initializes the application including database setup and service catalog seeding
    /// </summary>
    Task<StartupResult> InitializeApplicationAsync();

    /// <summary>
    /// Checks if this is the first run of the application
    /// </summary>
    Task<bool> IsFirstRunAsync();

    /// <summary>
    /// Performs application health checks
    /// </summary>
    Task<HealthCheckResult> PerformHealthCheckAsync();
}

/// <summary>
/// Result of application startup initialization
/// </summary>
public class StartupResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public bool IsFirstRun { get; set; }
    public int ServiceCount { get; set; }

    public static StartupResult CreateSuccess(bool isFirstRun, int serviceCount)
    {
        return new StartupResult
        {
            Success = true,
            IsFirstRun = isFirstRun,
            ServiceCount = serviceCount
        };
    }

    public static StartupResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new StartupResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}

/// <summary>
/// Result of application health checks
/// </summary>
public class HealthCheckResult
{
    public bool DatabaseAccessible { get; set; }
    public bool ServiceCatalogLoaded { get; set; }
    public int ServiceCount { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();

    public bool IsHealthy => DatabaseAccessible && ServiceCatalogLoaded && string.IsNullOrEmpty(ErrorMessage);
}