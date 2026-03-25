using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Service._3D;
using WpfViewport3D = System.Windows.Controls.Viewport3D;

namespace AutoCADTools.Presentation._3D.Viewport3D.Selection
{
  public class SelectionService
  {
    private readonly IObjectManager3D _objectManager;
    private readonly ICameraService _cameraService;

    public SelectionService(IObjectManager3D objectManager, ICameraService cameraService)
    {
      _objectManager = objectManager;
      _cameraService = cameraService;
      SelectionOverlay = new Model3DGroup();
      _hits = new List<SelectionHitResult>();
    }

    private readonly List<SelectionHitResult> _hits;
    public IReadOnlyList<SelectionHitResult> Hits => _hits.AsReadOnly();
    public SelectionHitResult? TopHit => _hits.FirstOrDefault();
    public Ray3D LastRay { get; private set; }

    /// <summary>
    /// WPF Model3DGroup used as wireframe overlay for selected objects.
    /// </summary>
    public Model3DGroup SelectionOverlay { get; }

    public void PerformHitTest(Point screenPt, WpfViewport3D viewport)
    {
      _hits.Clear();

      var ray = ScreenPointToRay(screenPt, viewport);
      LastRay = ray;

      var tolerance = 5.0; // pixels — convert to world units approximately

      foreach (var obj in _objectManager.Objects)
      {
        if (!obj.IsVisible || obj.IsLocked) continue;
        if (obj.HitTest(screenPt, ray, tolerance))
        {
          var bb = obj.GetBoundingBox();
          var center = new Point3D(bb.X + bb.SizeX / 2, bb.Y + bb.SizeY / 2, bb.Z + bb.SizeZ / 2);
          var distance = Distance(ray.Origin, center);
          _hits.Add(new SelectionHitResult(obj, center, ray.Direction, distance));
        }
      }

      _hits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
    }

    public void ClearHits()
    {
      _hits.Clear();
    }

    public void RefreshOverlay()
    {
      SelectionOverlay.Children.Clear();

      foreach (var obj in _objectManager.SelectedObjects)
      {
        var bb = obj.GetBoundingBox();
        var wireframe = CreateWireframeBox(bb, Colors.DodgerBlue, 0.02);
        wireframe.Freeze();
        SelectionOverlay.Children.Add(wireframe);
      }
    }

