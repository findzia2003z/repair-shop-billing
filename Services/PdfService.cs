using RepairShopBilling.Models;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using System.Text;

namespace RepairShopBilling.Services
{
    /// <summary>
    /// Service for generating PDF documents from bills
    /// Note: This is a simplified implementation. In production, use a proper PDF library like PdfSharp or iTextSharp
    /// </summary>
    public class PdfService : IPdfService
    {
        /// <summary>
        /// Generates a PDF document for the specified bill
        /// </summary>
        public async Task<string> GenerateBillPdfAsync(Bill bill, string outputPath)
        {
            if (bill == null)
                throw new ArgumentNullException(nameof(bill));

            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));

            try
            {
                // Generate the bill content
                var content = GenerateBillContent(bill);
                
                // For this implementation, we'll create a formatted text file
                // In a real implementation, you would use a PDF library
                var file = await StorageFile.GetFileFromPathAsync(outputPath);
                await FileIO.WriteTextAsync(file, content);

                return outputPath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Shares a bill using the system share dialog
        /// </summary>
        public async Task<bool> ShareBillAsync(Bill bill)
        {
            if (bill == null)
                throw new ArgumentNullException(nameof(bill));

            try
            {
                // Generate a temporary PDF file
                var tempPath = await GenerateTemporaryPdfAsync(bill);
                
                // TODO: Implement actual sharing using Windows.ApplicationModel.DataTransfer
                // For now, this is a placeholder implementation
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a temporary PDF file for sharing purposes
        /// </summary>
        public async Task<string> GenerateTemporaryPdfAsync(Bill bill)
        {
            if (bill == null)
                throw new ArgumentNullException(nameof(bill));

            try
            {
                // Create temporary file
                var tempFolder = ApplicationData.Current.TemporaryFolder;
                var customerName = SanitizeFileName(bill.CustomerName);
                var dateString = bill.Date.ToString("yyyy-MM-dd");
                var fileName = $"Bill_{customerName}_{dateString}_{DateTime.Now.Ticks}.txt";
                
                var tempFile = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                
                // Generate content and write to file
                var content = GenerateBillContent(bill);
                await FileIO.WriteTextAsync(tempFile, content);
                
                return tempFile.Path;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate temporary PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates the formatted content for the bill
        /// </summary>
        private string GenerateBillContent(Bill bill)
        {
            var content = new StringBuilder();
            
            // Header
            content.AppendLine("=====================================");
            content.AppendLine("           SOLID STATE DAVE          ");
            content.AppendLine("            Service Invoice          ");
            content.AppendLine("=====================================");
            content.AppendLine();
            
            // Customer Information
            content.AppendLine("CUSTOMER INFORMATION:");
            content.AppendLine($"Name: {bill.CustomerName}");
            content.AppendLine($"Device: {bill.DeviceType}");
            content.AppendLine();
            
            // Bill Information
            content.AppendLine("BILL INFORMATION:");
            content.AppendLine($"Date: {bill.Date:MM/dd/yyyy}");
            content.AppendLine($"Bill #: #{bill.BillId:D4}");
            content.AppendLine();
            
            // Services Table
            content.AppendLine("SERVICES:");
            content.AppendLine("=====================================");
            content.AppendLine("Service                    Qty  Price");
            content.AppendLine("-------------------------------------");
            
            foreach (var item in bill.Items)
            {
                var description = item.Description.Length > 22 
                    ? item.Description.Substring(0, 19) + "..." 
                    : item.Description;
                
                content.AppendLine($"{description,-22} {item.Quantity,3}  ${item.TotalPrice,6:F2}");
            }
            
            content.AppendLine("-------------------------------------");
            content.AppendLine($"{"TOTAL:",-26} ${bill.TotalAmount,6:F2}");
            content.AppendLine("=====================================");
            content.AppendLine();
            
            // Footer
            content.AppendLine("Thank you for your business!");
            content.AppendLine();
            content.AppendLine("Contact Information:");
            content.AppendLine("Website: www.solidstatedave.com");
            content.AppendLine("Phone: (555) 123-4567");
            content.AppendLine();
            content.AppendLine("=====================================");
            
            return content.ToString();
        }

        /// <summary>
        /// Sanitizes a filename by removing invalid characters
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Unknown";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new StringBuilder();
            
            foreach (char c in fileName)
            {
                if (!invalidChars.Contains(c))
                {
                    sanitized.Append(c);
                }
                else
                {
                    sanitized.Append('_');
                }
            }
            
            return sanitized.ToString().Trim();
        }
    }
}