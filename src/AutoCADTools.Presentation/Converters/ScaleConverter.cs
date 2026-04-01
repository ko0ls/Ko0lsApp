using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCADTools.Presentation.Converters;

public class ScaleConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value is not int intValue
      ? "1 : 100"
      : $"1 : {intValue}";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
