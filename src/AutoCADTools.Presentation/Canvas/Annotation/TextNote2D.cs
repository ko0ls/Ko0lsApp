using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class TextNote2D
{
  public TextNote2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    string text,
    double textFontSize,
    Point position,
    Brush? textColor,
    double angle = 0,
    double margin = 0,
    EnumTextAlignment textAlignment = EnumTextAlignment.CenterMiddle,
    int zIndex = 0)
  {
    if (canvas == null || scale < 1e-9 || string.IsNullOrEmpty(text) || textFontSize < 1e-9 || !position.IsValid() || textColor == null)
      return;

    _ = new TextBlock2D(canvas, text, textFontSize * scale, position, textColor, angle, margin * scale, textAlignment, zIndex);
  }
}
