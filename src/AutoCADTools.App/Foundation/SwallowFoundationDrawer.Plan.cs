#nullable enable

using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // ╔══════════════════════════════════════════════════════════════╗
  // ║                    A.  MẶT BẰNG (PLAN)                        ║
  // ╚══════════════════════════════════════════════════════════════╝

  private void DrawPlanAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    double Lx = Model.LengthX * S;
    double Ly = Model.LengthY * S;
    double cpX = Model.ColumnPositionX * S;
    double cpY = Model.ColumnPositionY * S;
    double padW = Model.ConcretePadExtension * S;
    double cwX = Model.ColumnWidthX * S;
    double cwY = Model.ColumnWidthY * S;

    // Footing outline corners
    var p1 = new Point3d(bp.X - Lx / 2, bp.Y - Ly / 2, bp.Z);
    var p2 = new Point3d(bp.X + Lx / 2, bp.Y - Ly / 2, bp.Z);
    var p3 = new Point3d(bp.X + Lx / 2, bp.Y + Ly / 2, bp.Z);
    var p4 = new Point3d(bp.X - Lx / 2, bp.Y + Ly / 2, bp.Z);

    // ── Footing outline (THAY) ────────────────────────────────────
    AddLine(p1, p2, LayerOutline, null, tr, btr);
    AddLine(p2, p3, LayerOutline, null, tr, btr);
    AddLine(p3, p4, LayerOutline, null, tr, btr);
    AddLine(p4, p1, LayerOutline, null, tr, btr);

    // ── Concrete pad (HATCH + AR-CONC) ─────────────────────────────
    var pp1 = new Point3d(p1.X - padW, p1.Y - padW, bp.Z);
    var pp2 = new Point3d(p2.X + padW, p2.Y - padW, bp.Z);
    var pp3 = new Point3d(p3.X + padW, p3.Y + padW, bp.Z);
    var pp4 = new Point3d(p4.X - padW, p4.Y + padW, bp.Z);

    var padPL = CreateRectanglePolyline(pp1, pp2, pp3, pp4);
    HatchPolyline(padPL, "AR-CONC", 20 * S, LayerConcreteHatch, tr, btr);

    // ── Axis lines (TIM, dashed) ──────────────────────────────────
    var lnAxisX = AddLine(
      new Point3d(bp.X - Lx, bp.Y + cpY, bp.Z),
      new Point3d(bp.X + Lx, bp.Y + cpY, bp.Z),
      LayerAxis, "DASHED", tr, btr);

    var lnAxisY = AddLine(
      new Point3d(bp.X + cpX, bp.Y - Ly, bp.Z),
      new Point3d(bp.X + cpX, bp.Y + Ly, bp.Z),
      LayerAxis, "DASHED", tr, btr);

    // ── Column ─────────────────────────────────────────────────────
    var cpBase = new Point3d(bp.X + cpX, bp.Y + cpY, bp.Z);

    if (Model.IsRectangularColumn)
    {
      var c1 = new Point3d(cpBase.X - cwX / 2, cpBase.Y - cwY / 2, cpBase.Z);
      var c2 = new Point3d(cpBase.X + cwX / 2, cpBase.Y - cwY / 2, cpBase.Z);
      var c3 = new Point3d(cpBase.X + cwX / 2, cpBase.Y + cwY / 2, cpBase.Z);
      var c4 = new Point3d(cpBase.X - cwX / 2, cpBase.Y + cwY / 2, cpBase.Z);

      var colPL = CreateRectanglePolyline(c1, c2, c3, c4);
      HatchPolyline(colPL, "DOTS", 5 * S, LayerColumn, tr, btr);
    }
    else // Circular column
    {
      var cRadius = Math.Min(cwX, cwY) / 2;
      using var circ = new Circle(cpBase, Vector3d.ZAxis, cRadius) { Layer = LayerColumn };
      btr.AppendEntity(circ);
      tr.AddNewlyCreatedDBObject(circ, true);

      using var circPL = new Polyline();
      const int Segs = 32;
      for (int i = 0; i < Segs; i++)
      {
        double a = 2 * Math.PI * i / Segs;
        circPL.AddVertexAt(i, new Point2d(cpBase.X + cRadius * Math.Cos(a), cpBase.Y + cRadius * Math.Sin(a)), 0, 0, 0);
      }
      circPL.Closed = true;
      var circIds = new ObjectIdCollection { circPL.ObjectId };
      btr.AppendEntity(circPL);
      tr.AddNewlyCreatedDBObject(circPL, true);
      AddSolidHatch(circIds, LayerColumn, tr, btr);
    }

    // ── Step boundary lines when H1 != H2 ──────────────────────────
    if (Math.Abs(Model.StepHeightH1 - Model.StepHeightH2) > 0.001)
    {
      var margin = 0.05 * S;
      var sp1 = new Point3d(bp.X - Lx / 2 + margin, bp.Y - Ly / 2 + margin, bp.Z);
      var sp2 = new Point3d(bp.X + Lx / 2 - margin, bp.Y - Ly / 2 + margin, bp.Z);
      var sp3 = new Point3d(bp.X + Lx / 2 - margin, bp.Y + Ly / 2 - margin, bp.Z);
      var sp4 = new Point3d(bp.X - Lx / 2 + margin, bp.Y + Ly / 2 - margin, bp.Z);

      AddLine(sp1, sp2, LayerOutline, null, tr, btr);
      AddLine(sp2, sp3, LayerOutline, null, tr, btr);
      AddLine(sp3, sp4, LayerOutline, null, tr, btr);
      AddLine(sp4, sp1, LayerOutline, null, tr, btr);
    }

    // ── Foundation name label ──────────────────────────────────────
    string label = $"{Model.FoundationName}(SL:{Model.Quantity:D2})";
    AddMText(new Point3d(bp.X + Lx / 2 + 0.3, bp.Y + Ly / 2 + 0.3, bp.Z),
      label, 0.35 * S, LayerLabel, tr, btr);

    // ── Lx / Ly dimension labels (outside footing) ─────────────────
    // Lx: below the bottom edge of footing
    double dimLabelOff = padW + 1.0 * S;
    double lblY = bp.Y - Ly / 2 - dimLabelOff;
    AddDbText(
      new Point3d(bp.X, lblY, bp.Z),
      "Lx = " + Model.LengthX.ToString("F0"),
      0.35 * S, LayerLabel, tr, btr);

    // Ly: right of the right edge of footing
    double lblX = bp.X + Lx / 2 + dimLabelOff;
    AddDbText(
      new Point3d(lblX, bp.Y, bp.Z),
      "Ly = " + Model.LengthY.ToString("F0"),
      0.35 * S, LayerLabel, tr, btr);
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║               B.  DIM  MẶT BẰNG (PLAN DIMENSIONS)             ║
  // ╚══════════════════════════════════════════════════════════════╝

  private void DrawPlanDimensionsAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    double Lx = Model.LengthX * S;
    double Ly = Model.LengthY * S;
    double cpX = Model.ColumnPositionX * S;
    double cpY = Model.ColumnPositionY * S;
    double dimOff = 1.5 * S;
    double shortOff = 0.8 * S;

    double xCol1 = bp.X + cpX - Model.ColumnWidthX * S / 2;
    double xCol2 = bp.X + cpX + Model.ColumnWidthX * S / 2;
    double yCol1 = bp.Y + cpY - Model.ColumnWidthY * S / 2;
    double yCol2 = bp.Y + cpY + Model.ColumnWidthY * S / 2;

    // ── X-dimension line (top, full Lx) ────────────────────────────
    double yOut = bp.Y + Ly / 2 + dimOff;
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yOut + 0.5, bp.Z),
      new Point3d(bp.X - Lx / 2, yOut, bp.Z),
      new Point3d(bp.X + Lx / 2, yOut, bp.Z),
      LayerDimension, "Length", "Lx", tr, btr);

    // ── X-dimension: left stub ─────────────────────────────────────
    double yIn = bp.Y + Ly / 2 + shortOff;
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yIn + 0.5, bp.Z),
      new Point3d(bp.X - Lx / 2, yIn, bp.Z),
      new Point3d(xCol1, yIn, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── X-dimension: column width ───────────────────────────────────
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yIn + 0.5, bp.Z),
      new Point3d(xCol1, yIn, bp.Z),
      new Point3d(xCol2, yIn, bp.Z),
      LayerDimension, "ColWidth", "Cx", tr, btr);

    // ── X-dimension: right stub ────────────────────────────────────
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yIn + 0.5, bp.Z),
      new Point3d(xCol2, yIn, bp.Z),
      new Point3d(bp.X + Lx / 2, yIn, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── Y-dimension line (right, full Ly) ──────────────────────────
    double xOut = bp.X + Lx / 2 + dimOff;
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xOut + 0.5, bp.Y, bp.Z),
      new Point3d(xOut, bp.Y - Ly / 2, bp.Z),
      new Point3d(xOut, bp.Y + Ly / 2, bp.Z),
      LayerDimension, "Length", "Ly", tr, btr);

    // ── Y-dimension: bottom stub ───────────────────────────────────
    double xIn = bp.X + Lx / 2 + shortOff;
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xIn + 0.5, bp.Y, bp.Z),
      new Point3d(xIn, bp.Y - Ly / 2, bp.Z),
      new Point3d(xIn, yCol1, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── Y-dimension: column width ──────────────────────────────────
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xIn + 0.5, bp.Y, bp.Z),
      new Point3d(xIn, yCol1, bp.Z),
      new Point3d(xIn, yCol2, bp.Z),
      LayerDimension, "ColWidth", "Cy", tr, btr);

    // ── Y-dimension: top stub ──────────────────────────────────────
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xIn + 0.5, bp.Y, bp.Z),
      new Point3d(xIn, yCol2, bp.Z),
      new Point3d(xIn, bp.Y + Ly / 2, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── Axis offset dims ────────────────────────────────────────────
    if (Math.Abs(Model.AxisPositionX) > 0.1)
    {
      double yOff = bp.Y - Ly / 2 - dimOff;
      AddRotatedDimensionEntity(0,
        new Point3d(bp.X, yOff - 0.5, bp.Z),
        new Point3d(bp.X, yOff, bp.Z),
        new Point3d(bp.X + cpX, yOff, bp.Z),
        LayerDimension, "AxisOffset", "OffsetX", tr, btr);
    }

    if (Math.Abs(Model.AxisPositionY) > 0.1)
    {
      double xOff = bp.X - Lx / 2 - dimOff;
      AddRotatedDimensionEntity(Math.PI / 2,
        new Point3d(xOff - 0.5, bp.Y, bp.Z),
        new Point3d(xOff, bp.Y, bp.Z),
        new Point3d(xOff, bp.Y + cpY, bp.Z),
        LayerDimension, "AxisOffset", "OffsetY", tr, btr);
    }
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║            C.  THÉP  MẶT BẰNG (PLAN REBAR)                    ║
  // ╚══════════════════════════════════════════════════════════════╝

  private void DrawPlanRebarAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    double Lx = Model.LengthX * S;
    double Ly = Model.LengthY * S;
    double cover = Model.Cover * S;

    var (rxCount, rxDia, _) = ParseRebarSpec(Model.RebarX);
    var (ryCount, ryDia, _) = ParseRebarSpec(Model.RebarY);
    if (rxCount <= 0 && ryCount <= 0) return;

    // ── Rebar X (horizontal rebars, along X axis) ─────────────────
    if (rxCount > 0 && rxDia > 0)
    {
      double usableY = Ly - 2 * cover;
      double spacing = usableY / (rxCount + 1);

      for (int i = 1; i <= rxCount; i++)
      {
        double y = bp.Y - Ly / 2 + cover + i * spacing;
        var ln = AddLine(
          new Point3d(bp.X - Lx / 2 + cover, y, bp.Z),
          new Point3d(bp.X + Lx / 2 - cover, y, bp.Z),
          LayerRebar, null, tr, btr);
        ln.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);

        var tagPt = new Point3d(bp.X + 0.5 * S, y + 0.5 * S, bp.Z);
        InsertMongRebarTag(tagPt, 0, $"phi{(int)rxDia}", tr, btr);
      }
    }

    // ── Rebar Y (vertical rebars, along Y axis) ───────────────────
    if (ryCount > 0 && ryDia > 0)
    {
      double usableX = Lx - 2 * cover;
      double spacing = usableX / (ryCount + 1);

      for (int i = 1; i <= ryCount; i++)
      {
        double x = bp.X - Lx / 2 + cover + i * spacing;
        var ln = AddLine(
          new Point3d(x, bp.Y - Ly / 2 + cover, bp.Z),
          new Point3d(x, bp.Y + Ly / 2 - cover, bp.Z),
          LayerRebar, null, tr, btr);
        ln.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);

        var tagPt = new Point3d(x + 0.5 * S, bp.Y + 0.5 * S, bp.Z);
        InsertMongRebarTag(tagPt, Math.PI / 2, $"phi{(int)ryDia}", tr, btr);
      }
    }
  }
}
