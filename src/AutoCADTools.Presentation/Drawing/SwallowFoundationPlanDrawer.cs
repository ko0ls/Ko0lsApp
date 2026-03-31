using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Core;
using AutoCADTools.Core.Localization;
using AutoCADTools.Presentation.Canvas.Annotation;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Drawing;

internal sealed class SwallowFoundationPlanDrawer
{
  private readonly FoundationDrawingContext _ctx;

  public SwallowFoundationPlanDrawer(FoundationDrawingContext ctx)
  {
    _ctx = ctx;
  }

  public void Draw(SwallowFoundationModel model, double originX, double originY)
  {
    var lx = model.LengthX;
    var ly = model.LengthY;
    var pad = model.ConcretePadExtension;
    var colPosX = model.ColumnPositionX;
    var colPosY = model.ColumnPositionY;
    var colW = model.ColumnWidthX;
    var colH = model.ColumnWidthY;
    var axisX = model.AxisPositionX;
    var axisY = model.AxisPositionY;

    var totalW = lx + 2 * pad;
    var totalH = ly + 2 * pad;
    var footingLeft = originX + pad;
    var footingTop = originY + pad;
    var footingRect = new Rect(footingLeft, footingTop, lx, ly);

    var dimSetting = new Dimension2DSetting { TextPlacement = EnumTextPlacement.BesideDim };

    DrawPadAndFooting(originX, originY, totalW, totalH, footingLeft, footingTop, footingRect);
    DrawAxisLines(footingLeft, footingTop, axisX, axisY, originX, originY, totalW, totalH);
    DrawColumn(footingLeft, footingTop, footingRect, colPosX, colPosY, colW, colH, model);
    DrawDimensions(footingLeft, footingTop, lx, ly, pad, axisX, axisY, dimSetting);
    DrawTitle(originX, originY, totalW);
  }

  private void DrawPadAndFooting(
    double originX, double originY,
    double totalW, double totalH,
    double footingLeft, double footingTop,
    Rect footingRect)
  {
    // Concrete Pad (bleed area)
    var padPoints = FoundationDrawingPrimitives.CreateRectPoints(originX, originY, totalW, totalH);
    FoundationDrawingPrimitives.DrawPolygon(_ctx, padPoints,
      Brushes.Gray, Brushes.Gray, zIndex: 0);

    // Footing outline
    var footingPoints = FoundationDrawingPrimitives.CreateRectPoints(
      footingLeft, footingTop, footingRect.Width, footingRect.Height);
    FoundationDrawingPrimitives.DrawPolygon(_ctx, footingPoints,
      Brushes.Blue, Brushes.White, zIndex: 1);
  }

  private void DrawAxisLines(
    double footingLeft, double footingTop,
    double axisX, double axisY,
    double originX, double originY,
    double totalW, double totalH)
  {
    var axCx = footingLeft + axisX;
    var axCy = footingTop + axisY;

    var vAxisStart = new Point(axCx, originY);
    var vAxisEnd = new Point(axCx, originY + totalH);
    var hAxisStart = new Point(originX, axCy);
    var hAxisEnd = new Point(originX + totalW, axCy);

    FoundationDrawingPrimitives.DrawAxis(_ctx, "X", vAxisStart, vAxisEnd,
      EnumGridSymbolStyle.None, EnumGridSymbolStyle.Circle, zIndex: 4);
    FoundationDrawingPrimitives.DrawAxis(_ctx, "Y", hAxisStart, hAxisEnd,
      EnumGridSymbolStyle.None, EnumGridSymbolStyle.Circle, zIndex: 4);
  }

  private void DrawColumn(
    double footingLeft, double footingTop,
    Rect footingRect,
    double colPosX, double colPosY,
    double colW, double colH,
    SwallowFoundationModel model)
  {
    // Column extension rect (50px bleed each side)
    var colLeftExtend = footingLeft + colPosX - colW / 2 - 50;
    var colTopExtend = footingTop + colPosY - colH / 2 - 50;
    var colExtendPoints = FoundationDrawingPrimitives.CreateRectPoints(
      colLeftExtend, colTopExtend, colW + 100, colH + 100);

    // Clip column extension to footing bounds
    var clippedPoints = FoundationDrawingPrimitives.ClipColumnToFooting(colExtendPoints, footingRect);

    // Column extension polygon
    FoundationDrawingPrimitives.DrawPolygon(_ctx, clippedPoints,
      Brushes.Blue, Brushes.Green, zIndex: 2);

    // Connecting lines from footing corners to clipped column extension corners
    var footingPoints = FoundationDrawingPrimitives.CreateRectPoints(
      footingRect.Left, footingRect.Top, footingRect.Width, footingRect.Height);
    for (var i = 0; i < footingPoints.Count; i++)
    {
      FoundationDrawingPrimitives.DrawLine(_ctx,
        footingPoints[i], clippedPoints[i],
        EnumLineType.Solid, Brushes.Blue, zIndex: 3);
    }

    // Actual column: rectangular or circular
    if (model.IsRectangularColumn)
    {
      var colLeft = footingLeft + colPosX - colW / 2;
      var colTop = footingTop + colPosY - colH / 2;
      FoundationDrawingPrimitives.DrawRect(_ctx, colLeft, colTop, colW, colH,
        Brushes.Blue, Brushes.Red, zIndex: 3);
    }
    else if (model.IsCircularColumn)
    {
      var center = new Point(footingLeft + colPosX, footingTop + colPosY);
      FoundationDrawingPrimitives.DrawEllipse(_ctx, center, colW, colH,
        Brushes.Blue, Brushes.Red, zIndex: 3);
    }
  }

  private void DrawDimensions(
    double footingLeft, double footingTop,
    double lx, double ly, double pad,
    double axisX, double axisY,
    Dimension2DSetting dimSetting)
  {
    // Lx: horizontal dimension below footing
    var dimLxStart = new Point(footingLeft, footingTop + ly + pad);
    var dimLxEnd = new Point(footingLeft + lx, footingTop + ly + pad);
    var dimLxAxis = new Point(footingLeft + axisX, footingTop + ly + pad);
    var dimLxDir = UtilsVector.CreateVector(dimLxStart, dimLxEnd);
    FoundationDrawingPrimitives.DrawHorizontalLxDimension(
      _ctx, dimLxStart, dimLxEnd, dimLxAxis, dimLxDir, dimSetting);

    // Ly: vertical dimension right of footing
    var dimLyStart = new Point(footingLeft + lx + pad, footingTop);
    var dimLyEnd = new Point(footingLeft + lx + pad, footingTop + ly);
    var dimLyAxis = new Point(footingLeft + lx + pad, footingTop + axisY);
    var dimLyDir = UtilsVector.CreateVector(dimLyStart, dimLyEnd);
    FoundationDrawingPrimitives.DrawVerticalLyDimension(
      _ctx, dimLyStart, dimLyEnd, dimLyAxis, dimLyDir, dimSetting);
  }

  private void DrawTitle(double originX, double originY, double totalW)
  {
    var titlePos = new Point(originX + totalW / 2, originY - 6 * _ctx.Scale);
    FoundationDrawingPrimitives.DrawText(_ctx,
      "SwallowFoundation.View.Plan".GetString(), 5,
      titlePos, Brushes.Black,
      EnumTextAlignment.BottomMiddle, zIndex: 4);
  }
}
