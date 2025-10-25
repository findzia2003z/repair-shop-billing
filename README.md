# Repair Shop Billing System

A modern WinUI 3 application for managing repair shop billing and customer records.

## Features

- **Customer Management**: Add, edit, and manage customer information
- **Service Catalog**: Maintain a list of services with pricing
- **Bill Creation**: Create detailed bills with multiple items and services
- **Bill History**: View and search through past bills
- **PDF Export**: Generate professional PDF invoices
- **Modern UI**: Clean, responsive interface built with WinUI 3

## Technology Stack

- **Framework**: .NET 6+ with WinUI 3
- **Architecture**: MVVM (Model-View-ViewModel)
- **Database**: SQLite with Entity Framework Core
- **PDF Generation**: Custom PDF service
- **UI Framework**: Microsoft WinUI 3

## Getting Started

### Prerequisites

- Visual Studio 2022 with WinUI 3 workload
- .NET 6.0 SDK or later
- Windows 10 version 1809 (build 17763) or later

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/[username]/repair-shop-billing.git
   ```

2. Open `RepairShopBilling.sln` in Visual Studio 2022

3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

4. Build and run the application:
   ```bash
   dotnet run
   ```

## Project Structure

```
RepairShopBilling/
├── Models/           # Data models (Customer, Bill, Service, etc.)
├── ViewModels/       # MVVM ViewModels
├── Views/           # XAML UI views
├── Services/        # Business logic and data services
├── Helpers/         # Utility classes and converters
├── Assets/          # Images, styles, and resources
└── Properties/      # Application properties and settings
```

## Key Components

- **Models**: Define the data structure for customers, bills, services, and bill items
- **Services**: Handle database operations, PDF generation, and application startup
- **ViewModels**: Implement business logic and data binding for views
- **Views**: Provide the user interface using WinUI 3 XAML

## 📦 Creating Distributable Installer

This project includes complete packaging solutions to create installers with all dependencies bundled.

### Quick Start
```batch
# Just double-click this file:
quick-package.bat
```

Choose option 2 for a self-contained installer that includes:
- ✅ Complete application
- ✅ All PNG images and assets
- ✅ Ezra Bold font
- ✅ PDFsharp library
- ✅ SQLite database
- ✅ .NET runtime
- ✅ All dependencies

**No separate installations needed by end users!**

### Distribution
1. Run `quick-package.bat`
2. Compress the `installer` folder to ZIP
3. Share with users
4. Users extract and run `install.bat` as administrator

### Documentation
- **START-HERE.md** - Quick start guide
- **PACKAGING-QUICK-START.md** - 2-minute packaging guide
- **INSTALLATION-GUIDE.md** - Complete reference
- **COMPLETE-WORKFLOW.md** - Full workflow from code to distribution
- **TROUBLESHOOTING.md** - Common issues and solutions

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please open an issue in the GitHub repository.