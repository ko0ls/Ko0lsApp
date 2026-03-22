using System.Windows;
using AutoCADTools.Presentation.ViewModels;

namespace AutoCADTools.Presentation.Views
{
  public partial class SettingsWindow : System.Windows.Window
  {
    private readonly SettingViewModel _viewModel;

    public SettingsWindow(SettingViewModel viewModel)
    {
      _viewModel = viewModel;

      InitializeComponent();

      LanguageComboBox.ItemsSource = _viewModel.AvailableLanguages;
      LanguageComboBox.DisplayMemberPath = nameof(LanguageOption.DisplayName);
      LanguageComboBox.SelectedValuePath = nameof(LanguageOption.Code);
      LanguageComboBox.SelectedValue = _viewModel.SelectedLanguage;

      SaveButton.Content = "Save";
      CancelButton.Content = "Cancel";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
      _viewModel.SelectedLanguage = LanguageComboBox.SelectedValue as string ?? "en";
      _viewModel.OnSave();
      DialogResult = true;
      Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }
  }
}
