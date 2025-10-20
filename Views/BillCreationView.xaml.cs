using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RepairShopBilling.ViewModels;
using RepairShopBilling.Models;

namespace RepairShopBilling.Views
{
    /// <summary>
    /// Page for creating new bills
    /// </summary>
    public sealed partial class BillCreationView : Page
    {
        public BillCreationViewModel ViewModel { get; }

        public BillCreationView()
        {
            this.InitializeComponent();
            ViewModel = new BillCreationViewModel();
            this.DataContext = ViewModel;
        }

        private void OnServiceCategoryClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string category)
            {
                ViewModel.ShowServiceCategoryFlyout(category, button);
            }
        }

        private void OnRemoveItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BillItem item)
            {
                ViewModel.RemoveBillItem(item);
            }
        }
    }
}