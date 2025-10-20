using RepairShopBilling.Models;

namespace RepairShopBilling.Services
{
    /// <summary>
    /// Interface for PDF generation and sharing services
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Generates a PDF document for the specified bill
        /// </summary>
        /// <param name="bill">The bill to generate PDF for</param>
        /// <param name="outputPath">The file path where the PDF should be saved</param>
        /// <returns>The path to the generated PDF file</returns>
        Task<string> GenerateBillPdfAsync(Bill bill, string outputPath);

        /// <summary>
        /// Shares a bill using the system share dialog
        /// </summary>
        /// <param name="bill">The bill to share</param>
        /// <returns>True if sharing was successful, false otherwise</returns>
        Task<bool> ShareBillAsync(Bill bill);

        /// <summary>
        /// Generates a temporary PDF file for sharing purposes
        /// </summary>
        /// <param name="bill">The bill to generate PDF for</param>
        /// <returns>The path to the temporary PDF file</returns>
        Task<string> GenerateTemporaryPdfAsync(Bill bill);
    }
}