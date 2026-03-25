using AutoCADTools.Presentation._3D.ObjectManager3D;
using AutoCADTools.Presentation._3D.Properties3D;
using AutoCADTools.Presentation._3D.Viewport3D;
using AutoCADTools.Presentation.Utils;

namespace AutoCADTools.Presentation._3D
{
  public class Viewport3DWindowViewModel : BindableObject
  {
    public Viewport3DWindowViewModel(
      Viewport3DViewModel viewportViewModel,
      Properties3DViewModel propertiesViewModel,
      ObjectManager3DViewModel objectManagerViewModel)
    {
      ViewportViewModel = viewportViewModel;
      PropertiesViewModel = propertiesViewModel;
      ObjectManagerViewModel = objectManagerViewModel;
    }

    public Viewport3DViewModel ViewportViewModel { get; }
    public Properties3DViewModel PropertiesViewModel { get; }
    public ObjectManager3DViewModel ObjectManagerViewModel { get; }
  }
}
