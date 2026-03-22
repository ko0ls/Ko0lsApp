using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Utils;
using Line = System.Windows.Shapes.Line;

namespace AutoCADTools.Presentation.Canvas.Shapes;

public class Line2D : ShapeBase
{
  private readonly Line? _line;
  private readonly double _lineThickness;
  private readonly Brush? _lineColor;

  public Line2D(
    System.Windows.Controls.Canvas? canvas,
    Point startPoint,
    Point endPoint,
    EnumLineType lineType,
    double lineThickness,
    Brush? lineColor,
    double angle = 0,
    int zIndex = 0)
  {
    if (canvas == null)
      return;

    if (!startPoint.IsValid() || !endPoint.IsValid())
      return;

    if (startPoint.DistanceTo(endPoint) < 1e-9)
      return;

    if (lineThickness < 1e-9)
      return;

    if (lineColor == null)
      return;

    _lineThickness = lineThickness;
    _lineColor = lineColor;

    _line = new Line
    {
      X1 = startPoint.X,
      Y1 = startPoint.Y,
      X2 = endPoint.X,
      Y2 = endPoint.Y,
      Stroke = _lineColor,
      StrokeThickness = _lineThickness,
      StrokeDashArray = GetStrokeDashArray(lineType)
    };

    if (Math.Abs(angle) > 1e-9) {
      var center = UtilsPoint.MidPoint(startPoint, endPoint);
      _line.RenderTransform = new RotateTransform(angle, center.X, center.Y);
    }

    System.Windows.Controls.Canvas.SetZIndex(_line, zIndex);
    canvas.Children.Add(_line);
  }

  public Line GetLine() => _line!;

  public override void MakeHighLight()
  {
    if (_line == null) return;
    _line.StrokeThickness = 5 * _lineThickness;
    _line.Stroke = Brushes.Red;
    System.Windows.Controls.Canvas.SetZIndex(_line, 1);
  }

  public override void ResetHighLight()
  {
    if (_line == null) return;
    _line.StrokeThickness = _lineThickness;
    _line.Stroke = _lineColor;
    System.Windows.Controls.Canvas.SetZIndex(_line, 0);
  }

  public override bool CheckMoveOver(Point point)
  {
    if (_line == null || _line.RenderedGeometry == null)
      return false;

    var pen = new Pen
    {
      Thickness = 5 * _lineThickness
    };

    return _line.RenderedGeometry.StrokeContains(pen, point);
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
