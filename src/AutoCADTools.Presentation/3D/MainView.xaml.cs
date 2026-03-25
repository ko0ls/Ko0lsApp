using System.Windows;

namespace AutoCADTools.Presentation._3D
{
  public partial class MainView : Window
  {
    public MainView(object dataContext)
    {
      InitializeComponent();
      DataContext = dataContext;
    }
  }
}
