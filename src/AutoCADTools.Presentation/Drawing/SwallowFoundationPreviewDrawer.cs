using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Core;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Canvas.Annotation;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Drawing;

public class SwallowFoundationPreviewDrawer
{
  private readonly global::System.Windows.Controls.Canvas _canvas;
  private double LineThickness { get; set; }
  private double _scale;

  public SwallowFoundationPreviewDrawer(global::System.Windows.Controls.Canvas canvas, double scale)
  {
    _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    _scale = scale;
    LineThickness = UtilsCanvas.GetLineThickness(_scale, 2);
  }

  public void RefreshDrawing(SwallowFoundationModel? model)
  {
    _canvas.Children.Clear();
    if (model == null) return;
    double originX = 0;
    double originY = 0;

    DrawPlan(model, originX, originY);

    // Position section BELOW plan without overlap
    var planBounds = GetCanvasBoundingBox(_canvas);
    if (!planBounds.IsEmpty) {
      var sectionGap = 50.0;
      var sectionOriginY = planBounds.Bottom + sectionGap; // below plan
      DrawSection(model, originX + model.ConcretePadExtension, sectionOriginY);
    }
    else {
      DrawSection(model, originX + model.ConcretePadExtension, 0);
    }
  }

  private void DrawPlan(SwallowFoundationModel model, double originX, double originY)
  {
    var dimSetting = new Dimension2DSetting { TextPlacement = EnumTextPlacement.BesideDim } ;
    var lx = model.LengthX;
    var ly = model.LengthY;
    var pad = model.ConcretePadExtension;
    var colPosX = model.ColumnPositionX;
    var colPosY = model.ColumnPositionY;
    var colW = model.ColumnWidthX;
    var colH = model.ColumnWidthY;
    var axisPositionX = model.AxisPositionX;
    var axisPositionY = model.AxisPositionY;

    var totalW = lx + 2 * pad;
    var totalH = ly + 2 * pad;

    var footingLeft = originX + pad;
    var footingTop = originY + pad;

    // --- Concrete Pad (bleed area) ---
    var padRectPoints = CreateRectPoints(originX, originY, totalW, totalH);
    _ = new Polygon2D(_canvas, padRectPoints, LineThickness, Brushes.Gray, Brushes.Gray, zIndex: 0);

    // --- Footing outline ---
    var footingRectPoints = CreateRectPoints(footingLeft, footingTop, lx, ly);
    _ = new Polygon2D(_canvas, footingRectPoints, LineThickness, Brushes.Blue, Brushes.White, zIndex: 1);

    // --- Axis lines (dashed gray) ---
    var axCx = footingLeft + axisPositionX;
    var axCy = footingTop + axisPositionY;

    var vAxisStartPoint = new Point(axCx, originY);
    var vAxisEndPoint = new Point(axCx, originY + totalH);
    var hAxisStartPoint = new Point(originX, axCy);
    var hAxisEndPoint = new Point(originX + totalW, axCy);

    _ = new Grid2D(_canvas, _scale, "-", vAxisStartPoint, vAxisEndPoint, EnumGridSymbolStyle.None,
      EnumGridSymbolStyle.Circle, 5, 35, zIndex: 4);
    _ = new Grid2D(_canvas, _scale, "-", hAxisStartPoint, hAxisEndPoint, EnumGridSymbolStyle.None,
      EnumGridSymbolStyle.Circle, 5, 35, zIndex: 4);

    // --- Column ---
    var colLeftExtend = footingLeft + colPosX - colW / 2 - 50;
    var colTopExtend = footingTop + colPosY - colH / 2 - 50;
    var columnExtendRectPoints = CreateRectPoints(colLeftExtend, colTopExtend, colW + 100, colH + 100);
    var footingRect = new Rect(
      footingRectPoints.Min(p => p.X), footingRectPoints.Min(p => p.Y),
      footingRectPoints.Max(p => p.X) - footingRectPoints.Min(p => p.X),
      footingRectPoints.Max(p => p.Y) - footingRectPoints.Min(p => p.Y));

    for (var i = 0 ; i < columnExtendRectPoints.Count ; i++) {
      var p = columnExtendRectPoints[i];
      var cx = Math.Min(Math.Max(p.X, footingRect.Left), footingRect.Right);
      var cy = Math.Min(Math.Max(p.Y, footingRect.Top), footingRect.Bottom);
      columnExtendRectPoints[i] = new Point(cx, cy);
    }

    _ = new Polygon2D(_canvas, columnExtendRectPoints, LineThickness, Brushes.Blue, Brushes.Green, zIndex: 2);
    for (var i = 0 ; i < footingRectPoints.Count ; i++) {
      _ = new Line2D(_canvas, footingRectPoints[i], columnExtendRectPoints[i], EnumLineType.Solid, LineThickness,
        Brushes.Blue, zIndex: 3);
    }

    if (model.IsRectangularColumn) {
      var colLeft = footingLeft + colPosX - colW / 2;
      var colTop = footingTop + colPosY - colH / 2;
      var columnRectPoints = CreateRectPoints(colLeft, colTop, colW, colH);
      _ = new Polygon2D(_canvas, columnRectPoints, LineThickness, Brushes.Blue, Brushes.Red, zIndex: 3);
    }
    else if (model.IsCircularColumn) {
      var centerX = footingLeft + colPosX;
      var centerY = footingTop + colPosY;
      var center = new Point(centerX, centerY);
      _ = new Ellipse2D(_canvas, center, colW, colH, LineThickness, Brushes.Blue, Brushes.Red, zIndex: 3);
    }

    // --- Dimension: Lx (horizontal, below footing) ---
    var dimLxStartPoint = new Point(footingLeft, footingTop + ly + pad);
    var dimLxEndPoint = new Point(footingLeft + lx, footingTop + ly + pad);
    var dimVerticalAxis = new Point(footingLeft + axisPositionX, footingTop + ly + pad);
    var dimLxDirection = UtilsVector.CreateVector(dimLxStartPoint, dimLxEndPoint);
    var dimLxPlacePoint = new Point(0, footingTop + ly + pad) + dimLxDirection.Rotate(90) * 20 * _scale;
    _ = new Dimension2D(_canvas, _scale, dimLxStartPoint, dimVerticalAxis, dimLxPlacePoint, dimLxDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting, isNonStandardRight: false);
    _ = new Dimension2D(_canvas, _scale, dimVerticalAxis, dimLxEndPoint, dimLxPlacePoint, dimLxDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting);
    _ = new Dimension2D(_canvas, _scale, dimLxStartPoint, dimLxEndPoint, dimLxPlacePoint, dimLxDirection,
      EnumDimensionLevel.Level2, assignTextValue: "Lx");

    // --- Dimension: Ly (vertical, right of footing) ---
    var dimLyStartPoint = new Point(footingLeft + lx + pad, footingTop);
    var dimLyEndPoint = new Point(footingLeft + lx + pad, footingTop + ly);
    var dimHorizontalAxis = new Point(footingLeft + lx + pad, footingTop + axisPositionY);
    var dimLyDirection = UtilsVector.CreateVector(dimLyStartPoint, dimLyEndPoint);
    var dimLyPlacePoint = new Point(footingLeft + lx + pad, 0) +
                          dimLyDirection.Rotate(-90) * 20 * _scale;
    _ = new Dimension2D(_canvas, _scale, dimLyStartPoint, dimHorizontalAxis, dimLyPlacePoint, dimLyDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting);
    _ = new Dimension2D(_canvas, _scale, dimHorizontalAxis, dimLyEndPoint, dimLyPlacePoint, dimLyDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting, isNonStandardRight: false);
    _ = new Dimension2D(_canvas, _scale, dimLyStartPoint, dimLyEndPoint, dimLyPlacePoint, dimLyDirection,
      EnumDimensionLevel.Level2, assignTextValue: "Ly", dimSetting: dimSetting);

    // --- Plan title ---
    _ = new TextNote2D(_canvas, _scale, "SwallowFoundation.View.Plan".GetString(), 5,
      new Point(originX + totalW / 2, originY - 6 * _scale), Brushes.Black, margin: 1,
      textAlignment: EnumTextAlignment.BottomMiddle);
  }

