#nullable enable

using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // ╔══════════════════════════════════════════════════════════════╗
  // ║                    D.  MẶT CẮT (SECTION)                       ║
  // ╚══════════════════════════════════════════════════════════════╝

  private const double SectionOffsetX = 8.0; // drawing-units offset to the right of plan

  private void DrawSectionAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var sec = new Point3d(bp.X + SectionOffsetX, bp.Y, bp.Z);

    double Lx = Model.LengthX * S;
    double h2 = Model.StepHeightH2 * S;
    double h1 = Model.StepHeightH1 * S;
    double cwX = Model.ColumnWidthX * S;
    double cwY = Model.ColumnWidthY * S;
    double padT = Model.ConcretePadThickness * S;
    double padW = Model.ConcretePadExtension * S;
    double colBaseZ = Model.FoundationBottomLevel * S + h1 + h2;
    double fBot = Model.FoundationBottomLevel * S;
    double textH = 0.25 * S;

    // ── Concrete pad (AR-CONC) ─────────────────────────────────────
    var padPL = CreateRectanglePolyline(
      new Point3d(sec.X - Lx / 2 - padW, fBot - padT, sec.Z),
      new Point3d(sec.X + Lx / 2 + padW, fBot - padT, sec.Z),
      new Point3d(sec.X + Lx / 2 + padW, fBot, sec.Z),
      new Point3d(sec.X - Lx / 2 - padW, fBot, sec.Z));
    HatchPolyline(padPL, "AR-CONC", 20 * S, LayerConcreteHatch, tr, btr);

    // ── Step H2 rectangle ──────────────────────────────────────────
    double step2Top = fBot + h2;
    var step2PL = CreateRectanglePolyline(
      new Point3d(sec.X - Lx / 2, fBot, sec.Z),
      new Point3d(sec.X + Lx / 2, fBot, sec.Z),
      new Point3d(sec.X + Lx / 2, step2Top, sec.Z),
      new Point3d(sec.X - Lx / 2, step2Top, sec.Z));
    HatchPolyline(step2PL, "", 1, LayerOutline, tr, btr);

    // ── Step H1 / chamfer ───────────────────────────────────────────
    double step1Top = step2Top + h1;

    if (Math.Abs(h1 - h2) > 0.001)
    {
      // Trapezoid chamfer
      AddLine(new Point3d(sec.X - Lx / 2, step1Top, sec.Z), new Point3d(sec.X + Lx / 2, step1Top, sec.Z), LayerOutline, null, tr, btr);
      AddLine(new Point3d(sec.X + Lx / 2, step1Top, sec.Z), new Point3d(sec.X + cwX / 2, step1Top + 0.3 * S, sec.Z), LayerOutline, null, tr, btr);
      AddLine(new Point3d(sec.X + cwX / 2, step1Top + 0.3 * S, sec.Z), new Point3d(sec.X - cwX / 2, step1Top + 0.3 * S, sec.Z), LayerOutline, null, tr, btr);
      AddLine(new Point3d(sec.X - cwX / 2, step1Top + 0.3 * S, sec.Z), new Point3d(sec.X - Lx / 2, step1Top, sec.Z), LayerOutline, null, tr, btr);
    }
    else
    {
      AddLine(new Point3d(sec.X - Lx / 2, step1Top, sec.Z), new Point3d(sec.X + Lx / 2, step1Top, sec.Z), LayerOutline, null, tr, btr);
    }

    // ── Column stub ─────────────────────────────────────────────────
    double colStubH = 0.5 * S;
    double colTopZ = colBaseZ + colStubH;
    AddLine(new Point3d(sec.X - cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X + cwX / 2, colBaseZ, sec.Z), LayerColumn, null, tr, btr);
    AddLine(new Point3d(sec.X - cwX / 2, colTopZ, sec.Z), new Point3d(sec.X + cwX / 2, colTopZ, sec.Z), LayerColumn, null, tr, btr);
    AddLine(new Point3d(sec.X - cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X - cwX / 2, colTopZ, sec.Z), LayerColumn, null, tr, btr);
    AddLine(new Point3d(sec.X + cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X + cwX / 2, colTopZ, sec.Z), LayerColumn, null, tr, btr);

    // ── Elevation labels ───────────────────────────────────────────
    double lblX = sec.X - Lx / 2 - 1.5;

    AddDbText(new Point3d(lblX, sec.Y + Model.GroundLevel * S, sec.Z), $"{-Model.FoundationBottomLevel / 1000:F3}", textH, LayerLabel, tr, btr);
    AddDbText(new Point3d(lblX, sec.Y + Model.GroundLevel * S + 0.3 * S, sec.Z), "MC", textH, LayerLabel, tr, btr);
    AddDbText(new Point3d(lblX, sec.Y + Model.FloorLevel1 * S, sec.Z), $"{-Model.FloorLevel1 / 1000:F3}", textH, LayerLabel, tr, btr);
    AddDbText(new Point3d(lblX, sec.Y + Model.FoundationBottomLevel * S, sec.Z), $"{Model.FoundationBottomLevel / 1000:F3}", textH, LayerLabel, tr, btr);

    // ── Section label A-A ──────────────────────────────────────────
    AddMText(new Point3d(sec.X, sec.Y + Model.GroundLevel * S + 1.5 * S, sec.Z),
      "A", textH * 2, LayerLabel, tr, btr);
    AddDbText(new Point3d(sec.X + 0.5 * S, sec.Y + Model.GroundLevel * S + 1.5 * S, sec.Z),
      "- A", textH, LayerLabel, tr, btr);
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║          E.  DIM MẶT CẮT (SECTION DIMENSIONS)                ║
  // ╚══════════════════════════════════════════════════════════════╝

  private void DrawSectionDimensionsAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var sec = new Point3d(bp.X + SectionOffsetX, bp.Y, bp.Z);
    double Lx = Model.LengthX * S;
    double h2 = Model.StepHeightH2 * S;
    double h1 = Model.StepHeightH1 * S;
    double fBot = Model.FoundationBottomLevel * S;
    double dimOff = 1.5 * S;

    // ── Horizontal Lx dim ───────────────────────────────────────────
    double dimElev = fBot - 2.5 * S;
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, dimElev + 0.5, sec.Z),
      new Point3d(sec.X - Lx / 2, dimElev, sec.Z),
      new Point3d(sec.X + Lx / 2, dimElev, sec.Z),
      LayerDimension, "Length", "Lx", tr, btr);

    // ── Vertical H2 dim ────────────────────────────────────────────
    double dimX = sec.X + Lx / 2 + dimOff;
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(dimX + 0.5, sec.Y, sec.Z),
      new Point3d(dimX, fBot, sec.Z),
      new Point3d(dimX, fBot + h2, sec.Z),
      LayerDimension, "StepHeight", "H2", tr, btr);

    // ── Vertical H1 dim ────────────────────────────────────────────
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(dimX + 0.5, sec.Y, sec.Z),
      new Point3d(dimX, fBot + h2, sec.Z),
      new Point3d(dimX, fBot + h1 + h2, sec.Z),
      LayerDimension, "StepHeight", "H1", tr, btr);

    // ── Elevation dim ──────────────────────────────────────────────
    double elevX = sec.X - Lx / 2 - 2.5 * S;
    if (Math.Abs(Model.GroundLevel - Model.FoundationBottomLevel) > 1)
    {
      AddRotatedDimensionEntity(Math.PI / 2,
        new Point3d(elevX - 0.5, sec.Y, sec.Z),
        new Point3d(elevX, fBot, sec.Z),
        new Point3d(elevX, sec.Y + Model.GroundLevel * S, sec.Z),
        LayerElevation, "Elevation", "Floor", tr, btr);
    }
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║            F.  THÉP MẶT CẮT (SECTION REBAR)                  ║
  // ╚══════════════════════════════════════════════════════════════╝

  private void DrawSectionRebarAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var sec = new Point3d(bp.X + SectionOffsetX, bp.Y, bp.Z);
    double Lx = Model.LengthX * S;
    double h2 = Model.StepHeightH2 * S;
    double h1 = Model.StepHeightH1 * S;
    double cwX = Model.ColumnWidthX * S;
    double cover = Model.Cover * S;
    double colBaseZ = Model.FoundationBottomLevel * S + h1 + h2;
    double tagH = 0.3 * S;

    // ── Bottom main rebars X (horizontal across section) ───────────
    var (rxCount, rxDia, _) = ParseRebarSpec(Model.RebarX);
    if (rxCount > 0 && rxDia > 0)
    {
      double usableX = Lx - 2 * cover;
      double spacing = usableX / (rxCount + 1);
      double rebarY = sec.Y + Model.FoundationBottomLevel * S + h2 + cover;

      for (int i = 1; i <= rxCount; i++)
      {
        double x = sec.X - Lx / 2 + cover + i * spacing;
        var ln = AddLine(new Point3d(x, rebarY, sec.Z), new Point3d(x, rebarY + 0.1, sec.Z), LayerRebar, null, tr, btr);
        ln.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);

        var tagPt = new Point3d(x + 0.4 * S, rebarY + 0.4 * S, sec.Z);
        InsertMongRebarTag(tagPt, 0, $"phi{(int)rxDia}", tr, btr);
      }
    }

    // ── Bottom main rebars Y (vertical, showing on section face) ──
    var (ryCount, ryDia, _) = ParseRebarSpec(Model.RebarY);
    if (ryCount > 0 && ryDia > 0)
    {
      double rebarY = sec.Y + Model.FoundationBottomLevel * S + h2 + cover;
      var lnY = AddLine(
        new Point3d(sec.X - Lx / 2 + cover, rebarY, sec.Z),
        new Point3d(sec.X + Lx / 2 - cover, rebarY, sec.Z),
        LayerRebar, null, tr, btr);
      lnY.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);
    }

    // ── Column rebars ───────────────────────────────────────────────
    if (Model.DrawColumnRebar)
    {
      var (stCount, stDia, stSpacing) = ParseRebarSpec(Model.StirrupRebar);
      int crCount = Model.ColumnRebarCountX;
      double crDia = Model.ColumnRebar * S;

      if (crCount > 0 && crDia > 0)
      {
        double rebarY1 = colBaseZ;
        double rebarY2 = colBaseZ + Model.GroundLevel * S;
        double spY = crCount > 1 ? (rebarY2 - rebarY1) / (crCount + 1) : 0;

        for (int i = 1; i <= crCount; i++)
        {
          double y = rebarY1 + i * spY;
          var lnCr = AddLine(
            new Point3d(sec.X - cwX / 2 + cover, y, sec.Z),
            new Point3d(sec.X + cwX / 2 - cover, y, sec.Z),
            LayerRebar, null, tr, btr);
          lnCr.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);
        }

        if (Model.LapSpliceLength > 0)
        {
          AddMText(new Point3d(sec.X, colBaseZ - 0.5 * S, sec.Z),
            $"Lap {Model.LapSpliceLength / 1000:F3}m", tagH, LayerLabel, tr, btr);
        }
      }

      // ── Stirrups ─────────────────────────────────────────────────
      if (stCount > 0 && stDia > 0 && stSpacing > 0)
      {
        double sY1 = colBaseZ;
        double sY2 = Math.Min(colBaseZ + stSpacing * S, colBaseZ + Model.GroundLevel * S);
        double sX1 = sec.X - cwX / 2;
        double sX2 = sec.X + cwX / 2;

        AddLine(new Point3d(sX1, sY1, sec.Z), new Point3d(sX2, sY1, sec.Z), LayerRebar, null, tr, btr);
        AddLine(new Point3d(sX2, sY1, sec.Z), new Point3d(sX2, sY2, sec.Z), LayerRebar, null, tr, btr);
        AddLine(new Point3d(sX2, sY2, sec.Z), new Point3d(sX1, sY2, sec.Z), LayerRebar, null, tr, btr);
        AddLine(new Point3d(sX1, sY2, sec.Z), new Point3d(sX1, sY1, sec.Z), LayerRebar, null, tr, btr);

        AddMText(new Point3d(sec.X + cwX / 2 + 0.2 * S, sY1 + 0.2 * S, sec.Z),
          $"phi{(int)stDia}@{(int)stSpacing}", tagH * 0.7, LayerLabel, tr, btr);
      }
    }

    // ── Column cross-section cut marker (IBMcCot) ──────────────────
    double mcX = sec.X + Lx / 2 + 0.5 * S;
    double mcY = colBaseZ + 0.3 * S;
    AddCircle(new Point3d(mcX, mcY, sec.Z), 0.2 * S, LayerColumn, tr, btr);
    AddCircle(new Point3d(mcX, mcY - 0.4 * S, sec.Z), 0.2 * S, LayerColumn, tr, btr);
    AddLine(new Point3d(mcX, mcY - 0.2 * S, sec.Z), new Point3d(mcX, mcY - 0.2 * S, sec.Z), LayerColumn, null, tr, btr);
    AddMText(new Point3d(mcX + 0.3 * S, mcY - 0.2 * S, sec.Z), "MC", tagH, LayerLabel, tr, btr);
  }
}
