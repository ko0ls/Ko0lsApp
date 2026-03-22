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
      if (DataContext is SettingViewModel vm)
      {
        vm.CloseRequested -= OnCloseRequested;
      }
      Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      if (DataContext is SettingViewModel vm)
      {
        vm.CloseRequested -= OnCloseRequested;
        vm.CancelCommand.Execute(null); // trigger cancel logic for X button
      }
      base.OnClosing(e);
    }
  }
}
