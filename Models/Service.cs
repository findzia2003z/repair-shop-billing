using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace RepairShopBilling.Models
{
    public class Service : INotifyPropertyChanged
    {
        private int _serviceId;
        private string _name = string.Empty;
        private decimal _price;
        private string _category = string.Empty;
        private bool _isActive = true;

        public int ServiceId
        {
            get => _serviceId;
            set => SetProperty(ref _serviceId, value);
        }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value ?? string.Empty);
        }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value ?? string.Empty);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public Service()
        {
            IsActive = true;
        }

        public Service(string name, decimal price, string category)
        {
            Name = name;
            Price = price;
            Category = category;
            IsActive = true;
        }

        public Service(string name, decimal price, string category, bool isActive)
        {
            Name = name;
            Price = price;
            Category = category;
            IsActive = isActive;
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

        public override string ToString()
        {
            return $"{Name} - ${Price:F2}";
        }
    }
}