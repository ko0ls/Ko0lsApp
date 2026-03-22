using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class Hatch2D
{
  public Hatch2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    PointCollection? points,
    double lineThickness,
    Brush? lineColor,
    Brush? fillColor,
    string text = "",
    int zIndex = 0)
  {
    if (canvas == null || scale < 1e-9 || points == null || points.Count < 3 || lineThickness < 1e-9 ||
        lineColor == null)
      return;

    _ = new Polygon2D(canvas, points, lineThickness, lineColor, fillColor, zIndex);

    if (string.IsNullOrEmpty(text)) return;
    var center = UtilsCanvas.CalculateCenter(points);
    var textFontSize = Hatch2DSetting.Instance.TextFontSize * scale;
    _ = new TextBlock2D(canvas, text, textFontSize, center, lineColor, zIndex: zIndex);
  }
}