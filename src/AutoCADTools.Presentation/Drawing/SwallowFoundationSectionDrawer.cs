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

internal sealed class SwallowFoundationSectionDrawer
{
  private readonly FoundationDrawingContext _ctx;

  public SwallowFoundationSectionDrawer(FoundationDrawingContext ctx)
  {
    _ctx = ctx;
  }

  public void Draw(SwallowFoundationModel model, double originX, double sectionOriginY)
  {
    var lx = model.LengthX;
    var pad = model.ConcretePadExtension;
    var padThick = model.ConcretePadThickness;
    var colW = model.ColumnWidthX;
    var colPosX = model.ColumnPositionX;
    var axisX = model.AxisPositionX;
    var h1 = model.StepHeightH1;
    var h2 = model.StepHeightH2;
    var foundationHeight = Math.Abs(model.FloorLevel1 - model.FoundationBottomLevel);
    var totalW = lx + 2 * pad;

    var dimSetting = new Dimension2DSetting { TextPlacement = EnumTextPlacement.BesideDim };

    // Compute all geometry points
    var geom = ComputeSectionGeometry(originX, sectionOriginY, lx, colW, colPosX, h1, h2, foundationHeight);

    DrawTitle(lx, originX, sectionOriginY);
    DrawConcretePad(originX, pad, geom.FoundationBottom, totalW, padThick);
    DrawFootingPolygon(geom.FootingPolygon);
    DrawAxisLine(originX, axisX, geom.PadY, foundationHeight, padThick);
    DrawLxDimension(geom, originX, axisX, dimSetting);
    DrawElevationDimension(geom, pad, dimSetting);
  }

  // ── Geometry ─────────────────────────────────────────────────────────────

  private SectionGeometry ComputeSectionGeometry(double originX, double sectionOriginY,
    double lx, double colW, double colPosX,
    double h1, double h2,
    double foundationHeight)
  {
    // Section title is drawn above the column stub.
    // (title pos = originY + 6*scale above; column stub starts below)
    var pedestalColumnExtend = 100.0;
    var padY = sectionOriginY + 6 * _ctx.Scale + 15 * _ctx.Scale + pedestalColumnExtend;

    var colStubCenter = originX + colPosX;
    var footingCenter = originX + lx / 2;

    var topLeft = new Point(colStubCenter - colW / 2, padY - pedestalColumnExtend);
    var topRight = new Point(colStubCenter + colW / 2, padY - pedestalColumnExtend);

    var heightDiff = foundationHeight - h1;
    var pedestalBottomRight = topRight with
    {
      Y = topRight.Y + pedestalColumnExtend + (heightDiff < 0 ? 0 : heightDiff)
    };
    var pedestalBottomRightExtend = pedestalBottomRight with
    {
      X = Math.Min(pedestalBottomRight.X + 50, originX + lx)
    };

    var foundationRightSlope = new Point(
      footingCenter + lx / 2,
      pedestalBottomRightExtend.Y + (h1 - h2));

    var foundationBottomRight = foundationRightSlope with { Y = foundationRightSlope.Y + h2 };
    var foundationBottomLeft = foundationBottomRight with { X = footingCenter - lx / 2 };

    var foundationLeftSlope = new Point(
      footingCenter - lx / 2,
      foundationBottomLeft.Y - h2);

    var pedestalBottomLeftExtend = pedestalBottomRight with
    {
      X = Math.Max(colStubCenter - colW / 2 - 50, foundationBottomLeft.X)
    };
    var pedestalBottomLeft = pedestalBottomLeftExtend with
    {
      X = Math.Min(pedestalBottomLeftExtend.X + 50, colStubCenter - colW / 2)
    };

    var footingPolygon = new PointCollection([
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
    ]);

    var dimBottom = foundationBottomRight;
    var dimH2 = dimBottom with { Y = dimBottom.Y - h2 };
    var dimH1 = dimBottom with { Y = dimBottom.Y - h1 };
    var dimTop = dimBottom with { Y = dimBottom.Y - foundationHeight };
    var foundationBottom = padY + foundationHeight;

    return new SectionGeometry(
      padY, footingPolygon,
      dimBottom, dimH2, dimH1, dimTop, foundationBottom);
  }

  // ── Draw methods ───────────────────────────────────────────────────────

