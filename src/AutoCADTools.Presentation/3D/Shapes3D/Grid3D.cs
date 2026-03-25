using System.Windows.Media;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation._3D.Shapes3D.Utils;

namespace AutoCADTools.Presentation._3D.Shapes3D
{
  /// <summary>
  /// A ground grid for spatial reference — infinite-looking lines on XY plane.
  /// </summary>
  public class Grid3D : Shape3DBase
  {
    public Grid3D() { }

    public Grid3D(double size, double step)
    {
      Size = size;
      Step = step;
    }

    public override Enum3DObjectType ObjectType => Enum3DObjectType.Grid;

    public double Size { get; set; } = 20.0;
    public double Step { get; set; } = 1.0;
    public Color GridColor { get; set; } = Colors.Gray;

    protected override Model3D BuildCoreModel()
    {
      var mesh = MeshGenerator.CreateGrid(Size, Step, GridColor);
      mesh.Freeze();

      var model = new GeometryModel3D
      {
        Geometry = mesh,
        Material = new DiffuseMaterial(new SolidColorBrush(GridColor)),
        BackMaterial = null,
        Transform = new TranslateTransform3D(
          Transform.Position.X,
          Transform.Position.Y,
          Transform.Position.Z)
      };

      model.Freeze();
      return model;
    }

    protected override Rect3D ComputeBoundingBox()
    {
      var p = Transform.Position;
      return new Rect3D(
        new Point3D(p.X - Size / 2, p.Y - Size / 2, p.Z),
        new Size3D(Size, Size, 0));
    }

    protected override bool CoreHitTest(Ray3D ray, double tolerance)
    {
      return ray.IntersectsBox(ComputeBoundingBox());
    }
  }
}
