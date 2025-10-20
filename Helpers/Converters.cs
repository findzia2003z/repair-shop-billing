using Microsoft.UI.Xaml.Data;
using System;

namespace RepairShopBilling.Helpers
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double amount)
                return $"${amount:F2}";
            if (value is decimal decimalAmount)
                return $"${decimalAmount:F2}";
            if (value is int intAmount)
                return $"${intAmount:F2}";
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
                return date.ToString("MM/dd/yyyy");
            if (value is DateTimeOffset dateOffset)
                return dateOffset.ToString("MM/dd/yyyy");
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ShortDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
                return date.ToString("M-dd-yy");
            if (value is DateTimeOffset dateOffset)
                return dateOffset.ToString("M-dd-yy");
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BillIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int id)
                return $"#{id:D4}";
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}