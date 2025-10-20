using Microsoft.UI.Xaml.Navigation;
using RepairShopBilling.Views;
using RepairShopBilling.Services;
using Microsoft.UI.Xaml.Controls;

namespace RepairShopBilling
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow? _mainWindow;
        private IDatabaseService? _databaseService;
        private IStartupService? _startupService;

        /// <summary>
        /// Gets the main window instance
        /// </summary>
        public MainWindow? MainWindow => _mainWindow;

        /// <summary>
        /// Gets the database service instance
        /// </summary>
        public IDatabaseService? DatabaseService => _databaseService;

        /// <summary>
        /// Gets the startup service instance
        /// </summary>
        public IStartupService? StartupService => _startupService;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                // Initialize services
                _databaseService = new DatabaseService();
                _startupService = new StartupService(_databaseService);
                
                // Initialize application
                var startupResult = await _startupService.InitializeApplicationAsync();
                
                if (!startupResult.Success)
                {
                    await HandleStartupErrorAsync(new InvalidOperationException(startupResult.ErrorMessage, startupResult.Exception));
                    return;
                }

                // Log startup information
                System.Diagnostics.Debug.WriteLine($"Application started successfully. First run: {startupResult.IsFirstRun}, Services loaded: {startupResult.ServiceCount}");
                
                // Create and activate main window
                _mainWindow = new MainWindow();
                _mainWindow.Activate();

                // Perform health check in background (optional)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var healthCheck = await _startupService.PerformHealthCheckAsync();
                        if (!healthCheck.IsHealthy)
                        {
                            System.Diagnostics.Debug.WriteLine($"Health check warnings: {string.Join(", ", healthCheck.Warnings)}");
                        }
                    }
                    catch (Exception healthEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Health check failed: {healthEx.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle startup errors
                await HandleStartupErrorAsync(ex);
            }
        }



        /// <summary>
        /// Handles startup errors by showing an error dialog and optionally exiting the application
        /// </summary>
        private async Task HandleStartupErrorAsync(Exception ex)
        {
            try
            {
                // Create a temporary window to show the error dialog
                var tempWindow = new MainWindow();
                tempWindow.Activate();

                var dialog = new ContentDialog()
                {
                    Title = "Application Startup Error",
                    Content = $"The application failed to start properly:\n\n{ex.Message}\n\nThe application will now exit. Please try again or contact support if the problem persists.",
                    CloseButtonText = "Exit",
                    XamlRoot = tempWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
                
                // Exit the application
                Exit();
            }
            catch
            {
                // If we can't even show an error dialog, just exit
                System.Diagnostics.Debug.WriteLine($"Critical startup error: {ex.Message}");
                Exit();
            }
        }
    }
}
