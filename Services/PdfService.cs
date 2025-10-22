using RepairShopBilling.Models;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;
using PdfSharp.Fonts;
using System.IO;

namespace RepairShopBilling.Services
{
    /// <summary>
    /// Font resolver for Ezra Bold font
    /// </summary>
    public class EzraFontResolver : IFontResolver
    {
        public byte[]? GetFont(string faceName)
        {
            try
            {
                var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", "Ezra Bold.otf");
                if (File.Exists(fontPath))
                {
                    return File.ReadAllBytes(fontPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetFont error: {ex.Message}");
            }
            return null;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            if (familyName.Equals("EzraCustom", StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo("EzraCustom");
            }
            return PlatformFontResolver.ResolveTypeface(familyName, bold, italic);
        }
    }

    /// <summary>
    /// Service for generating PDF documents from bills matching the Bill Viewer design
    /// </summary>
    public class PdfService : IPdfService
    {
        static PdfService()
        {
            // Enable UTF-8 encoding for PDFsharp
            if (PdfSharp.Capabilities.Build.IsCoreBuild)
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            }

            // Set up font resolver
            try
            {
                if (GlobalFontSettings.FontResolver == null)
                {
                    GlobalFontSettings.FontResolver = new EzraFontResolver();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font resolver setup error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a PDF document for the specified bill matching the Bill Viewer design
        /// </summary>
        public async Task<string> GenerateBillPdfAsync(Bill bill, string outputPath)
        {
            if (bill == null)
                throw new ArgumentNullException(nameof(bill));

            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));

            try
            {
                await Task.Run(() =>
                {
                    // Create PDF document
                    var document = new PdfDocument();
                    document.Info.Title = $"Bill - {bill.CustomerName}";
                    document.Info.Author = "Solid State Dave";
                    document.Info.Subject = "Service Invoice";
                    
                    // Draw the bill design with multi-page support
                    DrawBillDesignMultiPage(document, bill);
                    
                    // Save the document
                    document.Save(outputPath);
                    document.Close();
                });

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
                var fileName = $"Bill_{customerName}_{dateString}_{DateTime.Now.Ticks}.pdf";
                
                var tempFile = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                
                // Generate PDF using the same method
                await GenerateBillPdfAsync(bill, tempFile.Path);
                
                return tempFile.Path;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate temporary PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Draws the bill design with multi-page support
        /// </summary>
        private void DrawBillDesignMultiPage(PdfDocument document, Bill bill)
        {
            const double pageWidth = 612;
            const double pageHeight = 792;
            const double margin = 50;
            const double contentWidth = 500;
            const double startX = (pageWidth - contentWidth) / 2;
            const double rowHeight = 25;
            const int maxRowsPerPage = 20; // Maximum rows that fit on first page
            const int maxRowsPerContinuationPage = 28; // More rows on continuation pages
            
            // Split items into pages
            var itemsPerPage = new List<List<BillItem>>();
            var currentPageItems = new List<BillItem>();
            var maxRows = maxRowsPerPage;
            
            foreach (var item in bill.Items)
            {
                if (currentPageItems.Count >= maxRows)
                {
                    itemsPerPage.Add(currentPageItems);
                    currentPageItems = new List<BillItem>();
                    maxRows = maxRowsPerContinuationPage; // Continuation pages have more space
                }
                currentPageItems.Add(item);
            }
            
            if (currentPageItems.Any())
            {
                itemsPerPage.Add(currentPageItems);
            }
            
            // Draw first page
            var page = document.AddPage();
            page.Width = XUnit.FromPoint(pageWidth);
            page.Height = XUnit.FromPoint(pageHeight);
            var gfx = XGraphics.FromPdfPage(page);
            
            DrawFirstPage(gfx, bill, itemsPerPage[0], pageWidth, pageHeight, startX, contentWidth, rowHeight, itemsPerPage.Count == 1);
            
            // Draw continuation pages if needed
            for (int i = 1; i < itemsPerPage.Count; i++)
            {
                page = document.AddPage();
                page.Width = XUnit.FromPoint(pageWidth);
                page.Height = XUnit.FromPoint(pageHeight);
                gfx = XGraphics.FromPdfPage(page);
                
                bool isLastPage = (i == itemsPerPage.Count - 1);
                DrawContinuationPage(gfx, bill, itemsPerPage[i], pageWidth, pageHeight, startX, contentWidth, rowHeight, isLastPage, i + 1);
            }
        }
        
        /// <summary>
        /// Draws the first page with header, customer info, and table start
        /// </summary>
        private void DrawFirstPage(XGraphics gfx, Bill bill, List<BillItem> items, double pageWidth, double pageHeight, 
            double startX, double contentWidth, double rowHeight, bool isOnlyPage)
        {
            var currentY = 50.0;
            
            // Fonts
            var titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
            var headerFont = new XFont("Arial", 16, XFontStyleEx.Bold);
            var normalFont = new XFont("Arial", 11, XFontStyleEx.Regular);
            
            // Colors
            var darkGray = XColor.FromArgb(51, 51, 51);
            var lightGray = XColor.FromArgb(128, 128, 128);
            var redAccent = XColor.FromArgb(229, 62, 62);
            var black = XColors.Black;
            
            // 1. Header Logo
            DrawHeader(gfx, pageWidth, titleFont);
            currentY = 80;
            
            // 2. Customer Header
            DrawCustomerHeader(gfx, bill, startX, currentY, contentWidth, headerFont, lightGray);
            currentY += 40;
            
            // 3. Table Header
            DrawTableHeader(gfx, startX, currentY, contentWidth, headerFont, redAccent, black);
            currentY += 35;
            
            // 4. Table Items
            currentY = DrawTableItems(gfx, items, startX, currentY, contentWidth, rowHeight, normalFont, darkGray);
            
            // 5. If this is the only page, draw total, logo, and footer
            if (isOnlyPage)
            {
                DrawTotal(gfx, bill, startX, currentY, contentWidth, lightGray, redAccent);
                currentY += 65;
                
                currentY = DrawLogo(gfx, currentY, pageWidth);
                DrawThankYou(gfx, currentY, pageWidth, redAccent);
                currentY += 90;
                
                DrawFooter(gfx, startX, currentY, contentWidth, redAccent);
            }
            else
            {
                // Draw "Continued on next page"
                gfx.DrawString("(Continued on next page...)", new XFont("Arial", 10, XFontStyleEx.Italic),
                    XBrushes.Gray, new XRect(startX, currentY + 10, contentWidth, 20), XStringFormats.Center);
            }
        }
        
        /// <summary>
        /// Draws continuation pages with table items
        /// </summary>
        private void DrawContinuationPage(XGraphics gfx, Bill bill, List<BillItem> items, double pageWidth, double pageHeight,
            double startX, double contentWidth, double rowHeight, bool isLastPage, int pageNumber)
        {
            var currentY = 50.0;
            var normalFont = new XFont("Arial", 11, XFontStyleEx.Regular);
            var headerFont = new XFont("Arial", 16, XFontStyleEx.Bold);
            var darkGray = XColor.FromArgb(51, 51, 51);
            var lightGray = XColor.FromArgb(128, 128, 128);
            var redAccent = XColor.FromArgb(229, 62, 62);
            var black = XColors.Black;
            
            // Page number
            gfx.DrawString($"Page {pageNumber}", new XFont("Arial", 10), XBrushes.Gray,
                new XRect(0, 20, pageWidth, 20), XStringFormats.TopCenter);
            
            // Table Header
            DrawTableHeader(gfx, startX, currentY, contentWidth, headerFont, redAccent, black);
            currentY += 35;
            
            // Table Items
            currentY = DrawTableItems(gfx, items, startX, currentY, contentWidth, rowHeight, normalFont, darkGray);
            
            // If last page, draw total, logo, and footer
            if (isLastPage)
            {
                DrawTotal(gfx, bill, startX, currentY, contentWidth, lightGray, redAccent);
                currentY += 65;
                
                currentY = DrawLogo(gfx, currentY, pageWidth);
                DrawThankYou(gfx, currentY, pageWidth, redAccent);
                currentY += 90;
                
                DrawFooter(gfx, startX, currentY, contentWidth, redAccent);
            }
            else
            {
                gfx.DrawString("(Continued on next page...)", new XFont("Arial", 10, XFontStyleEx.Italic),
                    XBrushes.Gray, new XRect(startX, currentY + 10, contentWidth, 20), XStringFormats.Center);
            }
        }

        /// <summary>
        /// Draws the bill design matching the Bill Viewer layout with pagination support
        /// </summary>
        private void DrawBillDesign(XGraphics gfx, Bill bill, PdfPage page)
        {
            double pageWidth = page.Width;
            double pageHeight = page.Height;
            double margin = 50;
            double contentWidth = 500;
            double startX = (pageWidth - contentWidth) / 2;
            double currentY = margin;

            // Fonts - using XFontStyleEx for PDFsharp 6.x
            var titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
            var headerFont = new XFont("Arial", 16, XFontStyleEx.Bold);
            var normalFont = new XFont("Arial", 12, XFontStyleEx.Regular);
            var boldFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            var smallFont = new XFont("Arial", 10, XFontStyleEx.Regular);

            // Colors
            var darkGray = XColor.FromArgb(51, 51, 51);
            var lightGray = XColor.FromArgb(128, 128, 128);
            var redAccent = XColor.FromArgb(229, 62, 62);
            var black = XColors.Black;
            var white = XColors.White;

            // 1. Header - Solid State Dave Logo with image
            gfx.DrawRectangle(XBrushes.Black, 0, 0, pageWidth, 60);
            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Styles", "Solid State Dave.png");
                if (File.Exists(logoPath))
                {
                    var logoImage = XImage.FromFile(logoPath);
                    var logoHeight = 40;
                    var logoWidth = logoImage.PixelWidth * logoHeight / logoImage.PixelHeight;
                    gfx.DrawImage(logoImage, (pageWidth - logoWidth) / 2, 10, logoWidth, logoHeight);
                }
                else
                {
                    // Fallback to text if image not found
                    var logoText = "Solid State Dave";
                    gfx.DrawString(logoText, titleFont, XBrushes.White, 
                        new XRect(0, 15, pageWidth, 60), XStringFormats.TopCenter);
                }
            }
            catch
            {
                // Fallback to text if image loading fails
                var logoText = "Solid State Dave";
                gfx.DrawString(logoText, titleFont, XBrushes.White, 
                    new XRect(0, 15, pageWidth, 60), XStringFormats.TopCenter);
            }
            currentY = 80;

            // 2. Customer Header (Gray background)
            var customerHeaderRect = new XRect(startX, currentY, contentWidth, 40);
            gfx.DrawRoundedRectangle(new XPen(lightGray), new XSolidBrush(lightGray), 
                customerHeaderRect, new XSize(12, 12));
            
            gfx.DrawString(bill.CustomerName, headerFont, XBrushes.White,
                new XRect(startX + 20, currentY + 10, contentWidth / 2, 40), XStringFormats.TopLeft);
            
            var dateStr = bill.Date.ToString("M-dd-yy");
            gfx.DrawString(dateStr, headerFont, XBrushes.White,
                new XRect(startX, currentY + 10, contentWidth - 20, 40), XStringFormats.TopRight);
            
            currentY += 40;

            // 3. Table Header (Red background)
            var tableHeaderRect = new XRect(startX, currentY, contentWidth, 35);
            gfx.DrawRectangle(new XSolidBrush(redAccent), tableHeaderRect);
            
            // Draw vertical separators
            gfx.DrawLine(new XPen(black, 3), startX + contentWidth * 0.6, currentY, 
                startX + contentWidth * 0.6, currentY + 35);
            gfx.DrawLine(new XPen(black, 3), startX + contentWidth * 0.8, currentY, 
                startX + contentWidth * 0.8, currentY + 35);
            
            gfx.DrawString("Services", headerFont, XBrushes.Black,
                new XRect(startX + 15, currentY + 8, contentWidth * 0.6, 35), XStringFormats.TopLeft);
            gfx.DrawString("Amount", headerFont, XBrushes.Black,
                new XRect(startX + contentWidth * 0.6, currentY + 8, contentWidth * 0.2, 35), XStringFormats.TopCenter);
            gfx.DrawString("Price", headerFont, XBrushes.Black,
                new XRect(startX + contentWidth * 0.8, currentY + 8, contentWidth * 0.2, 35), XStringFormats.TopCenter);
            
            currentY += 35;

            // 4. Table Content (White background with items) - with dynamic sizing for long bills
            // Calculate space needed: Total(45) + Logo(150) + ThankYou(90) + Footer(60) + margins(50) = 395
            var spaceNeededAfterTable = 395;
            var maxTableHeight = pageHeight - currentY - spaceNeededAfterTable;
            
            // Calculate optimal row height based on number of items
            var rowHeight = (int)(maxTableHeight / bill.Items.Count);
            
            // Set minimum and maximum row heights
            if (rowHeight > 30) rowHeight = 30; // Maximum row height
            if (rowHeight < 18) rowHeight = 18; // Minimum row height
            
            // Adjust font size based on row height
            var itemFont = rowHeight < 25 ? new XFont("Arial", 9, XFontStyleEx.Regular) : normalFont;
            
            foreach (var item in bill.Items)
            {
                var rowRect = new XRect(startX, currentY, contentWidth, rowHeight);
                gfx.DrawRectangle(XBrushes.White, rowRect);
                
                // Draw vertical separators
                gfx.DrawLine(new XPen(XColor.FromArgb(208, 208, 208), 2), 
                    startX + contentWidth * 0.6, currentY, 
                    startX + contentWidth * 0.6, currentY + rowHeight);
                gfx.DrawLine(new XPen(XColor.FromArgb(208, 208, 208), 2), 
                    startX + contentWidth * 0.8, currentY, 
                    startX + contentWidth * 0.8, currentY + rowHeight);
                
                // Draw bottom border
                gfx.DrawLine(new XPen(XColor.FromArgb(208, 208, 208), 1), 
                    startX, currentY + rowHeight, 
                    startX + contentWidth, currentY + rowHeight);
                
                // Center text vertically in the row
                var textY = currentY + (rowHeight - 10) / 2;
                
                gfx.DrawString(item.Description, itemFont, new XSolidBrush(darkGray),
                    new XRect(startX + 15, textY, contentWidth * 0.6 - 20, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(item.Quantity.ToString(), itemFont, new XSolidBrush(darkGray),
                    new XRect(startX + contentWidth * 0.6, textY, contentWidth * 0.2, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString($"${item.TotalPrice:F2}", itemFont, new XSolidBrush(darkGray),
                    new XRect(startX + contentWidth * 0.8, textY, contentWidth * 0.2, rowHeight), XStringFormats.TopCenter);
                
                currentY += rowHeight;
            }

            // 5. Total Row (Gray background with black total box)
            var totalRowRect = new XRect(startX, currentY, contentWidth, 45);
            gfx.DrawRoundedRectangle(new XPen(lightGray), new XSolidBrush(lightGray), 
                totalRowRect, new XSize(0, 12));
            
            gfx.DrawString("TOTAL", new XFont("Arial", 18, XFontStyleEx.Bold), XBrushes.White,
                new XRect(startX, currentY + 12, contentWidth * 0.6, 45), XStringFormats.TopCenter);
            
            var totalBoxRect = new XRect(startX + contentWidth * 0.6, currentY, contentWidth * 0.4, 45);
            gfx.DrawRectangle(XBrushes.Black, totalBoxRect);
            gfx.DrawString($"${bill.TotalAmount:F2}", new XFont("Arial", 16, XFontStyleEx.Bold), 
                new XSolidBrush(redAccent),
                new XRect(startX + contentWidth * 0.6, currentY + 12, contentWidth * 0.4, 45), XStringFormats.TopCenter);
            
            currentY += 65;

            // 6. Brain Logo image (using LogoPDF.png for PDF generation)
            try
            {
                var brainLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Styles", "LogoPDF.png");
                if (File.Exists(brainLogoPath))
                {
                    var brainImage = XImage.FromFile(brainLogoPath);
                    var brainSize = 150;
                    gfx.DrawImage(brainImage, (pageWidth - brainSize) / 2, currentY, brainSize, brainSize);
                    currentY += brainSize + 20;
                }
                else
                {
                    gfx.DrawString("[Brain Logo]", normalFont, XBrushes.White,
                        new XRect(0, currentY, pageWidth, 20), XStringFormats.TopCenter);
                    currentY += 40;
                }
            }
            catch
            {
                gfx.DrawString("[Brain Logo]", normalFont, XBrushes.White,
                    new XRect(0, currentY, pageWidth, 20), XStringFormats.TopCenter);
                currentY += 40;
            }

            // 7. Thank You Section
            gfx.DrawString("Thank You", new XFont("Arial", 20, XFontStyleEx.Bold), 
                new XSolidBrush(redAccent),
                new XRect(0, currentY, pageWidth, 30), XStringFormats.TopCenter);
            currentY += 25;
            gfx.DrawString("Please Leave A Review On", new XFont("Arial", 20, XFontStyleEx.Bold), 
                new XSolidBrush(redAccent),
                new XRect(0, currentY, pageWidth, 30), XStringFormats.TopCenter);
            currentY += 25;
            gfx.DrawString("Google Maps", new XFont("Arial", 20, XFontStyleEx.Bold), 
                new XSolidBrush(redAccent),
                new XRect(0, currentY, pageWidth, 30), XStringFormats.TopCenter);
            currentY += 40;

            // 8. Footer with image
            try
            {
                var footerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Styles", "Footer.png");
                if (File.Exists(footerPath))
                {
                    var footerImage = XImage.FromFile(footerPath);
                    var footerWidth = contentWidth;
                    var footerHeight = footerImage.PixelHeight * footerWidth / footerImage.PixelWidth;
                    gfx.DrawImage(footerImage, startX, currentY, footerWidth, footerHeight);
                }
                else
                {
                    // Fallback to text-based footer
                    gfx.DrawString("SolidStateDave.com", new XFont("Arial", 16, XFontStyleEx.Regular), 
                        XBrushes.White,
                        new XRect(margin, currentY, 200, 30), XStringFormats.TopLeft);
                    
                    var phoneRect = new XRect(pageWidth - margin - 250, currentY - 5, 250, 40);
                    gfx.DrawRectangle(new XSolidBrush(redAccent), phoneRect);
                    gfx.DrawString("Phone: 908-442-3927", new XFont("Arial", 16, XFontStyleEx.Bold), 
                        XBrushes.White, phoneRect, XStringFormats.Center);
                }
            }
            catch
            {
                // Fallback to text-based footer
                gfx.DrawString("SolidStateDave.com", new XFont("Arial", 16, XFontStyleEx.Regular), 
                    XBrushes.White,
                    new XRect(margin, currentY, 200, 30), XStringFormats.TopLeft);
                
                var phoneRect = new XRect(pageWidth - margin - 250, currentY - 5, 250, 40);
                gfx.DrawRectangle(new XSolidBrush(redAccent), phoneRect);
                gfx.DrawString("Phone: 908-442-3927", new XFont("Arial", 16, XFontStyleEx.Bold), 
                    XBrushes.White, phoneRect, XStringFormats.Center);
            }
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
        
        // Helper methods for multi-page PDF generation
        private void DrawHeader(XGraphics gfx, double pageWidth, XFont titleFont)
        {
            gfx.DrawRectangle(XBrushes.Black, 0, 0, pageWidth, 60);
            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Styles", "Solid State Dave.png");
                if (File.Exists(logoPath))
                {
                    var logoImage = XImage.FromFile(logoPath);
                    var logoHeight = 40;
                    var logoWidth = logoImage.PixelWidth * logoHeight / logoImage.PixelHeight;
                    gfx.DrawImage(logoImage, (pageWidth - logoWidth) / 2, 10, logoWidth, logoHeight);
                }
                else
                {
                    gfx.DrawString("Solid State Dave", titleFont, XBrushes.White,
                        new XRect(0, 15, pageWidth, 60), XStringFormats.TopCenter);
                }
            }
            catch
            {
                gfx.DrawString("Solid State Dave", titleFont, XBrushes.White,
                    new XRect(0, 15, pageWidth, 60), XStringFormats.TopCenter);
            }
        }
        
        private void DrawCustomerHeader(XGraphics gfx, Bill bill, double startX, double currentY, double contentWidth, XFont headerFont, XColor lightGray)
        {
            var customerHeaderRect = new XRect(startX, currentY, contentWidth, 40);
            gfx.DrawRoundedRectangle(new XPen(lightGray), new XSolidBrush(lightGray),
                customerHeaderRect, new XSize(12, 12));
            
            gfx.DrawString(bill.CustomerName, headerFont, XBrushes.White,
                new XRect(startX + 20, currentY + 10, contentWidth / 2, 40), XStringFormats.TopLeft);
            
            var dateStr = bill.Date.ToString("M-dd-yy");
            gfx.DrawString(dateStr, headerFont, XBrushes.White,
                new XRect(startX, currentY + 10, contentWidth - 20, 40), XStringFormats.TopRight);
        }
        
        private void DrawTableHeader(XGraphics gfx, double startX, double currentY, double contentWidth, XFont headerFont, XColor redAccent, XColor black)
        {
            var tableHeaderRect = new XRect(startX, currentY, contentWidth, 35);
            gfx.DrawRectangle(new XSolidBrush(redAccent), tableHeaderRect);
            
            var darkGray = XColor.FromArgb(51, 51, 51);
            gfx.DrawLine(new XPen(darkGray, 2), startX + contentWidth * 0.6, currentY,
                startX + contentWidth * 0.6, currentY + 35);
            gfx.DrawLine(new XPen(darkGray, 2), startX + contentWidth * 0.8, currentY,
                startX + contentWidth * 0.8, currentY + 35);
            
            gfx.DrawString("Services", headerFont, XBrushes.Black,
                new XRect(startX + 15, currentY + 8, contentWidth * 0.6, 35), XStringFormats.TopLeft);
            gfx.DrawString("Amount", headerFont, XBrushes.Black,
                new XRect(startX + contentWidth * 0.6, currentY + 8, contentWidth * 0.2, 35), XStringFormats.TopCenter);
            gfx.DrawString("Price", headerFont, XBrushes.Black,
                new XRect(startX + contentWidth * 0.8, currentY + 8, contentWidth * 0.2, 35), XStringFormats.TopCenter);
        }
        
        private double DrawTableItems(XGraphics gfx, List<BillItem> items, double startX, double currentY, double contentWidth, double rowHeight, XFont normalFont, XColor darkGray)
        {
            // Use custom Ezra font with embedding
            XFont itemFont;
            try
            {
                var options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
                itemFont = new XFont("EzraCustom", 11, XFontStyleEx.Regular, options);
                System.Diagnostics.Debug.WriteLine("Successfully loaded EzraCustom font");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load EzraCustom font: {ex.Message}");
                itemFont = new XFont("Arial", 11, XFontStyleEx.Bold);
            }
            
            foreach (var item in items)
            {
                var rowRect = new XRect(startX, currentY, contentWidth, rowHeight);
                gfx.DrawRectangle(XBrushes.White, rowRect);
                
                gfx.DrawLine(new XPen(XColor.FromArgb(51, 51, 51), 2),
                    startX + contentWidth * 0.6, currentY,
                    startX + contentWidth * 0.6, currentY + rowHeight);
                gfx.DrawLine(new XPen(XColor.FromArgb(51, 51, 51), 2),
                    startX + contentWidth * 0.8, currentY,
                    startX + contentWidth * 0.8, currentY + rowHeight);
                
                gfx.DrawLine(new XPen(XColor.FromArgb(208, 208, 208), 1),
                    startX, currentY + rowHeight,
                    startX + contentWidth, currentY + rowHeight);
                
                var textY = currentY + 7;
                
                gfx.DrawString(item.Description, itemFont, new XSolidBrush(darkGray),
                    new XRect(startX + 15, textY, contentWidth * 0.6 - 20, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(item.Quantity.ToString(), itemFont, new XSolidBrush(darkGray),
                    new XRect(startX + contentWidth * 0.6, textY, contentWidth * 0.2, rowHeight), XStringFormats.TopCenter);
                gfx.DrawString($"${item.TotalPrice:F2}", itemFont, new XSolidBrush(darkGray),
                    new XRect(startX + contentWidth * 0.8, textY, contentWidth * 0.2, rowHeight), XStringFormats.TopCenter);
                
                currentY += rowHeight;
            }
            return currentY;
        }
        
        private void DrawTotal(XGraphics gfx, Bill bill, double startX, double currentY, double contentWidth, XColor lightGray, XColor redAccent)
        {
            var totalRowRect = new XRect(startX, currentY, contentWidth, 45);
            gfx.DrawRoundedRectangle(new XPen(lightGray), new XSolidBrush(lightGray),
                totalRowRect, new XSize(0, 12));
            
            gfx.DrawString("TOTAL", new XFont("Arial", 18, XFontStyleEx.Bold), XBrushes.White,
                new XRect(startX, currentY + 12, contentWidth * 0.6, 45), XStringFormats.TopCenter);
            
            var totalBoxRect = new XRect(startX + contentWidth * 0.6, currentY, contentWidth * 0.4, 45);
            gfx.DrawRectangle(XBrushes.Black, totalBoxRect);
            gfx.DrawString($"${bill.TotalAmount:F2}", new XFont("Arial", 16, XFontStyleEx.Bold),
                new XSolidBrush(redAccent),
                new XRect(startX + contentWidth * 0.6, currentY + 12, contentWidth * 0.4, 45), XStringFormats.TopCenter);
        }
        
        private double DrawLogo(XGraphics gfx, double currentY, double pageWidth)
        {
            try
            {
                var brainLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Styles", "LogoPDF.png");
                if (File.Exists(brainLogoPath))
                {
                    var brainImage = XImage.FromFile(brainLogoPath);
                    var brainSize = 120;
                    gfx.DrawImage(brainImage, (pageWidth - brainSize) / 2, currentY, brainSize, brainSize);
                    return currentY + brainSize + 10;
                }
            }
            catch { }
            return currentY;
        }
        
        private void DrawThankYou(XGraphics gfx, double currentY, double pageWidth, XColor redAccent)
        {
            gfx.DrawString("Thank You", new XFont("Arial", 20, XFontStyleEx.Bold),
                new XSolidBrush(redAccent),
                new XRect(0, currentY, pageWidth, 30), XStringFormats.TopCenter);
            currentY += 25;
            gfx.DrawString("Please Leave A Review On", new XFont("Arial", 20, XFontStyleEx.Bold),
                new XSolidBrush(redAccent),
                new XRect(0, currentY, pageWidth, 30), XStringFormats.TopCenter);
            currentY += 25;
            gfx.DrawString("Google Maps", new XFont("Arial", 20, XFontStyleEx.Bold),
                new XSolidBrush(redAccent),
                new XRect(0, currentY, pageWidth, 30), XStringFormats.TopCenter);
        }
        
        private void DrawFooter(XGraphics gfx, double startX, double currentY, double contentWidth, XColor redAccent)
        {
            try
            {
                var footerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Styles", "Footer.png");
                if (File.Exists(footerPath))
                {
                    var footerImage = XImage.FromFile(footerPath);
                    var footerWidth = contentWidth;
                    var footerHeight = footerImage.PixelHeight * footerWidth / footerImage.PixelWidth;
                    gfx.DrawImage(footerImage, startX, currentY, footerWidth, footerHeight);
                }
            }
            catch { }
        }
        

    }
}