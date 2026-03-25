using System.Windows.Media;
using System.Windows.Media.Media3D;
using AutoCADTools.Presentation._3D.Models;
using AutoCADTools.Presentation._3D.Shapes3D;
using AutoCADTools.Presentation.Utils;

namespace AutoCADTools.CanvasTest.Canvas3D;

public class Canvas3DTestViewModel : BindableObject
{
  public Model3DGroup SceneRoot { get; } = new Model3DGroup();

  public PerspectiveCamera Camera { get; } = new PerspectiveCamera
  {
    Position = new Point3D(24, -24, 18),
    LookDirection = new Vector3D(-24, 24, -18),
    UpDirection = new Vector3D(0, 0, 1),
    FieldOfView = 45
  };

  public Canvas3DTestViewModel()
  {
    AddLights();
    AddSampleObjects();
  }

  private void AddLights()
  {
    var ambient = new AmbientLight(Color.FromArgb(255, 72, 72, 72));
    var directional = new DirectionalLight(Colors.White, new Vector3D(-1, 1, -1));
    ambient.Freeze();
    directional.Freeze();
    SceneRoot.Children.Add(ambient);
    SceneRoot.Children.Add(directional);
  }

  private void AddSampleObjects()
  {
    var grid = new Grid3D(40, 2)
    {
      Name = "Grid"
    };
    SceneRoot.Children.Add(grid.WpfModel);

    var axis = new Axis3D(8, 0.1)
    {
      Name = "Axis"
    };
    SceneRoot.Children.Add(axis.WpfModel);

    var box1 = new Box3D(4, 3, 2)
    {
      Name = "Box-A"
    };
    box1.Transform.Position = new Point3D(0, 0, 1);
    if (box1.Material is Material3D box1Material)
      box1Material.DiffuseColor = Colors.SteelBlue;
    SceneRoot.Children.Add(box1.WpfModel);

    var box2 = new Box3D(2, 2, 4)
    {
      Name = "Box-B"
    };
    box2.Transform.Position = new Point3D(6, 4, 2);
    if (box2.Material is Material3D box2Material)
      box2Material.DiffuseColor = Colors.Orange;
    SceneRoot.Children.Add(box2.WpfModel);

    var plane = new Plane3D(12)
    {
      Name = "Plane-Top"
    };
    plane.Transform.Position = new Point3D(-8, -4, 4);
    if (plane.Material is Material3D planeMaterial)
      planeMaterial.DiffuseColor = Colors.ForestGreen;
    SceneRoot.Children.Add(plane.WpfModel);
  }
}
