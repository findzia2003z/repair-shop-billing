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

        private void OnServiceClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string serviceInfo)
            {
                var parts = serviceInfo.Split('|');
                if (parts.Length >= 2 && decimal.TryParse(parts[1], out decimal price))
                {
                    var serviceName = parts[0];
                    var category = parts.Length > 2 ? parts[2] : "";
                    ViewModel.AddServiceToBill(serviceName, price, category);
                }
            }
        }

        private async void OnEditablePriceServiceClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string serviceInfo)
            {
                var parts = serviceInfo.Split('|');
                var serviceName = parts[0];
                var category = parts.Length > 1 ? parts[1] : "";
                await ViewModel.ShowPriceInputDialog(serviceName, category);
            }
        }

        private void OnToggleOSXClick(object sender, RoutedEventArgs e)
        {
            ToggleSection(OSXSubPanel, OSXHeaderButton, "OS X");
        }

        private void OnToggleLaserClick(object sender, RoutedEventArgs e)
        {
            ToggleSection(LaserSubPanel, LaserHeaderButton, "LASER");
        }

        private void OnToggleEquipmentClick(object sender, RoutedEventArgs e)
        {
            ToggleSection(EquipmentSubPanel, EquipmentHeaderButton, "Equipment");
        }

        private void ToggleSection(StackPanel subPanel, Button headerButton, string sectionName)
        {
            if (subPanel.Visibility == Visibility.Visible)
            {
                subPanel.Visibility = Visibility.Collapsed;
                headerButton.Content = $"{sectionName} ▶";
            }
            else
            {
                subPanel.Visibility = Visibility.Visible;
                headerButton.Content = $"{sectionName} ▼";
            }
        }

        private void OnRemoveItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BillItem item)
            {
                ViewModel.RemoveBillItem(item);
            }
        }

        private async void OnCustomServiceClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.ShowCustomServiceDialog();
        }
    }
}