using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using RepairShopBilling.Models;
using RepairShopBilling.Services;
using Windows.UI.Text;

namespace RepairShopBilling.ViewModels
{
    /// <summary>
    /// ViewModel for bill creation functionality
    /// </summary>
    public class BillCreationViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private string _customerName = string.Empty;
        private string _deviceType = string.Empty;
        private DateTimeOffset _billDate = DateTimeOffset.Now;
        private ObservableCollection<BillItem> _billItems = new();
        private bool _isSaving = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public BillCreationViewModel() : this(new DatabaseService())
        {
        }

        public BillCreationViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            BillItems = new ObservableCollection<BillItem>();
            BillItems.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(CanSaveBill));
                OnPropertyChanged(nameof(CanPreviewBill));
            };

            SaveBillCommand = new RelayCommand(async () => await SaveBillAsync(), () => CanSaveBill);
            PreviewBillCommand = new RelayCommand(PreviewBill, () => CanPreviewBill);
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (SetProperty(ref _customerName, value))
                {
                    OnPropertyChanged(nameof(CanSaveBill));
                    OnPropertyChanged(nameof(CanPreviewBill));
                }
            }
        }

        public string DeviceType
        {
            get => _deviceType;
            set => SetProperty(ref _deviceType, value);
        }

        public DateTimeOffset BillDate
        {
            get => _billDate;
            set => SetProperty(ref _billDate, value);
        }

        public ObservableCollection<BillItem> BillItems
        {
            get => _billItems;
            set => SetProperty(ref _billItems, value);
        }

        public string TotalAmount => BillItems.Sum(item => item.TotalPrice).ToString("C");

        public bool CanSaveBill => !string.IsNullOrWhiteSpace(CustomerName) && BillItems.Any() && !_isSaving;

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                if (SetProperty(ref _isSaving, value))
                {
                    OnPropertyChanged(nameof(CanSaveBill));
                    OnPropertyChanged(nameof(CanPreviewBill));
                    ((RelayCommand)SaveBillCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanPreviewBill => !string.IsNullOrWhiteSpace(CustomerName) && BillItems.Any() && !_isSaving;

        public ICommand SaveBillCommand { get; }
        public ICommand PreviewBillCommand { get; }

        public async void ShowServiceCategoryFlyout(string category, FrameworkElement target)
        {
            if (category == "CUSTOM")
            {
                await ShowCustomServiceDialog();
                return;
            }

            var flyout = new Flyout();
            var stackPanel = new StackPanel { Spacing = 8 };

            try
            {
                var services = await GetServicesForCategoryAsync(category);
                
                if (!services.Any())
                {
                    var noServicesLabel = new TextBlock
                    {
                        Text = $"No services available in {category} category",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                        FontStyle = Windows.UI.Text.FontStyle.Italic,
                        Margin = new Thickness(8, 8, 8, 8)
                    };
                    stackPanel.Children.Add(noServicesLabel);
                }
                else
                {
                    foreach (var service in services)
                    {
                        var button = new Button
                        {
                            Content = $"{service.Name} - {service.Price:C}",
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Margin = new Thickness(0, 2, 0, 2),
                            Tag = service
                        };
                        button.Click += OnServiceSelected;
                        stackPanel.Children.Add(button);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLabel = new TextBlock
                {
                    Text = $"Error loading services: {ex.Message}",
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                    Margin = new Thickness(8, 8, 8, 8)
                };
                stackPanel.Children.Add(errorLabel);
                System.Diagnostics.Debug.WriteLine($"Error loading services for category {category}: {ex.Message}");
            }

            flyout.Content = stackPanel;
            flyout.ShowAt(target);
        }

        public void RemoveBillItem(BillItem item)
        {
            BillItems.Remove(item);
        }

        private void OnServiceSelected(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Service service)
            {
                var billItem = new BillItem
                {
                    Description = service.Name,
                    Quantity = 1,
                    UnitPrice = service.Price
                };

                BillItems.Add(billItem);

                // Close the flyout
                if (button.Parent is StackPanel stackPanel && 
                    stackPanel.Parent is Flyout flyout)
                {
                    flyout.Hide();
                }
            }
        }

        private async Task<List<Service>> GetServicesForCategoryAsync(string category)
        {
            try
            {
                return await _databaseService.GetServicesByCategoryAsync(category);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting services for category {category}: {ex.Message}");
                return new List<Service>();
            }
        }

        private async Task SaveBillAsync()
        {
            if (!CanSaveBill) return;

            IsSaving = true;

            var bill = new Bill
            {
                CustomerName = CustomerName,
                DeviceType = DeviceType,
                Date = BillDate.DateTime,
                Items = new ObservableCollection<BillItem>(BillItems)
            };

            try
            {
                var billId = await _databaseService.SaveBillAsync(bill);
                bill.BillId = billId;
                
                // Show success message to user
                var app = Microsoft.UI.Xaml.Application.Current as App;
                var dialog = new ContentDialog()
                {
                    Title = "Bill Saved",
                    Content = $"Bill for {CustomerName} has been saved successfully.\nBill ID: {billId}",
                    CloseButtonText = "OK",
                    XamlRoot = app?.MainWindow?.Content.XamlRoot
                };
                await dialog.ShowAsync();
                
                // Clear the form after successful save
                CustomerName = string.Empty;
                DeviceType = string.Empty;
                BillDate = DateTimeOffset.Now;
                BillItems.Clear();
            }
            catch (Exception ex)
            {
                // Show error message to user
                var app = Microsoft.UI.Xaml.Application.Current as App;
                var dialog = new ContentDialog()
                {
                    Title = "Save Error",
                    Content = $"Failed to save bill: {ex.Message}\n\nPlease try again or contact support if the problem persists.",
                    CloseButtonText = "OK",
                    XamlRoot = app?.MainWindow?.Content.XamlRoot
                };
                await dialog.ShowAsync();
                
                System.Diagnostics.Debug.WriteLine($"Error saving bill: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void PreviewBill()
        {
            if (!CanPreviewBill) return;

            var bill = new Bill
            {
                CustomerName = CustomerName,
                DeviceType = DeviceType,
                Date = BillDate.DateTime,
                Items = new ObservableCollection<BillItem>(BillItems)
            };

            // TODO: Navigate to BillViewerView with the current bill
            System.Diagnostics.Debug.WriteLine($"Preview bill for {CustomerName} with total {TotalAmount}");
        }

        private async Task ShowCustomServiceDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Add Custom Service",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var stackPanel = new StackPanel { Spacing = 16 };

            // Service Name Input
            var nameLabel = new TextBlock 
            { 
                Text = "Service Name:", 
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black),
                FontSize = 14 
            };
            var nameTextBox = new TextBox 
            { 
                PlaceholderText = "Enter service name",
                MinWidth = 300
            };

            // Price Input
            var priceLabel = new TextBlock 
            { 
                Text = "Price:", 
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black),
                FontSize = 14 
            };
            var priceTextBox = new TextBox 
            { 
                PlaceholderText = "Enter price (e.g., 25.00)",
                MinWidth = 300
            };

            stackPanel.Children.Add(nameLabel);
            stackPanel.Children.Add(nameTextBox);
            stackPanel.Children.Add(priceLabel);
            stackPanel.Children.Add(priceTextBox);

            dialog.Content = stackPanel;

            // Validation
            dialog.PrimaryButtonClick += (sender, args) =>
            {
                args.Cancel = true; // Prevent dialog from closing initially

                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    // TODO: Show validation error
                    return;
                }

                if (!decimal.TryParse(priceTextBox.Text, out decimal price) || price <= 0)
                {
                    // TODO: Show validation error
                    return;
                }

                // Add the custom service
                var billItem = new BillItem
                {
                    Description = nameTextBox.Text.Trim(),
                    Quantity = 1,
                    UnitPrice = price
                };

                BillItems.Add(billItem);
                args.Cancel = false; // Allow dialog to close
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing custom service dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets a property value and raises PropertyChanged if the value changed
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}