  private void DrawTitle(double lx, double originX, double sectionOriginY)
  {
    var titlePos = new Point(originX + lx / 2, sectionOriginY + 6 * _ctx.Scale);
    FoundationDrawingPrimitives.DrawText(_ctx,
      "SwallowFoundation.View.Section".GetString(), 5,
      titlePos, Brushes.Black,
      EnumTextAlignment.TopMiddle, zIndex: 0);
  }

  private void DrawConcretePad(
    double originX, double pad,
    double foundationBottom, double totalW, double padThick)
  {
    var padPoints = FoundationDrawingPrimitives.CreateRectPoints(
      originX - pad, foundationBottom, totalW, padThick);
    FoundationDrawingPrimitives.DrawPolygon(_ctx, padPoints,
      Brushes.Gray, Brushes.LightGray, zIndex: 0);
  }

  private void DrawFootingPolygon(PointCollection footingPolygon)
  {
    FoundationDrawingPrimitives.DrawPolygon(_ctx, footingPolygon,
      Brushes.Blue, Brushes.White, zIndex: 1);
  }

  private void DrawAxisLine(
    double originX, double axisX,
    double padY, double foundationHeight, double padThick)
  {
    var vAxisStart = new Point(originX + axisX, padY);
    var vAxisEnd = new Point(originX + axisX, padY + foundationHeight + padThick);
    FoundationDrawingPrimitives.DrawAxis(_ctx, "X", vAxisStart, vAxisEnd,
      EnumGridSymbolStyle.None, EnumGridSymbolStyle.Circle, zIndex: 2);
  }

  private void DrawLxDimension(
    SectionGeometry geom, double originX, double axisX,
    Dimension2DSetting dimSetting)
  {
    var dimLxStart = geom.DimBottom with { X = originX };
    var dimLxEnd = geom.DimBottom;
    var dimLxDir = UtilsVector.CreateVector(dimLxStart, dimLxEnd);
    var dimPlacePoint = dimLxStart + dimLxDir.Rotate(90) * 15 * _ctx.Scale;

    _ = new Dimension2D(_ctx.Canvas, _ctx.Scale, dimLxStart, dimLxStart with { X = originX + axisX },
      dimPlacePoint, dimLxDir, EnumDimensionLevel.Level1,
      dimSetting: dimSetting, isNonStandardRight: false, assignTextValue: _ctx.IsShowDim ? "" : " ");
    _ = new Dimension2D(_ctx.Canvas, _ctx.Scale, dimLxStart with { X = originX + axisX }, dimLxEnd,
      dimPlacePoint, dimLxDir, EnumDimensionLevel.Level1,
      dimSetting: dimSetting, assignTextValue: _ctx.IsShowDim ? "" : " ");
    _ = new Dimension2D(_ctx.Canvas, _ctx.Scale, dimLxStart, dimLxEnd,
      dimPlacePoint, dimLxDir, EnumDimensionLevel.Level2,
      assignTextValue: _ctx.IsShowDim ? "" : "Lx", dimSetting: dimSetting);
  }

  private void DrawElevationDimension(
    SectionGeometry geom, double pad,
    Dimension2DSetting dimSetting)
  {
    var dimDir = UtilsVector.CreateVector(geom.DimTop, geom.DimBottom);
    FoundationDrawingPrimitives.DrawElevationDimension(_ctx,
      geom.DimBottom, geom.DimH2, geom.DimH1, geom.DimTop,
      dimDir, pad, dimSetting);
  }

  // ── Private struct ──────────────────────────────────────────────────────

  private struct SectionGeometry
  {
    public double PadY { get; }
    public PointCollection FootingPolygon { get; }
    public Point DimBottom { get; }
    public Point DimH2 { get; }
    public Point DimH1 { get; }
    public Point DimTop { get; }
    public double FoundationBottom { get; }

    public SectionGeometry(
      double padY,
      PointCollection footingPolygon,
      Point dimBottom, Point dimH2, Point dimH1, Point dimTop,
      double foundationBottom)
    {
      PadY = padY;
      FootingPolygon = footingPolygon;
      DimBottom = dimBottom;
      DimH2 = dimH2;
      DimH1 = dimH1;
      DimTop = dimTop;
      FoundationBottom = foundationBottom;
    }
  }
}
