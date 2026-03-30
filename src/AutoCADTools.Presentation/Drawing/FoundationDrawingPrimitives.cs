using System;
using System.Windows;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Annotation;
using AutoCADTools.Presentation.Canvas.Settings;
using AutoCADTools.Presentation.Canvas.Shapes;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Drawing;

public static class FoundationDrawingPrimitives
{
  // ── Polygon ────────────────────────────────────────────────────────────────

  public static void DrawPolygon(
    FoundationDrawingContext ctx,
    PointCollection points,
    Brush? lineBrush,
    Brush? fillBrush,
    int zIndex = 0)
    => _ = new Polygon2D(ctx.Canvas, points, ctx.LineThickness, lineBrush, fillBrush, zIndex: zIndex);

  // ── Rectangle shorthand ────────────────────────────────────────────────────

  public static void DrawRect(
    FoundationDrawingContext ctx,
    double originX, double originY, double width, double height,
    Brush? lineBrush, Brush? fillBrush,
    int zIndex = 0)
  {
    var pts = CreateRectPoints(originX, originY, width, height);
    _ = new Polygon2D(ctx.Canvas, pts, ctx.LineThickness, lineBrush, fillBrush, zIndex: zIndex);
  }

  // ── Line ──────────────────────────────────────────────────────────────────

  public static void DrawLine(
    FoundationDrawingContext ctx,
    Point start, Point end,
    EnumLineType lineType,
    Brush? color,
    int zIndex = 0)
    => _ = new Line2D(ctx.Canvas, start, end, lineType, ctx.LineThickness, color, zIndex: zIndex);

  // ── Ellipse (center-based) ─────────────────────────────────────────────────

  public static void DrawEllipse(
    FoundationDrawingContext ctx,
    Point center, double width, double height,
    Brush? lineBrush, Brush? fillBrush,
    int zIndex = 0)
    => _ = new Ellipse2D(ctx.Canvas, center, width, height, ctx.LineThickness, lineBrush, fillBrush, zIndex: zIndex);

  // ── Axis line (Grid2D wrapper) ─────────────────────────────────────────────

  public static void DrawAxis(
    FoundationDrawingContext ctx,
    string name,
    Point start, Point end,
    EnumGridSymbolStyle startSymbol,
    EnumGridSymbolStyle endSymbol,
    int zIndex = 0)
    => _ = new Grid2D(ctx.Canvas, ctx.Scale, name, start, end, startSymbol, endSymbol,
        startOffset: 5, endOffset: 35, zIndex: zIndex);

  // ── Horizontal Lx dimension (3-segment: left + right + combined) ───────────

  public static void DrawHorizontalLxDimension(
    FoundationDrawingContext ctx,
    Point left, Point right, Point axis,
    Vector direction,
    Dimension2DSetting? dimSetting = null)
  {
    var place = new Point(0, left.Y) + direction * 20 * ctx.Scale;
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, left, axis, place, direction,
      EnumDimensionLevel.Level1, dimSetting: dimSetting, isNonStandardRight: false);
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, axis, right, place, direction,
      EnumDimensionLevel.Level1, dimSetting: dimSetting);
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, left, right, place, direction,
      EnumDimensionLevel.Level2, assignTextValue: "Lx");
  }

  // ── Vertical Ly dimension (3-segment: bottom + top + combined) ───────────────

  public static void DrawVerticalLyDimension(
    FoundationDrawingContext ctx,
    Point bottom, Point top, Point axis,
    Vector direction,
    Dimension2DSetting? dimSetting = null)
  {
    var place = new Point(bottom.X, 0) + direction * 20 * ctx.Scale;
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, bottom, axis, place, direction,
      EnumDimensionLevel.Level1, dimSetting: dimSetting, isNonStandardRight: false);
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, axis, top, place, direction,
      EnumDimensionLevel.Level1, dimSetting: dimSetting);
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, bottom, top, place, direction,
      EnumDimensionLevel.Level2, assignTextValue: "Ly", dimSetting: dimSetting);
  }

  // ── Elevation dimension (bottom + H2 + H1 + top: 3× Level1 + 1× Level2) ─────

  public static void DrawElevationDimension(
    FoundationDrawingContext ctx,
    Point bottom, Point h2, Point h1, Point top,
    Vector direction,
    double pad,
    Dimension2DSetting? dimSetting = null)
  {
    var place = new Point(bottom.X + pad, 0) + direction * 20 * ctx.Scale;
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, bottom, h2, place, direction,
      EnumDimensionLevel.Level1, dimSetting: dimSetting, isNonStandardRight: false);
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, h2, h1, place, direction,
      EnumDimensionLevel.Level1, isNonStandardRight: false);
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, h1, top, place, direction,
      EnumDimensionLevel.Level1, dimSetting: dimSetting);
    _ = new Dimension2D(ctx.Canvas, ctx.Scale, bottom, top, place, direction,
      EnumDimensionLevel.Level2, dimSetting: dimSetting, isNonStandardRight: false);
  }

  // ── Text note ─────────────────────────────────────────────────────────────

  public static void DrawText(
    FoundationDrawingContext ctx,
    string text, double fontSizeScaled,
    Point position,
    Brush? color,
    EnumTextAlignment alignment = EnumTextAlignment.CenterMiddle,
    int zIndex = 0)
    => _ = new TextNote2D(ctx.Canvas, ctx.Scale, text, fontSizeScaled, position, color,
        margin: 1, textAlignment: alignment, zIndex: zIndex);

  // ── Column-extension-to-footing clip ──────────────────────────────────────

  public static PointCollection ClipColumnToFooting(
    PointCollection columnRect,
    Rect footingRect)
  {
    var result = new PointCollection();
    for (var i = 0; i < columnRect.Count; i++)
    {
      var p = columnRect[i];
      var cx = Math.Min(Math.Max(p.X, footingRect.Left), footingRect.Right);
      var cy = Math.Min(Math.Max(p.Y, footingRect.Top), footingRect.Bottom);
      result.Add(new Point(cx, cy));
    }
    return result;
  }

  // ── Rectangle points factory ──────────────────────────────────────────────

  public static PointCollection CreateRectPoints(
    double originX, double originY, double width, double height)
  {
    return new PointCollection([
      new Point(originX, originY),
      new Point(originX + width, originY),
      new Point(originX + width, originY + height),
      new Point(originX, originY + height)
    ]);
  }
}
