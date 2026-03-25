using System.Windows.Media.Media3D;

namespace AutoCADTools.Core._3D
{
  public interface ITransform3D
  {
    Point3D Position { get; set; }
    Quaternion Rotation { get; set; }
    Vector3D Scale { get; set; }
    Vector3D Direction { get; }
    Matrix3D Matrix { get; }
    void Rotate(Vector3D axis, double degrees);
    void Translate(Vector3D delta);
    void ResetRotation();
  }
}