    private static double Distance(Point3D a, Point3D b)
    {
      var dx = a.X - b.X; var dy = a.Y - b.Y; var dz = a.Z - b.Z;
      return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private Ray3D ScreenPointToRay(Point screenPt, WpfViewport3D viewport)
    {
      var camera = _cameraService.Camera;

      var screenWidth = viewport.ActualWidth;
      var screenHeight = viewport.ActualHeight;

      if (screenWidth <= 0 || screenHeight <= 0)
        return new Ray3D(camera.Position, camera.LookDirection);

      // NDC coordinates (-1 to +1)
      var nx = (screenPt.X / screenWidth) * 2 - 1;
      var ny = -((screenPt.Y / screenHeight) * 2 - 1);

      var aspect = screenWidth / screenHeight;
      var fovRad = camera.FieldOfView * Math.PI / 180.0;
      var halfHeight = Math.Tan(fovRad / 2);
      var halfWidth = halfHeight * aspect;

      var cameraMatrix = GetViewMatrix(camera);

      // Transform NDC point to world direction
      var dx = (float)(nx * halfWidth);
      var dy = (float)(ny * halfHeight);
      var dz = (float)(-1.0);

      var lookDir = camera.LookDirection;
      lookDir.Normalize();
      var right = Vector3D.CrossProduct(lookDir, camera.UpDirection);
      right.Normalize();
      var up = Vector3D.CrossProduct(right, lookDir);
      up.Normalize();

      var worldDir = new Vector3D(
        right.X * dx + up.X * dy - lookDir.X * dz,
        right.Y * dx + up.Y * dy - lookDir.Y * dz,
        right.Z * dx + up.Z * dy - lookDir.Z * dz);
      worldDir.Normalize();

      return new Ray3D(camera.Position, worldDir);
    }

    private static Matrix3D GetViewMatrix(PerspectiveCamera camera)
    {
      var lookDir = camera.LookDirection;
      lookDir.Normalize();
      var right = Vector3D.CrossProduct(lookDir, camera.UpDirection);
      right.Normalize();
      var up = Vector3D.CrossProduct(right, lookDir);
      up.Normalize();

      var pos = camera.Position;
      return new Matrix3D(
        right.X, up.X, -lookDir.X, 0,
        right.Y, up.Y, -lookDir.Y, 0,
        right.Z, up.Z, -lookDir.Z, 0,
        -pos.X * right.X - pos.Y * right.Y - pos.Z * right.Z,
        -pos.X * up.X - pos.Y * up.Y - pos.Z * up.Z,
        pos.X * lookDir.X + pos.Y * lookDir.Y + pos.Z * lookDir.Z,
        1);
    }

    private static Model3D CreateWireframeBox(Rect3D box, Color color, double thickness)
    {
      var group = new Model3DGroup();

      // 12 edges of a box
      var p000 = new Point3D(box.X, box.Y, box.Z);
      var p100 = new Point3D(box.X + box.SizeX, box.Y, box.Z);
      var p010 = new Point3D(box.X, box.Y + box.SizeY, box.Z);
      var p110 = new Point3D(box.X + box.SizeX, box.Y + box.SizeY, box.Z);
      var p001 = new Point3D(box.X, box.Y, box.Z + box.SizeZ);
      var p101 = new Point3D(box.X + box.SizeX, box.Y, box.Z + box.SizeZ);
      var p011 = new Point3D(box.X, box.Y + box.SizeY, box.Z + box.SizeZ);
      var p111 = new Point3D(box.X + box.SizeX, box.Y + box.SizeY, box.Z + box.SizeZ);

      var edges = new[]
      {
        (p000, p100), (p010, p110), (p001, p101), (p011, p111), // bottom-front, bottom-back, top-front, top-back
        (p000, p010), (p100, p110), (p001, p011), (p101, p111), // bottom-left, bottom-right, top-left, top-right
        (p000, p001), (p100, p101), (p010, p011), (p110, p111)  // front-left, front-right, back-left, back-right
      };

      foreach (var (p0, p1) in edges)
      {
        var mesh = CreateLineMesh(p0, p1, thickness);
        mesh.Freeze();
        group.Children.Add(new GeometryModel3D
        {
          Geometry = mesh,
          Material = new DiffuseMaterial(new SolidColorBrush(color)),
          BackMaterial = null
        });
      }

      return group;
    }

    private static MeshGeometry3D CreateLineMesh(Point3D p0, Point3D p1, double thickness)
    {
      var mesh = new MeshGeometry3D();

      var dir = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
      var len = dir.Length;
      if (len < 0.0001) return mesh;
      dir.Normalize();

      var up = Math.Abs(dir.Y) < 0.9 ? new Vector3D(0, 1, 0) : new Vector3D(1, 0, 0);
      var right = Vector3D.CrossProduct(dir, up);
      right.Normalize();

      var t = thickness / 2;
      var pa = new Point3D(p0.X + right.X * t, p0.Y + right.Y * t, p0.Z + right.Z * t);
      var pb = new Point3D(p0.X - right.X * t, p0.Y - right.Y * t, p0.Z - right.Z * t);
      var pc = new Point3D(p1.X + right.X * t, p1.Y + right.Y * t, p1.Z + right.Z * t);
      var pd = new Point3D(p1.X - right.X * t, p1.Y - right.Y * t, p1.Z - right.Z * t);

      var bi = mesh.Positions.Count;
      mesh.Positions.Add(pa); mesh.Positions.Add(pb);
      mesh.Positions.Add(pc); mesh.Positions.Add(pd);
      mesh.Normals.Add(right); mesh.Normals.Add(right);
      mesh.Normals.Add(right); mesh.Normals.Add(right);
      mesh.TriangleIndices.Add(bi); mesh.TriangleIndices.Add(bi + 1); mesh.TriangleIndices.Add(bi + 2);
      mesh.TriangleIndices.Add(bi + 1); mesh.TriangleIndices.Add(bi + 3); mesh.TriangleIndices.Add(bi + 2);

      return mesh;
    }
  }
}
