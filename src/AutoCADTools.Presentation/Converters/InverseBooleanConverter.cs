using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCADTools.Presentation.Converters;

/// <summary>
/// Inverts a boolean value. Used to bind mutually-exclusive RadioButtons
/// that both reference the same source property (e.g. Rectangular vs Circular).
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value is false;
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value is true ? false : Binding.DoNothing;
  }
}
