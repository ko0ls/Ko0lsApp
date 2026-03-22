using System;
using AutoCADTools.Presentation.ViewModels;

namespace AutoCADTools.Presentation.Views
{
  public partial class SettingsWindow : System.Windows.Window
  {
    public SettingsWindow(SettingViewModel viewModel)
    {
      InitializeComponent();
      DataContext = viewModel;
      viewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested()
    {
      Close();
    }
  }
}
