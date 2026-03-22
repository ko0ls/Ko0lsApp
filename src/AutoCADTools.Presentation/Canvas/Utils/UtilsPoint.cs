using System;
using System.Collections.Generic;
using System.Linq;
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
    var vector = point2 - point1;
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

  /// <summary>
  /// Calculates the intersection point of two lines, each defined by a point and a direction vector.
  /// Returns null if lines are parallel.
  /// </summary>
  public static Point? Intersection(Point p1, Vector v1, Point p2, Vector v2)
  {
    var a1 = v1.X;
    var b1 = -v2.X;
    var c1 = p2.X - p1.X;
    var a2 = v1.Y;
    var b2 = -v2.Y;
    var c2 = p2.Y - p1.Y;

    var denom = a1 * b2 - a2 * b1;
    if (Math.Abs(denom) < 1e-9)
      return null;

    var t = ( c1 * b2 - c2 * b1 ) / denom;
    return new Point(p1.X + t * v1.X, p1.Y + t * v1.Y);
  }

  /// <summary>
  /// Calculates the perpendicular distance from a point to an unbounded line defined by a start point and a direction vector.
  /// </summary>
  public static double DistanceToLineUnbound(Point point, Point startPoint, Vector vector)
  {
    var numerator = Math.Abs(vector.X * ( point.Y - startPoint.Y ) - vector.Y * ( point.X - startPoint.X ));
    var denominator = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
    return numerator / denominator;
  }

  /// <summary>
  /// Calculates the signed perpendicular distance from a point to a bounded line segment defined by two endpoints.
  /// Positive if point is on the left side, negative if on the right side.
  /// </summary>
  public static double SignedDistanceToLineBound(Point point, Point startPoint, Point endPoint)
  {
    var dx = endPoint.X - startPoint.X;
    var dy = endPoint.Y - startPoint.Y;
    var numerator = ( endPoint.X - startPoint.X ) * ( startPoint.Y - point.Y ) -
                    ( endPoint.Y - startPoint.Y ) * ( startPoint.X - point.X );
    var denominator = Math.Sqrt(dx * dx + dy * dy);
    return denominator < 1e-9 ? 0 : numerator / denominator;
  }

  /// <summary>
  /// Calculates the signed perpendicular distance from a point to an unbounded line defined by a start point and a direction vector.
  /// </summary>
  public static double SignedDistanceToLineUnbound(Point point, Point startPoint, Vector vector)
  {
    var numerator = vector.Y * ( point.X - startPoint.X ) - vector.X * ( point.Y - startPoint.Y );
    var denominator = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
    return denominator < 1e-9 ? 0 : numerator / denominator;
  }

  /// <summary>
  /// Projects a point onto a bounded line segment, then rotates the projection along the line by angle degrees.
  /// </summary>
  public static Point ProjectOnLineBound(Point point, Point startPoint, Point endPoint, double angle)
  {
    var projectedPoint = point.ProjectOnLineBound(startPoint, endPoint);
    if (point.DistanceTo(projectedPoint) < 1e-9)
      return projectedPoint;

    var vector = CreateVector(point, projectedPoint);
    var vectorRotate = Rotate(vector, angle);
    var lineDirection = CreateVector(startPoint, endPoint);
    if (IsAlmostEqualTo(vectorRotate, lineDirection))
      return new Point();

    var projectedPointWithAngle = Intersection(point, vectorRotate, startPoint, lineDirection);
    return projectedPointWithAngle ?? new Point();
  }

  /// <summary>
  /// Projects a point onto an unbounded line defined by a start point and a direction vector, then rotates the projection.
  /// </summary>
  public static Point? ProjectOnLineUnbound(Point point, Point startPoint, Vector direction, double angle)
  {
    var endPoint = new Point(startPoint.X + direction.X, startPoint.Y + direction.Y);
    var projectedPoint = point.ProjectOnLineUnbound(startPoint, endPoint);
    if (point.DistanceTo(projectedPoint) < 1e-9)
      return projectedPoint;

    var vector = CreateVector(point, projectedPoint);
    var vectorRotate = Rotate(vector, angle);
    return IsAlmostEqualTo(vectorRotate, direction) ? null : Intersection(point, vectorRotate, startPoint, direction);
  }

  private static Vector CreateVector(Point start, Point end)
  {
    var v = end - start;
    v.Normalize();
    return v;
  }

  private static bool IsAlmostEqualTo(Vector v1, Vector v2)
  {
    return Math.Abs(v1.X - v2.X) < 1e-9 && Math.Abs(v1.Y - v2.Y) < 1e-9;
  }

  private static Vector Rotate(Vector vector, double angleInDegrees)
  {
    var rotationMatrix = new System.Windows.Media.Matrix();
    rotationMatrix.Rotate(angleInDegrees);
    return System.Windows.Vector.Multiply(vector, rotationMatrix);
  }

  /// <summary>
  /// Finds the closest start/end points accounting for skew at each end.
  /// </summary>
  public static (Point, Point) FindClosestPoints(Point sp, Point ep, double spacing, double startGridSkew,
    double endGridSkew)
  {
    var closestPointStart = sp;
    var closestPointEnd = ep;

    var vector = CreateVector(sp, ep);

    if (startGridSkew >= -1e-9) {
      closestPointStart = sp;
    }
    else {
      var horizontalOffset = spacing * Math.Tan(startGridSkew * Math.PI / 180.0);
      closestPointStart += vector * horizontalOffset;
    }

    if (endGridSkew <= 1e-9) {
      closestPointEnd = ep;
    }
    else {
      var horizontalOffset = spacing * Math.Tan(endGridSkew * Math.PI / 180.0);
      closestPointEnd += vector * horizontalOffset;
    }

    return ( closestPointStart, closestPointEnd );
  }

  /// <summary>
  /// Finds the pair of points (one from each segment) that are closest to each other.
  /// </summary>
  public static (Point, Point) FindClosestPoints(Point sp, Point ep, Point sp2, Point ep2)
  {
    var points = new List<(Point, Point)> {
      ( sp, ep ),
      ( sp2, ep2 ),
      ( sp, ep2 ),
      ( sp2, ep )
    };
    return points.OrderBy(x => x.Item1.DistanceTo(x.Item2)).FirstOrDefault();
  }

  /// <summary>
  /// Determines orientation of three points. Returns 1 for counterclockwise, -1 for clockwise, 0 for collinear.
  /// </summary>
  public static int GetOrientation(Point p1, Point p2, Point p3)
  {
    var orientation = ( p2.X - p1.X ) * ( p3.Y - p1.Y ) - ( p2.Y - p1.Y ) * ( p3.X - p1.X );
    return orientation switch {
      > 1e-9 => 1,
      < -1e-9 => -1,
      _ => 0
    };
  }
}