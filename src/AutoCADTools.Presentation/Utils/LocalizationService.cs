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
    // Fire PropertyChanged("Item[]") so all active {Loc} bindings rebind.
    // Dispatch to UI thread if called from a background thread.
    if (Application.Current?.Dispatcher.CheckAccess() == false)
      Application.Current.Dispatcher.BeginInvoke(() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]")));
    else
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
  }
}