using System.Collections.Generic;
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

  public IReadOnlyList<Shape3DBase> Shapes => _shapes;
  private readonly List<Shape3DBase> _shapes = new List<Shape3DBase>();

  private Shape3DBase? _selectedShape;
  public Shape3DBase? SelectedShape
  {
    get => _selectedShape;
    private set => SetProperty(ref _selectedShape, value, nameof(SelectedShape));
  }

  private Shape3DBase? _hoveredShape;
  public Shape3DBase? HoveredShape
  {
    get => _hoveredShape;
    private set => SetProperty(ref _hoveredShape, value, nameof(HoveredShape));
  }

  public Canvas3DTestViewModel()
  {
    AddLights();
    AddSampleObjects();
  }

  public void SetSelectedShape(Shape3DBase? shape)
  {
    if (ReferenceEquals(SelectedShape, shape)) return;

    if (SelectedShape != null)
      SelectedShape.IsSelected = false;

    SelectedShape = shape;

    if (SelectedShape != null)
      SelectedShape.IsSelected = true;
  }

  public void SetHoveredShape(Shape3DBase? shape)
  {
    if (ReferenceEquals(HoveredShape, shape)) return;

    if (HoveredShape != null)
      HoveredShape.IsMoveOver = false;

    HoveredShape = shape;

    if (HoveredShape != null)
      HoveredShape.IsMoveOver = true;
  }

  public void ClearSelection()
  {
    SetSelectedShape(null);
  }

  public void ClearHover()
  {
    SetHoveredShape(null);
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
    AddShape(grid);

    var axis = new Axis3D(8, 0.1)
    {
      Name = "Axis"
    };
    AddShape(axis);

    var box1 = new Box3D(4, 3, 2)
    {
      Name = "Box-A"
    };
    box1.Transform.Position = new Point3D(0, 0, 1);
    if (box1.Material is Material3D box1Material)
      box1Material.DiffuseColor = Colors.SteelBlue;
    AddShape(box1);

    var box2 = new Box3D(2, 2, 4)
    {
      Name = "Box-B"
    };
    box2.Transform.Position = new Point3D(6, 4, 2);
    if (box2.Material is Material3D box2Material)
      box2Material.DiffuseColor = Colors.Orange;
    AddShape(box2);

    var plane = new Plane3D(12)
    {
      Name = "Plane-Top"
    };
    plane.Transform.Position = new Point3D(-8, -4, 4);
    if (plane.Material is Material3D planeMaterial)
      planeMaterial.DiffuseColor = Colors.ForestGreen;
    AddShape(plane);
  }

  private void AddShape(Shape3DBase shape)
  {
    _shapes.Add(shape);
    SceneRoot.Children.Add(shape.WpfModel);
  }
}
