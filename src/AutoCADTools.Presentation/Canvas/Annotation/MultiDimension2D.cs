using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class MultiDimension2D
{
  public MultiDimension2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    PointCollection? points,
    Point placePoint,
    Vector dimDirection,
    EnumDimensionLevel dimLevel,
    double obliqueStart = 90,
    double obliqueEnd = 90,
    string assignTextValue = "",
    Dimension2DSetting? dimSetting = null,
    int zIndex = 0)
  {
    if (canvas == null || scale < 1e-9 || points == null || points.Count < 2 || !placePoint.IsValid() ||
        !dimDirection.IsValid())
      return;

    for (var i = 0 ; i < points.Count - 1 ; i++) {
      var currentPoint = points[i];
      var nextPoint = points[i + 1];
      var isNonStandardRight = i > 0;
      _ = new Dimension2D(canvas, scale, currentPoint, nextPoint, placePoint, dimDirection, dimLevel,
        obliqueStart, obliqueEnd, assignTextValue, isNonStandardRight: isNonStandardRight, dimSetting: dimSetting,
        zIndex: zIndex);
    }
  }
}