using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;

namespace AutoCADTools.Service._3D
{
  public class CameraService : ICameraService
  {
    private PerspectiveCamera _camera;
    private double _fieldOfView = 45.0;
    private double _zoom = 1.0;
    private Point3D _target = new Point3D(0, 0, 0);

    public CameraService()
    {
      _camera = new PerspectiveCamera(
        new Point3D(10, 10, 10),
        new Vector3D(-1, -1, -1),
        new Vector3D(0, 0, 1),
        _fieldOfView);
    }

    public PerspectiveCamera Camera => _camera;

    public double FieldOfView
    {
      get => _fieldOfView;
      set
      {
        _fieldOfView = value;
        _camera.FieldOfView = value;
        OnCameraChanged();
      }
    }

    public bool IsOrthographic { get; set; }

    public double Zoom
    {
      get => _zoom;
      set
      {
        _zoom = value;
        OnCameraChanged();
      }
    }

    public Point3D Target
    {
      get => _target;
      set
      {
        _target = value;
        OnCameraChanged();
      }
    }

    public event EventHandler CameraChanged;

    public void Orbit(Vector3D axis, double degrees)
    {
      var pos = _camera.Position;
      var q = new Quaternion(axis, degrees);
      var delta = new Vector3D(pos.X - _target.X, pos.Y - _target.Y, pos.Z - _target.Z);
      var rotated = RotateVector(delta, q);
      _camera.Position = new Point3D(_target.X + rotated.X, _target.Y + rotated.Y, _target.Z + rotated.Z);
      _camera.UpDirection = RotateVector(_camera.UpDirection, q);
      OnCameraChanged();
    }

    private static Vector3D RotateVector(Vector3D v, Quaternion q)
    {
      // Rodrigues' rotation via quaternion
      var w = q.W; var x = q.X; var y = q.Y; var z = q.Z;
      var vx = v.X * (1 - 2 * (y * y + z * z)) + v.Y * (2 * (x * y - z * w))         + v.Z * (2 * (x * z + y * w));
      var vy = v.X * (2 * (x * y + z * w))         + v.Y * (1 - 2 * (x * x + z * z)) + v.Z * (2 * (y * z - x * w));
      var vz = v.X * (2 * (x * z - y * w))         + v.Y * (2 * (y * z + x * w))       + v.Z * (1 - 2 * (x * x + y * y));
      return new Vector3D(vx, vy, vz);
    }

    public void Pan(Vector3D direction, double distance)
    {
      _camera.Position = new Point3D(
        _camera.Position.X + direction.X * distance,
        _camera.Position.Y + direction.Y * distance,
        _camera.Position.Z + direction.Z * distance);
      _target = new Point3D(
        _target.X + direction.X * distance,
        _target.Y + direction.Y * distance,
        _target.Z + direction.Z * distance);
      OnCameraChanged();
    }

    public void ZoomIn()
    {
      var dir = _camera.Position - _target;
      var len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y + dir.Z * dir.Z);
      if (len < 0.001) return;
      dir = new Vector3D(dir.X / len, dir.Y / len, dir.Z / len);
      _camera.Position = new Point3D(
        _camera.Position.X + dir.X,
        _camera.Position.Y + dir.Y,
        _camera.Position.Z + dir.Z);
      OnCameraChanged();
    }

    public void ZoomOut()
    {
      var dir = _camera.Position - _target;
      var len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y + dir.Z * dir.Z);
      if (len < 0.001) return;
      dir = new Vector3D(dir.X / len, dir.Y / len, dir.Z / len);
      _camera.Position = new Point3D(
        _camera.Position.X - dir.X,
        _camera.Position.Y - dir.Y,
        _camera.Position.Z - dir.Z);
      OnCameraChanged();
    }

    public void ZoomToFit(Rect3D worldBox)
    {
      var center = new Point3D(
        worldBox.X + worldBox.SizeX / 2,
        worldBox.Y + worldBox.SizeY / 2,
        worldBox.Z + worldBox.SizeZ / 2);
      var maxDim = Math.Max(Math.Max(worldBox.SizeX, worldBox.SizeY), worldBox.SizeZ);
      if (maxDim < 0.001) maxDim = 1;
      var distance = maxDim * 2.5;

      _camera.Position = new Point3D(center.X + distance, center.Y + distance, center.Z + distance);
      _camera.LookDirection = new Vector3D(-1, -1, -1);
      _camera.UpDirection = new Vector3D(0, 0, 1);
      _target = center;
      OnCameraChanged();
    }

    public void ZoomToObjects(IReadOnlyCollection<IObject3DBase> objs)
    {
      if (objs.Count == 0)
      {
        ResetCamera();
        return;
      }

      double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
      double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

      foreach (var obj in objs)
      {
        var bb = obj.GetBoundingBox();
        minX = Math.Min(minX, bb.X);
        minY = Math.Min(minY, bb.Y);
        minZ = Math.Min(minZ, bb.Z);
        maxX = Math.Max(maxX, bb.X + bb.SizeX);
        maxY = Math.Max(maxY, bb.Y + bb.SizeY);
        maxZ = Math.Max(maxZ, bb.Z + bb.SizeZ);
      }

      ZoomToFit(new Rect3D(new Point3D(minX, minY, minZ), new Size3D(maxX - minX, maxY - minY, maxZ - minZ)));
    }

    private void ResetCamera()
    {
      _camera.Position = new Point3D(10, 10, 10);
      _camera.LookDirection = new Vector3D(-1, -1, -1);
      _camera.UpDirection = new Vector3D(0, 0, 1);
      _target = new Point3D(0, 0, 0);
      OnCameraChanged();
    }

    public void SetPresetView(EnumPresetView view)
    {
      switch (view)
      {
        case EnumPresetView.Top:
          _camera.Position = new Point3D(0, 0, 100); _camera.LookDirection = new Vector3D(0, 0, -1); _camera.UpDirection = new Vector3D(0, 1, 0); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.Bottom:
          _camera.Position = new Point3D(0, 0, -100); _camera.LookDirection = new Vector3D(0, 0, 1); _camera.UpDirection = new Vector3D(0, 1, 0); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.Front:
          _camera.Position = new Point3D(0, 100, 0); _camera.LookDirection = new Vector3D(0, -1, 0); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.Back:
          _camera.Position = new Point3D(0, -100, 0); _camera.LookDirection = new Vector3D(0, 1, 0); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.Left:
          _camera.Position = new Point3D(-100, 0, 0); _camera.LookDirection = new Vector3D(1, 0, 0); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.Right:
          _camera.Position = new Point3D(100, 0, 0); _camera.LookDirection = new Vector3D(-1, 0, 0); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.Isometric:
        case EnumPresetView.NorthEast:
          _camera.Position = new Point3D(10, 10, 10); _camera.LookDirection = new Vector3D(-1, -1, -1); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.SouthEast:
          _camera.Position = new Point3D(10, -10, 10); _camera.LookDirection = new Vector3D(-1, 1, -1); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.SouthWest:
          _camera.Position = new Point3D(-10, -10, 10); _camera.LookDirection = new Vector3D(1, 1, -1); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
        case EnumPresetView.NorthWest:
          _camera.Position = new Point3D(-10, 10, 10); _camera.LookDirection = new Vector3D(1, -1, -1); _camera.UpDirection = new Vector3D(0, 0, 1); _target = new Point3D(0, 0, 0); break;
      }
      OnCameraChanged();
    }

    protected virtual void OnCameraChanged()
    {
      CameraChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}
