using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RepairShopBilling.ViewModels
{
    /// <summary>
    /// Main ViewModel for coordinating navigation and application-level state
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _currentView = "BillCreation";
        private bool _isNavigating = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the current view identifier
        /// </summary>
        public string CurrentView
        {
            get => _currentView;
            set
            {
                if (SetProperty(ref _currentView, value))
                {
                    OnCurrentViewChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether navigation is in progress
        /// </summary>
        public bool IsNavigating
        {
            get => _isNavigating;
            set => SetProperty(ref _isNavigating, value);
        }

        /// <summary>
        /// Gets the title for the current view
        /// </summary>
        public string CurrentViewTitle => CurrentView switch
        {
            "BillCreation" => "Create New Bill",
            "BillHistory" => "Bill History",
            "BillViewer" => "Bill Viewer",
            _ => "Repair Shop Billing"
        };

        public MainViewModel()
        {
            // Initialize with default view
            CurrentView = "BillCreation";
        }

        /// <summary>
        /// Navigates to the specified view
        /// </summary>
        /// <param name="viewName">The name of the view to navigate to</param>
        public void NavigateToView(string viewName)
        {
            if (CurrentView != viewName)
            {
                IsNavigating = true;
                CurrentView = viewName;
                IsNavigating = false;
            }
        }

        /// <summary>
        /// Navigates to the bill creation view
        /// </summary>
        public void NavigateToBillCreation()
        {
            NavigateToView("BillCreation");
        }

        /// <summary>
        /// Navigates to the bill history view
        /// </summary>
        public void NavigateToBillHistory()
        {
            NavigateToView("BillHistory");
        }

        /// <summary>
        /// Navigates to the bill viewer with optional bill ID
        /// </summary>
        /// <param name="billId">The ID of the bill to view, or 0 for new bill preview</param>
        public void NavigateToBillViewer(int billId = 0)
        {
            // Store the bill ID for the viewer
            SelectedBillId = billId;
            NavigateToView("BillViewer");
        }

        /// <summary>
        /// Gets or sets the currently selected bill ID for viewing
        /// </summary>
        public int SelectedBillId { get; set; }

        /// <summary>
        /// Called when the current view changes
        /// </summary>
        private void OnCurrentViewChanged()
        {
            OnPropertyChanged(nameof(CurrentViewTitle));
            
            // Notify any listeners about view change
            ViewChanged?.Invoke(CurrentView);
        }

        /// <summary>
        /// Event raised when the current view changes
        /// </summary>
        public event Action<string>? ViewChanged;

        /// <summary>
        /// Sets a property value and raises PropertyChanged if the value changed
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property (automatically provided)</param>
        /// <returns>True if the value changed, false otherwise</returns>
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
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}