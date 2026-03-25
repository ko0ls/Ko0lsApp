using System.Windows;

namespace AutoCADTools.Presentation._3D
{
  public partial class Viewport3DWindow : Window
  {
    public Viewport3DWindow(object dataContext)
    {
      InitializeComponent();
      DataContext = dataContext;
    }
  }
}
