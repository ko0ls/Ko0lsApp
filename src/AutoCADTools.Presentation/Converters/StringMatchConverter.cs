using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCADTools.Presentation.Converters;

/// <summary>
/// Converts a string value to bool by comparing it against a parameter.
/// Returns true when value equals parameter, false otherwise.
/// Used to bind RadioButtons to a string property (e.g. PrimaryDirection = "X" or "Y").
/// </summary>
public class StringMatchConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value == null || parameter == null) return false;
    return value.ToString() == parameter.ToString();
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is true && parameter != null)
      return parameter.ToString()!;
    return Binding.DoNothing;
  }
}
