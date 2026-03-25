using System.Windows.Controls;

namespace AutoCADTools.CanvasTest.Canvas3D;

public partial class Canvas3DTestView : UserControl
{
  public Canvas3DTestView()
  {
    InitializeComponent();
    DataContext = new Canvas3DTestViewModel();
  }
}
