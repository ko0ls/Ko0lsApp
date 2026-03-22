using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public class Angular2D : AnnotationBase
{
  private readonly System.Windows.Controls.Canvas? _canvas;
  private double _scale;
  private Point _centerPoint;
  private Point _startPoint;
  private Point _endPoint;
  private double _radius;
  private int _orientation;
  private double _lineThickness;
  private double _textFontSize;

  private void DrawAngularArc()
  {
    var arc = new Path {
      Stroke = Brushes.Black,
      StrokeThickness = _lineThickness,
      Data = new PathGeometry {
        Figures = new PathFigureCollection {
          new PathFigure {
            StartPoint = _startPoint,
            Segments = new PathSegmentCollection {
              new ArcSegment {
                Point = _endPoint,
                Size = new Size(_radius, _radius),
                SweepDirection = _orientation == 1 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                IsLargeArc = false
              }
            }
          }
        }
      }
    };

    _canvas?.Children.Add(arc);
  }

  private void DrawAngular()
  {
    var startVector = UtilsVector.CreateVector(_centerPoint, _startPoint);
    var endVector = UtilsVector.CreateVector(_centerPoint, _endPoint);
    Vector vectorBisector = UtilsVector.GetBisector(startVector, endVector);

    double angle = Math.Round(UtilsVector.AngleTo(startVector, endVector), 1);
    if (Math.Abs(angle - 90) < 1e-9 || Math.Abs(angle - 180) < 1e-9 || angle < 1e-9)
      return;

    string angleText = $"{angle}°";
    Point textLocation = _centerPoint + vectorBisector * ( _radius + _textFontSize * _scale );

    _ = new TextBlock2D(_canvas, angleText, _textFontSize * _scale, textLocation, Brushes.Black);
    DrawAngularArc();
  }

  public Angular2D(System.Windows.Controls.Canvas? canvas, double scale, Point centerPoint, Point p1, Point p2)
  {
    if (canvas == null || UtilsPoint.IsAlmostEqualTo(p1, p2))
      return;

    _canvas = canvas;
    _scale = scale;
    _centerPoint = centerPoint;

    var startVector = UtilsVector.CreateVector(centerPoint, p1);
    var endVector = UtilsVector.CreateVector(centerPoint, p2);
    _radius = 5 * _scale;
    _startPoint = _centerPoint + startVector * _radius;
    _endPoint = _centerPoint + endVector * _radius;

    var lineWeightId = 2;
    _lineThickness = UtilsCanvas.GetLineThickness(_scale, lineWeightId);
    _textFontSize = 5;

    _orientation = UtilsPoint.GetOrientation(centerPoint, _startPoint, _endPoint);
    if (_orientation == 0)
      return;

    DrawAngular();
  }

  public override void MakeHighLight() { }
  public override void ResetHighLight() { }
  public override bool CheckMoveOver(Point point) => false;
}