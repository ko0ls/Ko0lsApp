using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Settings;
using Polyline = System.Windows.Shapes.Polyline;

namespace AutoCADTools.Presentation.Canvas.Shapes;

public class Polyline2D : ShapeBase
{
  private readonly Polyline? _polyline;
  private readonly double _lineThickness;
  private readonly Brush? _lineColor;

  public Polyline2D(
    System.Windows.Controls.Canvas? canvas,
    PointCollection points,
    EnumLineType lineType,
    double lineThickness,
    Brush? lineColor,
    int zIndex = 0)
  {
    if (canvas == null)
      return;

    if (points == null || points.Count < 2)
      return;

    if (lineThickness < 1e-9)
      return;

    if (lineColor == null)
      return;

    _lineThickness = lineThickness;
    _lineColor = lineColor;

    _polyline = new Polyline
    {
      Points = points,
      Stroke = _lineColor,
      StrokeThickness = _lineThickness,
      StrokeDashArray = GetStrokeDashArray(lineType)
    };

    System.Windows.Controls.Canvas.SetZIndex(_polyline, zIndex);
    canvas.Children.Add(_polyline);
  }

  public Polyline GetPolyline() => _polyline!;

  public override void MakeHighLight()
  {
    if (_polyline == null) return;
    _polyline.StrokeThickness = 5 * _lineThickness;
    _polyline.Stroke = Brushes.Red;
    System.Windows.Controls.Canvas.SetZIndex(_polyline, 1);
  }

  public override void ResetHighLight()
  {
    if (_polyline == null) return;
    _polyline.StrokeThickness = _lineThickness;
    _polyline.Stroke = _lineColor;
    System.Windows.Controls.Canvas.SetZIndex(_polyline, 0);
  }

  public override bool CheckMoveOver(Point point)
  {
    if (_polyline == null || _polyline.RenderedGeometry == null)
      return false;

    var pen = new Pen
    {
      Thickness = 5 * _lineThickness
    };

    return _polyline.RenderedGeometry.StrokeContains(pen, point);
  }

  private static DoubleCollection GetStrokeDashArray(EnumLineType lineType)
  {
    return lineType switch
    {
      EnumLineType.Dash => new DoubleCollection([4, 2, 4, 2]),
      EnumLineType.DashDot => new DoubleCollection([12, 3, 3, 3]),
      _ => new DoubleCollection()
    };
  }
}
