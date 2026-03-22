using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AutoCADTools.Presentation.Canvas.Shapes;

public class Polygon2D : ShapeBase
{
  private readonly Polygon? _polygon;
  private readonly double _lineThickness;
  private readonly Brush? _lineColor;

  public Polygon2D(
    System.Windows.Controls.Canvas? canvas,
    PointCollection? points,
    double lineThickness,
    Brush? lineColor,
    Brush? fillColor = null,
    int zIndex = 0)
  {
    if (canvas == null)
    {
      return;
    }

    if (points == null || points.Count == 0)
    {
      return;
    }

    if (lineThickness < 1e-9)
    {
      return;
    }

    if (lineColor == null)
    {
      return;
    }

    _lineThickness = lineThickness;
    _lineColor = lineColor;

    _polygon = new Polygon();
    _polygon.Stroke = lineColor;
    _polygon.StrokeThickness = lineThickness;
    _polygon.Fill = fillColor;
    _polygon.Points = points;

    System.Windows.Controls.Canvas.SetZIndex(_polygon, zIndex);
    canvas.Children.Add(_polygon);
  }

  public override void MakeHighLight()
  {
    if (_polygon == null)
    {
      return;
    }

    _polygon.StrokeThickness = 5 * _lineThickness;
    _polygon.Stroke = Brushes.Red;
    System.Windows.Controls.Canvas.SetZIndex(_polygon, 1);
  }

  public override void ResetHighLight()
  {
    if (_polygon == null)
    {
      return;
    }

    _polygon.StrokeThickness = _lineThickness;
    _polygon.Stroke = _lineColor;
    System.Windows.Controls.Canvas.SetZIndex(_polygon, 0);
  }

  public override bool CheckMoveOver(Point point)
  {
    if (_polygon == null)
    {
      return false;
    }

    return _polygon.RenderedGeometry.FillContains(point);
  }
}