  private void DrawSection(SwallowFoundationModel model, double originX, double originY)
  {
    var dimSetting = new Dimension2DSetting { TextPlacement = EnumTextPlacement.BesideDim } ;
    var lx = model.LengthX;
    var pad = model.ConcretePadExtension;
    var padThick = model.ConcretePadThickness;
    var colW = model.ColumnWidthX;
    var foundationBottomLevel = model.FoundationBottomLevel;
    var floorLevel1 = model.FloorLevel1;
    var colPositionX = model.ColumnPositionX;
    var vAxisPositionX = model.AxisPositionX;

    var totalW = lx + 2 * pad;

    // Note: WPF canvas Y increases downward.
    // Section originY = top of footing (bottom of column stub).
    // All section elements have POSITIVE Y (drawn downward from originY).

    // --- H1 / H2 / H3 ---
    var h1 = model.StepHeightH1;
    var h2 = model.StepHeightH2;

    // --- 0. Section Title (above column stub) ---
    var sectionTitle = new TextNote2D(_canvas, _scale, "SwallowFoundation.View.Section".GetString(),
      5, new Point(originX + lx / 2, originY + 6 * _scale), Brushes.Black,
      margin: 1, textAlignment: EnumTextAlignment.TopMiddle);
    var titleHeight = sectionTitle.Height;
    var pedestalColumnExtend = 100;

    // --- 1. Concrete Pad (Rectangle, dashed gray) ---
    var yPosition = originY + 6 * _scale + titleHeight + 15 * _scale + pedestalColumnExtend;
    var foundationHeight = Math.Abs(floorLevel1 - foundationBottomLevel);
    var padRect = CreateRectPoints(originX - pad, yPosition + foundationHeight, totalW, padThick);
    _ = new Polygon2D(_canvas, padRect, LineThickness, Brushes.Gray, Brushes.LightGray, zIndex: 0);

    // --- 2. Footing polygon (single Polygon2D for all footing parts) ---
    var colStubCenter = originX + colPositionX;
    var footingCenter = originX + lx / 2;
    var topLeft = new Point(colStubCenter - colW / 2, yPosition - pedestalColumnExtend);
    var topRight = new Point(colStubCenter + colW / 2, yPosition - pedestalColumnExtend);
    var pedestalBottomRight = topRight with { Y = topRight.Y + pedestalColumnExtend + ( foundationHeight - h1 < 0 ? 0 : foundationHeight - h1 ) };
    var pedestalBottomRightExtend = pedestalBottomRight with {
      X = pedestalBottomRight.X + 50 > originX + lx ? originX + lx : pedestalBottomRight.X + 50
    };
    var foundationRightSlope = new Point(footingCenter + lx / 2, pedestalBottomRightExtend.Y + ( h1 - h2 ));

    var foundationBottomRight = foundationRightSlope with { Y = foundationRightSlope.Y + h2 };
    var foundationBottomLeft = foundationBottomRight with { X = footingCenter - lx / 2 };

    var foundationLeftSlope = new Point(footingCenter - lx / 2, foundationBottomLeft.Y - h2);
    var pedestalBottomLeftExtend = pedestalBottomRight with {
      X = colStubCenter - colW / 2 - 50 < foundationBottomLeft.X
        ? foundationBottomLeft.X
        : colStubCenter - colW / 2 - 50
    };
    var pedestalBottomLeft = pedestalBottomLeftExtend with {
      X = pedestalBottomLeftExtend.X + 50 > colStubCenter - colW / 2
        ? colStubCenter - colW / 2
        : pedestalBottomLeftExtend.X + 50
    };

    var footingPolygon = new PointCollection {
      topLeft,
      topRight,
      pedestalBottomRight,
      pedestalBottomRightExtend,
      foundationRightSlope,
      foundationBottomRight,
      foundationBottomLeft,
      foundationLeftSlope,
      pedestalBottomLeftExtend,
      pedestalBottomLeft
    };
    _ = new Polygon2D(_canvas, footingPolygon, LineThickness, Brushes.Blue, Brushes.White, zIndex: 1);

    // --- 3. Axis Line ---
    var vAxisStartPoint = new Point(originX + vAxisPositionX, yPosition);
    var vAxisEndPoint = new Point(originX + vAxisPositionX, yPosition + foundationHeight + padThick);
    _ = new Grid2D(_canvas, _scale, "*", vAxisStartPoint, vAxisEndPoint, EnumGridSymbolStyle.None,
      EnumGridSymbolStyle.Circle, 5, 35, zIndex: 2);

    // ---4.  Dimension: Lx (horizontal, below footing) ---
    var dimLxStartPoint = foundationBottomLeft;
    var dimLxEndPoint = foundationBottomRight;
    var dimAxisX = dimLxStartPoint with { X = originX + vAxisPositionX };
    var dimLxDirection = UtilsVector.CreateVector(dimLxStartPoint, dimLxEndPoint);
    var dimLxPlacePoint = new Point(0, dimLxEndPoint.Y + pad) + dimLxDirection.Rotate(90) * 20 * _scale;
    _ = new Dimension2D(_canvas, _scale, dimAxisX, dimLxStartPoint, dimLxPlacePoint, dimLxDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting, isNonStandardRight: false);
    _ = new Dimension2D(_canvas, _scale, dimAxisX, dimLxEndPoint, dimLxPlacePoint, dimLxDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting);
    _ = new Dimension2D(_canvas, _scale, dimLxStartPoint, dimLxEndPoint, dimLxPlacePoint, dimLxDirection,
      EnumDimensionLevel.Level2, assignTextValue: "Lx", dimSetting: dimSetting);

    // ---5.  Dimension: elevation ---
    var dimBottomFoundationStartPoint = foundationBottomRight;
    var dimH2 = dimBottomFoundationStartPoint with { Y = dimBottomFoundationStartPoint.Y - h2 };
    var dimH1 = dimBottomFoundationStartPoint with { Y = dimBottomFoundationStartPoint.Y - h1 };
    var dimFloorLevel1 = dimBottomFoundationStartPoint with { Y = dimBottomFoundationStartPoint.Y - foundationHeight };
    var dimDirection = UtilsVector.CreateVector(dimFloorLevel1, dimBottomFoundationStartPoint);
    var dimPlacePoint = new Point(dimBottomFoundationStartPoint.X + pad, 0) +
                        dimDirection.Rotate(-90) * 20 * _scale;
    _ = new Dimension2D(_canvas, _scale, dimBottomFoundationStartPoint, dimH2, dimPlacePoint, dimDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting, isNonStandardRight: false);
    _ = new Dimension2D(_canvas, _scale, dimH2, dimH1, dimPlacePoint, dimDirection,
      EnumDimensionLevel.Level1, isNonStandardRight: false);
    _ = new Dimension2D(_canvas, _scale, dimH1, dimFloorLevel1, dimPlacePoint, dimDirection,
      EnumDimensionLevel.Level1, dimSetting: dimSetting);
    _ = new Dimension2D(_canvas, _scale, dimBottomFoundationStartPoint, dimFloorLevel1, dimPlacePoint, dimDirection,
      EnumDimensionLevel.Level2, dimSetting: dimSetting, isNonStandardRight: false);
  }

