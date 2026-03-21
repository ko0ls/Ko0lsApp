using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Utils;
using Ellipse = System.Windows.Shapes.Ellipse;

namespace AutoCADTools.Presentation.Canvas.Shapes;

public class Ellipse2D : ShapeBase
{
  private readonly Ellipse? _ellipse;
  private readonly double _lineThickness;
  private readonly Brush? _lineColor;
  private readonly Brush? _fillColor;

  public Ellipse2D(
    System.Windows.Controls.Canvas? canvas,
    Point center,
    double width,
    double height,
    double lineThickness,
    Brush? lineColor,
    Brush? fillColor = null,
    double angle = 0,
    int zIndex = 0)
  {
    if (canvas == null)
      return;

    if (!center.IsValid())
      return;

    if (width < 1e-9 || height < 1e-9)
      return;

    if (lineThickness < 1e-9)
      return;

    if (lineColor == null)
      return;

    _lineThickness = lineThickness;
    _lineColor = lineColor;
    _fillColor = fillColor;

    _ellipse = new Ellipse
    {
      Width = width,
      Height = height,
      Stroke = _lineColor,
      StrokeThickness = _lineThickness,
      Fill = _fillColor ?? Brushes.Transparent
    };

    System.Windows.Controls.Canvas.SetLeft(_ellipse, center.X - width / 2);
    System.Windows.Controls.Canvas.SetTop(_ellipse, center.Y - height / 2);

    if (Math.Abs(angle) > 1e-9) {
      _ellipse.RenderTransform = new RotateTransform(angle, width / 2, height / 2);
    }

    System.Windows.Controls.Canvas.SetZIndex(_ellipse, zIndex);
    canvas.Children.Add(_ellipse);
  }

  public Ellipse GetEllipse() => _ellipse!;

  public override void MakeHighLight()
  {
    if (_ellipse == null) return;
    _ellipse.StrokeThickness = 5 * _lineThickness;
    _ellipse.Stroke = Brushes.Red;
    System.Windows.Controls.Canvas.SetZIndex(_ellipse, 1);
  }

  public override void ResetHighLight()
  {
    if (_ellipse == null) return;
    _ellipse.StrokeThickness = _lineThickness;
    _ellipse.Stroke = _lineColor;
    System.Windows.Controls.Canvas.SetZIndex(_ellipse, 0);
  }

  public override bool CheckMoveOver(Point point)
  {
    if (_ellipse == null || _ellipse.RenderedGeometry == null)
      return false;

    var pen = new Pen
    {
      Thickness = 5 * _lineThickness
    };

    return _ellipse.RenderedGeometry.StrokeContains(pen, point);
  }
}
