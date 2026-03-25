using System.Windows.Media;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation._3D.Models
{
  /// <summary>
  /// 3D transform: position, rotation, scale.
  /// Extends BindableObject so every property change is tracked (undo/redo, dirty state).
  /// Note: Quaternion lacks IEquatable, so we store as backing field and use custom comparison.
  /// </summary>
  public class Transform3D : BindableObject, ITransform3D
  {
    private Point3D _position;
    private Quaternion _rotation;
    private Vector3D _scale;

    public Transform3D()
    {
      _position = new Point3D(0, 0, 0);
      _rotation = Quaternion.Identity;
      _scale = new Vector3D(1, 1, 1);
    }

    [ChangeTracker]
    public Point3D Position
    {
      get => _position;
      set
      {
        if (SetProperty(ref _position, value))
          OnTransformChanged();
      }
    }

    [ChangeTracker]
    public Quaternion Rotation
    {
      get => _rotation;
      set
      {
        if (!_rotation.Equals(value))
        {
          _rotation = value;
          OnPropertyChanged(_rotation, nameof(Rotation));
          OnTransformChanged();
        }
      }
    }

    [ChangeTracker]
    public Vector3D Scale
    {
      get => _scale;
      set
      {
        if (SetProperty(ref _scale, value))
          OnTransformChanged();
      }
    }

    public Vector3D Direction
    {
      get
      {
        var m = Matrix;
        return new Vector3D(m.OffsetX, m.OffsetY, m.OffsetZ);
      }
    }

    public Matrix3D Matrix
    {
      get
      {
        var m = Matrix3D.Identity;
        m.Translate(new Vector3D(_position.X, _position.Y, _position.Z));
        m.Append(Matrix3D.Identity);
        m.Scale(_scale);
        return m;
      }
    }

    public void Rotate(Vector3D axis, double degrees)
    {
      var q = new Quaternion(axis, degrees);
      Rotation = q * _rotation;
    }

    public void Translate(Vector3D delta)
    {
      Position = new Point3D(_position.X + delta.X, _position.Y + delta.Y, _position.Z + delta.Z);
    }

    public void ResetRotation()
    {
      Rotation = Quaternion.Identity;
    }

    protected virtual void OnTransformChanged()
    {
      NotifyPropertyChanged(nameof(Direction));
      NotifyPropertyChanged(nameof(Matrix));
    }

    public override bool Equals(object? obj)
    {
      if (obj is Transform3D other)
      {
        return _position.Equals(other._position)
          && _rotation.Equals(other._rotation)
          && _scale.Equals(other._scale);
      }
      return false;
    }

    public override int GetHashCode()
    {
      return unchecked(
        _position.GetHashCode() ^ _rotation.GetHashCode() * 31 ^ _scale.GetHashCode() * 17);
    }
  }
}
