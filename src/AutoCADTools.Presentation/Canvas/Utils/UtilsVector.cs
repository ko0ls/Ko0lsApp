using System;
using System.Windows;

namespace AutoCADTools.Presentation.Canvas.Utils;

public static class UtilsVector
{
  private const double Tolerance = 1e-9;

  private static bool IsZero(this double value)
  {
    return Math.Abs(value) < Tolerance;
  }

  /// <summary>
  /// Checks if a given vector is valid by ensuring its X and Y components are not NaN.
  /// </summary>
  /// <param name="v">The vector to validate.</param>
  /// <returns>True if both coordinates are valid numbers; otherwise, false.</returns>
  public static bool IsValid(this Vector v)
  {
    return v.X is not double.NaN && v.Y is not double.NaN;
  }

  /// <summary>
  /// Creates a normalized vector from a start point to an end point.
  /// </summary>
  /// <param name="startPoint">The starting point.</param>
  /// <param name="endPoint">The ending point.</param>
  /// <returns>A normalized vector pointing from startPoint to endPoint.</returns>
  public static Vector CreateVector(Point startPoint, Point endPoint)
  {
    var v = endPoint - startPoint;
    v.Normalize();
    return v;
  }

  /// <summary>
  /// Calculates the angle in degrees of a vector relative to the X-axis.
  /// </summary>
  /// <param name="vector">The vector.</param>
  /// <returns>The angle in degrees, measured counterclockwise from the X-axis.</returns>
  public static double GetAngle(this Vector vector)
  {
    return Math.Atan2(vector.Y, vector.X) * ( 180.0 / Math.PI );
  }

  /// <summary>
  /// Calculates the angle in degrees between two vectors.
  /// </summary>
  /// <param name="vectorA">The first vector.</param>
  /// <param name="vectorB">The second vector.</param>
  /// <returns>The angle in degrees between the two vectors.</returns>
  public static double AngleTo(this Vector vectorA, Vector vectorB)
  {
    var dotProduct = Vector.Multiply(vectorA, vectorB);

    var magnitudeA = vectorA.Length;
    var magnitudeB = vectorB.Length;

    var cosTheta = dotProduct / ( magnitudeA * magnitudeB );

    var angleInRadians = Math.Acos(cosTheta);
    var angleInDegrees = angleInRadians * ( 180.0 / Math.PI );

    return angleInDegrees;
  }

  /// <summary>
  /// Rotates a vector by the specified angle in degrees.
  /// </summary>
  public static Vector Rotate(this Vector vector, double angleInDegrees)
  {
    var rotationMatrix = new System.Windows.Media.Matrix();
    rotationMatrix.Rotate(angleInDegrees);
    return Vector.Multiply(vector, rotationMatrix);
  }

  /// <summary>
  /// Checks if two vectors are approximately equal.
  /// </summary>
  public static bool IsAlmostEqualTo(this Vector v1, Vector v2)
  {
    return Math.Abs(v1.X - v2.X) < 1e-9 && Math.Abs(v1.Y - v2.Y) < 1e-9;
  }

  /// <summary>
  /// Calculates the unit vector that bisects the angle between two vectors.
  /// </summary>
  public static Vector GetBisector(Vector v1, Vector v2)
  {
    if (v1.IsAlmostEqualTo(v2))
      return v1;

    if (new Vector(-v1.X, -v1.Y).IsAlmostEqualTo(v2))
      return new Vector(0, 1);

    var normalizedV1 = Vector.Multiply(v1, 1.0 / v1.Length);
    var normalizedV2 = Vector.Multiply(v2, 1.0 / v2.Length);

    var bisector = normalizedV1 + normalizedV2;
    return bisector.Length > 1e-9 ? Vector.Multiply(bisector, 1.0 / bisector.Length) : new Vector(0, 0);
  }

  /// <summary>
  /// Calculates the signed angle in degrees between two vectors on a plane.
  /// Positive if rotation from vectorA to vectorB is counterclockwise.
  /// </summary>
  public static double AngleOnPlaneTo(this Vector vectorA, Vector vectorB)
  {
    var dotProduct = Vector.Multiply(vectorA, vectorB);
    var magnitudeA = vectorA.Length;
    var magnitudeB = vectorB.Length;

    if (magnitudeA < 1e-9 || magnitudeB < 1e-9)
      throw new InvalidOperationException("Vectors must not be zero vectors.");

    var cosAngle = dotProduct / ( magnitudeA * magnitudeB );
    cosAngle = Math.Max(-1, Math.Min(1, cosAngle));
    var angle = Math.Acos(cosAngle);
    var crossProductZ = vectorA.X * vectorB.Y - vectorA.Y * vectorB.X;
    var angleInDegrees = angle * ( 180.0 / Math.PI );
    return crossProductZ < -1e-9 ? -angleInDegrees : angleInDegrees;
  }

  /// <summary>
  /// Creates a unit vector from an angle in degrees relative to the X-axis.
  /// </summary>
  public static Vector CreateVectorFromAngle(double angleInDegrees)
  {
    var angleInRadians = angleInDegrees * Math.PI / 180.0;
    return new Vector(Math.Cos(angleInRadians), Math.Sin(angleInRadians));
  }
}