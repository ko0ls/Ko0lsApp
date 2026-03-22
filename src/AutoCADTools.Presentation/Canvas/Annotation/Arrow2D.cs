using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class Arrow2D
{
  private readonly System.Windows.Controls.Canvas? _canvas;
  private readonly Point _startPoint;
  private readonly Point _endPoint;
  private readonly EnumLineType _lineType;
  private readonly EnumArrowHeadStyle _symbolStyleAtStart;
  private readonly EnumArrowHeadStyle _symbolStyleAtEnd;
  private readonly Brush? _color;
  private readonly int _zIndex;
  private readonly double _dotWidth;
  private readonly double _diagonalLength;
  private readonly double _strokeThickness;
  private readonly Vector _direction;
  private readonly double _scale;
  private readonly Vector _directionLeft;
  private readonly Vector _directionRight;

  private void DrawLine()
  {
    _ = new Line2D(
      _canvas,
      _startPoint,
      _endPoint,
      _lineType,
      _strokeThickness,
      _color,
      zIndex: _zIndex);
  }

  private void DrawArrowHeadStart()
  {
    switch (_symbolStyleAtStart) {
      case EnumArrowHeadStyle.Dot:
        _ = new Ellipse2D(
          _canvas,
          _startPoint,
          _dotWidth,
          _dotWidth,
          _strokeThickness,
          _color,
          _color,
          zIndex: _zIndex);
        break;
      case EnumArrowHeadStyle.Diagonal: {
        var diagonalStartSp = _startPoint - _direction * 0.5 * _diagonalLength;
        var diagonalStartEp = _startPoint + _direction * 0.5 * _diagonalLength;
        _ = new Line2D(
          _canvas,
          diagonalStartSp,
          diagonalStartEp,
          EnumLineType.Solid,
          _strokeThickness,
          _color,
          -45,
          zIndex: _zIndex);
        break;
      }
      case EnumArrowHeadStyle.ArrowFilled: {
        var arrowStartP1 = _startPoint + _direction * 0.5 * _diagonalLength + _directionLeft * 0.25 * _diagonalLength;
        var arrowStartP2 = _startPoint;
        var arrowStartP3 = _startPoint + _direction * 0.5 * _diagonalLength + _directionRight * 0.25 * _diagonalLength;
        _ = new Hatch2D(
          _canvas,
          _scale,
          new PointCollection { arrowStartP1, arrowStartP2, arrowStartP3 },
          _strokeThickness,
          _color,
          _color,
          zIndex: _zIndex);
        break;
      }
      case EnumArrowHeadStyle.ArrowOpen: {
        var arrowStartP1 = _startPoint + _direction * 0.5 * _diagonalLength + _directionLeft * 0.25 * _diagonalLength;
        var arrowStartP2 = _startPoint;
        var arrowStartP3 = _startPoint + _direction * 0.5 * _diagonalLength + _directionRight * 0.25 * _diagonalLength;
        _ = new Polyline2D(
          _canvas,
          new PointCollection { arrowStartP1, arrowStartP2, arrowStartP3 },
          EnumLineType.Solid,
          _strokeThickness,
          _color,
          zIndex: _zIndex);
        break;
      }
      case EnumArrowHeadStyle.None:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private void DrawArrowHeadEnd()
  {
    switch (_symbolStyleAtEnd) {
      case EnumArrowHeadStyle.Dot:
        _ = new Ellipse2D(
          _canvas,
          _endPoint,
          _dotWidth,
          _dotWidth,
          _strokeThickness,
          _color,
          _color,
          zIndex: _zIndex);
        break;
      case EnumArrowHeadStyle.Diagonal: {
        var diagonalEndSp = _endPoint - _direction * 0.5 * _diagonalLength;
        var diagonalEndEp = _endPoint + _direction * 0.5 * _diagonalLength;
        _ = new Line2D(
          _canvas,
          diagonalEndSp,
          diagonalEndEp,
          EnumLineType.Solid,
          _strokeThickness,
          _color,
          -45,
          zIndex: _zIndex);
        break;
      }
      case EnumArrowHeadStyle.ArrowFilled: {
        var arrowEndP1 = _endPoint - _direction * 0.5 * _diagonalLength + _directionLeft * 0.25 * _diagonalLength;
        var arrowEndP2 = _endPoint;
        var arrowEndP3 = _endPoint - _direction * 0.5 * _diagonalLength + _directionRight * 0.25 * _diagonalLength;
        _ = new Hatch2D(
          _canvas,
          _scale,
          new PointCollection { arrowEndP1, arrowEndP2, arrowEndP3 },
          _strokeThickness,
          _color,
          _color,
          zIndex: _zIndex);
        break;
      }
      case EnumArrowHeadStyle.ArrowOpen: {
        var arrowEndP1 = _endPoint - _direction * 0.5 * _diagonalLength + _directionLeft * 0.25 * _diagonalLength;
        var arrowEndP2 = _endPoint;
        var arrowEndP3 = _endPoint - _direction * 0.5 * _diagonalLength + _directionRight * 0.25 * _diagonalLength;
        _ = new Polyline2D(
          _canvas,
          new PointCollection { arrowEndP1, arrowEndP2, arrowEndP3 },
          EnumLineType.Solid,
          _strokeThickness,
          _color,
          zIndex: _zIndex);
        break;
      }
      case EnumArrowHeadStyle.None:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  public Arrow2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    Point startPoint,
    Point endPoint,
    EnumLineType lineType,
    EnumArrowHeadStyle symbolStyleAtStart,
    EnumArrowHeadStyle symbolStyleAtEnd,
    int lineWeightId,
    Brush? color,
    int zIndex = 0)
  {
    if (canvas == null)
      return;

    if (scale < 1e-9 || !startPoint.IsValid() || !endPoint.IsValid() || startPoint.DistanceTo(endPoint) < 1e-9 ||
        lineWeightId == 0)
      return;

    _canvas = canvas;
    _startPoint = startPoint;
    _endPoint = endPoint;
    _lineType = lineType;
    _symbolStyleAtStart = symbolStyleAtStart;
    _symbolStyleAtEnd = symbolStyleAtEnd;
    _color = color;
    _zIndex = zIndex;

    var setting = Arrow2DSetting.Instance;
    _dotWidth = setting.DotWidth * scale;
    _diagonalLength = setting.DiagonalLength * scale;
    _strokeThickness = UtilsCanvas.GetLineThickness(scale, lineWeightId);
    _scale = scale;

    _direction = UtilsVector.CreateVector(startPoint, endPoint);
    _directionLeft = new Vector(_direction.Y, -_direction.X);
    _directionRight = new Vector(-_direction.Y, _direction.X);

    DrawLine();
    DrawArrowHeadStart();
    DrawArrowHeadEnd();
  }
}