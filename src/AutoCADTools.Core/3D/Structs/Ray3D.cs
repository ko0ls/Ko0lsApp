using System.Windows.Media.Media3D;

namespace AutoCADTools.Core._3D
{
  /// <summary>
  /// Represents a 3D ray (origin + direction) for hit-testing.
  /// </summary>
  public struct Ray3D
  {
    public Point3D Origin { get; set; }
    public Vector3D Direction { get; set; }

    public Ray3D(Point3D origin, Vector3D direction)
    {
      Origin = origin;
      Direction = direction;
    }

    public Point3D GetPoint(double t) => new Point3D(
      Origin.X + Direction.X * t,
      Origin.Y + Direction.Y * t,
      Origin.Z + Direction.Z * t);

    private static double Dot(Vector3D a, Vector3D b)
    {
      return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    /// <summary>
    /// Returns true if this ray intersects a given sphere.
    /// </summary>
    public bool IntersectsSphere(Point3D center, double radius)
    {
      var oc = new Vector3D(Origin.X - center.X, Origin.Y - center.Y, Origin.Z - center.Z);
      var a = Dot(Direction, Direction);
      var b = 2.0 * Dot(oc, Direction);
      var c = Dot(oc, oc) - radius * radius;
      var discriminant = b * b - 4 * a * c;
      return discriminant >= 0;
    }

    /// <summary>
    /// Returns true if this ray intersects a given axis-aligned bounding box.
    /// </summary>
    public bool IntersectsBox(Rect3D box)
    {
      var invDir = new Vector3D(
        Direction.X != 0 ? 1.0 / Direction.X : double.PositiveInfinity,
        Direction.Y != 0 ? 1.0 / Direction.Y : double.PositiveInfinity,
        Direction.Z != 0 ? 1.0 / Direction.Z : double.PositiveInfinity);

      var t1 = (box.X - Origin.X) * invDir.X;
      var t2 = (box.X + box.SizeX - Origin.X) * invDir.X;
      var t3 = (box.Y - Origin.Y) * invDir.Y;
      var t4 = (box.Y + box.SizeY - Origin.Y) * invDir.Y;
      var t5 = (box.Z - Origin.Z) * invDir.Z;
      var t6 = (box.Z + box.SizeZ - Origin.Z) * invDir.Z;

      var tmin = System.Math.Max(System.Math.Max(System.Math.Min(t1, t2), System.Math.Min(t3, t4)), System.Math.Min(t5, t6));
      var tmax = System.Math.Min(System.Math.Min(System.Math.Max(t1, t2), System.Math.Max(t3, t4)), System.Math.Max(t5, t6));

      return tmax >= 0 && tmin <= tmax;
    }
  }
}
