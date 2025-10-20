using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RepairShopBilling.Models;
using RepairShopBilling.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.IO;

namespace RepairShopBilling.ViewModels
{
    /// <summary>
    /// ViewModel for bill viewer functionality with PDF generation and sharing
    /// </summary>
    public class BillViewerViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private readonly IPdfService _pdfService;
        private Bill? _currentBill;
        private bool _isPreviewMode;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The current bill being displayed
        /// </summary>
        public Bill? CurrentBill
        {
            get => _currentBill;
            set
            {
                if (SetProperty(ref _currentBill, value))
                {
                    ((RelayCommand)RefreshBillCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Indicates if the viewer is in preview mode (new bill) or viewing mode (saved bill)
        /// </summary>
        public bool IsPreviewMode
        {
            get => _isPreviewMode;
            set
            {
                if (SetProperty(ref _isPreviewMode, value))
                {
                    ((RelayCommand)RefreshBillCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Indicates if the view is currently loading data
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    ((RelayCommand)RefreshBillCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Error message to display to the user
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Formatted display text for the current mode
        /// </summary>
        public string ModeDisplayText => IsPreviewMode ? "Bill Preview" : "Bill Details";

        /// <summary>
        /// Command to generate and save PDF
        /// </summary>
        public ICommand GeneratePdfCommand { get; }

        /// <summary>
        /// Command to share the bill
        /// </summary>
        public ICommand ShareBillCommand { get; }

        /// <summary>
        /// Command to load a bill by ID
        /// </summary>
        public ICommand LoadBillCommand { get; }

        /// <summary>
        /// Command to refresh the current bill
        /// </summary>
        public ICommand RefreshBillCommand { get; }

        public BillViewerViewModel() : this(new DatabaseService(), new PdfService())
        {
        }

        public BillViewerViewModel(IDatabaseService databaseService, IPdfService pdfService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            
            GeneratePdfCommand = new RelayCommand(async () => await GeneratePdfAsync(), CanExecuteBillCommands);
            ShareBillCommand = new RelayCommand(async () => await ShareBillAsync(), CanExecuteBillCommands);
            LoadBillCommand = new RelayCommand<int>(async (billId) => await LoadBillAsync(billId));
            RefreshBillCommand = new RelayCommand(async () => await RefreshCurrentBillAsync(), () => CurrentBill != null && !IsPreviewMode && !IsLoading);
        }

        /// <summary>
        /// Sets the bill for preview mode (new bill not yet saved)
        /// </summary>
        public void SetBillForPreview(Bill bill)
        {
            if (bill == null)
            {
                ErrorMessage = "Cannot preview null bill.";
                return;
            }

            CurrentBill = bill;
            IsPreviewMode = true;
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(ModeDisplayText));

            // Validate preview bill data
            if (bill.Items == null || !bill.Items.Any())
            {
                ErrorMessage = "Warning: This bill has no items to preview.";
            }
            else if (string.IsNullOrWhiteSpace(bill.CustomerName))
            {
                ErrorMessage = "Warning: Customer name is missing.";
            }
        }

        /// <summary>
        /// Sets the current bill (used for preview functionality)
        /// </summary>
        public void SetCurrentBill(Bill bill)
        {
            SetBillForPreview(bill);
        }

        /// <summary>
        /// Refreshes the current bill from the database (if it's a saved bill)
        /// </summary>
        public async Task RefreshCurrentBillAsync()
        {
            if (CurrentBill == null || IsPreviewMode || CurrentBill.BillId <= 0)
            {
                return;
            }

            await LoadBillAsync(CurrentBill.BillId);
        }

        /// <summary>
        /// Loads a saved bill for viewing
        /// </summary>
        public async Task LoadBillAsync(int billId)
        {
            if (billId <= 0)
            {
                ErrorMessage = "Invalid bill ID provided.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var bill = await _databaseService.GetBillByIdAsync(billId);
                if (bill != null)
                {
                    CurrentBill = bill;
                    IsPreviewMode = false;
                    OnPropertyChanged(nameof(ModeDisplayText));
                    
                    // Validate bill data integrity
                    if (bill.Items == null || !bill.Items.Any())
                    {
                        ErrorMessage = "Warning: This bill has no items.";
                    }
                    else
                    {
                        // Verify total calculation
                        var calculatedTotal = bill.Items.Sum(item => item.TotalPrice);
                        if (Math.Abs(calculatedTotal - bill.TotalAmount) > 0.01m)
                        {
                            ErrorMessage = $"Warning: Bill total mismatch. Calculated: {calculatedTotal:C}, Stored: {bill.TotalAmount:C}";
                        }
                    }
                }
                else
                {
                    ErrorMessage = $"Bill with ID {billId} was not found in the database.";
                    CurrentBill = null;
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException sqlEx)
            {
                ErrorMessage = $"Database error loading bill: {sqlEx.Message}";
                System.Diagnostics.Debug.WriteLine($"SQLite error loading bill {billId}: {sqlEx}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected error loading bill: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading bill {billId}: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Generates and saves a PDF of the current bill
        /// </summary>
        private async Task GeneratePdfAsync()
        {
            if (CurrentBill == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Create file picker for save location
                var savePicker = new FileSavePicker();
                
                // Get the current window handle for the picker
                var app = Application.Current as App;
                var window = app?.MainWindow;
                if (window != null)
                {
                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);
                }

                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("PDF Document", new List<string>() { ".pdf" });
                
                // Generate suggested filename
                var customerName = CurrentBill.CustomerName.Replace(" ", "_");
                var dateString = CurrentBill.Date.ToString("yyyy-MM-dd");
                savePicker.SuggestedFileName = $"Bill_{customerName}_{dateString}";

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Generate PDF using the PDF service
                    await _pdfService.GenerateBillPdfAsync(CurrentBill, file.Path);
                    
                    // Show success message
                    var currentApp = Application.Current as App;
                    var dialog = new ContentDialog()
                    {
                        Title = "PDF Generated",
                        Content = $"Bill saved successfully to:\n{file.Path}",
                        CloseButtonText = "OK",
                        XamlRoot = currentApp?.MainWindow?.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating PDF: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Shares the current bill using the system share dialog
        /// </summary>
        private async Task ShareBillAsync()
        {
            if (CurrentBill == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Use the PDF service to share the bill
                var success = await _pdfService.ShareBillAsync(CurrentBill);
                
                var app = Application.Current as App;
                var dialog = new ContentDialog()
                {
                    Title = success ? "Share Successful" : "Share Failed",
                    Content = success 
                        ? "Bill has been prepared for sharing." 
                        : "Failed to prepare bill for sharing. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = app?.MainWindow?.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error sharing bill: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }



        /// <summary>
        /// Determines if bill-related commands can be executed
        /// </summary>
        private bool CanExecuteBillCommands()
        {
            return CurrentBill != null && !IsLoading;
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