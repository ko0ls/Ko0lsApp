using System;
using System.Collections.Generic;
using System.Windows.Input;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Storage;

namespace AutoCADTools.Presentation.ViewModels
{
  public class LanguageOption
  {
    public string Code { get; set; }
    public string DisplayName { get; set; }

    public LanguageOption(string code, string displayName)
    {
      Code = code;
      DisplayName = displayName;
    }
  }

  public class SettingViewModel : BindableObject
  {
    private string _selectedLanguage;
    private readonly ISettingsRepository _repository;

    public SettingViewModel(ISettingsRepository repository)
    {
      _repository = repository ?? throw new ArgumentNullException(nameof(repository));
      _selectedLanguage = _repository.GetLanguage();
      SaveCommand = new RelayCommand(OnSave);
    }

    public string SelectedLanguage
    {
      get => _selectedLanguage;
      set => SetProperty(ref _selectedLanguage, value);
    }

    public IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new List<LanguageOption>
    {
      new LanguageOption("en", "English"),
      new LanguageOption("vi", "Tiếng Việt"),
    };

    public ICommand SaveCommand { get; }

    public void OnSave()
    {
      LocalizationManager.SetLanguage(SelectedLanguage);
      _repository.SaveLanguage(SelectedLanguage);
    }
  }
}
