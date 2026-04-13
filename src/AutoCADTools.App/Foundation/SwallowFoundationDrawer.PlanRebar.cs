#nullable enable

using System;
using AutoCADTools.App.Const;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // Rebar end margin: 500 mm offset from the usable rebar length.
  // This represents the standard clearance from the foundation edge.
  private const double RebarEndMargin = 500.0;

  // bp = góc dưới bên trái của móng (truyền từ DrawAtPoint sau khi đã chuyển từ tâm)
  private void DrawPlanRebarAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    // ── 1. Chuyển đổi kích thước mm → drawing units ──────────────────────────
    var Lx = Model.LengthX * S;
    var Ly = Model.LengthY * S;

    // ── 2. Parse qui cách thép ─────────────────────────────────────────────────
    var (priDia, priSpacing) = ParseRebarSpec(Model.RebarX);
    var (secDia, secSpacing) = ParseRebarSpec(Model.RebarY);

    if (priDia <= 0 || secDia <= 0)
      return;

    // ── Thanh Primary: bp + (0, Ly/4), không xoay ──────────────────────────
    var pt1 = new Point3d(bp.X, bp.Y + Ly / 4, 0);
    var L = (Model.LengthX - 2 * Model.Cover) * S;
    var L1 = Math.Max(L - RebarEndMargin * S, 0);
    IBR_Mong(pt1, L, L1, "1", FormatRebarSpec(priDia, priSpacing), tr, btr, 0);

    // ── Thanh Secondary: bp + (Lx/4, 0), xoay 90° ──────────────────────────
    var pt2 = new Point3d(bp.X + Lx / 4, bp.Y, 0);
    var Lsec = (Model.LengthY - 2 * Model.Cover) * S;
    var L1sec = Math.Max(Lsec - RebarEndMargin * S, 0);
    IBR_Mong(pt2, Lsec, L1sec, "2", FormatRebarSpec(secDia, secSpacing), tr, btr, Math.PI / 2);
  }
}
