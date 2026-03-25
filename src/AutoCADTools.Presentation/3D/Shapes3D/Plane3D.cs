using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation._3D.Shapes3D.Utils;

namespace AutoCADTools.Presentation._3D.Shapes3D
{
  public class Plane3D : Shape3DBase
  {
    public Plane3D() { }

    public Plane3D(double size)
    {
      Size = size;
    }

    public override Enum3DObjectType ObjectType => Enum3DObjectType.Plane;

    public double Size { get; set; } = 100.0;

    protected override Model3D BuildCoreModel()
    {
      var mesh = MeshGenerator.CreatePlane(Size, Transform.Position);
      mesh.Freeze();

      var mat = Material as Models.Material3D;
      var color = mat?.DiffuseColor ?? System.Windows.Media.Colors.DarkGray;

      var model = new GeometryModel3D
      {
        Geometry = mesh,
        Material = new DiffuseMaterial(new System.Windows.Media.SolidColorBrush(color)),
        BackMaterial = null
      };

      return model;
    }

    protected override Rect3D ComputeBoundingBox()
    {
      var p = Transform.Position;
      return new Rect3D(new Point3D(p.X - Size / 2, p.Y - Size / 2, p.Z), new Size3D(Size, Size, 0.001));
    }

    protected override bool CoreHitTest(Ray3D ray, double tolerance)
    {
      var bb = ComputeBoundingBox();
      return ray.IntersectsBox(bb);
    }
  }
}
