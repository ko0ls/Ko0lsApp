using System;
using System.ComponentModel;
using System.Windows;
using AutoCADTools.Core.Localization;

namespace AutoCADTools.Presentation.Utils;

public class LocalizationService : INotifyPropertyChanged
{
  private static readonly Lazy<LocalizationService> _instance =
    new Lazy<LocalizationService>(() => new LocalizationService());

  public static LocalizationService Instance => _instance.Value;

  public event PropertyChangedEventHandler? PropertyChanged;

  private LocalizationService()
  {
    LocalizationManager.LanguageChanged += OnLanguageChanged;
  }

  public string this[string key] => key.GetString();

  private void OnLanguageChanged(object? sender, EventArgs e)
  {
    // Set CultureInfo on the WPF UI thread synchronously BEFORE firing PropertyChanged,
    // so that GetString() bindings resolve with the new culture immediately.
    // BeginInvoke caused a race: PropertyChanged fired before CultureInfo was updated.
    if (Application.Current?.Dispatcher.CheckAccess() == false) {
      Application.Current.Dispatcher.Invoke(() => {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
      });
    }
    else {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
  }
}