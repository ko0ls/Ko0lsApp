using System;
using System.Windows;
using AutoCADTools.Core;
using AutoCADTools.Presentation.Canvas.Utils;

namespace AutoCADTools.Presentation.Drawing;

public class SwallowFoundationPreviewDrawer : IFoundationDrawer
{
  public SwallowFoundationPreviewDrawer(global::System.Windows.Controls.Canvas canvas, double scale)
  {
    Canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    Scale = scale;
  }

  public void RefreshDrawing(SwallowFoundationModel? model)
  {
    RefreshDrawing(model, (int)Scale, isShowDim: true);
  }

  public void RefreshDrawing(SwallowFoundationModel? model, int canvasScale, bool isShowDim = true)
  {
    Canvas.Children.Clear();
    if (model == null) return;
    Scale = canvasScale;

    var lineThickness = UtilsCanvas.GetLineThickness(canvasScale, 2);
    var ctx = new FoundationDrawingContext(Canvas, canvasScale, lineThickness) { IsShowDim = isShowDim };

    var planDrawer = new SwallowFoundationPlanDrawer(ctx);
    planDrawer.Draw(model, originX: 0, originY: 0);

    var planBounds = GetCanvasBoundingBox(Canvas);
    var sectionOriginY = planBounds.IsEmpty
      ? 0.0
      : planBounds.Bottom + SectionGap;

    var sectionDrawer = new SwallowFoundationSectionDrawer(ctx);
    sectionDrawer.Draw(model, model.ConcretePadExtension, sectionOriginY);
  }

  // ── Private state ────────────────────────────────────────────────────────

  private global::System.Windows.Controls.Canvas Canvas { get; }

  private double Scale { get; set; }

  private const double SectionGap = 50.0;

  // ── Bounding box utility ─────────────────────────────────────────────────

  /// <summary>
  /// Computes the bounding box of all WPF shapes added to the canvas.
  /// Reads geometry directly from shape properties (Polygon.Points, Line.X1/Y1/X2/Y2,
  /// Ellipse Canvas.Left/Top/Width/Height) since Canvas.GetLeft/Top returns NaN
  /// and VisualTreeHelper.GetDescendantBounds returns Empty for WPF shapes.
  /// </summary>
  private static Rect GetCanvasBoundingBox(global::System.Windows.Controls.Canvas? canvas)
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
