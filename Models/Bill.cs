using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

namespace RepairShopBilling.Models
{
    public class Bill : INotifyPropertyChanged
    {
        private int _billId;
        private int _customerId;
        private string _customerName = string.Empty;
        private string _deviceType = string.Empty;
        private DateTime _date;
        private ObservableCollection<BillItem> _items;

        public int BillId
        {
            get => _billId;
            set => SetProperty(ref _billId, value);
        }

        public int CustomerId
        {
            get => _customerId;
            set => SetProperty(ref _customerId, value);
        }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters")]
        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value ?? string.Empty);
        }

        [StringLength(50, ErrorMessage = "Device type cannot exceed 50 characters")]
        public string DeviceType
        {
            get => _deviceType;
            set => SetProperty(ref _deviceType, value ?? string.Empty);
        }

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public ObservableCollection<BillItem> Items
        {
            get => _items;
            set
            {
                if (_items != null)
                {
                    _items.CollectionChanged -= Items_CollectionChanged;
                    foreach (var item in _items)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                }

                SetProperty(ref _items, value);

                if (_items != null)
                {
                    _items.CollectionChanged += Items_CollectionChanged;
                    foreach (var item in _items)
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                }

                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public decimal TotalAmount => Items?.Sum(item => item.TotalPrice) ?? 0;

        public Bill()
        {
            Date = DateTime.Now;
            _items = new ObservableCollection<BillItem>();
            _items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (BillItem item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (BillItem item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }

            OnPropertyChanged(nameof(TotalAmount));
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BillItem.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}