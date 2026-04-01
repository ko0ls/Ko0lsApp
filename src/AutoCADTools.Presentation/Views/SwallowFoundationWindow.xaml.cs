using AutoCADTools.Presentation.Drawing;
using AutoCADTools.Presentation.ViewModels;

namespace AutoCADTools.Presentation.Views;

public partial class SwallowFoundationWindow : System.Windows.Window
{
  public SwallowFoundationWindow()
  {
    InitializeComponent();
    var drawer = new SwallowFoundationPreviewDrawer(PreviewCanvas.Canvas, 100);
    var viewModel = new SwallowFoundationViewModel(drawer);
    DataContext = viewModel;
  }
}
