using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class Grid2D : AnnotationBase
{
  private readonly System.Windows.Controls.Canvas? _canvas;
  private readonly Line2D? _gridLine;

  private readonly string _name = string.Empty;
  private readonly double _ellipseWidth;
  private readonly double _ellipseHeight;
  private readonly double _textFontSize;
  private readonly Point _startPoint;
  private readonly Point _endPoint;
  private readonly Vector _direction;
  private readonly double _lineThickness;
  private readonly Brush? _lineColor;
  private readonly Brush? _symbolColor;
  private readonly EnumGridSymbolStyle _symbolStyleAtStart;
  private readonly EnumGridSymbolStyle _symbolStyleAtEnd;
  private readonly double _startOffset;
  private readonly double _endOffset;
  private readonly double _symbolAngle;
  private readonly int _zIndex;

  public override void MakeHighLight()
  {
    _gridLine?.MakeHighLight();
  }

  public override void ResetHighLight()
  {
    _gridLine?.ResetHighLight();
  }

  public override bool CheckMoveOver(Point point)
  {
    return _gridLine?.CheckMoveOver(point) ?? false;
  }

  protected override void OnIsMoveOverChanged(bool value)
  {
    if (value)
      _gridLine?.MakeHighLight();
    else
      _gridLine?.ResetHighLight();
  }

  protected override void OnIsSelectedChanged(bool value)
  {
    if (value)
      _gridLine?.MakeHighLight();
    else
      _gridLine?.ResetHighLight();
  }

  private void DrawSymbolAtStart()
  {
    switch (_symbolStyleAtStart) {
      case EnumGridSymbolStyle.Circle: {
        var ellipseStartPoint = _startPoint - _direction * ( 0.5 * _ellipseWidth + _startOffset );
        _ = new Ellipse2D(
          _canvas,
          ellipseStartPoint,
          _ellipseWidth,
          _ellipseWidth,
          _lineThickness,
          _symbolColor,
          zIndex: _zIndex);
        if (_symbolColor != null) {
          _ = new TextBlock2D(_canvas, _name, _textFontSize, ellipseStartPoint, _symbolColor, zIndex: _zIndex);
        }

        break;
      }
      case EnumGridSymbolStyle.Oval: {
        var ellipseStartPoint = _startPoint - _direction * ( 0.5 * _ellipseWidth + _startOffset );
        _ = new Ellipse2D(
          _canvas,
          ellipseStartPoint,
          _ellipseWidth,
          _ellipseHeight,
          _lineThickness,
          _symbolColor,
          angle: _symbolAngle,
          zIndex: _zIndex);
        if (_symbolColor != null) {
          _ = new TextBlock2D(_canvas, _name, _textFontSize, ellipseStartPoint, _symbolColor, zIndex: _zIndex);
        }

        break;
      }
      case EnumGridSymbolStyle.TextOnly: {
        var ellipseStartPoint = _startPoint - _direction * ( 0.5 * _ellipseWidth + _startOffset );
        if (_symbolColor != null) {
          _ = new TextBlock2D(_canvas, _name, _textFontSize, ellipseStartPoint, _symbolColor, zIndex: _zIndex);
        }

        break;
      }
      case EnumGridSymbolStyle.None:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private void DrawSymbolAtEnd()
  {
    switch (_symbolStyleAtEnd) {
      case EnumGridSymbolStyle.Circle: {
        var ellipseEndPoint = _endPoint + _direction * ( 0.5 * _ellipseWidth + _endOffset );
        _ = new Ellipse2D(
          _canvas,
          ellipseEndPoint,
          _ellipseWidth,
          _ellipseWidth,
          _lineThickness,
          _symbolColor,
          zIndex: _zIndex);
        if (_symbolColor != null) {
          _ = new TextBlock2D(_canvas, _name, _textFontSize, ellipseEndPoint, _symbolColor, zIndex: _zIndex);
        }

        break;
      }
      case EnumGridSymbolStyle.Oval: {
        var ellipseEndPoint = _endPoint + _direction * ( 0.5 * _ellipseWidth + _endOffset );
        _ = new Ellipse2D(
          _canvas,
          ellipseEndPoint,
          _ellipseWidth,
          _ellipseHeight,
          _lineThickness,
          _symbolColor,
          angle: _symbolAngle,
          zIndex: _zIndex);
        if (_symbolColor != null) {
          _ = new TextBlock2D(_canvas, _name, _textFontSize, ellipseEndPoint, _symbolColor, zIndex: _zIndex);
        }

        break;
      }
      case EnumGridSymbolStyle.TextOnly: {
        var ellipseEndPoint = _endPoint + _direction * ( 0.5 * _ellipseWidth + _endOffset );
        if (_symbolColor != null) {
          _ = new TextBlock2D(_canvas, _name, _textFontSize, ellipseEndPoint, _symbolColor, zIndex: _zIndex);
        }

        break;
      }
      case EnumGridSymbolStyle.None:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  public Grid2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    string name,
    Point startPoint,
    Point endPoint,
    EnumGridSymbolStyle symbolStyleAtStart,
    EnumGridSymbolStyle symbolStyleAtEnd,
    double startOffset = 0,
    double endOffset = 0,
    int zIndex = 0)
  {
    if (canvas == null)
      return;

    if (scale < 1e-9 || !startPoint.IsValid() || !endPoint.IsValid() || startPoint.DistanceTo(endPoint) < 1e-9)
      return;

    _canvas = canvas;
    _name = name;
    _symbolStyleAtStart = symbolStyleAtStart;
    _symbolStyleAtEnd = symbolStyleAtEnd;
    _startOffset = startOffset * scale;
    _endOffset = endOffset * scale;
    _startPoint = startPoint;
    _endPoint = endPoint;
    _zIndex = zIndex;

    var setting = Grid2DSetting.Instance;
    _textFontSize = setting.TextFontSize * scale;
    _ellipseWidth = setting.EllipseWidth * scale;
    _ellipseHeight = setting.EllipseHeight * scale;
    _lineColor = setting.LineColor;
    _symbolColor = setting.SymbolColor;
    var lineWeightId = setting.LineWeightId;

    _direction = UtilsVector.CreateVector(startPoint, endPoint);
    _lineThickness = UtilsCanvas.GetLineThickness(scale, lineWeightId);
    _symbolAngle = Math.Atan2(_direction.Y, _direction.X) * ( 180.0 / Math.PI );

    var gridLineStartPoint = _startPoint - _startOffset * _direction;
    var gridLineEndPoint = _endPoint + _endOffset * _direction;
    _gridLine = new Line2D(
      _canvas,
      gridLineStartPoint,
      gridLineEndPoint,
      EnumLineType.DashDot,
      _lineThickness,
      _lineColor,
      zIndex: _zIndex);

    DrawSymbolAtStart();
    DrawSymbolAtEnd();
  }
}