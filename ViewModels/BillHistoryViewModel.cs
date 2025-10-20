using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using RepairShopBilling.Models;
using RepairShopBilling.Services;

namespace RepairShopBilling.ViewModels
{
    /// <summary>
    /// ViewModel for bill history functionality with search and navigation
    /// </summary>
    public class BillHistoryViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private ObservableCollection<Bill> _allBills;
        private ObservableCollection<Bill> _filteredBills;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Bill> FilteredBills
        {
            get => _filteredBills;
            private set => SetProperty(ref _filteredBills, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterBills();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public bool IsEmpty => !IsLoading && (FilteredBills?.Count ?? 0) == 0;

        public ICommand SelectBillCommand { get; }

        public BillHistoryViewModel() : this(new DatabaseService())
        {
        }

        public BillHistoryViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _allBills = new ObservableCollection<Bill>();
            _filteredBills = new ObservableCollection<Bill>();
            
            SelectBillCommand = new RelayCommand<Bill>(OnBillSelected);
            
            _ = LoadBillsAsync();
        }

        public async Task LoadBillsAsync()
        {
            try
            {
                IsLoading = true;
                var bills = await _databaseService.GetBillsAsync();
                
                _allBills.Clear();
                foreach (var bill in bills)
                {
                    _allBills.Add(bill);
                }
                
                FilterBills();
            }
            catch (Exception ex)
            {
                // Handle error - could show a message to user
                System.Diagnostics.Debug.WriteLine($"Error loading bills: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        private void FilterBills()
        {
            FilteredBills.Clear();
            
            var filteredList = string.IsNullOrWhiteSpace(SearchText) 
                ? _allBills 
                : _allBills.Where(b => b.CustomerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            
            foreach (var bill in filteredList)
            {
                FilteredBills.Add(bill);
            }
            
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void OnBillSelected(Bill? bill)
        {
            if (bill == null) return;
            
            // Navigate to bill viewer - this would typically be handled by a navigation service
            // For now, we'll raise an event that the main view model can handle
            BillSelected?.Invoke(bill);
        }

        public event Action<Bill>? BillSelected;

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