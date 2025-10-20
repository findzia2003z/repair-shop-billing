using Microsoft.UI.Xaml.Controls;
using RepairShopBilling.ViewModels;

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
        }
    }
}