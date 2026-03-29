using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Core;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Canvas.Annotation;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Drawing;

public class SwallowFoundationPreviewDrawer
{
  private readonly global::System.Windows.Controls.Canvas _canvas;
  private double LineThickness { get ; set ; }
  private double _scale = 100;

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
    // DrawSection(model, margin, margin + planHeight, sectionScale);
  }

  public void DrawPlan(SwallowFoundationModel model, double originX, double originY)
  {
    var Lx = model.LengthX;
    var Ly = model.LengthY;
    var pad = model.ConcretePadExtension;
    var colPosX = model.ColumnPositionX;
    var colPosY = model.ColumnPositionY;
    var colW = model.ColumnWidthX;
    var colH = model.ColumnWidthY;
    var axisPositionX = model.AxisPositionX;
    var axisPositionY = model.AxisPositionY;

    var totalW = Lx + 2 * pad;
    var totalH = Ly + 2 * pad;

    var footingLeft = originX + pad;
    var footingTop = originY + pad;

    // --- Concrete Pad (bleed area) ---
    var padRectPoints = CreateRectPoints(originX, originY, totalW, totalH);
    _ = new Polygon2D(_canvas, padRectPoints, LineThickness, Brushes.Gray, Brushes.Gray, zIndex: 0);

    // --- Footing outline ---
    var footingRectPoints = CreateRectPoints(footingLeft, footingTop, Lx, Ly);
    _ = new Polygon2D(_canvas, footingRectPoints, LineThickness, Brushes.Blue, Brushes.White, zIndex: 1);

    // --- Axis lines (dashed gray) ---
    var axCx = footingLeft + axisPositionX;
    var axCy = footingTop + axisPositionY;

    var vAxisStartPoint = new Point(axCx, originY);
    var vAxisEndPoint = new Point(axCx, originY + totalH);
    var hAxisStartPoint = new Point(originX, axCy);
    var hAxisEndPoint = new Point(originX + totalW, axCy);

    _ = new Grid2D( _canvas, _scale, "*", vAxisStartPoint, vAxisEndPoint, EnumGridSymbolStyle.Circle, EnumGridSymbolStyle.Circle, 10, 10 ) ;
    _ = new Grid2D( _canvas, _scale, "*", hAxisStartPoint, hAxisEndPoint, EnumGridSymbolStyle.Circle, EnumGridSymbolStyle.Circle, 10, 10 ) ;

    // --- Column ---
    var colLeftExtend = footingLeft + colPosX - colW / 2 - 50;
    var colTopExtend = footingTop + colPosY - colH / 2 - 50;
    var columnExtendRectPoints = CreateRectPoints(colLeftExtend, colTopExtend, colW + 100, colH + 100);
    var footingRect = new Rect(
      footingRectPoints.Min(p => p.X), footingRectPoints.Min(p => p.Y),
      footingRectPoints.Max(p => p.X) - footingRectPoints.Min(p => p.X),
      footingRectPoints.Max(p => p.Y) - footingRectPoints.Min(p => p.Y));

    for (var i = 0; i < columnExtendRectPoints.Count; i++)
    {
      var p = columnExtendRectPoints[i];
      var cx = Math.Min(Math.Max(p.X, footingRect.Left), footingRect.Right);
      var cy = Math.Min(Math.Max(p.Y, footingRect.Top), footingRect.Bottom);
      columnExtendRectPoints[i] = new Point(cx, cy);
    }

    _ = new Polygon2D(_canvas, columnExtendRectPoints, LineThickness, Brushes.Blue, Brushes.Green, zIndex: 2);
    for (var i = 0; i < footingRectPoints.Count; i++)
    {
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
    var dimLxStartPoint = new Point(footingLeft, footingTop + Ly + pad);
    var dimLxEndPoint = new Point(footingLeft + Lx, footingTop + Ly + pad);
    var dimLxDirection = UtilsVector.CreateVector( dimLxStartPoint, dimLxEndPoint ) ;
    var dimLxPlacePoint = new Point(0, footingTop + Ly + pad) + dimLxDirection.Rotate(90) * 20 * _scale;
    _ = new Dimension2D(_canvas, _scale, dimLxStartPoint, dimLxEndPoint, dimLxPlacePoint, dimLxDirection,
      EnumDimensionLevel.Level1, assignTextValue: "Lx");

    // --- Dimension: Ly (vertical, right of footing) ---
    var dimLyStartPoint = new Point(footingLeft + Lx + pad, footingTop);
    var dimLyEndPoint = new Point(footingLeft + Lx + pad, footingTop + Ly);
    var dimLyDirection = UtilsVector.CreateVector( dimLyStartPoint, dimLyEndPoint ) ;
    var dimLyPlacePoint = new Point(footingLeft + Lx + pad, 0) +
                          dimLyDirection.Rotate(-90) * 20 * _scale;
    _ = new Dimension2D(_canvas, _scale, dimLyStartPoint, dimLyEndPoint, dimLyPlacePoint, dimLyDirection,
      EnumDimensionLevel.Level1, assignTextValue: "Ly");

    // --- Plan title ---
    _ = new TextNote2D(_canvas, _scale, "SwallowFoundation.View.Plan".GetString(), 5,
      new Point(originX + totalW / 2, originY - 6 * _scale), Brushes.Black, margin: 1,
      textAlignment: EnumTextAlignment.BottomMiddle);
  }

  private static PointCollection CreateRectPoints(double originX, double originY, double totalW, double totalH)
  {
    var p1 = new Point(originX, originY);
    var p2 = new Point(originX + totalW, originY);
    var p3 = new Point(originX + totalW, originY + totalH);
    var p4 = new Point(originX, originY + totalH);
    return [p1, p2, p3, p4];
  }

  /*public void DrawSection(SwallowFoundationModel model, double originX, double originY)
  {
    var Lx = model.LengthX;
    var Ly = model.LengthY;
    var pad = model.ConcretePadExtension;
    var colPosX = model.ColumnPositionX;
    var colPosY = model.ColumnPositionY;
    var colW = model.ColumnWidthX;
    var colH = model.ColumnWidthY;
    var axisPositionX = model.AxisPositionX;
    var axisPositionY = model.AxisPositionY;

    var totalW = Lx + 2 * pad;
    var totalH = Ly + 2 * pad;
    var stepHeightH1 = model.StepHeightH1;
    var stepHeightH2 = model.StepHeightH2;

    var groundLevel = model.GroundLevel;
    var floorLevel1 = model.FloorLevel1;
    var foundationBottomLevel = model.FoundationBottomLevel;

    // --- Concrete Pad (bottom, widest) ---
    var padRectPoints = CreateRectPoints(originX - pad, originY - foundationBottomLevel, totalW, totalH);
    _ = new Polygon2D(_canvas, padRectPoints, LineThickness, Brushes.Gray, Brushes.Gray, zIndex: 0);

    // --- Step H2 (footing lower step) ---
    var p1 = new Point(originX, originY - foundationBottomLevel);
    var p2 = new Point(originX + Lx, originY);
    var footingPoints = CreateRectPoints(originX - pad, originY - foundationBottomLevel, totalW, totalH);
    _ = new Polygon2D(_canvas, padRectPoints, LineThickness, Brushes.Gray, Brushes.Gray, zIndex: 0);
    var step2Top = padTop - H2;
    if (H2 > 0) {
      ctx.DrawRect(
        ctx.ptAbs(footingLeft, step2Top),
        Lx, H2,
        ctx.Black,
        fill: null,
        thickness: ctx.LineThickness,
        zIndex: 1);
    }

    // --- Step H1 (upper step / vat area) ---
    var step1Top = step2Top - H1;
    var step1H = H1;

    var colStubLeft = originX + totalW / 2 - colW / 2;
    var colStubTop = originY;

    if (Math.Abs(H1 - H2) < 1e-6 || H1 < 1e-6) {
      // Rectangular step H1 (no vat)
      if (step1H > 0) {
        ctx.DrawRect(
          ctx.ptAbs(footingLeft, step1Top),
          Lx, step1H,
          ctx.Black,
          fill: null,
          thickness: ctx.LineThickness,
          zIndex: 1);
      }
    }
    else {
      // Trapezoid vat: narrower at top (column stub width)
      var vatPoints = new PointCollection {
        new Point(footingLeft, step1Top + step1H), // bottom-left
        new Point(footingLeft + Lx, step1Top + step1H), // bottom-right
        new Point(colStubLeft + colW, step1Top), // top-right
        new Point(colStubLeft, step1Top) // top-left
      };
      ctx.DrawPolygon(
        vatPoints,
        ctx.Black,
        fill: null,
        thickness: ctx.LineThickness,
        zIndex: 1);
    }

    // --- Column stub (rectangular, top center) ---
    var colStubHeight = totalH - padThick - H2 - H1;
    if (colStubHeight < 0) colStubHeight = 0;

    if (colStubHeight > 0) {
      ctx.DrawRect(
        ctx.ptAbs(colStubLeft, colStubTop),
        colW, colStubHeight,
        ctx.Black,
        fill: ctx.White,
        thickness: ctx.LineThickness,
        zIndex: 2);
    }

    // --- Column Rebars ---
    if (model.DrawColumnRebar) {
      var rebarDia = model.ColumnRebar * s;
      if (rebarDia > 0) {
        DrawSectionColumnRebars(ctx, model, colStubLeft, colStubTop, colW, colStubHeight, rebarDia);
      }
    }

    // --- Elevation labels ---
    var dimFontSize = Math.Max(9, 11 * scalePreview / model.Scale);

    ctx.DrawText(
      "SwallowFoundation.Label.Elevation".GetString() + " " +
      model.GroundLevel.ToString("F0", CultureInfo.InvariantCulture),
      dimFontSize,
      ctx.ptAbs(originX, originY - dimFontSize - 4),
      ctx.Gray,
      EnumTextAlignment.BottomLeft,
      zIndex: 4);

    ctx.DrawText(
      "SwallowFoundation.Label.Elevation".GetString() + " " +
      model.FoundationBottomLevel.ToString("F0", CultureInfo.InvariantCulture),
      dimFontSize,
      ctx.ptAbs(originX, originY + totalH + 4),
      ctx.Gray,
      EnumTextAlignment.TopLeft,
      zIndex: 4);

    // --- Vertical dimension H2 (lower step) ---
    var labelX = originX + totalW + padW * 0.15;

    if (H2 > 0) {
      ctx.DrawDimension(
        ctx.ptAbs(footingLeft, padTop - H2),
        ctx.ptAbs(footingLeft, padTop),
        ctx.ptAbs(labelX, padTop - H2 / 2),
        EnumDimensionLevel.Level1,
        assignTextValue: "H2=" + model.StepHeightH2.ToString("F0", CultureInfo.InvariantCulture),
        zIndex: 4);
    }

    // --- Vertical dimension H1 (upper step / vat) ---
    if (H1 > 0) {
      ctx.DrawDimension(
        ctx.ptAbs(footingLeft, step2Top),
        ctx.ptAbs(footingLeft, step2Top + H1),
        ctx.ptAbs(labelX, step2Top + H1 / 2),
        EnumDimensionLevel.Level1,
        assignTextValue: "H1=" + model.StepHeightH1.ToString("F0", CultureInfo.InvariantCulture),
        zIndex: 4);
    }

    // --- Section title ---
    ctx.DrawText(
      "SwallowFoundation.View.Section".GetString(),
      dimFontSize + 2,
      ctx.ptAbs(originX, originY - dimFontSize - 6),
      ctx.Black,
      EnumTextAlignment.BottomLeft,
      zIndex: 4);
  }*/

  /*private void DrawSectionColumnRebars(
    SwallowFoundationDrawingContext ctx,
    SwallowFoundationModel model,
    double colLeft,
    double colTop,
    double colWidth,
    double colHeight,
    double rebarDia)
  {
    var countX = Math.Max(2, model.ColumnRebarCountX);
    var countY = Math.Max(2, model.ColumnRebarCountY);
    var cover = model.Cover * model.Scale;

    var innerW = colWidth - 2 * cover;
    var innerH = colHeight - 2 * cover;
    if (innerW < rebarDia || innerH < rebarDia || cover < 0) return;

    if (model.IsRectangularColumn) {
      var stepY = innerH / Math.Max(countY - 1, 1);
      for (var i = 0 ; i < countY ; i++) {
        var cy = colTop + cover + i * stepY;
        AddRebarCircle(ctx, colLeft + cover + rebarDia / 2, cy, rebarDia);
        AddRebarCircle(ctx, colLeft + colWidth - cover - rebarDia / 2, cy, rebarDia);
      }

      var stepX = innerW / Math.Max(countX - 1, 1);
      for (var i = 0 ; i < countX ; i++) {
        var cx = colLeft + cover + i * stepX;
        AddRebarCircle(ctx, cx, colTop + cover + rebarDia / 2, rebarDia);
        AddRebarCircle(ctx, cx, colTop + colHeight - cover - rebarDia / 2, rebarDia);
      }
    }
    else if (model.IsCircularColumn) {
      var totalCount = Math.Max(countX, countY) * 4;
      var r = ( Math.Min(colWidth, colHeight) / 2 ) - cover - rebarDia / 2;
      var cx = colLeft + colWidth / 2;
      var cy = colTop + colHeight / 2;
      if (r < rebarDia / 2) return;

      for (var i = 0 ; i < totalCount ; i++) {
        var angle = 2 * Math.PI * i / totalCount;
        var rx = cx + r * Math.Cos(angle);
        var ry = cy + r * Math.Sin(angle);
        AddRebarCircle(ctx, rx, ry, rebarDia);
      }
    }
  }*/

  /*private void AddRebarCircle(SwallowFoundationDrawingContext ctx, double cx, double cy, double dia)
  {
    ctx.DrawEllipse(
      ctx.ptAbs(cx, cy),
      dia, dia,
      ctx.Red,
      fill: ctx.Red,
      thickness: ctx.LineThickness,
      zIndex: 3);
  }*/
}