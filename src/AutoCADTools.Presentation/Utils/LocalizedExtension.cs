using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace AutoCADTools.Presentation.Utils;

[MarkupExtensionReturnType(typeof(string))]
public class LocExtension : MarkupExtension
{
  public string Key { get; set; } = string.Empty;

  public LocExtension() { }

  public LocExtension(string key) => Key = key;

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    // Return a OneWay binding to LocalizationService[key].
    // When LanguageChanged fires, LocalizationService raises PropertyChanged("Item[]"),
    // and WPF rebinds every {Loc} usage automatically.
    return new Binding($"[{Key}]") {
      Source = LocalizationService.Instance,
      Mode = BindingMode.OneWay
    }.ProvideValue(serviceProvider);
  }
}