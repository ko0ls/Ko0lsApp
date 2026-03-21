using System;
using System.Windows;

namespace AutoCADTools.Presentation.Canvas.Utils;

public static class UtilsPoint
{
  private const double Tolerance = 1e-9;

  private static bool IsZero(this double value)
  {
    return Math.Abs(value) < Tolerance;
  }

  private static bool IsAlmostEqualsTo(this double value, double other)
  {
    return ( value - other ).IsZero();
  }

  /// <summary>
  /// Checks if a given point is valid by ensuring its X and Y components are not NaN.
  /// </summary>
  /// <param name="p">The point to validate.</param>
  /// <returns>True if both coordinates are valid numbers; otherwise, false.</returns>
  public static bool IsValid(this Point p)
  {
    return p.X is not double.NaN && p.Y is not double.NaN;
  }

  /// <summary>
  /// Calculates the distance between two points.
  /// </summary>
  /// <param name="point1">The first point.</param>
  /// <param name="point2">The second point.</param>
  /// <returns>The Euclidean distance between the two points.</returns>
  public static double DistanceTo(this Point point1, Point point2)
  {
    Vector vector = point2 - point1;
    return vector.Length;
  }

  /// <summary>
  /// Calculates the midpoint between two points.
  /// </summary>
  /// <param name="p1">The first point.</param>
  /// <param name="p2">The second point.</param>
  /// <returns>The midpoint between p1 and p2.</returns>
  public static Point MidPoint(Point p1, Point p2)
  {
    var midX = ( p1.X + p2.X ) / 2;
    var midY = ( p1.Y + p2.Y ) / 2;
    return new Point(midX, midY);
  }

  /// <summary>
  /// Projects a point onto a bounded line segment defined by two endpoints.
  /// The projection point lies on the line segment if it falls within the segment;
  /// otherwise, it will be clamped to the nearest endpoint.
  /// </summary>
  /// <param name="point">The point to project.</param>
  /// <param name="startPoint">The start point of the line segment.</param>
  /// <param name="endPoint">The end point of the line segment.</param>
  /// <returns>The projected point on the bounded line segment.</returns>
  public static Point ProjectOnLineBound(this Point point, Point startPoint, Point endPoint)
  {
    var dx = endPoint.X - startPoint.X;
    var dy = endPoint.Y - startPoint.Y;

    var t = ( ( point.X - startPoint.X ) * dx + ( point.Y - startPoint.Y ) * dy )
            / ( dx * dx + dy * dy );

    // Clamp t to [0, 1] so projection stays within the segment
    t = Math.Max(0, Math.Min(1, t));

    return new Point(startPoint.X + t * dx, startPoint.Y + t * dy);
  }

  /// <summary>
  /// Projects a point onto an unbounded line defined by a start point and an end point.
  /// The projection point lies on the line regardless of whether it falls within any segment.
  /// </summary>
  /// <param name="point">The point to project.</param>
  /// <param name="startPoint">The start point of the line.</param>
  /// <param name="endPoint">The end point of the line.</param>
  /// <returns>The projected point on the unbounded line.</returns>
  public static Point ProjectOnLineUnbound(this Point point, Point startPoint, Point endPoint)
  {
    var dx = endPoint.X - startPoint.X;
    var dy = endPoint.Y - startPoint.Y;

    var t = ( ( point.X - startPoint.X ) * dx + ( point.Y - startPoint.Y ) * dy )
            / ( dx * dx + dy * dy );

    return new Point(startPoint.X + t * dx, startPoint.Y + t * dy);
  }

  /// <summary>
  /// Calculates the perpendicular distance from a point to a bounded line segment
  /// defined by two endpoints.
  /// </summary>
  /// <param name="point">The point.</param>
  /// <param name="startPoint">The start point of the line segment.</param>
  /// <param name="endPoint">The end point of the line segment.</param>
  /// <returns>The perpendicular distance from the point to the line segment.</returns>
  public static double DistanceToLineBound(this Point point, Point startPoint, Point endPoint)
  {
    var dx = endPoint.X - startPoint.X;
    var dy = endPoint.Y - startPoint.Y;

    var numerator = Math.Abs(
      dy * point.X - dx * point.Y + endPoint.X * startPoint.Y - endPoint.Y * startPoint.X);

    var denominator = Math.Sqrt(dx * dx + dy * dy);

    return numerator / denominator;
  }

  /// <summary>
  /// Checks if two points are approximately equal by comparing their distance.
  /// </summary>
  /// <param name="p1">The first point.</param>
  /// <param name="p2">The second point.</param>
  /// <returns>True if the distance between the two points is effectively zero.</returns>
  public static bool IsAlmostEqualTo(this Point p1, Point p2)
  {
    return p1.DistanceTo(p2).IsZero();
  }
}