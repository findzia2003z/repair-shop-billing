# Design Document

## Overview

The Repair Shop Bill Management System is a WinUI 3 desktop application built using the MVVM (Model-View-ViewModel) architectural pattern. The application provides a modern, dark-themed interface for managing repair shop billing operations, including bill creation, history management, and PDF generation. The system uses SQLite for local data storage and follows Microsoft's recommended practices for packaged desktop applications.

## Architecture

### Application Structure
```
RepairShopBilling/
├── Models/
│   ├── Customer.cs
│   ├── Bill.cs
│   ├── BillItem.cs
│   └── Service.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── BillCreationViewModel.cs
│   ├── BillHistoryViewModel.cs
│   └── BillViewerViewModel.cs
├── Views/
│   ├── MainWindow.xaml
│   ├── BillCreationView.xaml
│   ├── BillHistoryView.xaml
│   └── BillViewerView.xaml
├── Services/
│   ├── DatabaseService.cs
│   ├── PdfService.cs
│   └── ServiceCatalogService.cs
├── Helpers/
│   ├── RelayCommand.cs
│   └── ObservableObject.cs
└── Assets/
    └── Styles/
        └── AppStyles.xaml
```

### MVVM Pattern Implementation
- **Models**: Plain C# classes representing data entities (Customer, Bill, BillItem, Service)
- **Views**: XAML files defining the user interface with data binding to ViewModels
- **ViewModels**: Classes implementing INotifyPropertyChanged, handling UI logic and data binding
- **Services**: Business logic layer for database operations, PDF generation, and service management

### Navigation Architecture
The application uses a NavigationView control in the MainWindow with three primary navigation items:
- Bill Creation
- Bill History  
- Settings (future expansion)

Each view is loaded into a Frame control within the NavigationView content area.

## Components and Interfaces

### Data Models

