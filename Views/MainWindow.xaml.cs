using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RepairShopBilling.ViewModels;

namespace RepairShopBilling.Views
{
    /// <summary>
    /// Main window with navigation structure for the repair shop billing application
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();
            
            // Set window properties
            Title = "Solid State Dave - Repair Shop Billing";
            
            // Subscribe to ViewModel events
            ViewModel.ViewChanged += OnViewModelViewChanged;
            
            // Navigate to default page (Bill Creation)
            NavigateToPage(typeof(BillCreationView));
            MainNavigationView.SelectedItem = BillCreationNavItem;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                string tag = selectedItem.Tag?.ToString();
                
                switch (tag)
                {
                    case "BillCreation":
                        ViewModel.NavigateToBillCreation();
                        NavigateToPage(typeof(BillCreationView));
                        break;
                    case "BillHistory":
                        ViewModel.NavigateToBillHistory();
                        NavigateToPage(typeof(BillHistoryView));
                        break;
                }
            }
        }

        private void OnViewModelViewChanged(string viewName)
        {
            // Handle programmatic navigation from ViewModel
            Type pageType = viewName switch
            {
                "BillCreation" => typeof(BillCreationView),
                "BillHistory" => typeof(BillHistoryView),
                "BillViewer" => typeof(BillViewerView),
                _ => typeof(BillCreationView)
            };

            NavigateToPage(pageType);
            UpdateNavigationSelection(viewName);
        }

        private void UpdateNavigationSelection(string viewName)
        {
            NavigationViewItem? itemToSelect = viewName switch
            {
                "BillCreation" => BillCreationNavItem,
                "BillHistory" => BillHistoryNavItem,
                _ => BillCreationNavItem
            };

            if (itemToSelect != null && MainNavigationView.SelectedItem != itemToSelect)
            {
                MainNavigationView.SelectedItem = itemToSelect;
            }
        }

        private void NavigateToPage(Type pageType)
        {
            try
            {
                ContentFrame.Navigate(pageType);
            }
            catch (Exception ex)
            {
                // Handle navigation errors gracefully
                System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
            }
        }

        public void NavigateToBillViewer(int billId = 0)
        {
            // Navigate to bill viewer - will be used by other ViewModels
            ViewModel.NavigateToBillViewer(billId);
        }

        /// <summary>
        /// Gets the current MainViewModel instance for access by child views
        /// </summary>
        public static MainViewModel? GetMainViewModel()
        {
            // This allows child ViewModels to access the main navigation
            if (Application.Current is App app && app.MainWindow is MainWindow mainWindow)
            {
                return mainWindow.ViewModel;
            }
            return null;
        }
    }
}