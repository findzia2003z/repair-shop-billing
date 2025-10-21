using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RepairShopBilling.ViewModels;
using RepairShopBilling.Models;

namespace RepairShopBilling.Views
{
    /// <summary>
    /// Page for viewing bill history with search and grid functionality
    /// </summary>
    public sealed partial class BillHistoryView : Page
    {
        public BillHistoryViewModel ViewModel { get; }

        public BillHistoryView()
        {
            this.InitializeComponent();
            ViewModel = new BillHistoryViewModel();
            this.DataContext = ViewModel;
            
            // Subscribe to bill selection event
            ViewModel.BillSelected += OnBillSelected;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Reload bills when navigating to this page
            await ViewModel.LoadBillsAsync();
        }

        private void OnBillSelected(Bill bill)
        {
            System.Diagnostics.Debug.WriteLine($"Bill selected: {bill.CustomerName}, ID: {bill.BillId}");
            
            // Navigate to BillViewerView with the selected bill
            var mainViewModel = MainWindow.GetMainViewModel();
            if (mainViewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"Navigating to BillViewer with ID: {bill.BillId}");
                mainViewModel.NavigateToBillViewer(bill.BillId);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MainViewModel is null!");
            }
        }
    }
}