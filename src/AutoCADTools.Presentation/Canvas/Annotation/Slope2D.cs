using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class Slope2D
{
  private double _arrowLength;
  private int _lineWeightId;
  private EnumArrowHeadStyle _arrowStyle;
  private Brush? _lineColor;
  private Brush? _textColor;
  private double _textFontSize;

  private System.Windows.Controls.Canvas? _canvas;
  private double _scale;
  private string _slope = string.Empty;
  private Point _position;
  private EnumSlopeDirection _slopeDirection;
  private double _angle;
  private bool _isFlip;
  private Vector _textDirection;
  private Point _arrowStartPoint;
  private Point _arrowEndPoint;
  private EnumArrowHeadStyle _arrowStyleStart;
  private EnumArrowHeadStyle _arrowStyleEnd;

  private void DrawCanvas()
  {
    _ = new Arrow2D(_canvas, _scale, _arrowStartPoint, _arrowEndPoint, EnumLineType.Solid, _arrowStyleStart, _arrowStyleEnd, _lineWeightId, _lineColor);

    // Text at position
    _ = new TextBlock2D(_canvas, _slope, _textFontSize, _position, _textColor, _angle);
  }

  private void InitGeometry()
  {
    // Note: _isFlip comes from the caller. If the span angle is outside [-90, 90),
    // the text would be upside-down, so we invert _isFlip to compensate.
    if (_angle >= 90 || _angle < -90)
    {
      _isFlip = !_isFlip;
    }

    Vector direction = UtilsVector.CreateVectorFromAngle(_angle);
    _textDirection = new Vector(direction.Y, -direction.X);

    if (_isFlip)
    {
      _textDirection.Negate();
    }

    _arrowStartPoint = _position - direction * 0.5 * _arrowLength - _textDirection * 0.5 * _textFontSize;
    _arrowEndPoint = _position + direction * 0.5 * _arrowLength - _textDirection * 0.5 * _textFontSize;

    if (_slopeDirection == EnumSlopeDirection.Left)
    {
      _arrowStyleStart = _arrowStyle;
    }
    else if (_slopeDirection == EnumSlopeDirection.Right)
    {
      _arrowStyleEnd = _arrowStyle;
    }
  }

  private void InitSetting(double scale)
  {
    var setting = Slope2DSetting.Instance;
    _arrowLength = setting.ArrowLength * scale;
    _lineWeightId = setting.LineWeightId;
    _arrowStyle = setting.ArrowStyle;
    _lineColor = setting.LineColor;
    _textColor = setting.TextColor;
    _textFontSize = setting.TextFontSize * scale;
  }

  public Slope2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    string slope,
    Point position,
    EnumSlopeDirection slopeDirection,
    double angle,
    bool isFlip)
  {
    if (canvas == null || scale < 1e-9 || !position.IsValid())
      return;

    _canvas = canvas;
    _scale = scale;
    _slope = slope;
    _position = position;
    _slopeDirection = slopeDirection;
    _angle = angle;
    _isFlip = isFlip;

    InitSetting(scale);
    InitGeometry();
    DrawCanvas();
  }
}
