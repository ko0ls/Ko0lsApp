#nullable enable

using System;
using AutoCADTools.App.Const;
using AutoCADTools.App.Enums;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Hatch = AutoCADTools.App.Const.Hatch;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // ╔══════════════════════════════════════════════════════════════╗
  // ║                    D.  MẶT CẮT (SECTION)                       ║
  // ╚══════════════════════════════════════════════════════════════╝

  private const double SectionOffsetX = 8.0; // drawing-units offset to the right of plan

  // ╔══════════════════════════════════════════════════════════════════════════════╗
  // ║  DrawSectionAtPoint: VẼ MẶT CẮT MÓNG (FOUNDATION SECTION A-A)              ║
  // ╠══════════════════════════════════════════════════════════════════════════════╣
  // ║  Vị trí mặt cắt:                                                            ║
  // ║    sec = bp + (SectionOffsetX, 0, 0) = bp + (8.0, 0, 0)                     ║
  // ║    → Mặt cắt được vẽ cách plan 8.0 đơn vị về phía phải.                     ║
  // ║    → Hệ tọa độ mặt cắt: sec.X = bp.X + 8.0, sec.Y = bp.Y, sec.Z = bp.Z     ║
  // ║                                                                                ║
  // ║  Scale: S = Model.Scale (mm → drawing units)                                ║
  // ║                                                                                ║
  // ║  Trục Y của mặt cắt = cao độ (elevation):                                    ║
  // ║    Model.FoundationBottomLevel × S = đáy móng (mặt phẳng nằm ngang)         ║
  // ║    h2 = StepHeightH2 × S     = chiều cao bậc dưới (H2)                      ║
  // ║    h1 = StepHeightH1 × S     = chiều cao bậc trên (H1)                      ║
  // ║    colBaseZ = FoundationBottomLevel×S + h1 + h2 = đỉnh bậc H1 (đáy cột)   ║
  // ║                                                                                ║
  // ║  Thứ tự vẽ (từ dưới lên theo cao độ):                                       ║
  // ║    1. Bản đế bê tông (AR-CONC): fBot-padT → fBot, Lx+2×padW                ║
  // ║    2. Bậc H2 (hình chữ nhật): fBot → step2Top (= fBot + h2)                 ║
  // ║    3. Bậc H1 / vát mặt (chamfer):                                           ║
  // ║         • H1 != H2: hình thang (trapezoid) → mép cột vát xiên 0.3×S        ║
  // ║         • H1 == H2: chỉ có đường ngang đỉnh bậc (không vát)              ║
  // ║    4. Cột stub (4 đoạn thẳng): từ colBaseZ đến colBaseZ + 0.5×S           ║
  // ║    5. Nhãn cao độ (CDA): MC, đáy móng, cao độ sàn                          ║
  // ║    6. Nhãn mặt cắt A-A                                                      ║
  // ╚══════════════════════════════════════════════════════════════════════════════╝
  private void DrawSectionAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    // ── Xác định gốc tọa độ mặt cắt ─────────────────────────────────────────────
    // sec = bp dịch sang phải 8.0 đơn vị → mặt cắt nằm bên phải mặt bằng plan.
    // sec.Y và sec.Z giữ nguyên bp.Y và bp.Z để cùng mặt phẳng với plan.
    var sec = new Point3d(bp.X + SectionOffsetX, bp.Y, bp.Z);

    // ── Chuyển đổi kích thước mm → drawing units ─────────────────────────────────
    double Lx = Model.LengthX * S; // chiều dài móng
    double h2 = Model.StepHeightH2 * S; // chiều cao bậc dưới H2
    double h1 = Model.StepHeightH1 * S; // chiều cao bậc trên H1
    double cwX = Model.ColumnWidthX * S; // bề rộng cột (phương X)
    double cwY = Model.ColumnWidthY * S; // bề rộng cột (phương Y, chỉ dùng trên plan)
    double padT = Model.ConcretePadThickness * S; // chiều dày bản đế
    double padW = Model.ConcretePadExtension * S; // phần nhô bản đế so với đài

    // fBot: cao độ đáy móng (= FoundationBottomLevel mm → drawing units)
    // colBaseZ: cao độ đỉnh bậc H1 = đáy cột = fBot + h1 + h2
    double colBaseZ = Model.FoundationBottomLevel * S + h1 + h2;
    double fBot = Model.FoundationBottomLevel * S;
    double textH = 0.25 * S; // chiều cao chữ

    // ── Bước 1: Vẽ bản đế bê tông (AR-CONC) ───────────────────────────────────
    // Hình chữ nhật đáy móng (trước khi đổ bê tông, làm cốp pha):
    //   Mặt trên: y = fBot         (cao độ đáy móng)
    //   Mặt dưới: y = fBot - padT  (thấp hơn đáy móng một khoảng padT)
    //   Chiều rộng: Lx + 2×padW (rộng hơn đài móng mỗi phía padW)
    // Tọa độ 4 đỉnh (theo chiều kim đồng hồ từ góc dưới-trái):
    //   (sec.X - Lx/2 - padW, fBot - padT) → dưới-trái ngoài
    //   (sec.X + Lx/2 + padW, fBot - padT) → dưới-phải ngoài
    //   (sec.X + Lx/2 + padW, fBot)        → trên-phải ngoài
    //   (sec.X - Lx/2 - padW, fBot)        → trên-trái ngoài
    var padPL = CreatePolyline(
    [
      new Point3d(sec.X - Lx / 2 - padW, fBot - padT, sec.Z),
      new Point3d(sec.X + Lx / 2 + padW, fBot - padT, sec.Z),
      new Point3d(sec.X + Lx / 2 + padW, fBot, sec.Z),
      new Point3d(sec.X - Lx / 2 - padW, fBot, sec.Z)
    ], Layer.Outline, tr, btr);
    AddHatch(new ObjectIdCollection() { padPL.ObjectId }, Hatch.Concrete, 20 * S, Layer.ConcreteHatch, tr, btr);

    // ── Bước 2: Vẽ bậc H2 (hình chữ nhật đứng) ─────────────────────────────────
    // Bậc dưới H2: hình chữ nhật chồng lên đáy móng.
    //   Đáy: y = fBot (mặt trên bản đế)
    //   Đỉnh: y = step2Top = fBot + h2
    //   Chiều rộng: bằng đúng Lx (không nhô như bản đế)
    double step2Top = fBot + h2;
    var step2PL = CreatePolyline(
    [
      new Point3d(sec.X - Lx / 2, fBot, sec.Z),
      new Point3d(sec.X + Lx / 2, fBot, sec.Z),
      new Point3d(sec.X + Lx / 2, step2Top, sec.Z),
      new Point3d(sec.X - Lx / 2, step2Top, sec.Z)
    ], Layer.Outline, tr, btr);
    AddHatch(new ObjectIdCollection() { step2PL.ObjectId }, "", 1, Layer.Outline, tr, btr);

    // ── Bước 3: Vẽ bậc H1 / vát mặt (chamfer) ─────────────────────────────────
    // step1Top = cao độ đỉnh bậc H1 = fBot + h2 + h1
    // Khi H1 != H2: vẽ hình thang (trapezoid) với phần vát 0.3×S:
    //   Đỉnh trên bậc H1 thu hẹp từ Lx → cwX (vát xiên vào giữa).
    //   Góc vát: mỗi bên thu hẹp (Lx - cwX) / 2 về phía trong.
    // Khi H1 == H2: chỉ vẽ đường ngang đỉnh bậc (không vát).
    double step1Top = step2Top + h1;

    if (Math.Abs(h1 - h2) > 0.001) {
      // ── Hình thang (H1 != H2): 4 cạnh vát mặt ────────────────────────────────
      // Cạnh 1: đỉnh hình thang, chạy ngang toàn bộ Lx tại step1Top.
      AddLine(new Point3d(sec.X - Lx / 2, step1Top, sec.Z), new Point3d(sec.X + Lx / 2, step1Top, sec.Z), Layer.Outline,
        null, tr, btr);
      // Cạnh 2: cạnh vát bên phải, từ mép phải đỉnh (sec.X + Lx/2) xiên vào
      //         đến mép phải cột (sec.X + cwX/2) tại cao độ step1Top + 0.3×S.
      AddLine(new Point3d(sec.X + Lx / 2, step1Top, sec.Z), new Point3d(sec.X + cwX / 2, step1Top + 0.3 * S, sec.Z),
        Layer.Outline, null, tr, btr);
      // Cạnh 3: đỉnh phía trên cùng của phần vát (nằm ngang, bằng bề rộng cột cwX).
      AddLine(new Point3d(sec.X + cwX / 2, step1Top + 0.3 * S, sec.Z),
        new Point3d(sec.X - cwX / 2, step1Top + 0.3 * S, sec.Z), Layer.Outline, null, tr, btr);
      // Cạnh 4: cạnh vát bên trái, từ mép trái cột vào đến mép trái đỉnh.
      AddLine(new Point3d(sec.X - cwX / 2, step1Top + 0.3 * S, sec.Z), new Point3d(sec.X - Lx / 2, step1Top, sec.Z),
        Layer.Outline, null, tr, btr);
    }
    else {
      // ── Hình chữ nhật (H1 == H2): chỉ vẽ đỉnh bậc, không vát ────────────────
      // Đường ngang tại step1Top chạy toàn bộ Lx.
      AddLine(new Point3d(sec.X - Lx / 2, step1Top, sec.Z), new Point3d(sec.X + Lx / 2, step1Top, sec.Z), Layer.Outline,
        null, tr, btr);
    }

    // ── Bước 4: Vẽ cột stub (đoạn cột hiển thị trên mặt cắt) ────────────────────
    // Cột stub là phần cột nằm phía trên cao độ đáy móng (colBaseZ),
    // cao colStubH = 0.5×S (tượng trưng cho đoạn cột cắt ngang).
    // 4 đỉnh:
    //   Dưới-trái:  (sec.X - cwX/2, colBaseZ)
    //   Dưới-phải: (sec.X + cwX/2, colBaseZ)
    //   Trên-trái:  (sec.X - cwX/2, colTopZ)
    //   Trên-phải: (sec.X + cwX/2, colTopZ)
    double colStubH = 0.5 * S;
    double colTopZ = colBaseZ + colStubH;
    AddLine(new Point3d(sec.X - cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X + cwX / 2, colBaseZ, sec.Z), Layer.Column,
      null, tr, btr);
    AddLine(new Point3d(sec.X - cwX / 2, colTopZ, sec.Z), new Point3d(sec.X + cwX / 2, colTopZ, sec.Z), Layer.Column,
      null, tr, btr);
    AddLine(new Point3d(sec.X - cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X - cwX / 2, colTopZ, sec.Z), Layer.Column,
      null, tr, btr);
    AddLine(new Point3d(sec.X + cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X + cwX / 2, colTopZ, sec.Z), Layer.Column,
      null, tr, btr);

    // ── Bước 5: Ghi nhãn cao độ (layer CDA / TEXT) ─────────────────────────────
    // Vị trí nhãn: phía trái mặt cắt, tại x = sec.X - Lx/2 - 1.5
    // Tọa độ Y (cao độ) trong mặt cắt: trục Y = trục cao độ.
    double lblX = sec.X - Lx / 2 - 1.5;

    // MC (Mặt Cắt): ghi tại cao độ mặt đất (GroundLevel).
    // Giá trị {-FoundationBottomLevel/1000:F3}: cao độ âm (đáy móng dưới mặt đất).
    AddDbText(new Point3d(lblX, sec.Y + Model.GroundLevel * S, sec.Z), $"{-Model.FoundationBottomLevel / 1000:F3}",
      textH, Layer.LabelYellow, tr, btr);
    AddDbText(new Point3d(lblX, sec.Y + Model.GroundLevel * S + 0.3 * S, sec.Z), "MC", textH, Layer.LabelCyan, tr, btr);

    // Cao độ sàn tầng 1 (FloorLevel1): giá trị âm.
    AddDbText(new Point3d(lblX, sec.Y + Model.FloorLevel1 * S, sec.Z), $"{-Model.FloorLevel1 / 1000:F3}", textH,
      Layer.LabelYellow, tr, btr);

    // Cao độ đáy móng (FoundationBottomLevel): giá trị dương.
    AddDbText(new Point3d(lblX, sec.Y + Model.FoundationBottomLevel * S, sec.Z),
      $"{Model.FoundationBottomLevel / 1000:F3}", textH, Layer.LabelYellow, tr, btr);

    // ── Bước 6: Ghi nhãn mặt cắt A-A ──────────────────────────────────────────
    // Chữ "A" lớn (MText) đặt tại vị trí trung tâm mặt cắt, phía trên cao độ mặt đất.
    // Chữ "- A" nhỏ (DBText) đặt bên phải chữ "A" để tạo định dạng "A - A".
    AddMText(new Point3d(sec.X, sec.Y + Model.GroundLevel * S + 1.5 * S, sec.Z),
      "A", textH * 2, Layer.LabelCyan, tr, btr);
    AddDbText(new Point3d(sec.X + 0.5 * S, sec.Y + Model.GroundLevel * S + 1.5 * S, sec.Z),
      "- A", textH, Layer.LabelCyan, tr, btr);
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║          E.  DIM MẶT CẮT (SECTION DIMENSIONS)                ║
  // ╚══════════════════════════════════════════════════════════════╝

  // ╔══════════════════════════════════════════════════════════════════════════════╗
  // ║  DrawSectionDimensionsAtPoint: VẼ KÍCH THƯỚC MẶT CẮT (SECTION DIMENSIONS) ║
  // ╠══════════════════════════════════════════════════════════════════════════════╣
  // ║  Hệ tọa độ mặt cắt: sec = bp + (SectionOffsetX, 0, 0)                     ║
  // ║    Trục X (phương ngang) = chiều dài móng                                  ║
  // ║    Trục Y (phương đứng)  = cao độ (elevation)                              ║
  // ║                                                                                ║
  // ║  4 kích thước được vẽ:                                                       ║
  // ║    1. Horizontal Lx (nằm ngang): đo mép trái → mép phải đài móng         ║
  // ║    2. Vertical H2 (thẳng đứng): đo chiều cao bậc dưới                      ║
  // ║    3. Vertical H1 (thẳng đứng): đo chiều cao bậc trên                     ║
  // ║    4. Elevation dim (thẳng đứng, layer Elevation): đo cao độ mặt đất       ║
  // ║         Chỉ vẽ khi |GroundLevel - FoundationBottomLevel| > 1mm               ║
  // ╚══════════════════════════════════════════════════════════════════════════════╝
  private void DrawSectionDimensionsAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var sec = new Point3d(bp.X + SectionOffsetX, bp.Y, bp.Z); // gốc tọa độ mặt cắt
    double Lx = Model.LengthX * S; // chiều dài móng (model mm → drawing units)
    double h2 = Model.StepHeightH2 * S; // chiều cao bậc dưới H2
    double h1 = Model.StepHeightH1 * S; // chiều cao bậc trên H1
    double fBot = Model.FoundationBottomLevel * S; // cao độ đáy móng
    double dimOff = 1.5 * S; // khoảng cách dim so với mép móng

    // ── Kích thước ngang Lx (đo chiều dài đài móng) ───────────────────────────
    // Vị trí đường dim: y = fBot - 2.5×S → phía dưới đáy móng.
    // Đo từ mép trái đài (sec.X - Lx/2) đến mép phải đài (sec.X + Lx/2).
    double dimElev = fBot - 2.5 * S;
    AddRotatedDimensionEntity(DimLevel.Level2,
      new Point3d(sec.X - Lx / 2, dimElev, sec.Z),
      new Point3d(sec.X + Lx / 2, dimElev, sec.Z),
      Layer.Dimension, tr, btr);

    // ── Kích thước đứng H2 (chiều cao bậc dưới) ───────────────────────────────
    // Vị trí đường dim: x = sec.X + Lx/2 + dimOff → phía phải mặt cắt.
    // Đo từ fBot (đáy móng / đỉnh bản đế) đến fBot + h2 (đỉnh bậc H2).
    double dimX = sec.X + Lx / 2 + dimOff;
    AddRotatedDimensionEntity(DimLevel.Level1,
      new Point3d(dimX, fBot, sec.Z),
      new Point3d(dimX, fBot + h2, sec.Z),
      Layer.Dimension, tr, btr);

    // ── Kích thước đứng H1 (chiều cao bậc trên) ───────────────────────────────
    // Đo từ fBot + h2 (đỉnh bậc H2) đến fBot + h1 + h2 (đỉnh bậc H1 / đáy cột).
    AddRotatedDimensionEntity(DimLevel.Level2,
      new Point3d(dimX, fBot + h2, sec.Z),
      new Point3d(dimX, fBot + h1 + h2, sec.Z),
      Layer.Dimension, tr, btr);

    // ── Kích thước cao độ (Elevation) ─────────────────────────────────────────
    // Vị trí: phía trái mặt cắt tại x = sec.X - Lx/2 - 2.5×S.
    // Đo khoảng cách từ fBot (đáy móng) đến cao độ mặt đất (GroundLevel).
    // Dùng layer Elevation để phân biệt với kích thước thông thường.
    // Chỉ vẽ khi |GroundLevel - FoundationBottomLevel| > 1 mm (tránh dim 0).
    double elevX = sec.X - Lx / 2 - 2.5 * S;
    if (Math.Abs(Model.GroundLevel - Model.FoundationBottomLevel) > 1) {
      AddRotatedDimensionEntity(DimLevel.Level1,
        new Point3d(elevX, fBot, sec.Z),
        new Point3d(elevX, sec.Y + Model.GroundLevel * S, sec.Z),
        Layer.Elevation, tr, btr);
    }
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║            F.  THÉP MẶT CẮT (SECTION REBAR)                  ║
  // ╚══════════════════════════════════════════════════════════════╝

  // ╔══════════════════════════════════════════════════════════════════════════════╗
  // ║  DrawSectionRebarAtPoint: VẼ THÉP MẶT CẮT (SECTION REBAR)                 ║
  // ╠══════════════════════════════════════════════════════════════════════════════╣
  // ║  Hệ tọa độ mặt cắt: sec = bp + (8.0, 0, 0)                              ║
  // ║  colBaseZ = fBot + h1 + h2 = cao độ đỉnh bậc H1 = đáy cột               ║
  // ║                                                                                ║
  // ║  1. Bottom main rebars X (thép chịu lực đáy móng - phương X):           ║
  // ║       rebarY = sec.Y + fBot + h2 + cover                                  ║
  // ║       → Thanh nằm ngang tại cao độ đáy bậc H2 + lớp bảo vệ             ║
  // ║       Vòng lặp i=1..rxCount:                                              ║
  // ║         x_i = sec.X - Lx/2 + cover + i × spacing                         ║
  // ║         spacing = (Lx - 2×cover) / (rxCount + 1)                         ║
  // ║         Thanh vẽ là đoạn đứng ngắn (0.1×S) tượng trưng cho tiết diện    ║
  // ║         cắt ngang thanh thép trên mặt cắt.                               ║
  // ║       Block nhãn IBR_Mong đặt tại góc phải-trên.                          ║
  // ║                                                                                ║
  // ║  2. Bottom main rebars Y (thép chịu lực đáy móng - phương Y):           ║
  // ║       Thanh nằm ngang tại cùng cao độ rebarY                              ║
  // ║       Đoạn từ (sec.X - Lx/2 + cover) đến (sec.X + Lx/2 - cover)         ║
  // ║       → Đại diện cho các thanh thép Y cắt ngang mặt cắt (hiển thị        ║
  // ║         bằng 1 đường nằm ngang tại vị trí rebarY).                       ║
  // ║                                                                                ║
  // ║  3. Column rebars (thép dọc cột):                                         ║
  // ║       crCount thanh phân bố đều từ colBaseZ đến colBaseZ + GroundLevel   ║
  // ║       spacing = (GroundLevel×S) / (crCount + 1)                             ║
  // ║       Mỗi thanh: đoạn nằm ngang từ (sec.X - cwX/2 + cover) →              ║
  // ║                  (sec.X + cwX/2 - cover)                                    ║
  // ║       Nếu LapSpliceLength > 0: ghi nhãn độ dài nối buộc.                  ║
  // ║                                                                                ║
  // ║  4. Stirrups (thép đai cột):                                               ║
  // ║       Hình chữ nhật tại colBaseZ, kích thước cwX × stSpacing×S           ║
  // ║       (4 đoạn thẳng: đáy→phải→trên→trái→đáy).                             ║
  // ║       Nhãn "phiXdY" tại góc trên-phải hình chữ nhật.                      ║
  // ║                                                                                ║
  // ║  5. Column cross-section cut marker (MC - Mặt Cắt cột):                   ║
  // ║       Vị trí: phía phải mặt cắt, tại x = sec.X + Lx/2 + 0.5×S             ║
  // ║       2 vòng tròn đồng tâm tượng trưng cho tiết diện cột tròn            ║
  // ║       (IBMcCot = hình tròn đồng tâm đánh dấu vị trí cắt cột).           ║
  // ╚══════════════════════════════════════════════════════════════════════════════╝
  private void DrawSectionRebarAtPoint(Point3d bp, BlockTableRecord btr, Transaction tr)
  {
    var sec = new Point3d(bp.X + SectionOffsetX, bp.Y, bp.Z); // gốc tọa độ mặt cắt
    double Lx = Model.LengthX * S; // chiều dài móng
    double h2 = Model.StepHeightH2 * S; // chiều cao bậc H2
    double h1 = Model.StepHeightH1 * S; // chiều cao bậc H1
    double cwX = Model.ColumnWidthX * S; // bề rộng cột
    double cover = Model.Cover * S; // lớp bê tông bảo vệ
    double colBaseZ = Model.FoundationBottomLevel * S + h1 + h2; // cao độ đáy cột
    double tagH = 0.3 * S; // chiều cao chữ nhãn

    // ══ 1. Bottom main rebars X (thép chịu lực đáy móng - phương X) ══════
    // Trên mặt cắt, các thanh thép X chạy dọc (thẳng đứng trên mặt bằng)
    // nên khi cắt ngang mặt cắt A-A sẽ thấy TIẾT DIỆN ngang của chúng
    // → vẽ bằng các đoạn đứng ngắn (0.1×S) tại các vị trí x_i.
    //
    // rebarY = sec.Y + fBot + h2 + cover
    //   = cao độ đáy móng (fBot) + bậc H2 (h2) + lớp bảo vệ (cover)
    //   → Vị trí Y của các thanh thép chính đáy móng trên mặt cắt.
    var (rxCount, rxDia, _) = ParseRebarSpec(Model.RebarX);
    if (rxCount > 0 && rxDia > 0) {
      double usableX = Lx - 2 * cover;
      double spacing = usableX / ( rxCount + 1 );
      double rebarY = sec.Y + Model.FoundationBottomLevel * S + h2 + cover;

      for (int i = 1 ; i <= rxCount ; i++) {
        // x_i = sec.X - Lx/2 + cover + i × spacing
        // → Vị trí X của thanh thứ i trong mặt cắt.
        double x = sec.X - Lx / 2 + cover + i * spacing;
        // Vẽ đoạn đứng ngắn (0.1×S): đại diện cho tiết diện thanh tròn cắt ngang.
        var ln = AddLine(new Point3d(x, rebarY, sec.Z), new Point3d(x, rebarY + 0.1, sec.Z), Layer.Rebar, null, tr, btr);
        ln.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0); // màu đỏ

        // Gắn block nhãn móng thép IBR_Mong tại góc phải-trên thanh.
        var tagPt = new Point3d(x + 0.4 * S, rebarY + 0.4 * S, sec.Z);
        InsertMongRebarTag(tagPt, 0, $"phi{(int) rxDia}", tr, btr);
      }
    }

    // ══ 2. Bottom main rebars Y (thép chịu lực đáy móng - phương Y) ══════
    // Thanh thép Y chạy ngang trên mặt bằng (vuông góc với mặt cắt A-A),
    // nên trên mặt cắt sẽ thấy một ĐƯỜNG NẰM NGANG tại vị trí rebarY.
    // Đoạn nằm ngang từ (sec.X - Lx/2 + cover) đến (sec.X + Lx/2 - cover).
    var (ryCount, ryDia, _) = ParseRebarSpec(Model.RebarY);
    if (ryCount > 0 && ryDia > 0) {
      double rebarY = sec.Y + Model.FoundationBottomLevel * S + h2 + cover;
      var lnY = AddLine(
        new Point3d(sec.X - Lx / 2 + cover, rebarY, sec.Z),
        new Point3d(sec.X + Lx / 2 - cover, rebarY, sec.Z),
        Layer.Rebar, null, tr, btr);
      lnY.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0); // màu đỏ
    }

    // ══ 3. Thép dọc cột (Column rebars) ═══════════════════════════════════
    // Chỉ vẽ khi Model.DrawColumnRebar = true.
    if (Model.DrawColumnRebar) {
      var (stCount, stDia, stSpacing) = ParseRebarSpec(Model.StirrupRebar);
      int crCount = Model.ColumnRebarCountX; // số thanh thép dọc cột
      double crDia = Model.ColumnRebar * S; // đường kính thép dọc cột

      // crCount thanh phân bố đều từ colBaseZ đến colBaseZ + GroundLevel×S.
      // spacing = (rebarY2 - rebarY1) / (crCount + 1)
      //   → crCount thanh chia đều, cách đều 2 đầu mút.
      if (crCount > 0 && crDia > 0) {
        double rebarY1 = colBaseZ;
        double rebarY2 = colBaseZ + Model.GroundLevel * S;
        double spY = crCount > 1 ? ( rebarY2 - rebarY1 ) / ( crCount + 1 ) : 0;

        for (int i = 1 ; i <= crCount ; i++) {
          double y = rebarY1 + i * spY;
          // Mỗi thanh: đoạn nằm ngang qua tiết diện cột, trừ lớp bảo vệ 2 bên.
          var lnCr = AddLine(
            new Point3d(sec.X - cwX / 2 + cover, y, sec.Z),
            new Point3d(sec.X + cwX / 2 - cover, y, sec.Z),
            Layer.Rebar, null, tr, btr);
          lnCr.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0); // màu đỏ
        }

        // Nếu có chiều dài nối buộc (lap splice): ghi nhãn tại colBaseZ - 0.5×S.
        if (Model.LapSpliceLength > 0) {
          AddMText(new Point3d(sec.X, colBaseZ - 0.5 * S, sec.Z),
            $"Lap {Model.LapSpliceLength / 1000:F3}m", tagH, Layer.LabelYellow, tr, btr);
        }
      }

      // ══ 4. Thép đai cột (Stirrups) ═══════════════════════════════════════
      // Thép đai vẽ dưới dạng hình chữ nhật tượng trưng tại colBaseZ.
      // sY1 = đỉnh dưới của đai = colBaseZ (tại đáy cột).
      // sY2 = đỉnh trên = colBaseZ + stSpacing×S (nhưng không vượt quá GroundLevel).
      // sX1 = mép trái cột, sX2 = mét phải cột.
      if (stCount > 0 && stDia > 0 && stSpacing > 0) {
        double sY1 = colBaseZ;
        double sY2 = Math.Min(colBaseZ + stSpacing * S, colBaseZ + Model.GroundLevel * S);
        double sX1 = sec.X - cwX / 2;
        double sX2 = sec.X + cwX / 2;

        // 4 cạnh hình chữ nhật thép đai (đáy → phải → trên → trái → đáy).
        AddLine(new Point3d(sX1, sY1, sec.Z), new Point3d(sX2, sY1, sec.Z), Layer.Rebar, null, tr, btr);
        AddLine(new Point3d(sX2, sY1, sec.Z), new Point3d(sX2, sY2, sec.Z), Layer.Rebar, null, tr, btr);
        AddLine(new Point3d(sX2, sY2, sec.Z), new Point3d(sX1, sY2, sec.Z), Layer.Rebar, null, tr, btr);
        AddLine(new Point3d(sX1, sY2, sec.Z), new Point3d(sX1, sY1, sec.Z), Layer.Rebar, null, tr, btr);

        // Nhãn đai: "phiXdY" (đường kính đai @ khoảng cách).
        AddMText(new Point3d(sec.X + cwX / 2 + 0.2 * S, sY1 + 0.2 * S, sec.Z),
          $"phi{(int) stDia}@{(int) stSpacing}", tagH * 0.7, Layer.LabelYellow, tr, btr);
      }
    }

    // ══ 5. Ký hiệu vị trí cắt mặt cắt cột (IBMcCot - Mặt Cắt Cột) ══════════
    // mcX = sec.X + Lx/2 + 0.5×S → phía phải ngoài mặt cắt móng.
    // mcY = colBaseZ + 0.3×S → cùng cao độ đáy cột.
    // 2 vòng tròn đồng tâm (bán kính 0.2×S và 0.2×S, lệch 0.4×S theo Y)
    // + 1 đoạn thẳng nối 2 tâm = hình "8" ngang (ký hiệu IBM cột).
    // Nhãn "MC" tại góc phải giữa 2 vòng tròn.
    double mcX = sec.X + Lx / 2 + 0.5 * S;
    double mcY = colBaseZ + 0.3 * S;
    AddCircle(new Point3d(mcX, mcY, sec.Z), 0.2 * S, Layer.Column, tr, btr);
    AddCircle(new Point3d(mcX, mcY - 0.4 * S, sec.Z), 0.2 * S, Layer.Column, tr, btr);
    AddLine(new Point3d(mcX, mcY, sec.Z), new Point3d(mcX, mcY - 0.4 * S, sec.Z), Layer.Column, null, tr, btr);
    AddMText(new Point3d(mcX + 0.3 * S, mcY - 0.2 * S, sec.Z), "MC", tagH, Layer.LabelCyan, tr, btr);
  }
}
