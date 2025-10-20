using Microsoft.Data.Sqlite;
using RepairShopBilling.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace RepairShopBilling.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly string _databasePath;

    public DatabaseService() : this(GetDefaultDatabasePath())
    {
    }

    public DatabaseService(string databasePath)
    {
        _databasePath = databasePath;
        _connectionString = $"Data Source={_databasePath}";
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string GetDefaultDatabasePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "RepairShopBilling");
        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "RepairShopBilling.db");
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Create Customers table
        var createCustomersTable = @"
            CREATE TABLE IF NOT EXISTS Customers (
                CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                ContactInfo TEXT,
                CreatedDate TEXT NOT NULL
            )";

        // Create Bills table
        var createBillsTable = @"
            CREATE TABLE IF NOT EXISTS Bills (
                BillId INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER,
                CustomerName TEXT NOT NULL,
                DeviceType TEXT,
                Date TEXT NOT NULL,
                TotalAmount REAL NOT NULL,
                FOREIGN KEY (CustomerId) REFERENCES Customers (CustomerId)
            )";

        // Create BillItems table
        var createBillItemsTable = @"
            CREATE TABLE IF NOT EXISTS BillItems (
                BillItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                BillId INTEGER NOT NULL,
                Description TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL,
                FOREIGN KEY (BillId) REFERENCES Bills (BillId)
            )";

        // Create Services table
        var createServicesTable = @"
            CREATE TABLE IF NOT EXISTS Services (
                ServiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Price REAL NOT NULL,
                Category TEXT NOT NULL,
                IsActive INTEGER DEFAULT 1
            )";

        using var command = connection.CreateCommand();
        
        command.CommandText = createCustomersTable;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createBillsTable;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createBillItemsTable;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createServicesTable;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Bill>> GetBillsAsync()
    {
        var bills = new List<Bill>();
        
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT BillId, CustomerId, CustomerName, DeviceType, Date 
            FROM Bills 
            ORDER BY Date DESC";
            
        using var command = new SqliteCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var bill = new Bill
            {
                BillId = reader.GetInt32(reader.GetOrdinal("BillId")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CustomerId")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                DeviceType = reader.IsDBNull(reader.GetOrdinal("DeviceType")) ? string.Empty : reader.GetString(reader.GetOrdinal("DeviceType")),
                Date = DateTime.Parse(reader.GetString(reader.GetOrdinal("Date")))
            };
            
            // Load bill items
            var billItems = await GetBillItemsAsync(bill.BillId);
            bill.Items = new ObservableCollection<BillItem>(billItems);
            bills.Add(bill);
        }
        
        return bills;
    }

    public async Task<List<Bill>> SearchBillsByCustomerAsync(string customerName)
    {
        var bills = new List<Bill>();
        
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT BillId, CustomerId, CustomerName, DeviceType, Date 
            FROM Bills 
            WHERE CustomerName LIKE @customerName 
            ORDER BY Date DESC";
            
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@customerName", $"%{customerName}%");
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var bill = new Bill
            {
                BillId = reader.GetInt32(reader.GetOrdinal("BillId")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CustomerId")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                DeviceType = reader.IsDBNull(reader.GetOrdinal("DeviceType")) ? string.Empty : reader.GetString(reader.GetOrdinal("DeviceType")),
                Date = DateTime.Parse(reader.GetString(reader.GetOrdinal("Date")))
            };
            
            // Load bill items
            var billItems = await GetBillItemsAsync(bill.BillId);
            bill.Items = new ObservableCollection<BillItem>(billItems);
            bills.Add(bill);
        }
        
        return bills;
    }

    public async Task<Bill?> GetBillByIdAsync(int billId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT BillId, CustomerId, CustomerName, DeviceType, Date 
            FROM Bills 
            WHERE BillId = @billId";
            
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@billId", billId);
        
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            var bill = new Bill
            {
                BillId = reader.GetInt32(reader.GetOrdinal("BillId")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CustomerId")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                DeviceType = reader.IsDBNull(reader.GetOrdinal("DeviceType")) ? string.Empty : reader.GetString(reader.GetOrdinal("DeviceType")),
                Date = DateTime.Parse(reader.GetString(reader.GetOrdinal("Date")))
            };
            
            // Load bill items
            var billItems = await GetBillItemsAsync(bill.BillId);
            bill.Items = new ObservableCollection<BillItem>(billItems);
            return bill;
        }
        
        return null;
    }

    private async Task<List<BillItem>> GetBillItemsAsync(int billId)
    {
        var items = new List<BillItem>();
        
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT BillItemId, BillId, Description, Quantity, UnitPrice 
            FROM BillItems 
            WHERE BillId = @billId";
            
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@billId", billId);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var item = new BillItem
            {
                BillItemId = reader.GetInt32(reader.GetOrdinal("BillItemId")),
                BillId = reader.GetInt32(reader.GetOrdinal("BillId")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice"))
            };
            
            items.Add(item);
        }
        
        return items;
    }

    public async Task<int> SaveBillAsync(Bill bill)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();
        
        try
        {
            int billId;
            
            if (bill.BillId == 0)
            {
                // Insert new bill
                var insertBillQuery = @"
                    INSERT INTO Bills (CustomerId, CustomerName, DeviceType, Date, TotalAmount) 
                    VALUES (@customerId, @customerName, @deviceType, @date, @totalAmount);
                    SELECT last_insert_rowid();";
                    
                using var billCommand = new SqliteCommand(insertBillQuery, connection, transaction);
                // Set CustomerId to NULL if it's 0 or invalid to avoid foreign key constraint
                billCommand.Parameters.AddWithValue("@customerId", bill.CustomerId > 0 ? (object)bill.CustomerId : DBNull.Value);
                billCommand.Parameters.AddWithValue("@customerName", bill.CustomerName);
                billCommand.Parameters.AddWithValue("@deviceType", bill.DeviceType ?? string.Empty);
                billCommand.Parameters.AddWithValue("@date", bill.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                billCommand.Parameters.AddWithValue("@totalAmount", bill.TotalAmount);
                
                var result = await billCommand.ExecuteScalarAsync();
                billId = Convert.ToInt32(result);
                bill.BillId = billId;
            }
            else
            {
                // Update existing bill
                var updateBillQuery = @"
                    UPDATE Bills 
                    SET CustomerId = @customerId, CustomerName = @customerName, 
                        DeviceType = @deviceType, Date = @date, TotalAmount = @totalAmount 
                    WHERE BillId = @billId";
                    
                using var billCommand = new SqliteCommand(updateBillQuery, connection, transaction);
                billCommand.Parameters.AddWithValue("@billId", bill.BillId);
                billCommand.Parameters.AddWithValue("@customerId", bill.CustomerId > 0 ? (object)bill.CustomerId : DBNull.Value);
                billCommand.Parameters.AddWithValue("@customerName", bill.CustomerName);
                billCommand.Parameters.AddWithValue("@deviceType", bill.DeviceType ?? string.Empty);
                billCommand.Parameters.AddWithValue("@date", bill.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                billCommand.Parameters.AddWithValue("@totalAmount", bill.TotalAmount);
                
                await billCommand.ExecuteNonQueryAsync();
                billId = bill.BillId;
                
                // Delete existing bill items
                var deleteItemsQuery = "DELETE FROM BillItems WHERE BillId = @billId";
                using var deleteCommand = new SqliteCommand(deleteItemsQuery, connection, transaction);
                deleteCommand.Parameters.AddWithValue("@billId", billId);
                await deleteCommand.ExecuteNonQueryAsync();
            }
            
            // Insert bill items
            foreach (var item in bill.Items)
            {
                var insertItemQuery = @"
                    INSERT INTO BillItems (BillId, Description, Quantity, UnitPrice) 
                    VALUES (@billId, @description, @quantity, @unitPrice)";
                    
                using var itemCommand = new SqliteCommand(insertItemQuery, connection, transaction);
                itemCommand.Parameters.AddWithValue("@billId", billId);
                itemCommand.Parameters.AddWithValue("@description", item.Description);
                itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                itemCommand.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                
                await itemCommand.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
            return billId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Service>> GetServicesByCategoryAsync(string category)
    {
        var services = new List<Service>();
        
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT ServiceId, Name, Price, Category, IsActive 
            FROM Services 
            WHERE Category = @category AND IsActive = 1 
            ORDER BY Name";
            
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@category", category);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var service = new Service
            {
                ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1
            };
            
            services.Add(service);
        }
        
        return services;
    }

    public async Task<List<Service>> GetAllServicesAsync()
    {
        var services = new List<Service>();
        
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT ServiceId, Name, Price, Category, IsActive 
            FROM Services 
            WHERE IsActive = 1 
            ORDER BY Category, Name";
            
        using var command = new SqliteCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var service = new Service
            {
                ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1
            };
            
            services.Add(service);
        }
        
        return services;
    }

    public async Task SeedServiceCatalogAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        // Check if services already exist
        var checkQuery = "SELECT COUNT(*) FROM Services";
        using var checkCommand = new SqliteCommand(checkQuery, connection);
        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
        
        if (count > 0)
        {
            return; // Services already seeded
        }
        
        var services = GetInitialServiceCatalog();
        
        using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();
        
        try
        {
            foreach (var service in services)
            {
                var insertQuery = @"
                    INSERT INTO Services (Name, Price, Category, IsActive) 
                    VALUES (@name, @price, @category, @isActive)";
                    
                using var command = new SqliteCommand(insertQuery, connection, transaction);
                command.Parameters.AddWithValue("@name", service.Name);
                command.Parameters.AddWithValue("@price", service.Price);
                command.Parameters.AddWithValue("@category", service.Category);
                command.Parameters.AddWithValue("@isActive", service.IsActive ? 1 : 0);
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateServiceCatalogAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        // Get current services
        var currentServices = await GetAllServicesAsync();
        var initialServices = GetInitialServiceCatalog();
        
        using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();
        
        try
        {
            // Add new services that don't exist
            foreach (var initialService in initialServices)
            {
                var exists = currentServices.Any(s => 
                    s.Name == initialService.Name && 
                    s.Category == initialService.Category);
                
                if (!exists)
                {
                    var insertQuery = @"
                        INSERT INTO Services (Name, Price, Category, IsActive) 
                        VALUES (@name, @price, @category, @isActive)";
                        
                    using var command = new SqliteCommand(insertQuery, connection, transaction);
                    command.Parameters.AddWithValue("@name", initialService.Name);
                    command.Parameters.AddWithValue("@price", initialService.Price);
                    command.Parameters.AddWithValue("@category", initialService.Category);
                    command.Parameters.AddWithValue("@isActive", initialService.IsActive ? 1 : 0);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<string>> GetServiceCategoriesAsync()
    {
        var categories = new List<string>();
        
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT DISTINCT Category 
            FROM Services 
            WHERE IsActive = 1 
            ORDER BY Category";
            
        using var command = new SqliteCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            categories.Add(reader.GetString(reader.GetOrdinal("Category")));
        }
        
        return categories;
    }

    private List<Service> GetInitialServiceCatalog()
    {
        return new List<Service>
        {
            // HOURS category - Fixed prices
            new Service { Name = "Hours - Red", Price = 50.00m, Category = "HOURS", IsActive = true },
            new Service { Name = "Hours - Blue", Price = 75.00m, Category = "HOURS", IsActive = true },
            
            // VHS CONVERT category - Fixed prices
            new Service { Name = "VHS Convert - Red", Price = 10.00m, Category = "VHS CONVERT", IsActive = true },
            new Service { Name = "VHS Convert - Blue", Price = 5.00m, Category = "VHS CONVERT", IsActive = true },
            
            // PHOTO PRINT category - Fixed prices
            new Service { Name = "Photo Print B&W", Price = 20.00m, Category = "PHOTO PRINT", IsActive = true },
            new Service { Name = "Photo Print Color", Price = 30.00m, Category = "PHOTO PRINT", IsActive = true },
            
            // OS X category - All $30
            new Service { Name = "Big Sur 11 (20)", Price = 30.00m, Category = "OS X", IsActive = true },
            new Service { Name = "Monterey 12 (21)", Price = 30.00m, Category = "OS X", IsActive = true },
            new Service { Name = "Ventura 13 (22)", Price = 30.00m, Category = "OS X", IsActive = true },
            new Service { Name = "Sonoma 14 (23)", Price = 30.00m, Category = "OS X", IsActive = true },
            new Service { Name = "Sequoia 15 (24)", Price = 30.00m, Category = "OS X", IsActive = true },
            new Service { Name = "Tahoe 26 (25)", Price = 30.00m, Category = "OS X", IsActive = true },
            
            // LASER category - Editable prices (set to 0 for manual entry)
            new Service { Name = "Materials", Price = 0.00m, Category = "LASER", IsActive = true },
            new Service { Name = "Packaging", Price = 0.00m, Category = "LASER", IsActive = true },
            new Service { Name = "Time Mark", Price = 0.00m, Category = "LASER", IsActive = true },
            new Service { Name = "Time Engrave", Price = 0.00m, Category = "LASER", IsActive = true },
            
            // EQUIPMENT category - Editable prices (set to 0 for manual entry)
            new Service { Name = "RAM", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "Laptop", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "Desktop", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "Modem", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "Router", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "Monitor", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "NVME", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "KB&M", Price = 0.00m, Category = "EQUIPMENT", IsActive = true },
            new Service { Name = "Sec. Camera", Price = 0.00m, Category = "EQUIPMENT", IsActive = true }
        };
    }
}