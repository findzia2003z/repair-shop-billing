using RepairShopBilling.Models;

namespace RepairShopBilling.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<List<Bill>> GetBillsAsync();
    Task<List<Bill>> SearchBillsByCustomerAsync(string customerName);
    Task<Bill?> GetBillByIdAsync(int billId);
    Task<int> SaveBillAsync(Bill bill);
    Task<List<Service>> GetServicesByCategoryAsync(string category);
    Task<List<Service>> GetAllServicesAsync();
    Task<List<string>> GetServiceCategoriesAsync();
    Task SeedServiceCatalogAsync();
    Task UpdateServiceCatalogAsync();
}