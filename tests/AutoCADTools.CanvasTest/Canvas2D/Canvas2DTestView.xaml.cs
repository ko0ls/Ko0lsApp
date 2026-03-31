using System.Windows.Controls;

namespace AutoCADTools.CanvasTest.Canvas2D;

public partial class Canvas2DTestView : UserControl
{
  public Canvas2DTestView()
  {
    InitializeComponent();
    DataContext = new Canvas2DTestViewModel();
  }
}
