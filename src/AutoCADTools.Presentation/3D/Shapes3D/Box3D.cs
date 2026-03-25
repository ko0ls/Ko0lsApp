using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation._3D.Shapes3D.Utils;

namespace AutoCADTools.Presentation._3D.Shapes3D
{
  public class Box3D : Shape3DBase
  {
    public Box3D() { }

    public Box3D(double width, double height, double depth)
    {
      Width = width;
      Height = height;
      Depth = depth;
    }

    public override Enum3DObjectType ObjectType => Enum3DObjectType.Box;

    public double Width { get; set; } = 1.0;
    public double Height { get; set; } = 1.0;
    public double Depth { get; set; } = 1.0;

    protected override Model3D BuildCoreModel()
    {
      var mesh = MeshGenerator.CreateBox(Width, Height, Depth);
      mesh.Freeze();

      var mat = Material as Models.Material3D;
      var color = mat?.DiffuseColor ?? System.Windows.Media.Colors.LightGray;

      var model = new GeometryModel3D
      {
        Geometry = mesh,
        Material = new DiffuseMaterial(new System.Windows.Media.SolidColorBrush(color)),
        BackMaterial = mat?.BackFaceCulling == true
          ? new DiffuseMaterial(new System.Windows.Media.SolidColorBrush(color))
          : null,
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
        new Point3D(p.X - Width / 2, p.Y - Height / 2, p.Z - Depth / 2),
        new Size3D(Width, Height, Depth));
    }

    protected override bool CoreHitTest(Ray3D ray, double tolerance)
    {
      return ray.IntersectsBox(ComputeBoundingBox());
    }
  }
}
