using PCCScale.Models.Enums;
using System.Globalization;
using System.Windows.Data;

namespace PCCScale.Converters
{
    public sealed class PanStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                PanStateEnum.ReadyToSoak=> "Yellow",
                PanStateEnum.SoakingInProgress => "Orange",
                PanStateEnum.SoakingComplete => "Green",
                _ => "Black"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
