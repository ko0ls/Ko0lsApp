using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AutoCADTools.Presentation.Canvas.Utils;

public static class UtilsCanvas
{
  /// <summary>
  /// Returns the minimum and maximum points of a UIElement based on its position and render transform.
  /// </summary>
  public static (Point minPoint, Point maxPoint) GetMinMaxPoints(this UIElement element)
  {
    double left = System.Windows.Controls.Canvas.GetLeft(element);
    double top = System.Windows.Controls.Canvas.GetTop(element);
    if (double.IsNaN(left)) left = 0;
    if (double.IsNaN(top)) top = 0;

    Rect bounds = VisualTreeHelper.GetDescendantBounds(element);
    Rect transformedBounds = element.RenderTransform.TransformBounds(bounds);

    if (element is FrameworkElement fe) {
      Thickness margin = fe.Margin;
      return (
        new Point(left + transformedBounds.Left - margin.Left,
                  top + transformedBounds.Top - margin.Top),
        new Point(left + transformedBounds.Right + margin.Right,
                  top + transformedBounds.Bottom + margin.Bottom));
    }

    return (
      new Point(left + transformedBounds.Left, top + transformedBounds.Top),
      new Point(left + transformedBounds.Right, top + transformedBounds.Bottom));
  }

  /// <summary>
  /// Returns the maximum point of a UIElement.
  /// </summary>
  public static Point GetMaxPoint(this UIElement uiElement)
  {
    switch (uiElement) {
      case Line line:
        return new Point(Math.Max(line.X1, line.X2), Math.Max(line.Y1, line.Y2));
      case Ellipse ellipse: {
        double left = System.Windows.Controls.Canvas.GetLeft(ellipse);
        double top = System.Windows.Controls.Canvas.GetTop(ellipse);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        return new Point(left + ellipse.Width, top + ellipse.Height);
      }
      case Polygon polygon: {
        if (polygon.Points.Count == 0)
          return new Point();
        double maxX = polygon.Points.Max(p => p.X);
        double maxY = polygon.Points.Max(p => p.Y);
        return new Point(maxX, maxY);
      }
      case Polyline polyline: {
        if (polyline.Points.Count == 0)
          return new Point();
        double maxX = polyline.Points.Max(p => p.X);
        double maxY = polyline.Points.Max(p => p.Y);
        return new Point(maxX, maxY);
      }
      case TextBlock textBlock: {
        Rect bounds = VisualTreeHelper.GetDescendantBounds(textBlock);
        Rect transformedBounds = textBlock.RenderTransform.TransformBounds(bounds);
        double left = System.Windows.Controls.Canvas.GetLeft(textBlock);
        double top = System.Windows.Controls.Canvas.GetTop(textBlock);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        return new Point(left + transformedBounds.Right, top + transformedBounds.Bottom);
      }
      default:
        return new Point();
    }
  }

  /// <summary>
  /// Returns the minimum point of a UIElement.
  /// </summary>
  public static Point GetMinPoint(this UIElement uiElement)
  {
    switch (uiElement) {
      case Line line:
        return new Point(Math.Min(line.X1, line.X2), Math.Min(line.Y1, line.Y2));
      case Ellipse ellipse: {
        double left = System.Windows.Controls.Canvas.GetLeft(ellipse);
        double top = System.Windows.Controls.Canvas.GetTop(ellipse);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        return new Point(left, top);
      }
      case Polygon polygon: {
        if (polygon.Points.Count == 0)
          return new Point();
        double minX = polygon.Points.Min(p => p.X);
        double minY = polygon.Points.Min(p => p.Y);
        return new Point(minX, minY);
      }
      case Polyline polyline: {
        if (polyline.Points.Count == 0)
          return new Point();
        double minX = polyline.Points.Min(p => p.X);
        double minY = polyline.Points.Min(p => p.Y);
        return new Point(minX, minY);
      }
      case TextBlock textBlock: {
        Rect bounds = VisualTreeHelper.GetDescendantBounds(textBlock);
        Rect transformedBounds = textBlock.RenderTransform.TransformBounds(bounds);
        double left = System.Windows.Controls.Canvas.GetLeft(textBlock);
        double top = System.Windows.Controls.Canvas.GetTop(textBlock);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        return new Point(left + transformedBounds.Left, top + transformedBounds.Top);
      }
      default:
        return new Point();
    }
  }

  /// <summary>
  /// Calculates the center point of a collection of points as the average of all coordinates.
  /// </summary>
  public static Point CalculateCenter(PointCollection points)
  {
    if (points == null || !points.Any())
      throw new ArgumentException("Points cannot be null or empty.");
    double cx = points.Average(p => p.X);
    double cy = points.Average(p => p.Y);
    return new Point(cx, cy);
  }

  /// <summary>
  /// Returns the line thickness unchanged (scaledThickness is used as-is).
  /// </summary>
  public static double GetLineThickness(double scaledThickness)
  {
    return scaledThickness;
  }
}
