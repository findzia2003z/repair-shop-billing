using Microsoft.Data.Sqlite;
using RepairShopBilling.Models;
using System.IO;

namespace RepairShopBilling.Services;

/// <summary>
/// Service for handling application startup operations including database initialization and service catalog seeding
/// </summary>
public class StartupService : IStartupService
{
    private readonly IDatabaseService _databaseService;
    private readonly string _appDataPath;

    public StartupService() : this(new DatabaseService())
    {
    }

    public StartupService(IDatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RepairShopBilling");
    }

    /// <summary>
    /// Initializes the application including database setup and service catalog seeding
    /// </summary>
    public async Task<StartupResult> InitializeApplicationAsync()
    {
        try
        {
            // Check if this is the first run
            var isFirstRun = await IsFirstRunAsync();

            // Ensure application data directory exists
            Directory.CreateDirectory(_appDataPath);

            // Initialize database
            await _databaseService.InitializeAsync();

            // Seed service catalog on first run or update existing catalog
            if (isFirstRun)
            {
                await _databaseService.SeedServiceCatalogAsync();
            }
            else
            {
                // Update service catalog with any new services
                await _databaseService.UpdateServiceCatalogAsync();
            }

            // Verify service catalog was loaded
            var services = await _databaseService.GetAllServicesAsync();
            var serviceCount = services.Count;

            if (serviceCount == 0)
            {
                return StartupResult.CreateFailure("Service catalog is empty after initialization");
            }

            // Create first run marker file
            if (isFirstRun)
            {
                await CreateFirstRunMarkerAsync();
            }

            return StartupResult.CreateSuccess(isFirstRun, serviceCount);
        }
        catch (SqliteException sqlEx)
        {
            return StartupResult.CreateFailure($"Database error: {sqlEx.Message}", sqlEx);
        }
        catch (UnauthorizedAccessException accessEx)
        {
            return StartupResult.CreateFailure($"Access denied: {accessEx.Message}", accessEx);
        }
        catch (DirectoryNotFoundException dirEx)
        {
            return StartupResult.CreateFailure($"Directory not found: {dirEx.Message}", dirEx);
        }
        catch (IOException ioEx)
        {
            return StartupResult.CreateFailure($"File system error: {ioEx.Message}", ioEx);
        }
        catch (Exception ex)
        {
            return StartupResult.CreateFailure($"Unexpected error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if this is the first run of the application
    /// </summary>
    public async Task<bool> IsFirstRunAsync()
    {
        try
        {
            var markerFile = Path.Combine(_appDataPath, ".initialized");
            return !File.Exists(markerFile);
        }
        catch
        {
            // If we can't check, assume it's the first run to be safe
            return true;
        }
    }

    /// <summary>
    /// Performs application health checks
    /// </summary>
    public async Task<HealthCheckResult> PerformHealthCheckAsync()
    {
        var result = new HealthCheckResult();

        try
        {
            // Check database accessibility
            var bills = await _databaseService.GetBillsAsync();
            result.DatabaseAccessible = true;

            // Check service catalog
            var services = await _databaseService.GetAllServicesAsync();
            result.ServiceCatalogLoaded = services.Any();
            result.ServiceCount = services.Count;

            // Add warnings for potential issues
            if (result.ServiceCount == 0)
            {
                result.Warnings.Add("Service catalog is empty");
            }
            else if (result.ServiceCount < 20)
            {
                result.Warnings.Add($"Service catalog has only {result.ServiceCount} services (expected ~25)");
            }

            // Check for required service categories
            var categories = await _databaseService.GetServiceCategoriesAsync();
            var expectedCategories = new[] { "HOURS", "EQUIPMENT", "WIN 11 PRO", "OS X", "DRIVERS", "VHS CONVERT", "DATA REC.", "PHOTO PRINT", "LASER" };
            var missingCategories = expectedCategories.Except(categories).ToList();
            
            if (missingCategories.Any())
            {
                result.Warnings.Add($"Missing service categories: {string.Join(", ", missingCategories)}");
            }
        }
        catch (SqliteException sqlEx)
        {
            result.DatabaseAccessible = false;
            result.ErrorMessage = $"Database error: {sqlEx.Message}";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Health check failed: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Creates a marker file to indicate the application has been initialized
    /// </summary>
    private async Task CreateFirstRunMarkerAsync()
    {
        try
        {
            var markerFile = Path.Combine(_appDataPath, ".initialized");
            var content = $"RepairShopBilling initialized on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            await File.WriteAllTextAsync(markerFile, content);
        }
        catch (Exception ex)
        {
            // Log but don't fail startup for this
            System.Diagnostics.Debug.WriteLine($"Failed to create first run marker: {ex.Message}");
        }
    }
}