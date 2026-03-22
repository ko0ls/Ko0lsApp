using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class ArrowWithText2D
{
  private readonly System.Windows.Controls.Canvas? _canvas;
  private readonly double _scale;
  private readonly Point _otherPoint;
  private readonly Point _endPoint;
  private readonly int _lineWeightId;
  private readonly EnumLineType _lineType;
  private readonly Brush? _color;
  private readonly string _textValue = string.Empty;
  private readonly double _angle;
  private readonly EnumTextAlignment _textAlignment;
  private readonly int _zIndex;
  private const double TextFontSize = 5;

  public ArrowWithText2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    Point startPoint,
    Point endPoint,
    Point pointOther,
    EnumLineType lineType,
    EnumArrowHeadStyle symbolStyleAtStart,
    EnumArrowHeadStyle symbolStyleAtEnd,
    int lineWeightId,
    Brush? color,
    string textValue = "",
    double angle = 0,
    EnumTextAlignment textAlignment = EnumTextAlignment.BottomLeft,
    int zIndex = 0)
  {
    if (canvas == null)
      return;

    _canvas = canvas;
    _scale = scale;
    _otherPoint = pointOther;
    _endPoint = endPoint;
    _lineWeightId = lineWeightId;
    _lineType = lineType;
    _color = color;
    _textValue = textValue;
    _angle = angle;
    _textAlignment = textAlignment;
    _zIndex = zIndex;

    var strokeThickness = UtilsCanvas.GetLineThickness(_scale, _lineWeightId);
    _ = new Arrow2D(canvas, scale, startPoint, endPoint, lineType, symbolStyleAtStart, symbolStyleAtEnd, lineWeightId,
      color, zIndex: _zIndex);
    DrawLine(strokeThickness);
    DrawText();
  }

  private void DrawLine(double strokeThickness)
  {
    _ = new Line2D(_canvas, _endPoint, _otherPoint, _lineType, strokeThickness, _color, zIndex: _zIndex);
  }

  private void DrawText()
  {
    var position = _otherPoint;
    _ = Math.Abs(_angle) < 1e-9
      ? new TextNote2D(_canvas, _scale, _textValue, TextFontSize, position, _color, textAlignment: _textAlignment)
      : new TextNote2D(_canvas, _scale, _textValue, TextFontSize, position, _color, _angle,
        textAlignment: _textAlignment);
  }
}