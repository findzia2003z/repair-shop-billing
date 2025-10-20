# Requirements Document

## Introduction

The Repair Shop Bill Management System is a WinUI 3 desktop application designed for "Solid State Dave," a computer and electronics repair shop. The application streamlines the billing process by allowing users to create professional bills, manage service catalogs, view billing history, and generate PDF invoices. The system features a modern dark-themed interface with red accent colors and follows MVVM architecture patterns.

## Requirements

### Requirement 1

**User Story:** As a repair shop employee, I want to create new bills for customers by selecting from predefined services and equipment, so that I can quickly generate accurate invoices without manual price calculations.

#### Acceptance Criteria

1. WHEN the user opens the bill creation screen THEN the system SHALL display input fields for customer name, device type, and date
2. WHEN the user clicks on a service category button THEN the system SHALL display a flyout with specific items in that category
3. WHEN the user selects an item from a category THEN the system SHALL add it to the current bill with the correct price
4. WHEN the user clicks the CUSTOM button THEN the system SHALL allow manual entry of service name and price
5. WHEN items are added to a bill THEN the system SHALL automatically calculate and display the total amount
6. WHEN the date field is displayed THEN the system SHALL auto-populate it with the current date but allow editing

### Requirement 2

**User Story:** As a repair shop employee, I want to view and search through all previous bills, so that I can quickly find customer billing history and reference past services.

#### Acceptance Criteria

1. WHEN the user opens the bill history screen THEN the system SHALL display a grid of all saved bills with customer name, date, and total amount
2. WHEN the user types in the search bar THEN the system SHALL filter the bill grid in real-time by customer name
3. WHEN the user clicks on a bill summary card THEN the system SHALL navigate to the detailed bill viewer for that specific bill
4. WHEN displaying bill summaries THEN the system SHALL show customer name and date in white/light grey text and total amount in red accent color

### Requirement 3

**User Story:** As a repair shop employee, I want to view detailed bills in a professional format and generate PDF invoices, so that I can provide customers with proper documentation and maintain records.

#### Acceptance Criteria

1. WHEN viewing a bill THEN the system SHALL display the shop name "Solid State Dave" as a header
2. WHEN viewing a bill THEN the system SHALL show customer name, date, and a three-column services table (Services, Amount, Price)
3. WHEN viewing a bill THEN the system SHALL display the calculated total prominently
4. WHEN viewing a bill THEN the system SHALL show footer information including logo placeholder, thank you message, website, and phone number
5. WHEN the user clicks "Download/Save as PDF" THEN the system SHALL generate a PDF file of the current bill
6. WHEN the user clicks "Share" THEN the system SHALL open the default OS share dialog

### Requirement 4

**User Story:** As a repair shop owner, I want the system to manage predefined service categories and pricing, so that employees can quickly select standardized services with consistent pricing.

#### Acceptance Criteria

1. WHEN the system loads THEN it SHALL provide predefined service categories: HOURS, EQUIPMENT, WIN 11 PRO, OS X, DRIVERS, VHS CONVERT, DATA REC., PHOTO PRINT, LASER, CUSTOM
2. WHEN the EQUIPMENT category is selected THEN the system SHALL display: RAM, Laptop, Desktop, Modem, Router, Monitor, NVME, KB&M, Sec. Camera
3. WHEN the OS X category is selected THEN the system SHALL display: Big Sur 11 (20), Monterey 12 (21), Ventura 13 (22), Sonoma 14 (23), Sequoia 15 (24), Tahoe 26 (25)
4. WHEN the LASER category is selected THEN the system SHALL display: Materials, Packaging, Time Mark, Time Engrave
5. WHEN the PHOTO PRINT category is selected THEN the system SHALL display: B&W, COLOR options

### Requirement 5

**User Story:** As a repair shop employee, I want the application to have a consistent dark theme with red accents, so that it matches the shop's branding and provides a modern user experience.

#### Acceptance Criteria

1. WHEN the application loads THEN the system SHALL use a dark grey background (#333333 or similar)
2. WHEN displaying buttons THEN the system SHALL use red background (#FF0000 or similar) with white text
3. WHEN displaying input fields THEN the system SHALL use white background with rounded corners
4. WHEN displaying bill cards THEN the system SHALL use slightly lighter grey background with rounded corners
5. WHEN displaying table headers THEN the system SHALL use the red accent color
6. WHEN displaying total amounts THEN the system SHALL use the red accent color for emphasis

### Requirement 6

**User Story:** As a repair shop owner, I want all bill and customer data to be stored locally in a database, so that the system works offline and maintains data privacy.

#### Acceptance Criteria

1. WHEN the application starts THEN the system SHALL initialize a local SQLite database
2. WHEN a bill is created THEN the system SHALL store customer information, bill details, and line items in the database
3. WHEN the system stores data THEN it SHALL use tables for Customers, Bills, BillItems, and Services
4. WHEN retrieving bill history THEN the system SHALL query the local database without requiring internet connectivity
5. WHEN managing service catalogs THEN the system SHALL store and retrieve service definitions from the local database

### Requirement 7

**User Story:** As a repair shop owner, I want the application to be distributed as an MSIX package, so that it can be easily installed and updated on employee workstations.

#### Acceptance Criteria

1. WHEN the application is built THEN the system SHALL generate a signed MSIX package
2. WHEN the MSIX package is installed THEN the system SHALL create all necessary database files and configurations
3. WHEN the application is packaged THEN it SHALL include all required dependencies and runtime components
4. WHEN the package is distributed THEN it SHALL be compatible with Windows 10 and Windows 11 systems

### Requirement 8

**User Story:** As a repair shop employee, I want to navigate between different screens easily, so that I can efficiently switch between creating bills, viewing history, and managing the application.

#### Acceptance Criteria

1. WHEN the application loads THEN the system SHALL provide a main navigation interface
2. WHEN navigating THEN the system SHALL allow access to Bill Creation, Bill History, and Bill Viewer screens
3. WHEN switching between screens THEN the system SHALL maintain application state and user context
4. WHEN using navigation THEN the system SHALL provide clear visual indicators of the current screen