  private static PointCollection CreateRectPoints(double originX, double originY, double totalW, double totalH)
  {
    var p1 = new Point(originX, originY);
    var p2 = new Point(originX + totalW, originY);
    var p3 = new Point(originX + totalW, originY + totalH);
    var p4 = new Point(originX, originY + totalH);
    return [p1, p2, p3, p4];
  }

  /// <summary>
  /// Computes the bounding box of all WPF shapes added to the canvas.
  /// Reads geometry directly from shape properties (Polygon.Points, Line.X1/Y1/X2/Y2,
  /// Ellipse Canvas.Left/Top/Width/Height) since Canvas.GetLeft/Top returns NaN
  /// and VisualTreeHelper.GetDescendantBounds returns Empty for WPF shapes.
  /// </summary>
  private static Rect GetCanvasBoundingBox(System.Windows.Controls.Canvas? canvas)
  {
    if (canvas == null || canvas.Children.Count == 0)
      return Rect.Empty;

    double minX = double.MaxValue, minY = double.MaxValue;
    double maxX = double.MinValue, maxY = double.MinValue;

    foreach (UIElement child in canvas.Children)
    {
      switch (child)
      {
        case System.Windows.Shapes.Polygon poly: {
          foreach (var pt in poly.Points) {
            if (pt.X < minX) minX = pt.X;
            if (pt.Y < minY) minY = pt.Y;
            if (pt.X > maxX) maxX = pt.X;
            if (pt.Y > maxY) maxY = pt.Y;
          }
          break;
        }
        case System.Windows.Shapes.Line line: {
          if (line.X1 < minX) minX = line.X1;
          if (line.Y1 < minY) minY = line.Y1;
          if (line.X1 > maxX) maxX = line.X1;
          if (line.Y1 > maxY) maxY = line.Y1;
          if (line.X2 < minX) minX = line.X2;
          if (line.Y2 < minY) minY = line.Y2;
          if (line.X2 > maxX) maxX = line.X2;
          if (line.Y2 > maxY) maxY = line.Y2;
          break;
        }
        case System.Windows.Shapes.Ellipse ellipse: {
          var el = System.Windows.Controls.Canvas.GetLeft(ellipse);
          var et = System.Windows.Controls.Canvas.GetTop(ellipse);
          if (double.IsNaN(el)) el = 0;
          if (double.IsNaN(et)) et = 0;
          var ew = ellipse.Width;
          var eh = ellipse.Height;
          if (el < minX) minX = el;
          if (et < minY) minY = et;
          if (el + ew > maxX) maxX = el + ew;
          if (et + eh > maxY) maxY = et + eh;
          break;
        }
        case System.Windows.Shapes.Polyline pl: {
          foreach (var pt in pl.Points) {
            if (pt.X < minX) minX = pt.X;
            if (pt.Y < minY) minY = pt.Y;
            if (pt.X > maxX) maxX = pt.X;
            if (pt.Y > maxY) maxY = pt.Y;
          }
          break;
        }
      }
    }

    return Math.Abs(minX - double.MaxValue) < 1e-9
      ? Rect.Empty
      : new Rect(minX, minY, maxX - minX, maxY - minY);
  }
}