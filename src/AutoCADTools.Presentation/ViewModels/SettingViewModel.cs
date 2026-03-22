using System;
using System.Collections.Generic;
using System.Windows.Input;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Storage;

namespace AutoCADTools.Presentation.ViewModels
{
  public class LanguageItem
  {
    public string Code { get; }
    public string DisplayName { get; }

    public LanguageItem(string code, string displayName)
    {
      Code = code;
      DisplayName = displayName;
    }
  }

  public class SettingViewModel : BindableObject
  {
    private readonly ISettingsRepository _repository;
    private LanguageItem _selectedLanguageItem;
    private LanguageItem _currentLanguage;

    public SettingViewModel(ISettingsRepository repository)
    {
      _repository = repository ?? throw new ArgumentNullException(nameof(repository));
      _currentLanguage = new LanguageItem(_repository.GetLanguage(), "Language.View.Settings.LanguageCurrent".GetString());
      var savedCode = repository.GetLanguage();
      _selectedLanguageItem = new LanguageItem(savedCode, savedCode == "vi" ? "Tiếng Việt" : "English");
      SaveCommand = new RelayCommand(OnSave);
      CancelCommand = new RelayCommand(OnCancel);
    }

    public LanguageItem SelectedLanguageItem
    {
      get => _selectedLanguageItem;
      set => SetProperty(ref _selectedLanguageItem, value);
    }

    public IReadOnlyList<LanguageItem> AvailableLanguages { get; } = new LanguageItem[]
    {
      new LanguageItem("en", "English"),
      new LanguageItem("vi", "Tiếng Việt"),
    };

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? CloseRequested;

    public string Title => "View.Settings.Title".GetString();
    public string LanguageLabel => "View.Settings.Language".GetString();
    public string SaveLabel => "View.Settings.Save".GetString();
    public string CancelLabel => "View.Settings.Cancel".GetString();

    private void OnSave()
    {
      if (SelectedLanguageItem == null) return;
      LocalizationManager.SetLanguage(SelectedLanguageItem.Code);
      _repository.SaveLanguage(SelectedLanguageItem.Code);
      CloseRequested?.Invoke();
    }

    private void OnCancel()
    {
      CloseRequested?.Invoke();
    }
  }
}
