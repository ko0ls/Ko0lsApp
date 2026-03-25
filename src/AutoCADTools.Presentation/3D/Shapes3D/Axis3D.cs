using System.Windows.Media;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation._3D.Shapes3D.Utils;

namespace AutoCADTools.Presentation._3D.Shapes3D
{
  /// <summary>
  /// 3D axis gizmo: X (red), Y (green), Z (blue) lines.
  /// </summary>
  public class Axis3D : Shape3DBase
  {
    public Axis3D() { }

    public Axis3D(double length, double thickness)
    {
      Length = length;
      Thickness = thickness;
    }

    public override Enum3DObjectType ObjectType => Enum3DObjectType.Axis;

    public double Length { get; set; } = 10.0;
    public double Thickness { get; set; } = 0.05;

    protected override Model3D BuildCoreModel()
    {
      var group = new Model3DGroup();

      // X axis — red
      var xMesh = CreateAxisLine(new Point3D(0, 0, 0), new Point3D(Length, 0, 0), Thickness, Colors.Red);
      xMesh.Freeze();
      group.Children.Add(xMesh);

      // Y axis — green
      var yMesh = CreateAxisLine(new Point3D(0, 0, 0), new Point3D(0, Length, 0), Thickness, Colors.Green);
      yMesh.Freeze();
      group.Children.Add(yMesh);

      // Z axis — blue
      var zMesh = CreateAxisLine(new Point3D(0, 0, 0), new Point3D(0, 0, Length), Thickness, Colors.Blue);
      zMesh.Freeze();
      group.Children.Add(zMesh);

      return group;
    }

    private static GeometryModel3D CreateAxisLine(Point3D p0, Point3D p1, double thickness, Color color)
    {
      var mesh = MeshGenerator.CreateBox(
        p1.X - p0.X + thickness,
        p1.Y - p0.Y + thickness,
        p1.Z - p0.Z + thickness);
      mesh.Freeze();

      return new GeometryModel3D
      {
        Geometry = mesh,
        Material = new DiffuseMaterial(new SolidColorBrush(color)),
        BackMaterial = null,
        Transform = new TranslateTransform3D(
          (p0.X + p1.X) / 2,
          (p0.Y + p1.Y) / 2,
          (p0.Z + p1.Z) / 2)
      };
    }

    protected override Rect3D ComputeBoundingBox()
    {
      var p = Transform.Position;
      return new Rect3D(
        new Point3D(p.X, p.Y, p.Z),
        new Size3D(Length, Length, Length));
    }

    protected override bool CoreHitTest(Ray3D ray, double tolerance)
    {
      return ray.IntersectsBox(ComputeBoundingBox());
    }
  }
}
