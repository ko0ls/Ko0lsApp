using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Canvas.LineWeightData;
using Wpf.Controls.PanAndZoom;

namespace AutoCADTools.Presentation.Canvas.Utils;

public static class UtilsCanvas
{
  /// <summary>
  /// Returns the minimum and maximum points of a UIElement based on its position and render transform.
  /// </summary>
  public static (Point minPoint, Point maxPoint) GetMinMaxPoints(this UIElement element)
  {
    var left = System.Windows.Controls.Canvas.GetLeft(element);
    var top = System.Windows.Controls.Canvas.GetTop(element);
    if (double.IsNaN(left)) left = 0;
    if (double.IsNaN(top)) top = 0;

    var bounds = VisualTreeHelper.GetDescendantBounds(element);
    var transformedBounds = element.RenderTransform.TransformBounds(bounds);

    if (element is not FrameworkElement fe)
      return (
        new Point(left + transformedBounds.Left, top + transformedBounds.Top),
        new Point(left + transformedBounds.Right, top + transformedBounds.Bottom) );
    var margin = fe.Margin;
    return (
      new Point(left + transformedBounds.Left - margin.Left,
        top + transformedBounds.Top - margin.Top),
      new Point(left + transformedBounds.Right + margin.Right,
        top + transformedBounds.Bottom + margin.Bottom) );
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
        var left = System.Windows.Controls.Canvas.GetLeft(ellipse);
        var top = System.Windows.Controls.Canvas.GetTop(ellipse);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        return new Point(left + ellipse.Width, top + ellipse.Height);
      }
      case Polygon polygon: {
        if (polygon.Points.Count == 0)
          return new Point();
        var maxX = polygon.Points.Max(p => p.X);
        var maxY = polygon.Points.Max(p => p.Y);
        return new Point(maxX, maxY);
      }
      case Polyline polyline: {
        if (polyline.Points.Count == 0)
          return new Point();
        var maxX = polyline.Points.Max(p => p.X);
        var maxY = polyline.Points.Max(p => p.Y);
        return new Point(maxX, maxY);
      }
      case TextBlock textBlock: {
        var bounds = VisualTreeHelper.GetDescendantBounds(textBlock);
        var transformedBounds = textBlock.RenderTransform.TransformBounds(bounds);
        var left = System.Windows.Controls.Canvas.GetLeft(textBlock);
        var top = System.Windows.Controls.Canvas.GetTop(textBlock);
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
        var left = System.Windows.Controls.Canvas.GetLeft(ellipse);
        var top = System.Windows.Controls.Canvas.GetTop(ellipse);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        return new Point(left, top);
      }
      case Polygon polygon: {
        if (polygon.Points.Count == 0)
          return new Point();
        var minX = polygon.Points.Min(p => p.X);
        var minY = polygon.Points.Min(p => p.Y);
        return new Point(minX, minY);
      }
      case Polyline polyline: {
        if (polyline.Points.Count == 0)
          return new Point();
        var minX = polyline.Points.Min(p => p.X);
        var minY = polyline.Points.Min(p => p.Y);
        return new Point(minX, minY);
      }
      case TextBlock textBlock: {
        var bounds = VisualTreeHelper.GetDescendantBounds(textBlock);
        var transformedBounds = textBlock.RenderTransform.TransformBounds(bounds);
        var left = System.Windows.Controls.Canvas.GetLeft(textBlock);
        var top = System.Windows.Controls.Canvas.GetTop(textBlock);
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
      throw new ArgumentException("Error.PointsNullOrEmpty".GetString());
    var cx = points.Average(p => p.X);
    var cy = points.Average(p => p.Y);
    return new Point(cx, cy);
  }

  private static List<LineWeightGroup>? _lineWeightCache;

  private static List<LineWeightGroup>? LoadLineWeightGroups()
  {
    if (_lineWeightCache != null)
      return _lineWeightCache;

    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
    var resourceName = "AutoCADTools.Presentation.Canvas.LineWeightData.LineWeightData.json";

    using (var stream = assembly.GetManifestResourceStream(resourceName)) {
      if (stream == null) return _lineWeightCache;
      using var reader = new System.IO.StreamReader(stream);
      var jsonString = reader.ReadToEnd();
      _lineWeightCache = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LineWeightGroup>>(jsonString);
    }

    return _lineWeightCache;
  }

