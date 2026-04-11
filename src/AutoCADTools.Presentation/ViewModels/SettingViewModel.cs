using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AutoCADTools.Core;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Storage;

namespace AutoCADTools.Presentation.ViewModels
{
  public class LanguageItem(string code, string displayName)
  {
    public string Code { get; } = code;
    public string DisplayName { get; } = displayName;
  }

  public class SettingViewModel : BindableObject
  {
    private readonly ISettingsRepository _repository;
    private LanguageItem? _selectedLanguageItem;

    public SettingViewModel(ISettingsRepository repository)
    {
      _repository = repository ?? throw new ArgumentNullException(nameof(repository));
      var savedCode = repository.GetLanguage();
      _selectedLanguageItem = AvailableLanguages.FirstOrDefault(l => l.Code == savedCode)
        ?? AvailableLanguages[0];
      SaveCommand = new RelayCommand(OnSave);
      CancelCommand = new RelayCommand(OnCancel);
    }

    public LanguageItem? SelectedLanguageItem
    {
      get => _selectedLanguageItem;
      set => SetProperty(ref _selectedLanguageItem, value);
    }

    public IReadOnlyList<LanguageItem> AvailableLanguages { get; } = [
      new("en", "English"),
      new("vi", "Tiếng Việt")
    ];

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? CloseRequested;

    private void OnSave()
    {
      if (SelectedLanguageItem == null) return;
      LocalizationManager.SetLanguage(SelectedLanguageItem.Code);
      _repository.SaveLanguage(SelectedLanguageItem.Code);
      CloseRequested?.Invoke();
      AppProxy.NotifySettingsSaved();
    }

    private void OnCancel()
    {
      CloseRequested?.Invoke();
    }
  }
}
