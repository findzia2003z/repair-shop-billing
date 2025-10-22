using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RepairShopBilling.ViewModels;

namespace RepairShopBilling.Views
{
    /// <summary>
    /// Page for viewing detailed bills and PDF generation
    /// </summary>
    public sealed partial class BillViewerView : Page
    {
        public BillViewerViewModel ViewModel { get; }

        public BillViewerView()
        {
            this.InitializeComponent();
            ViewModel = new BillViewerViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            System.Diagnostics.Debug.WriteLine("BillViewerView: OnNavigatedTo called");
            
            // Check if there's preview bill data to load (from Bill Creation preview)
            if (BillCreationViewModel.PreviewBillData != null)
            {
                System.Diagnostics.Debug.WriteLine("BillViewerView: Loading preview bill data");
                ViewModel.SetCurrentBill(BillCreationViewModel.PreviewBillData);
                BillCreationViewModel.PreviewBillData = null; // Clear after use
            }
            else
            {
                // Load bill from database using the bill ID from MainViewModel
                var mainViewModel = MainWindow.GetMainViewModel();
                if (mainViewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine($"BillViewerView: SelectedBillId = {mainViewModel.SelectedBillId}");
                    if (mainViewModel.SelectedBillId > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"BillViewerView: Loading bill {mainViewModel.SelectedBillId} from database");
                        await ViewModel.LoadBillAsync(mainViewModel.SelectedBillId);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("BillViewerView: MainViewModel is null!");
                }
            }
        }

        private void OnBackToEditClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Store the current bill data for editing
            if (ViewModel.CurrentBill != null)
            {
                BillCreationViewModel.PreviewBillData = ViewModel.CurrentBill;
            }
            
            // Navigate back to Bill Creation
            var mainViewModel = MainWindow.GetMainViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigateToBillCreation();
            }
        }

        private void OnBackClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Navigate back to Bill History
            var mainViewModel = MainWindow.GetMainViewModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigateToBillHistory();
            }
        }
    }
}