using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RepairShopBilling.ViewModels;
using RepairShopBilling.Models;
using System.Collections.ObjectModel;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            System.Diagnostics.Debug.WriteLine($"BillCreationView: OnNavigatedTo - PreviewBillData is {(BillCreationViewModel.PreviewBillData != null ? "NOT NULL" : "NULL")}");
            
            // Check if there's preview bill data to restore
            if (BillCreationViewModel.PreviewBillData != null)
            {
                var bill = BillCreationViewModel.PreviewBillData;
                
                System.Diagnostics.Debug.WriteLine($"BillCreationView: Restoring bill data for {bill.CustomerName}");
                
                // Restore customer information
                ViewModel.CustomerName = bill.CustomerName;
                ViewModel.DeviceType = bill.DeviceType;
                ViewModel.BillDate = new DateTimeOffset(bill.Date);
                
                // Restore bill items
                ViewModel.BillItems.Clear();
                foreach (var item in bill.Items)
                {
                    ViewModel.BillItems.Add(new BillItem
                    {
                        Description = item.Description,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }
                
                BillCreationViewModel.PreviewBillData = null; // Clear after use
            }
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
                
                // Special handling for Materials and Add-Ons - show custom input dialog
                if (serviceName == "Materials" || serviceName == "Add-Ons")
                {
                    await ViewModel.ShowMaterialInputDialog(category, serviceName);
                }
                else
                {
                    await ViewModel.ShowPriceInputDialog(serviceName, category);
                }
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

        private void OnToggleRAMClick(object sender, RoutedEventArgs e)
        {
            if (RAMSubPanel.Visibility == Visibility.Visible)
            {
                RAMSubPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                RAMSubPanel.Visibility = Visibility.Visible;
            }
        }

        private void OnToggleLaptopClick(object sender, RoutedEventArgs e)
        {
            if (LaptopSubPanel.Visibility == Visibility.Visible)
            {
                LaptopSubPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                LaptopSubPanel.Visibility = Visibility.Visible;
            }
        }

        private void OnToggleMonitorClick(object sender, RoutedEventArgs e)
        {
            if (MonitorSubPanel.Visibility == Visibility.Visible)
            {
                MonitorSubPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MonitorSubPanel.Visibility = Visibility.Visible;
            }
        }

        private void OnToggleNVMEClick(object sender, RoutedEventArgs e)
        {
            if (NVMESubPanel.Visibility == Visibility.Visible)
            {
                NVMESubPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                NVMESubPanel.Visibility = Visibility.Visible;
            }
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

        private async void OnEditItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BillItem item)
            {
                await ViewModel.ShowEditItemDialog(item);
            }
        }
    }
}