#nullable enable

using System;
using AutoCADTools.App.Const;
using AutoCADTools.Core.Utils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Hatch = AutoCADTools.App.Const.Hatch;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // ╔══════════════════════════════════════════════════════════════╗
  // ║                    A.  MẶT BẰNG (PLAN)                       ║
  // ╚══════════════════════════════════════════════════════════════╝
  private void DrawPlanAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var lx = Model.LengthX * S;
    var ly = Model.LengthY * S;
    var cpX = Model.ColumnPositionX * S;
    var cpY = Model.ColumnPositionY * S;
    var padW = Model.ConcretePadExtension * S;
    var cwX = Model.ColumnWidthX * S;
    var cwY = Model.ColumnWidthY * S;
    var axisX = Model.AxisPositionX * S;
    var axisY = Model.AxisPositionY * S;
    var topLeft = new Point3d(bp.X - lx / 2, bp.Y + ly / 2, bp.Z);

    var p1 = new Point3d(bp.X - lx / 2, bp.Y - ly / 2, bp.Z);
    var p2 = new Point3d(bp.X + lx / 2, bp.Y - ly / 2, bp.Z);
    var p3 = new Point3d(bp.X + lx / 2, bp.Y + ly / 2, bp.Z);
    var p4 = new Point3d(bp.X - lx / 2, bp.Y + ly / 2, bp.Z);
    CreatePolyline([p1, p2, p3, p4], Layer.Outline, tr, btr);

    var pp1 = new Point3d(p1.X - padW, p1.Y - padW, bp.Z);
    var pp2 = new Point3d(p2.X + padW, p2.Y - padW, bp.Z);
    var pp3 = new Point3d(p3.X + padW, p3.Y + padW, bp.Z);
    var pp4 = new Point3d(p4.X - padW, p4.Y + padW, bp.Z);
    CreatePolyline([pp1, pp2, pp3, pp4], Layer.ConcreteHatch, tr, btr);

    /* Axis */
    AddLine(
      new Point3d(bp.X - lx / 2 - padW - Numeric.ExtendCenterLine * TitleS, bp.Y + ly / 2 - axisY, bp.Z),
      new Point3d(bp.X + lx / 2 + padW + Numeric.ExtendCenterLine * TitleS, bp.Y + ly / 2 - axisY, bp.Z),
      Layer.Axis, null, tr, btr);

    AddLine(
      new Point3d(bp.X - lx / 2 + axisX, bp.Y + ly / 2 + padW + Numeric.ExtendCenterLine * TitleS, bp.Z),
      new Point3d(bp.X - lx / 2 + axisX, bp.Y - ly / 2 - padW - Numeric.ExtendCenterLine * TitleS, bp.Z),
      Layer.Axis, null, tr, btr);

    /* Column */
    var cpBase = topLeft.Add(Vector3d.XAxis.MultiplyBy(cpX)).Add(Vector3d.YAxis.Negate().MultiplyBy(cpY));

    if (Model.IsRectangularColumn) {
      var c1 = new Point3d(cpBase.X - cwX / 2, cpBase.Y - cwY / 2, cpBase.Z);
      var c2 = new Point3d(cpBase.X + cwX / 2, cpBase.Y - cwY / 2, cpBase.Z);
      var c3 = new Point3d(cpBase.X + cwX / 2, cpBase.Y + cwY / 2, cpBase.Z);
      var c4 = new Point3d(cpBase.X - cwX / 2, cpBase.Y + cwY / 2, cpBase.Z);

      var colPl = CreatePolyline([c1, c2, c3, c4], Layer.Column, tr, btr);
      AddHatch(new ObjectIdCollection() { colPl.ObjectId }, "DOTS", Numeric.HatchPatternScale * TitleS, Layer.Column,
        tr, btr);
    }
    else // Circular column
    {
      var cRadius = Math.Min(cwX, cwY) / 2;
      using var circ = new Circle(cpBase, Vector3d.ZAxis, cRadius);
      circ.Layer = Layer.Column;
      btr.AppendEntity(circ);
      tr.AddNewlyCreatedDBObject(circ, true);

      var circIds = new ObjectIdCollection { circ.ObjectId };
      AddHatch(circIds, Hatch.Dot, Numeric.HatchPatternScale * TitleS, Layer.Column, tr, btr);
    }

    /* Pedestal extend */
    var pwx = cwX + Numeric.PedestalExtend * 2 * S;
    var pwy = Model.IsRectangularColumn ? cwY + Numeric.PedestalExtend * 2 * S : pwx;
    var cc1 = new Point3d(cpBase.X - pwx / 2, cpBase.Y - pwy / 2, cpBase.Z);
    var cc2 = new Point3d(cpBase.X + pwx / 2, cpBase.Y - pwy / 2, cpBase.Z);
    var cc3 = new Point3d(cpBase.X + pwx / 2, cpBase.Y + pwy / 2, cpBase.Z);
    var cc4 = new Point3d(cpBase.X - pwx / 2, cpBase.Y + pwy / 2, cpBase.Z);
    if (cc1.X.IsSmaller(p1.X)) cc1 = new Point3d(p1.X, cc1.Y, cc1.Z);
    if (cc1.Y.IsSmaller(p1.Y)) cc1 = new Point3d(cc1.X, p1.Y, cc1.Z);
    if (cc2.X.IsGreater(p2.X)) cc2 = new Point3d(p2.X, cc2.Y, cc2.Z);
    if (cc2.Y.IsSmaller(p2.Y)) cc2 = new Point3d(cc2.X, p2.Y, cc2.Z);
    if (cc3.X.IsGreater(p3.X)) cc3 = new Point3d(p3.X, cc3.Y, cc3.Z);
    if (cc3.Y.IsGreater(p3.Y)) cc3 = new Point3d(cc3.X, p3.Y, cc3.Z);
    if (cc4.X.IsSmaller(p4.X)) cc4 = new Point3d(p4.X, cc4.Y, cc4.Z);
    if (cc4.Y.IsGreater(p4.Y)) cc4 = new Point3d(cc4.X, p4.Y, cc4.Z);
    var pedestalExtendPl = CreatePolyline([cc1, cc2, cc3, cc4], Layer.Column, tr, btr);

    /* Chamfer */
    if (Model.StepHeightH1.IsGreater(Model.StepHeightH2)) {
      AddLine(p1, cc1, Layer.Outline, null, tr, btr);
      AddLine(p2, cc2, Layer.Outline, null, tr, btr);
      AddLine(p3, cc3, Layer.Outline, null, tr, btr);
      AddLine(p4, cc4, Layer.Outline, null, tr, btr);
    }

    // ── Bước 9: Ghi nhãn tên móng ────────────────────────────────────────────────
    //   Nhãn đặt tại phía dưới trung tâm của mặt bằng móng
    //   Format: "TênMóng(SL:dd)" ví dụ: "M1(SL:01)".
    var label = $"{Model.FoundationName}(SL:{Model.Quantity:D2})";
    AddDbText(
      new Point3d(bp.X,
        bp.Y - ly / 2 - padW - Numeric.ExtendCenterLine * TitleS - Numeric.GapBetweenDimensions * TitleS -
        Numeric.DimensionGap * 2 * TitleS - Numeric.GapBetweenDimensions * TitleS, bp.Z),
      label, 5 * TitleS, Layer.LabelCyan, tr, btr, AttachmentPoint.TopCenter);
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║               B.  DIM  MẶT BẰNG (PLAN DIMENSIONS)             ║
  // ╚══════════════════════════════════════════════════════════════╝

  // ╔══════════════════════════════════════════════════════════════════════════════╗
  // ║  DrawPlanDimensionsAtPoint: VẼ KÍCH THƯỚC MẶT BẰNG (PLAN DIMENSIONS)          ║
  // ╠══════════════════════════════════════════════════════════════════════════════╣
  // ║  Hệ tọa độ: bp = tâm móng, tất cả tọa độ tính tương đối so với bp.           ║
  // ║                                                                                ║
  // ║  Kích thước chiều dài Lx (phía trên, góc = 0, nằm ngang):                   ║
  // ║    • Dim đầy đủ (full): đo từ mép trái đến mép phải đài móng, gắn XData    ║
  // ║      "Length"/"Lx".                                                            ║
  // ║    • Dim phần dư trái (left stub): từ mép trái đài → mép trái cột.          ║
  // ║    • Dim chiều rộng cột (ColWidth): từ mép trái → mép phải cột, gắn XData    ║
  // ║      "ColWidth"/"Cx".                                                          ║
  // ║    • Dim phần dư phải (right stub): từ mép phải cột → mép phải đài.         ║
  // ║                                                                                ║
  // ║  Kích thước chiều rộng Ly (phía phải, góc = PI/2, thẳng đứng):             ║
  // ║    • Dim đầy đủ (full): đo từ mép dưới đến mép trên đài móng, gắn XData     ║
  // ║      "Length"/"Ly".                                                            ║
  // ║    • Dim phần dư dưới (bottom stub): từ mép dưới đài → mép dưới cột.          ║
  // ║    • Dim chiều rộng cột (ColWidth): từ mép dưới → mép trên cột, gắn XData    ║
  // ║      "ColWidth"/"Cy".                                                          ║
  // ║    • Dim phần dư trên (top stub): từ mép trên cột → mép trên đài.           ║
  // ║                                                                                ║
  // ║  Kích thước lệch trục (Axis offset dims):                                    ║
  // ║    • OffsetX: khoảng lệch trục Y so với tâm móng theo phương X.              ║
  // ║    • OffsetY: khoảng lệch trục X so với tâm móng theo phương Y.              ║
  // ║    Chỉ vẽ khi |AxisPositionX/Y| > 0.1 (mm).                                   ║
  // ║                                                                                ║
  // ║  Cơ chế AddRotatedDimensionEntity:                                           ║
  // ║    angle = 0          → dim nằm ngang (đo theo phương X)                     ║
  // ║    angle = PI/2       → dim thẳng đứng (đo theo phương Y)                    ║
  // ║    xLine1, xLine2     → 2 điểm đầu mút cần đo (AutoCAD chiếu vuông góc       ║
  // ║                          lên đường dim)                                        ║
  // ║    dimPoint           → vị trí đặt đường dimension (dòng kích thước)         ║
  // ║    TextPosition offset 0.5 đơn vị để tránh đè lên đường dim                  ║
  // ╚══════════════════════════════════════════════════════════════════════════════╝
  /*private void DrawPlanDimensionsAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var lx = Model.LengthX * S;
    var ly = Model.LengthY * S;
    var axis = Model.AxisPositionX * S;
    var axisY = Model.AxisPositionY * S;
    var topLeft = new Point3d(bp.X - lx / 2, bp.Y + ly / 2, bp.Z);


    // ── KÍCH THƯỚC CHIỀU DÀI Lx (hàng trên, nằm ngang, angle = 0) ───────────────
    // Vị trí đường dim: yOut = bp.Y + Ly/2 + dimOff → phía trên đài móng.
    // Đo đầy đủ từ mép trái đài (bp - Lx/2) đến mép phải đài (bp + Lx/2).
    // Gắn XData "Length"/"Lx" để nhận diện loại kích thước.
    double yOut = bp.Y + ly / 2 + dimOff;
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yOut + 0.5, bp.Z),
      new Point3d(bp.X - lx / 2, yOut, bp.Z),
      new Point3d(bp.X + lx / 2, yOut, bp.Z),
      LayerDimension, "Length", "Lx", tr, btr);

    // ── Dim phần dư trái (left stub): mép trái đài → mép trái cột ───────────────
    // yIn = bp.Y + Ly/2 + shortOff → nằm giữa dim đầy đủ và đài móng.
    // xLine1 = mép trái đài, xLine2 = mép trái cột (xCol1).
    double yIn = bp.Y + ly / 2 + shortOff;
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yIn + 0.5, bp.Z),
      new Point3d(bp.X - lx / 2, yIn, bp.Z),
      new Point3d(xCol1, yIn, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── Dim chiều rộng cột theo X (ColWidth Cx): mép trái → mép phải cột ─────────
    // Gắn XData "ColWidth"/"Cx" để truy vấn bề rộng cột từ bản vẽ.
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yIn + 0.5, bp.Z),
      new Point3d(xCol1, yIn, bp.Z),
      new Point3d(xCol2, yIn, bp.Z),
      LayerDimension, "ColWidth", "Cx", tr, btr);

    // ── Dim phần dư phải (right stub): mép phải cột → mép phải đài ───────────────
    // xLine1 = mép phải cột (xCol2), xLine2 = mép phải đài.
    AddRotatedDimensionEntity(0,
      new Point3d(bp.X, yIn + 0.5, bp.Z),
      new Point3d(xCol2, yIn, bp.Z),
      new Point3d(bp.X + lx / 2, yIn, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── KÍCH THƯỚC CHIỀU RỘNG Ly (hàng bên phải, thẳng đứng, angle = PI/2) ──────
    // Vị trí đường dim: xOut = bp.X + Lx/2 + dimOff → phía phải đài móng.
    // Đo đầy đủ từ mép dưới đài (bp - Ly/2) đến mép trên đài (bp + Ly/2).
    // Gắn XData "Length"/"Ly".
    double xOut = bp.X + lx / 2 + dimOff;
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xOut + 0.5, bp.Y, bp.Z),
      new Point3d(xOut, bp.Y - ly / 2, bp.Z),
      new Point3d(xOut, bp.Y + ly / 2, bp.Z),
      LayerDimension, "Length", "Ly", tr, btr);

    // ── Dim phần dư dưới (bottom stub): mép dưới đài → mép dưới cột ──────────────
    // xIn = bp.X + Lx/2 + shortOff → nằm giữa dim đầy đủ và đài móng.
    // xLine1 = mép dưới đài, xLine2 = mép dưới cột (yCol1).
    double xIn = bp.X + lx / 2 + shortOff;
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xIn + 0.5, bp.Y, bp.Z),
      new Point3d(xIn, bp.Y - ly / 2, bp.Z),
      new Point3d(xIn, yCol1, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── Dim chiều rộng cột theo Y (ColWidth Cy): mép dưới → mép trên cột ────────
    // Gắn XData "ColWidth"/"Cy".
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xIn + 0.5, bp.Y, bp.Z),
      new Point3d(xIn, yCol1, bp.Z),
      new Point3d(xIn, yCol2, bp.Z),
      LayerDimension, "ColWidth", "Cy", tr, btr);

    // ── Dim phần dư trên (top stub): mép trên cột → mép trên đài ────────────────
    // xLine1 = mép trên cột (yCol2), xLine2 = mép trên đài.
    AddRotatedDimensionEntity(Math.PI / 2,
      new Point3d(xIn + 0.5, bp.Y, bp.Z),
      new Point3d(xIn, yCol2, bp.Z),
      new Point3d(xIn, bp.Y + ly / 2, bp.Z),
      LayerDimension, null, null, tr, btr);

    // ── KÍCH THƯỚC LỆCH TRỤC (Axis Offset) ───────────────────────────────────────
    // OffsetX: khoảng lệch tâm cột theo phương X (so với tâm móng bp).
    //   yOff = bp.Y - Ly/2 - dimOff → phía dưới đài móng, ngoài bản đế.
    //   Đo từ tâm móng (bp) đến tâm cột theo X (bp + cpX).
    //   Chỉ vẽ khi |Model.AxisPositionX| > 0.1 mm (tránh dim 0).
    if (Math.Abs(Model.AxisPositionX) > 0.1) {
      double yOff = bp.Y - ly / 2 - dimOff;
      AddRotatedDimensionEntity(0,
        new Point3d(bp.X, yOff - 0.5, bp.Z),
        new Point3d(bp.X, yOff, bp.Z),
        new Point3d(bp.X + cpX, yOff, bp.Z),
        LayerDimension, "AxisOffset", "OffsetX", tr, btr);
    }

    // OffsetY: khoảng lệch tâm cột theo phương Y (so với tâm móng bp).
    //   xOff = bp.X - Lx/2 - dimOff → phía trái đài móng, ngoài bản đế.
    //   Đo từ tâm móng (bp) đến tâm cột theo Y (bp + cpY).
    //   Chỉ vẽ khi |Model.AxisPositionY| > 0.1 mm.
    if (Math.Abs(Model.AxisPositionY) > 0.1) {
      double xOff = bp.X - lx / 2 - dimOff;
      AddRotatedDimensionEntity(Math.PI / 2,
        new Point3d(xOff - 0.5, bp.Y, bp.Z),
        new Point3d(xOff, bp.Y, bp.Z),
        new Point3d(xOff, bp.Y + cpY, bp.Z),
        LayerDimension, "AxisOffset", "OffsetY", tr, btr);
    }
  }*/

  // ╔══════════════════════════════════════════════════════════════╗
  // ║            C.  THÉP  MẶT BẰNG (PLAN REBAR)                    ║
  // ╚══════════════════════════════════════════════════════════════╝

  // ╔══════════════════════════════════════════════════════════════════════════════╗
  // ║  DrawPlanRebarAtPoint: VẼ THÉP MẶT BẰNG (FOUNDATION PLAN REBAR)              ║
  // ╠══════════════════════════════════════════════════════════════════════════════╣
  // ║  Hệ tọa độ: bp = tâm móng, scale S = Model.Scale (mm → drawing units)       ║
  // ║  cover     = lớp bê tông bảo vệ (mm → × S)                                 ║
  // ║                                                                                ║
  // ║  Rebar X – Thép theo phương X (thanh nằm ngang, chạy dọc theo trục X):  ║
  // ║    • Vòng lặp i = 1..rxCount:                                               ║
  // ║      y_i = bp.Y - Ly/2 + cover + i × spacing                                ║
  // ║      spacing = (Ly - 2×cover) / (rxCount + 1) → phân bố đều, cách đều     ║
  // ║                thanh 1 thanh cuối mỗi thanh 1 khoảng spacing.              ║
  // ║      Đoạn thép: từ (bp.X - Lx/2 + cover) đến (bp.X + Lx/2 - cover)         ║
  // ║                → thanh nằm ngang, dài bằng Lx trừ 2×cover.               ║
  // ║      Màu đỏ (255,0,0) phân biệt với đường kết cấu.                         ║
  // ║      Block nhãn IBR_Mong đặt tại (bp.X + 0.5×S, y_i + 0.5×S),              ║
  // ║      góc xoay = 0 (nhãn nằm ngang).                                        ║
  // ║                                                                                ║
  // ║  Rebar Y – Thép theo phương Y (thanh thẳng đứng, chạy dọc theo trục Y): ║
  // ║    • Vòng lặp i = 1..ryCount:                                               ║
  // ║      x_i = bp.X - Lx/2 + cover + i × spacing                                ║
  // ║      spacing = (Lx - 2×cover) / (ryCount + 1)                               ║
  // ║      Đoạn thép: từ (bp.Y - Ly/2 + cover) đến (bp.Y + Ly/2 - cover)         ║
  // ║                → thanh thẳng đứng, cao bằng Ly trừ 2×cover.               ║
  // ║      Block nhãn đặt tại (x_i + 0.5×S, bp.Y + 0.5×S),                       ║
  // ║      góc xoay = PI/2 (nhãn thẳng đứng).                                    ║
  // ║                                                                                ║
  // ║  ParseRebarSpec: phân tích chuỗi qui cách thép từ Model.                     ║
  // ║    "2a150" → (count=2, dia=150, spacing=150)                                 ║
  // ║    "1d25"  → (count=1, dia=25, spacing=0)                                   ║
  // ╚══════════════════════════════════════════════════════════════════════════════╝
  /*private void DrawPlanRebarAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    // ── Chuyển đổi kích thước mm → drawing units ─────────────────────────────────
    double Lx = Model.LengthX * S;
    double Ly = Model.LengthY * S;
    double cover = Model.Cover * S; // lớp bê tông bảo vệ (mm → drawing units)

    // ── Phân tích chuỗi qui cách thép từ Model ───────────────────────────────────
    // RebarX: qui cách thép theo phương X (Model.RebarX)
    // RebarY: qui cách thép theo phương Y (Model.RebarY)
    var (rxCount, rxDia, _) = ParseRebarSpec(Model.RebarX);
    var (ryCount, ryDia, _) = ParseRebarSpec(Model.RebarY);
    if (rxCount <= 0 && ryCount <= 0) return; // không có thép → thoát

    // ── REBAR X: Thép nằm ngang (thanh chạy dọc theo trục X) ───────────────────
    // Đặt thép từ i=1 đến rxCount:
    //   Chiều cao khả dụng: usableY = Ly - 2×cover (trừ lớp bảo vệ 2 phía).
    //   Khoảng cách giữa các thanh: spacing = usableY / (rxCount + 1).
    //   Tọa độ Y của thanh i: y = bp.Y - Ly/2 + cover + i × spacing
    //     → Thanh 1 cách mép dưới đài 1 khoảng spacing.
    //     → Thanh cuối cách mép trên đài 1 khoảng spacing.
    //   Chiều dài thanh: bp.X - Lx/2 + cover → bp.X + Lx/2 - cover (trừ cover 2 đầu).
    if (rxCount > 0 && rxDia > 0) {
      double usableY = Ly - 2 * cover;
      double spacing = usableY / ( rxCount + 1 );

      for (int i = 1 ; i <= rxCount ; i++) {
        double y = bp.Y - Ly / 2 + cover + i * spacing;
        var ln = AddLine(
          new Point3d(bp.X - Lx / 2 + cover, y, bp.Z),
          new Point3d(bp.X + Lx / 2 - cover, y, bp.Z),
          LayerRebar, null, tr, btr);
        ln.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0); // màu đỏ

        // Gắn block nhãn móng thép IBR_Mong tại góc phải-trên của thanh.
        // Vị trí: (bp.X + 0.5×S, y + 0.5×S) → đặt nhãn về phía trên-phải thanh.
        // Góc xoay = 0 → nhãn nằm ngang.
        var tagPt = new Point3d(bp.X + 0.5 * S, y + 0.5 * S, bp.Z);
        InsertMongRebarTag(tagPt, 0, $"phi{(int) rxDia}", tr, btr);
      }
    }

    // ── REBAR Y: Thép thẳng đứng (thanh chạy dọc theo trục Y) ───────────────────
    // Đặt thép từ i=1 đến ryCount:
    //   Chiều rộng khả dụng: usableX = Lx - 2×cover.
    //   spacing = usableX / (ryCount + 1).
    //   Tọa độ X của thanh i: x = bp.X - Lx/2 + cover + i × spacing.
    //   Chiều cao thanh: bp.Y - Ly/2 + cover → bp.Y + Ly/2 - cover.
    //   Block nhãn góc xoay = PI/2 → nhãn thẳng đứng (phù hợp với thanh dọc).
    if (ryCount > 0 && ryDia > 0) {
      double usableX = Lx - 2 * cover;
      double spacing = usableX / ( ryCount + 1 );

      for (int i = 1 ; i <= ryCount ; i++) {
        double x = bp.X - Lx / 2 + cover + i * spacing;
        var ln = AddLine(
          new Point3d(x, bp.Y - Ly / 2 + cover, bp.Z),
          new Point3d(x, bp.Y + Ly / 2 - cover, bp.Z),
          LayerRebar, null, tr, btr);
        ln.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0); // màu đỏ

        // Gắn nhãn tại góc trên-phải thanh: (x + 0.5×S, bp.Y + 0.5×S).
        var tagPt = new Point3d(x + 0.5 * S, bp.Y + 0.5 * S, bp.Z);
        InsertMongRebarTag(tagPt, Math.PI / 2, $"phi{(int) ryDia}", tr, btr);
      }
    }
  }*/
}