  /// <summary>
  /// Returns line thickness (in pixels) based on scale and lineWeightId.
  /// Reads LineWeightData.json from embedded assembly resources.
  /// </summary>
  public static double GetLineThickness(double scale, int lineWeightId)
  {
    var groups = LoadLineWeightGroups();
    if (groups == null)
      return 3;

    foreach (var group in groups) {
      if (!( scale <= group.Scale )) continue;
      var lw = group.LineWeights.FirstOrDefault(x => x.Id == lineWeightId);
      return lw?.Thickness ?? 1;
    }

    return 1;
  }

  /// <summary>
  /// Calculates the bounding box of all UIElement children in a Canvas.
  /// </summary>
  public static void GenerateGeometryFromCanvas(
    System.Windows.Controls.Canvas? canvas,
    out double minX,
    out double minY,
    out double maxX,
    out double maxY,
    out double width,
    out double height)
  {
    minX = 0;
    minY = 0;
    maxX = 0;
    maxY = 0;
    width = 0;
    height = 0;

    if (canvas == null)
      return;

    var allPoints = new List<Point>();
    foreach (UIElement child in canvas.Children) {
      var (min, max) = child.GetMinMaxPoints();
      allPoints.Add(min);
      allPoints.Add(max);
    }

    if (allPoints.Count == 0)
      return;

    minX = allPoints.Min(p => p.X);
    maxX = allPoints.Max(p => p.X);
    minY = allPoints.Min(p => p.Y);
    maxY = allPoints.Max(p => p.Y);

    width = maxX - minX;
    height = maxY - minY;
  }

  /// <summary>
  /// Zooms and pans the ZoomBorder to fit all Canvas children with a 10% margin.
  /// </summary>
  public static void ZoomToFit(System.Windows.Controls.Canvas? canvas, object? zoomBorderObj)
  {
    if (canvas == null || zoomBorderObj == null)
      return;

    if (canvas.Children.Count == 0)
      return;

    if (zoomBorderObj is not ZoomBorder zoomBorder)
      return;

    canvas.UpdateLayout();

    GenerateGeometryFromCanvas(canvas, out var minX, out var minY, out _, out _, out var width,
      out var height);

    zoomBorder.Reset();
    var margin = 0.1 * Math.Min(width, height);
    width += 2 * margin;
    height += 2 * margin;

    if (width < 1e-9 || height < 1e-9)
      return;

    var zoom = Math.Min(zoomBorder.ActualWidth / width, zoomBorder.ActualHeight / height);
    zoomBorder.ZoomTo(zoom, 0, 0);
    zoomBorder.StartPan(minX, minY);

    var panX = 0.5 * zoomBorder.ActualWidth / zoom - 0.5 * width + margin;
    var panY = 0.5 * zoomBorder.ActualHeight / zoom - 0.5 * height + margin;
    zoomBorder.PanTo(panX, panY);
  }

  /// <summary>
  /// Zooms and pans the ZoomBorder to fit all Canvas children, with the zoom centered on the mouse position.
  /// </summary>
  public static void ZoomToFit(System.Windows.Input.MouseButtonEventArgs e, System.Windows.Controls.Canvas? canvas,
    object? zoomBorderObj)
  {
    if (canvas == null || zoomBorderObj == null)
      return;

    if (canvas.Children.Count == 0)
      return;

    if (zoomBorderObj is not ZoomBorder zoomBorder)
      return;

    canvas.UpdateLayout();

    GenerateGeometryFromCanvas(canvas, out var minX, out var minY, out _, out _, out var width,
      out var height);

    zoomBorder.Reset();
    var margin = 0.1 * Math.Min(width, height);
    width += 2 * margin;
    height += 2 * margin;

    if (width < 1e-9 || height < 1e-9)
      return;

    var zoom = Math.Min(zoomBorder.ActualWidth / width, zoomBorder.ActualHeight / height);
    var pointOnZoomBorder = e.GetPosition(zoomBorder);
    zoomBorder.ZoomTo(zoom, zoomBorder.ActualWidth / zoom, zoomBorder.ActualHeight / zoom);
    zoomBorder.StartPan(
      minX + 0.5 * width + pointOnZoomBorder.X / zoom - 0.5 * zoomBorder.ActualWidth / zoom - margin,
      minY + 0.5 * height + pointOnZoomBorder.Y / zoom - 0.5 * zoomBorder.ActualHeight / zoom - margin);
  }
}