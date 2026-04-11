#nullable enable

using System;
using System.Globalization;
using System.Text;
using AutoCADTools.App.Const;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // ╔══════════════════════════════════════════════════════════════════════════════╗
  // ║                   C.  THÉP MẶT BẰNG (PLAN REBAR)                        ║
  // ╚══════════════════════════════════════════════════════════════════════════════╝

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

    // Spec string cho SH/FI: "/g{phi}a{spacing}"
    var priSpec = priSpacing > 0
        ? $"/g{priDia.ToString(CultureInfo.InvariantCulture)}a{priSpacing.ToString(CultureInfo.InvariantCulture)}"
        : $"/g{priDia.ToString(CultureInfo.InvariantCulture)}";
    var secSpec = secSpacing > 0
        ? $"/g{secDia.ToString(CultureInfo.InvariantCulture)}a{secSpacing.ToString(CultureInfo.InvariantCulture)}"
        : $"/g{secDia.ToString(CultureInfo.InvariantCulture)}";

    // ── Thanh Primary: bp + (0, Ly/4), không xoay ──────────────────────────
    var pt1 = new Point3d(bp.X, bp.Y + Ly / 4, 0);
    var L = (Model.LengthX - 2 * Model.Cover) * S;
    var L1 = Math.Max((Model.LengthX - 2 * Model.Cover - 500) * S, 0);
    _ = IBR_Mong(pt1, L, L1, "1", priSpec, tr, btr);

    // ── Thanh Secondary: bp + (Lx/4, 0), xoay 90° ─────────────────────
    var pt2 = new Point3d(bp.X + Lx / 4, bp.Y, 0);
    var Lsec = (Model.LengthY - 2 * Model.Cover) * S;
    var L1sec = Math.Max((Model.LengthY - 2 * Model.Cover - 500) * S, 0);
    var br2 = IBR_Mong(pt2, Lsec, L1sec, "2", secSpec, tr, btr);
    if (br2 != null)
      Matrix_Rotation(br2, Math.PI / 2, pt2);
  }
}
