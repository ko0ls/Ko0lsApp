#nullable enable

using System;
using AutoCADTools.App.Const;
using AutoCADTools.App.Enums;
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
    CreatePolyline([cc1, cc2, cc3, cc4], Layer.Column, tr, btr);

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
  // ║  DrawPlanDimensionsAtPoint: VẼ KÍCH THƯỚC MẶT BẰNG (PLAN DIMENSIONS)    ║
  // ╠══════════════════════════════════════════════════════════════════════════════╣
  // ║  Hệ tọa độ: bp = tâm móng, tất cả tọa độ tính tương đối so với bp.     ║
  // ║                                                                              ║
  // ║  Level1 — mép móng → đường trục:                                          ║
  // ║    Phương X (hàng trên, nằm ngang):                                        ║
  // ║      2 dim: (mép trái cột → trục dọc) + (trục dọc → mép phải cột)      ║
  // ║    Phương Y (hàng bên phải, thẳng đứng):                                  ║
  // ║      2 dim: (mép dưới cột → trục ngang) + (trục ngang → mép trên cột)  ║
  // ║                                                                              ║
  // ║  Level2 — Lx / Ly đầy đủ:                                                  ║
  // ║    Phương X: đo mép trái → mép phải đài móng                             ║
  // ║    Phương Y: đo mép dưới → mép trên đài móng                             ║
  // ╚══════════════════════════════════════════════════════════════════════════════╝
  private void DrawPlanDimensionsAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var lx = Model.LengthX * S;
    var ly = Model.LengthY * S;
    var padW = Model.ConcretePadExtension * S;
    var axisX = Model.AxisPositionX * S;
    var axisY = Model.AxisPositionY * S;

    var pointFootingLeftX = bp.Add(Vector3d.XAxis.Negate().MultiplyBy(lx / 2))
      .Add(Vector3d.YAxis.Negate().MultiplyBy(ly / 2 + padW));
    var pointAxisX = pointFootingLeftX.Add(Vector3d.XAxis.MultiplyBy(axisX));
    var pointFootingRightX = pointFootingLeftX.Add(Vector3d.XAxis.MultiplyBy(lx));
    var pointPadLeftX = pointFootingLeftX.Add(Vector3d.XAxis.Negate().MultiplyBy(padW));
    var pointPadRightX = pointFootingRightX.Add(Vector3d.XAxis.MultiplyBy(padW));

    AddRotatedDimensionEntity(DimLevel.Level1, pointFootingLeftX, pointPadLeftX, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100, isReverse: true);

    AddRotatedDimensionEntity(DimLevel.Level1, pointFootingLeftX, pointAxisX, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100);

    AddRotatedDimensionEntity(DimLevel.Level1, pointAxisX, pointFootingRightX, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100);

    AddRotatedDimensionEntity(DimLevel.Level1, pointFootingRightX, pointPadRightX, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100);

    AddRotatedDimensionEntity(DimLevel.Level2, pointFootingLeftX, pointFootingRightX, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100);

    // ── Ly: hàng dimension bên phải mặt bằng (4× Level1 + 1× Level2) ───────────
    var pointFootingTopY = bp.Add(Vector3d.XAxis.MultiplyBy(lx / 2))
      .Add(Vector3d.XAxis.MultiplyBy(padW))
      .Add(Vector3d.YAxis.MultiplyBy(ly / 2));
    var pointAxisY = pointFootingTopY.Add(Vector3d.YAxis.Negate().MultiplyBy(axisY));
    var pointFootingBottomY = pointFootingTopY.Add(Vector3d.YAxis.Negate().MultiplyBy(ly));
    var pointPadBottomY = pointFootingBottomY.Add(Vector3d.YAxis.Negate().MultiplyBy(padW));
    var pointPadTopY = pointFootingTopY.Add(Vector3d.YAxis.MultiplyBy(padW));

    AddRotatedDimensionEntity(DimLevel.Level1, pointFootingTopY, pointPadTopY, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100);
    AddRotatedDimensionEntity(DimLevel.Level1, pointFootingTopY, pointAxisY, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100, isReverse: true);
    AddRotatedDimensionEntity(DimLevel.Level1, pointAxisY, pointFootingBottomY, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100, isReverse: true);
    AddRotatedDimensionEntity(DimLevel.Level1, pointFootingBottomY, pointPadBottomY, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100, isReverse: true);
    AddRotatedDimensionEntity(DimLevel.Level2, pointFootingTopY, pointFootingBottomY, Layer.Dimension, tr, btr,
      scaleFactor: 1 / S, dimScale: TitleS / 100,isReverse: true);
  }
}
