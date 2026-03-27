using AutoCADTools.Presentation.Drawing;
using AutoCADTools.Presentation.ViewModels;

namespace AutoCADTools.Presentation.Views;

public partial class SwallowFoundationWindow : System.Windows.Window
{
  private readonly SwallowFoundationPreviewDrawer _drawer;
  private readonly SwallowFoundationViewModel _viewModel;
  private bool _closeRequestedHandler;

  public SwallowFoundationWindow()
  {
    InitializeComponent();

    _drawer = new SwallowFoundationPreviewDrawer(PreviewCanvas);
    _viewModel = new SwallowFoundationViewModel(_drawer);
    DataContext = _viewModel;

    _viewModel.CloseRequested += OnCloseRequested;
  }

  private void OnCloseRequested()
  {
    if (_closeRequestedHandler) return;
    _closeRequestedHandler = true;

    if (DataContext is SwallowFoundationViewModel vm)
    {
      vm.CloseRequested -= OnCloseRequested;
    }
    Close();
  }

  protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    if (DataContext is SwallowFoundationViewModel vm)
    {
      vm.CloseRequested -= OnCloseRequested;
      vm.CancelCommand.Execute(null);
    }
    base.OnClosing(e);
  }

  protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
  {
    base.OnRenderSizeChanged(sizeInfo);
    _viewModel.RefreshPreview();
  }
}
