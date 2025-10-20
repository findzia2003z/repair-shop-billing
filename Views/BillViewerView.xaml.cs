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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Check if there's preview bill data to load
            if (BillCreationViewModel.PreviewBillData != null)
            {
                ViewModel.SetCurrentBill(BillCreationViewModel.PreviewBillData);
                BillCreationViewModel.PreviewBillData = null; // Clear after use
            }
        }
    }
}