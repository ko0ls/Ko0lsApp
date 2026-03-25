using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AutoCADTools.Presentation._3D.Shapes3D.Utils
{
  public static class MeshGenerator
  {
    /// <summary>
    /// Creates a box mesh with given dimensions, centered at origin.
    /// </summary>
    public static MeshGeometry3D CreateBox(double width, double height, double depth, Point3D center = default)
    {
      var mesh = new MeshGeometry3D();

      var hw = width / 2;
      var hh = height / 2;
      var hd = depth / 2;

      var p0 = new Point3D(center.X - hw, center.Y - hh, center.Z - hd);
      var p1 = new Point3D(center.X + hw, center.Y - hh, center.Z - hd);
      var p2 = new Point3D(center.X + hw, center.Y + hh, center.Z - hd);
      var p3 = new Point3D(center.X - hw, center.Y + hh, center.Z - hd);
      var p4 = new Point3D(center.X - hw, center.Y - hh, center.Z + hd);
      var p5 = new Point3D(center.X + hw, center.Y - hh, center.Z + hd);
      var p6 = new Point3D(center.X + hw, center.Y + hh, center.Z + hd);
      var p7 = new Point3D(center.X - hw, center.Y + hh, center.Z + hd);

      // Front face (Z-)
      AddQuad(mesh, p0, p1, p2, p3);
      // Back face (Z+)
      AddQuad(mesh, p5, p4, p7, p6);
      // Top face (Y+)
      AddQuad(mesh, p3, p2, p6, p7);
      // Bottom face (Y-)
      AddQuad(mesh, p4, p5, p1, p0);
      // Left face (X-)
      AddQuad(mesh, p4, p0, p3, p7);
      // Right face (X+)
      AddQuad(mesh, p1, p5, p6, p2);

      return mesh;
    }

    /// <summary>
    /// Creates a ground plane mesh.
    /// </summary>
    public static MeshGeometry3D CreatePlane(double size, Point3D center = default)
    {
      var mesh = new MeshGeometry3D();

      var hs = size / 2;
      var p0 = new Point3D(center.X - hs, center.Y - hs, center.Z);
      var p1 = new Point3D(center.X + hs, center.Y - hs, center.Z);
      var p2 = new Point3D(center.X + hs, center.Y + hs, center.Z);
      var p3 = new Point3D(center.X - hs, center.Y + hs, center.Z);

      // One quad
      AddQuad(mesh, p0, p1, p2, p3);

      return mesh;
    }

    /// <summary>
    /// Creates a 3D axis gizmo (3 lines along X, Y, Z).
    /// </summary>
    public static MeshGeometry3D CreateAxis(double length, double thickness)
    {
      var mesh = new MeshGeometry3D();
      AddLineSegment(mesh, new Point3D(0, 0, 0), new Point3D(length, 0, 0), thickness);
      AddLineSegment(mesh, new Point3D(0, 0, 0), new Point3D(0, length, 0), thickness);
      AddLineSegment(mesh, new Point3D(0, 0, 0), new Point3D(0, 0, length), thickness);
      return mesh;
    }

    /// <summary>
    /// Creates a UV grid on the XY plane.
    /// </summary>
    public static MeshGeometry3D CreateGrid(double size, double step, Color color)
    {
      var mesh = new MeshGeometry3D();
      var half = size / 2;

      for (var x = -half; x <= half; x += step)
        AddLineSegment(mesh, new Point3D(x, -half, 0), new Point3D(x, half, 0), 0.002);

      for (var y = -half; y <= half; y += step)
        AddLineSegment(mesh, new Point3D(-half, y, 0), new Point3D(half, y, 0), 0.002);

      return mesh;
    }

    /// <summary>
    /// Creates a sphere mesh using latitude/longitude.
    /// </summary>
    public static MeshGeometry3D CreateSphere(double radius, int segments = 24)
    {
      var mesh = new MeshGeometry3D();

      for (var lat = 0; lat < segments; lat++)
      {
        var theta1 = lat * Math.PI / segments;
        var theta2 = (lat + 1) * Math.PI / segments;

        for (var lon = 0; lon < segments; lon++)
        {
          var phi1 = lon * 2 * Math.PI / segments;
          var phi2 = (lon + 1) * 2 * Math.PI / segments;

          var v00 = SphericalToCartesian(radius, theta1, phi1);
          var v01 = SphericalToCartesian(radius, theta1, phi2);
          var v10 = SphericalToCartesian(radius, theta2, phi1);
          var v11 = SphericalToCartesian(radius, theta2, phi2);

          AddQuad(mesh, v00, v01, v11, v10);
        }
      }

      return mesh;
    }

    /// <summary>
    /// Creates a cylinder mesh.
    /// </summary>
    public static MeshGeometry3D CreateCylinder(double radius, double height, int segments = 24)
    {
      var mesh = new MeshGeometry3D();
      var hh = height / 2;

      // Body quads
      for (var i = 0; i < segments; i++)
      {
        var a1 = i * 2 * Math.PI / segments;
        var a2 = (i + 1) * 2 * Math.PI / segments;

        var p0 = new Point3D(radius * Math.Cos(a1), radius * Math.Sin(a1), -hh);
        var p1 = new Point3D(radius * Math.Cos(a2), radius * Math.Sin(a2), -hh);
        var p2 = new Point3D(radius * Math.Cos(a2), radius * Math.Sin(a2), hh);
        var p3 = new Point3D(radius * Math.Cos(a1), radius * Math.Sin(a1), hh);

        AddQuad(mesh, p0, p1, p2, p3);
      }

      // Top cap
      AddCircleCap(mesh, radius, hh, segments);
      // Bottom cap
      AddCircleCap(mesh, radius, -hh, segments);

      return mesh;
    }

    private static void AddCircleCap(MeshGeometry3D mesh, double radius, double z, int segments)
    {
      var center = new Point3D(0, 0, z);
      mesh.Positions.Add(center);

      for (var i = 0; i <= segments; i++)
      {
        var a = i * 2 * Math.PI / segments;
        mesh.Positions.Add(new Point3D(radius * Math.Cos(a), radius * Math.Sin(a), z));
      }

      for (var i = 0; i < segments; i++)
      {
        mesh.TriangleIndices.Add(0);
        mesh.TriangleIndices.Add(i + 1);
        mesh.TriangleIndices.Add(i + 2);
      }
    }

    private static Point3D SphericalToCartesian(double r, double theta, double phi)
    {
      var x = r * Math.Sin(theta) * Math.Cos(phi);
      var y = r * Math.Sin(theta) * Math.Sin(phi);
      var z = r * Math.Cos(theta);
      return new Point3D(x, y, z);
    }

    /// <summary>
    /// Adds a quad (two triangles) to the mesh.
    /// </summary>
    private static void AddQuad(MeshGeometry3D mesh, Point3D p0, Point3D p1, Point3D p2, Point3D p3)
    {
      var baseIndex = mesh.Positions.Count;
      mesh.Positions.Add(p0);
      mesh.Positions.Add(p1);
      mesh.Positions.Add(p2);
      mesh.Positions.Add(p3);

      // Compute normal
      var u = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
      var v = new Vector3D(p3.X - p0.X, p3.Y - p0.Y, p3.Z - p0.Z);
      var n = Vector3D.CrossProduct(u, v);
      n.Normalize();

      mesh.Normals.Add(n);
      mesh.Normals.Add(n);
      mesh.Normals.Add(n);
      mesh.Normals.Add(n);

      mesh.TriangleIndices.Add(baseIndex);
      mesh.TriangleIndices.Add(baseIndex + 1);
      mesh.TriangleIndices.Add(baseIndex + 2);
      mesh.TriangleIndices.Add(baseIndex);
      mesh.TriangleIndices.Add(baseIndex + 2);
      mesh.TriangleIndices.Add(baseIndex + 3);
    }

    /// <summary>
    /// Adds a line segment as a thin quad (cylinder approximation).
    /// </summary>
    private static void AddLineSegment(MeshGeometry3D mesh, Point3D p0, Point3D p1, double thickness)
    {
      var dir = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
      var len = dir.Length;
      if (len < 0.0001) return;
      dir.Normalize();

      // Find perpendicular vectors
      var up = Math.Abs(dir.Y) < 0.9 ? new Vector3D(0, 1, 0) : new Vector3D(1, 0, 0);
      var right = Vector3D.CrossProduct(dir, up);
      right.Normalize();
      var n2 = Vector3D.CrossProduct(dir, right);
      n2.Normalize();

      var t = thickness / 2;
      var pa = new Point3D(p0.X + right.X * t, p0.Y + right.Y * t, p0.Z + right.Z * t);
      var pb = new Point3D(p0.X - right.X * t, p0.Y - right.Y * t, p0.Z - right.Z * t);
      var pc = new Point3D(p1.X + right.X * t, p1.Y + right.Y * t, p1.Z + right.Z * t);
      var pd = new Point3D(p1.X - right.X * t, p1.Y - right.Y * t, p1.Z - right.Z * t);

      var baseIndex = mesh.Positions.Count;
      mesh.Positions.Add(pa);
      mesh.Positions.Add(pb);
      mesh.Positions.Add(pc);
      mesh.Positions.Add(pd);

      mesh.Normals.Add(right);
      mesh.Normals.Add(right);
      mesh.Normals.Add(right);
      mesh.Normals.Add(right);

      mesh.TriangleIndices.Add(baseIndex);
      mesh.TriangleIndices.Add(baseIndex + 1);
      mesh.TriangleIndices.Add(baseIndex + 2);
      mesh.TriangleIndices.Add(baseIndex + 1);
      mesh.TriangleIndices.Add(baseIndex + 3);
      mesh.TriangleIndices.Add(baseIndex + 2);
    }
  }
}