#### Customer Model
```csharp
public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string ContactInfo { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

#### Bill Model
```csharp
public class Bill
{
    public int BillId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string DeviceType { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BillItem> Items { get; set; }
}
```

#### BillItem Model
```csharp
public class BillItem
{
    public int BillItemId { get; set; }
    public int BillId { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
```

#### Service Model
```csharp
public class Service
{
    public int ServiceId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; }
}
```

### Service Interfaces

#### IDatabaseService
```csharp
public interface IDatabaseService
{
    Task InitializeAsync();
    Task<List<Bill>> GetBillsAsync();
    Task<List<Bill>> SearchBillsByCustomerAsync(string customerName);
    Task<Bill> GetBillByIdAsync(int billId);
    Task<int> SaveBillAsync(Bill bill);
    Task<List<Service>> GetServicesByCategoryAsync(string category);
    Task<List<Service>> GetAllServicesAsync();
}
```

#### IPdfService
```csharp
public interface IPdfService
{
    Task<string> GenerateBillPdfAsync(Bill bill, string outputPath);
    Task<bool> ShareBillAsync(Bill bill);
}
```

### ViewModels

#### MainViewModel
- Manages navigation between views
- Handles application-level state
- Coordinates communication between child ViewModels

#### BillCreationViewModel
- Manages bill creation workflow
- Handles service category selection and item addition
- Calculates bill totals in real-time
- Validates customer input
- Saves completed bills

#### BillHistoryViewModel
- Loads and displays bill history
- Implements real-time search filtering
- Handles bill selection for detailed viewing

#### BillViewerViewModel
- Displays detailed bill information
- Handles PDF generation
- Manages sharing functionality
- Supports both new bill preview and saved bill viewing modes

## Data Models

### Database Schema

#### Customers Table
```sql
CREATE TABLE Customers (
    CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    ContactInfo TEXT,
    CreatedDate TEXT NOT NULL
);
```

#### Bills Table
```sql
CREATE TABLE Bills (
    BillId INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId INTEGER,
    CustomerName TEXT NOT NULL,
    DeviceType TEXT,
    Date TEXT NOT NULL,
    TotalAmount REAL NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers (CustomerId)
);
```

#### BillItems Table
```sql
CREATE TABLE BillItems (
    BillItemId INTEGER PRIMARY KEY AUTOINCREMENT,
    BillId INTEGER NOT NULL,
    Description TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice REAL NOT NULL,
    FOREIGN KEY (BillId) REFERENCES Bills (BillId)
);
```

#### Services Table
```sql
CREATE TABLE Services (
    ServiceId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Price REAL NOT NULL,
    Category TEXT NOT NULL,
    IsActive INTEGER DEFAULT 1
);
```

### Service Catalog Data Structure
The system will initialize with predefined service categories and items:

**Categories:**
- HOURS: Labor hours with configurable hourly rate
- EQUIPMENT: Hardware items (RAM, Laptop, Desktop, etc.)
- WIN 11 PRO: Windows 11 Pro installation service
- OS X: macOS versions (Big Sur through Sequoia)
- DRIVERS: Driver update services
- VHS CONVERT: VHS to digital conversion
- DATA REC.: Data recovery services
- PHOTO PRINT: Photo printing (B&W, Color)
- LASER: Laser engraving services (Materials, Packaging, Time Mark, Time Engrave)
- CUSTOM: User-defined services

## Error Handling

### Database Error Handling
- Connection failures: Graceful degradation with user notification
- Data corruption: Automatic backup and recovery mechanisms
- Constraint violations: User-friendly validation messages

### PDF Generation Error Handling
- File system permissions: Alternative save locations
- Disk space issues: User notification and cleanup suggestions
- Template errors: Fallback to basic formatting

### UI Error Handling
- Invalid input validation with real-time feedback
- Network connectivity issues for sharing functionality
- Memory constraints for large bill histories

### Exception Handling Strategy
```csharp
public class ErrorHandlingService
{
    public void HandleException(Exception ex, string context)
    {
        // Log error details
        // Show user-friendly message
        // Attempt recovery if possible
        // Report critical errors for debugging
    }
}
```

## Testing Strategy

### Unit Testing
- **Models**: Validation logic, calculations, property changes
- **ViewModels**: Command execution, property binding, business logic
- **Services**: Database operations, PDF generation, service catalog management

### Integration Testing
- Database initialization and migration
- PDF generation with various bill configurations
- Service catalog loading and filtering
- Navigation flow between views

### UI Testing
- User input validation
- Button click handling
- Data binding verification
- Theme application consistency

### Test Data Management
- Seed database with sample services and categories
- Create test bills with various item combinations
- Mock PDF generation for automated testing
- Test search functionality with diverse customer names

### Performance Testing
- Large bill history loading (1000+ bills)
- Real-time search performance
- PDF generation speed
- Memory usage during extended sessions

## UI Design Specifications

### Color Scheme
- **Primary Background**: #333333 (Dark Grey)
- **Secondary Background**: #404040 (Lighter Grey for cards)
- **Accent Color**: #FF0000 (Red for buttons and highlights)
- **Text Primary**: #FFFFFF (White)
- **Text Secondary**: #CCCCCC (Light Grey)
- **Input Background**: #FFFFFF (White)

### Typography
- **Headers**: Segoe UI, 24pt, Bold
- **Subheaders**: Segoe UI, 18pt, Semi-Bold
- **Body Text**: Segoe UI, 14pt, Regular
- **Button Text**: Segoe UI, 14pt, Bold

### Layout Guidelines
- **Margins**: 20px standard margin for main content areas
- **Padding**: 16px for buttons, 12px for input fields
- **Corner Radius**: 8px for buttons and cards, 4px for input fields
- **Grid Spacing**: 12px between grid items
- **Button Minimum Size**: 120px width, 40px height

### Responsive Design
- Minimum window size: 1024x768
- Maximum window size: Unrestricted
- Content scaling for different DPI settings
- Adaptive layout for various screen sizes

## Deployment and Packaging

### MSIX Package Configuration
- **Package Identity**: SolidStateDave.RepairShopBilling
- **Version**: 1.0.0.0
- **Target Frameworks**: .NET 8.0, WinUI 3
- **Minimum OS Version**: Windows 10 version 1809
- **Architecture Support**: x64, ARM64

### Dependencies
- Microsoft.WindowsAppSDK
- Microsoft.Data.Sqlite
- PdfSharp or similar PDF generation library
- CommunityToolkit.Mvvm

### Installation Requirements
- Windows 10 version 1809 or later
- .NET 8.0 Runtime (included in package)
- 100MB available disk space
- SQLite database file permissions

### Update Strategy
- Automatic update checks on application startup
- Silent updates for minor versions
- User notification for major version updates
- Rollback capability for failed updates