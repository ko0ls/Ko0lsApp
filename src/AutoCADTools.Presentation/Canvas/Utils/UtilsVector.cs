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
    return Math.Atan2(vector.Y, vector.X) * (180.0 / Math.PI);
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

    var cosTheta = dotProduct / (magnitudeA * magnitudeB);

    var angleInRadians = Math.Acos(cosTheta);
    var angleInDegrees = angleInRadians * (180.0 / Math.PI);

    return angleInDegrees;
  }
}
