using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class Dimension2D
{
  private readonly System.Windows.Controls.Canvas? _canvas;
  private readonly double _scale;
  private Vector _dimDirection;
  private Vector _dimDirectionLeft;
  private Vector _dimLineDirectionStart;
  private Vector _dimLineDirectionEnd;
  private double _dimLength;
  private int _lineWeightId;
  private double _lineThickness;
  private Point _startPoint;
  private Point _endPoint;
  private EnumDimensionLevel _dimLevel;
  private double _dimObliqueStart;
  private double _dimObliqueEnd;
  private Point _placePoint;
  private bool _isStandard;

  private Brush? _dimLineColor;
  private Point _startDimLine;
  private Point _endDimLine;
  private Point _centerDimLine;
  private EnumArrowHeadStyle _tickMarkType;
  private double _dimensionLineExtension;
  private double _witnessLineExtension;
  private double _witnessLineGapToElement;
  private double _snapDistance;

  private Brush? _textColor;
  private double _textOffset;
  private double _textLeaderOffset;
  private double _textFontSize;
  private int _roundingDigit;
  private EnumTextPlacementVertical _textPlacementVertical;
  private EnumTextPlacementHorizontal _textPlacementHorizontal;
  private double _textAngle;
  private Point _textLocation;
  private EnumTextPlacement _textPlacement;
  private EnumTextAlignment _textAlignment;
  private EnumTextAlignment _textFieldAlignment;
  private string _assignTextValue = string.Empty;
  private int _dimTextLength;
  private string _textField = string.Empty;
  private bool _isNonStandardRight;
  private Dimension2DSetting? _dimSetting;
  private int _zIndex;

  private void DrawDimLine()
  {
    _ = new Arrow2D(_canvas, _scale, _startDimLine, _endDimLine, EnumLineType.Solid, _tickMarkType, _tickMarkType,
      _lineWeightId, _dimLineColor, zIndex: _zIndex);

    if (_dimensionLineExtension > 1e-9) {
      _ = new Arrow2D(_canvas, _scale, _startDimLine - _dimDirection * _dimensionLineExtension, _startDimLine,
        EnumLineType.Solid,
        EnumArrowHeadStyle.None, EnumArrowHeadStyle.None, _lineWeightId, _dimLineColor, zIndex: _zIndex);
      _ = new Arrow2D(_canvas, _scale, _endDimLine, _endDimLine + _dimDirection * _dimensionLineExtension,
        EnumLineType.Solid,
        EnumArrowHeadStyle.None, EnumArrowHeadStyle.None, _lineWeightId, _dimLineColor, zIndex: _zIndex);
    }

    var startWitnessLineSP = _startDimLine + _dimLineDirectionStart * _witnessLineExtension;
    var startWitnessLineEP = _startDimLine;
    var endWitnessLineSP = _endDimLine + _dimLineDirectionEnd * _witnessLineExtension;
    var endWitnessLineEP = _endDimLine;
    _ = new Line2D(_canvas, startWitnessLineSP, startWitnessLineEP, EnumLineType.Solid, _lineThickness, _dimLineColor,
      zIndex: _zIndex);
    _ = new Line2D(_canvas, endWitnessLineSP, endWitnessLineEP, EnumLineType.Solid, _lineThickness, _dimLineColor,
      zIndex: _zIndex);

    if (_witnessLineGapToElement > _startPoint.DistanceTo(_startDimLine)) {
      _witnessLineGapToElement = _startPoint.DistanceTo(_startDimLine);
    }

    var startWitnessLineFromElementSP = _startPoint + _dimLineDirectionStart * _witnessLineGapToElement;
    var startWitnessLineFromElementEP = _startDimLine;
    var endWitnessLineFromElementSP = _endPoint + _dimLineDirectionEnd * _witnessLineGapToElement;
    var endWitnessLineFromElementEP = _endDimLine;

    _ = new Line2D(_canvas, startWitnessLineFromElementSP, startWitnessLineFromElementEP, EnumLineType.Solid,
      _lineThickness, _dimLineColor, zIndex: _zIndex);
    _ = new Line2D(_canvas, endWitnessLineFromElementSP, endWitnessLineFromElementEP, EnumLineType.Solid,
      _lineThickness, _dimLineColor, zIndex: _zIndex);
  }

  private void DrawText()
  {
    var textValue = _assignTextValue == string.Empty
      ? _dimLength.ToString(CultureInfo.InvariantCulture)
      : _assignTextValue;
    _ = new TextBlock2D(_canvas, textValue, _textFontSize, _textLocation, _textColor, _textAngle, _textOffset,
      _textAlignment, zIndex: _zIndex);

    if (!string.IsNullOrEmpty(_textField)) {
      _ = new TextBlock2D(_canvas, _textField, _textFontSize, _textLocation, _textColor, _textAngle, _textOffset,
        _textFieldAlignment, zIndex: _zIndex);
    }
  }

  private void DrawNonstandardDimension()
  {
    switch (_textPlacement) {
      case EnumTextPlacement.BesideDim: {
        Point textLeaderStart;
        Point textLeaderEnd;
        if (_isNonStandardRight) {
          textLeaderStart = _endDimLine;
          textLeaderEnd = textLeaderStart + _dimDirection * 4 * _dimTextLength * _scale;
        }
        else {
          textLeaderStart = _startDimLine;
          textLeaderEnd = textLeaderStart - _dimDirection * 4 * _dimTextLength * _scale;
        }

        _ = new Line2D(_canvas, textLeaderStart, textLeaderEnd, EnumLineType.Solid, _lineThickness, _dimLineColor,
          zIndex: _zIndex);

        _textLocation = UtilsPoint.MidPoint(textLeaderStart, textLeaderEnd);
        string textValue = _assignTextValue == string.Empty
          ? _dimLength.ToString(CultureInfo.InvariantCulture)
          : _assignTextValue;
        _ = new TextBlock2D(_canvas, textValue, _textFontSize, _textLocation, _textColor, _textAngle, 0, _textAlignment,
          zIndex: _zIndex);
        break;
      }
      case EnumTextPlacement.OverDimWithLeader: {
        Point textLeaderStart = _centerDimLine;
        Point? textLeaderElbow;
        Point? textLeaderEnd;
        if (_isNonStandardRight) {
          textLeaderElbow = _centerDimLine + _dimDirectionLeft * _textLeaderOffset +
                            _dimDirection * 0.5 * _textLeaderOffset;
          textLeaderEnd = textLeaderElbow + _dimDirection * _textLeaderOffset;
        }
        else {
          textLeaderElbow = _centerDimLine + _dimDirectionLeft * _textLeaderOffset -
                            _dimDirection * 0.5 * _textLeaderOffset;
          textLeaderEnd = textLeaderElbow - _dimDirection * _textLeaderOffset;
        }

        _ = new Polyline2D(_canvas,
          new PointCollection { textLeaderStart, textLeaderElbow!.Value, textLeaderEnd!.Value }, EnumLineType.Solid,
          _lineThickness, _dimLineColor, zIndex: _zIndex);

        _textLocation = UtilsPoint.MidPoint(textLeaderElbow.Value, textLeaderEnd.Value);
        string textValue = _assignTextValue == string.Empty
          ? _dimLength.ToString(CultureInfo.InvariantCulture)
          : _assignTextValue;
        _ = new TextBlock2D(_canvas, textValue, _textFontSize, _textLocation, _textColor, _textAngle, 0, _textAlignment,
          zIndex: _zIndex);
        break;
      }
      case EnumTextPlacement.OverDimNoLeader:
        _textOffset = 2 * _textOffset;
        DrawText();
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private bool InitGeometry()
  {
    _lineThickness = UtilsCanvas.GetLineThickness(_scale, _lineWeightId);
    var dimAngle = _dimDirection.GetAngle();

    _dimLineDirectionStart = UtilsVector.CreateVectorFromAngle(dimAngle + _dimObliqueStart);
    _dimLineDirectionEnd = UtilsVector.CreateVectorFromAngle(dimAngle + _dimObliqueEnd);

    var absAngle = Math.Abs(dimAngle);
    if (absAngle is >= 90 or < -90) {
      ( _startPoint, _endPoint ) = ( _endPoint, _startPoint );
      _dimDirection.Negate();
    }

    _dimDirectionLeft = _textPlacementVertical switch {
      EnumTextPlacementVertical.Above or EnumTextPlacementVertical.Centered => _dimDirection.Rotate(-90),
      EnumTextPlacementVertical.Bellow => _dimDirection.Rotate(90),
      _ => _dimDirectionLeft
    };

    var startPointIntersection =
      UtilsPoint.Intersection(_startPoint, _dimLineDirectionStart, _placePoint, _dimDirection);
    var endPointIntersection = UtilsPoint.Intersection(_endPoint, _dimLineDirectionEnd, _placePoint, _dimDirection);

    if (startPointIntersection == null || endPointIntersection == null)
      return false;

    _dimLength =
      Math.Round(UtilsPoint.DistanceToLineUnbound(_endPoint, startPointIntersection.Value, _dimDirectionLeft),
        _roundingDigit);

    if (_dimLength < 1e-9)
      return false;

    if (!_startPoint.IsAlmostEqualTo(startPointIntersection.Value)) {
      _dimLineDirectionStart = UtilsVector.CreateVector(_startPoint, startPointIntersection.Value);
    }

    if (!_endPoint.IsAlmostEqualTo(endPointIntersection.Value)) {
      _dimLineDirectionEnd = UtilsVector.CreateVector(_endPoint, endPointIntersection.Value);
    }

    var angleRadStart = Math.Abs(Math.Abs(_dimObliqueStart) - 90) * Math.PI / 180.0;
    var angleRadEnd = Math.Abs(Math.Abs(_dimObliqueEnd) - 90) * Math.PI / 180.0;
    _startDimLine = startPointIntersection.Value +
                    _dimLineDirectionStart * (int) _dimLevel * ( _snapDistance / Math.Cos(angleRadStart) );
    _endDimLine = endPointIntersection.Value +
                  _dimLineDirectionEnd * (int) _dimLevel * ( _snapDistance / Math.Cos(angleRadEnd) );
    _centerDimLine = UtilsPoint.MidPoint(_startDimLine, _endDimLine);

    _dimTextLength = _dimLength.ToString(CultureInfo.InvariantCulture).Length;
    _isStandard = ( _dimLength / _scale / _dimTextLength ) >= 2;

    _textAngle = Math.Atan2(_dimDirection.Y, _dimDirection.X) * ( 180.0 / Math.PI );

    _textLocation = _textPlacementHorizontal switch {
      EnumTextPlacementHorizontal.Centered => _centerDimLine,
      EnumTextPlacementHorizontal.OverExtLine1 => _startDimLine,
      EnumTextPlacementHorizontal.OverExtLine2 => _endDimLine,
      _ => _textLocation
    };

    switch (_textPlacementVertical) {
      case EnumTextPlacementVertical.Above:
        _textAlignment = EnumTextAlignment.BottomMiddle;
        _textFieldAlignment = EnumTextAlignment.TopMiddle;
        break;
      case EnumTextPlacementVertical.Centered:
        _textAlignment = EnumTextAlignment.CenterMiddle;
        _textFieldAlignment = EnumTextAlignment.CenterMiddle;
        break;
      case EnumTextPlacementVertical.Bellow:
        _textAlignment = EnumTextAlignment.TopMiddle;
        _textFieldAlignment = EnumTextAlignment.BottomMiddle;
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    return true;
  }

  private void InitSetting(double scale, Dimension2DSetting? dimSetting)
  {
    _dimSetting = dimSetting ?? Dimension2DSetting.Instance;

    _lineWeightId = _dimSetting.LineWeightId;
    _dimLineColor = _dimSetting.DimLineColor;
    _tickMarkType = _dimSetting.TickMarkType;
    _dimensionLineExtension = _dimSetting.DimensionLineExtension * scale;
    _witnessLineExtension = _dimSetting.WitnessLineExtension * scale;
    _witnessLineGapToElement = _dimSetting.WitnessLineGapToElement * scale;
    _snapDistance = _dimSetting.SnapDistance * scale;
    _textColor = _dimSetting.TextColor;
    _textOffset = _dimSetting.TextOffset * scale;
    _textLeaderOffset = _dimSetting.TextLeaderOffset * scale;
    _textFontSize = _dimSetting.TextFontSize * scale;
    _roundingDigit = _dimSetting.RoundingDigit;
    _textPlacementVertical = _dimSetting.TextPlacementVertical;
    _textPlacementHorizontal = _dimSetting.TextPlacementHorizontal;
    _textPlacement = _dimSetting.TextPlacement;
  }

  public Dimension2D(
    System.Windows.Controls.Canvas? canvas,
    double scale,
    Point startPoint,
    Point endPoint,
    Point placePoint,
    Vector dimDirection,
    EnumDimensionLevel dimLevel,
    double obliqueStart = 90,
    double obliqueEnd = 90,
    string assignTextValue = "",
    string textField = "",
    bool isNonStandardRight = true,
    Dimension2DSetting? dimSetting = null,
    int zIndex = 0)
  {
    if (canvas == null || scale < 1e-9 || !startPoint.IsValid() || !endPoint.IsValid() ||
        startPoint.DistanceTo(endPoint) < 1e-9 || !placePoint.IsValid() || !dimDirection.IsValid())
      return;

    _canvas = canvas;
    _scale = scale;
    _startPoint = startPoint;
    _endPoint = endPoint;
    _placePoint = placePoint;
    _dimDirection = dimDirection;
    _dimLevel = dimLevel;
    _dimObliqueStart = obliqueStart;
    _dimObliqueEnd = obliqueEnd;
    _assignTextValue = assignTextValue;
    _textField = textField;
    _isNonStandardRight = isNonStandardRight;
    _zIndex = zIndex;

    InitSetting(_scale, _dimSetting);
    if (!InitGeometry())
      throw new ArgumentException("Dimension2D: invalid geometry (parallel lines or zero-length dimension).");

    DrawDimLine();

    if (_isStandard) {
      DrawText();
    }
    else {
      DrawNonstandardDimension();
    }
  }
}