#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AutoCADTools.App.Const;
using AutoCADTools.App.Enums;
using Hatch = AutoCADTools.App.Const.Hatch;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // ╔══════════════════════════════════════════════════════════════════╗
  // ║               A.  GEOMETRY PRIMITIVES                              ║
  // ║  Các hàm cơ bản nhất dùng để tạo Line, Circle, Hatch...           ║
  // ║  Tất cả đều nhận Transaction (tr) và BlockTableRecord (btr)       ║
  // ║  để đăng ký đối tượng vào database của AutoCAD.                   ║
  // ╚══════════════════════════════════════════════════════════════════╝

  // AddLine: Tạo một Line từ 2 điểm Point3d.
  //
  // Logic tọa độ:
  //   - pt1, pt2 là 2 đầu mút của đoạn thẳng (Point3d chứa X, Y, Z).
  //   - Tọa độ được tính dựa trên bp (base point) + các giá trị từ Model * S (scale).
  //   - Nếu linetypeName != null → áp dụng kiểu đường nét (vd: "DASHED" cho trục TIM).
  //
  // Quy trình đăng ký đối tượng vào AutoCAD DB:
  //   1. new Line(pt1, pt2)      → tạo đối tượng Line
  //   2. ln.Layer = layer        → gán layer (THAY, TIM, THEP, DIM...)
  //   3. btr.AppendEntity(ln)    → nối đối tượng vào khối hiện tại (ModelSpace)
  //   4. tr.AddNewlyCreatedDBObject(ln, true) → đăng ký với transaction để commit/rollback
  private Line AddLine(Point3d pt1, Point3d pt2, string layer, string? linetypeName, Transaction tr, BlockTableRecord btr)
  {
    var ln = new Line(pt1, pt2) { Layer = layer };
    if (!string.IsNullOrEmpty(linetypeName))
      ln.Linetype = linetypeName;
    btr.AppendEntity(ln);
    tr.AddNewlyCreatedDBObject(ln, true);
    return ln;
  }

  // AddCircle: Tạo một Circle (vòng tròn) tại tâm center với bán kính radius.
  //
  // Logic tọa độ:
  //   - center: Point3d → tâm vòng tròn (X, Y, Z)
  //   - Vector3d.ZAxis → xác định mặt phẳng vòng tròn nằm trong mặt phẳng XY (vuông góc trục Z)
  //   - radius: bán kính vẽ bằng đơn vị đã scale (S)
  //
  // Dùng cho: thể hiện cột tròn trên mặt bằng, vòng đánh dấu cốt thép (mong thép).
  private Circle AddCircle(Point3d center, double radius, string layer, Transaction tr, BlockTableRecord btr)
  {
    var c = new Circle(center, Vector3d.ZAxis, radius) { Layer = layer };
    btr.AppendEntity(c);
    tr.AddNewlyCreatedDBObject(c, true);
    return c;
  }

  // AddHatch: Tạo một hatch (tô mẫu) cho vùng khép kín bởi boundaryIds.
  //
  // Logic tọa độ:
  //   - boundaryIds: ObjectIdCollection chứa ObjectId của các đối tượng tạo boundary
  //     (Polyline khép kín, Line loop...). Các điểm của boundary xác định vùng tô.
  //   - patName: tên mẫu hatch (Pattern có sẵn trong AutoCAD)
  //     "AR-CONC" = aggregate concrete (bê tông cốt liệu) → dùng cho đế móng
  //     "DOTS"    = chấm tròn → dùng cho cột
  //   - patScale: tỷ lệ lặp lại của mẫu hatch (số lớn → mẫu nhỏ hơn)
  //   - HatchLoopTypes.External: vùng hatch nằm phía ngoài boundary (vùng đặc bên ngoài)
  //   - EvaluateHatch(false): tính toán hatch ngay mà không cần AutoCAD tự cập nhật
  private Autodesk.AutoCAD.DatabaseServices.Hatch AddHatch(ObjectIdCollection boundaryIds, string patName, double patScale, string layer, Transaction tr,
    BlockTableRecord btr)
  {
    var ht = new Autodesk.AutoCAD.DatabaseServices.Hatch { Layer = layer, PatternScale = patScale, HatchStyle = HatchStyle.Normal};

    if (!string.IsNullOrEmpty(patName))
      ht.SetHatchPattern(HatchPatternType.PreDefined, patName);

    ht.AppendLoop(HatchLoopTypes.External, boundaryIds);
    ht.EvaluateHatch(false);

    btr.AppendEntity(ht);
    tr.AddNewlyCreatedDBObject(ht, true);

    ht.Associative = true;
    ht.EvaluateHatch(true);
    return ht;
  }

  // AddRotatedDimensionEntity: Tạo RotatedDimension từ 2 điểm xLine1-xLine2.
  //
  // Angle và dimPoint được tính tự động từ xLine1, xLine2 và level:
  //   - Vector hướng dir = xLine2 - xLine1 → angle = Atan2(dir.Y, dir.X)
  //   - Vector vuông góc perp = (-dir.Y, dir.X, 0) → dimPoint nằm trên đường
  //     song song cách xLine1-xLine2 một khoảng offset = (DimBaseGap + (level-1)*DimensionGap) * TitleS
  //   - TextPosition offset nhỏ 0.5 units theo hướng perp
  //
  // TitleS = tỷ lệ khung tên (dùng cho text height và khoảng cách dim)
  private void AddRotatedDimensionEntity(DimLevel level, Point3d xLine1, Point3d xLine2, string layer, Transaction tr, BlockTableRecord btr,
    double dimScale = 1.0, double scaleFactor = 1.0, bool isReverse = false)
  {
    if (xLine1.IsEqualTo(xLine2)) return;

    var dir = ( xLine2 - xLine1 ).GetNormal();
    var angle = Math.Atan2(dir.Y, dir.X);
    var norm = dir.CrossProduct(isReverse ? Vector3d.ZAxis.Negate() : Vector3d.ZAxis);

    var p1 = xLine1.Add(norm.MultiplyBy(Numeric.DimBaseGap * TitleS));
    var p2 = xLine2.Add(norm.MultiplyBy(Numeric.DimBaseGap * TitleS));

    var dist = ( Numeric.DimBaseGap + (int) level * Numeric.DimensionGap ) * TitleS;
    var mid = xLine1.Add(norm.MultiplyBy(dist));

    var rd = new RotatedDimension(0, p1, p2, mid, "", _dimStyleId) {
      Dimlfac = scaleFactor,
      DimfxlenOn = false,
      Rotation = angle,
      Dimscale = dimScale,
      DimLinePoint = mid,
      Layer = layer,
      Dimtmove = 0,
      TextRotation = isReverse ? angle + Math.PI : angle
    };

    btr.AppendEntity(rd);
    tr.AddNewlyCreatedDBObject(rd, true);
  }

  // AddMText: Tạo một MText (text nhiều dòng, định dạng được) tại location.
  //
  // Logic tọa độ:
  //   - location: Point3d → điểm neo góc trên-trái của khối text
  //   - Contents: nội dung text (hỗ trợ ký tự đặc biệt như %%c cho đường kính)
  //   - TextHeight: chiều cao text (đã scale với S)
  //
  // Dùng cho: nhãn móng (FoundationName), cao độ, mặt cắt A-A.
  private MText AddMText(Point3d location, string text, double height, string layer, Transaction tr, BlockTableRecord btr)
  {
    var mt = new MText
    {
      Location = location,
      Contents = text,
      TextHeight = height,
      Layer = layer
    };
    btr.AppendEntity(mt);
    tr.AddNewlyCreatedDBObject(mt, true);
    return mt;
  }

  // AddDbText: Tạo một DBText (text đơn dòng) canh giữa tại position.
  //
  // Logic tọa độ:
  //   - position: điểm đặt text (= AlignmentPoint = điểm canh giữa)
  //   - Justify = AttachmentPoint.MiddleCenter → text canh giữa cả ngang lẫn dọc
  //   - HorizontalMode/VerticalMode đặt MiddleCenter để đảm bảo canh chuẩn đúng
  //
  // Dùng cho: kích thước (Lx = ..., Ly = ...), cao độ, nhãn kích thước.
  private static DBText AddDbText(Point3d position, string text, double height, string layer, Transaction tr, BlockTableRecord btr, AttachmentPoint attachmentPoint = AttachmentPoint.MiddleCenter, int? colorIndex = null)
  {
    var dt = new DBText
    {
      Position = position,
      TextString = text,
      Height = height,
      Layer = layer,
      HorizontalMode = TextHorizontalMode.TextCenter,
      VerticalMode = TextVerticalMode.TextVerticalMid,
      AlignmentPoint = position,
      Justify = attachmentPoint,
    };
    if (colorIndex != null) dt.ColorIndex = colorIndex.Value;
    btr.AppendEntity(dt);
    tr.AddNewlyCreatedDBObject(dt, true);
    return dt;
  }

  // SetXDataString: Gắn dữ liệu mở rộng (XData) vào entity để lưu trữ metadata.
  //
  // Logic:
  //   - XData là cặp (appName, value) lưu kèm đối tượng AutoCAD (không hiển thị).
  //   - appName: tên ứng dụng đăng ký trong RegAppTable (EnsureAppNames).
  //   - value: chuỗi chứa nội dung (vd: "Lx", "H1", "OffsetX").
  //   - Khi truy vấn đối tượng, có thể đọc XData để biết nó đại diện cho thông số nào.
  private void SetXDataString(Entity entity, string appName, string value)
  {
    var rb = new ResultBuffer(
      new TypedValue((int)DxfCode.ExtendedDataRegAppName, appName),
      new TypedValue((int)DxfCode.ExtendedDataAsciiString, value));
    entity.XData = rb;
  }


  // CreateRectanglePolyline: Tạo Polyline hình chữ nhật từ 4 đỉnh Point3d.
  //
  // Logic tọa độ:
  //   - Nhận 4 Point3d (p1,p2,p3,p4) là 4 góc hình chữ nhật.
  //   - AddVertexAt(i, new Point2d(p.X, p.Y), bulge, startWidth, endWidth)
  //     → Chuyển Point3d → Point2d (bỏ Z), bulge=0 (không vát góc).
  //   - Closed=true → AutoCAD tự nối đỉnh cuối về đỉnh đầu → hình khép kín.
  //
  // Thứ tự đỉnh quan trọng: p1→p2→p3→p4 phải theo chiều kim đồng hồ hoặc ngược
  // để boundary hatch đúng (nếu ngược chiều → vùng hatch sẽ bị đảo).
  private static Polyline CreatePolyline(List<Point3d> point3ds, string layer, Transaction tr, BlockTableRecord btr)
  {
    var pl = new Polyline();
    for (var i = 0 ; i < point3ds.Count ; i++) {
      pl.AddVertexAt(i, new Point2d(point3ds[i].X, point3ds[i].Y), 0, 0, 0);
    }

    pl.Closed = true;
    pl.Layer = layer;
    pl.Elevation = 0;
    btr.AppendEntity(pl);
    tr.AddNewlyCreatedDBObject(pl, true);
    return pl;
  }

  // ╔══════════════════════════════════════════════════════════════════╗
  // ║               B.  BLOCK & ATTRIBUTE HELPERS                       ║
  // ║  Hàm hỗ trợ tạo Block (khối), Attribute (thuộc tính),              ║
  // ║  và block nhãn móng thép IBR_Mong.                               ║
  // ╚══════════════════════════════════════════════════════════════════╝

  // InsertBlock: Chèn một BlockReference tại vị trí position, xoay rotation.
  //
  // Logic tọa độ:
  //   - position: Point3d → vị trí điểm chèn block (điểm gốc 0,0 của block definition)
  //   - rotation: góc xoay block quanh trục Z (rad)
  //
  // Quy trình:
  //   1. Kiểm tra BlockTable (bt) đã có blockName chưa.
  //   2. Nếu chưa → tạo BlockTableRecord mới (btrDef), đăng ký vào bt.
  //      Đây là lần đầu gặp block → cần vẽ nội dung block (geometry, attribute def).
  //   3. Nếu đã có → dùng lại ObjectId đã lưu.
  //   4. Tạo BlockReference tham chiếu đến blockId, append vào btr (ModelSpace).
  //
  // Chú ý: BlockReference chỉ là tham chiếu; nội dung thực nằm trong BlockTableRecord.
  private BlockReference InsertBlock(string blockName, Point3d position, double rotation, Transaction tr, BlockTableRecord btr)
  {
    if (_db == null) throw new InvalidOperationException("Database is null");

    var bt = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForRead);
    ObjectId blockId;

    if (bt.Has(blockName))
    {
      blockId = bt[blockName];
    }
    else
    {
      using var btrDef = new BlockTableRecord { Name = blockName };
      bt.UpgradeOpen();
      bt.Add(btrDef);
      tr.AddNewlyCreatedDBObject(btrDef, true);
      blockId = btrDef.ObjectId;
    }

    var br = new BlockReference(position, blockId)
    {
      Rotation = rotation,
      ScaleFactors = new Scale3d(1, 1, 1)
    };

    btr.AppendEntity(br);
    tr.AddNewlyCreatedDBObject(br, true);
    return br;
  }

  // AddAttributeToBlock: Gắn Attribute (thuộc tính hiển thị trong block) vào BlockReference.
  //
  // Logic tọa độ:
  //   - position: Point3d → vị trí đặt attribute bên trong block
  //   - tag: tên thuộc tính (dùng khi trích xuất dữ liệu block)
  //   - value: giá trị hiển thị (vd: "phi18", "2a200")
  //
  // Quy trình 2 bước:
  //   1. Tạo AttributeDefinition trong BlockDefinition (btrDef) → khai báo có attribute này.
  //   2. Tạo AttributeReference trong BlockReference (br) → bản thể hiện của attribute.
  private void AddAttributeToBlock(BlockReference br, string tag, string value, Point3d position, double height, Transaction tr)
  {
    if (_db == null) return;

    var blockDef = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
    var textStyleId = _db.Textstyle;
    var attDef = new AttributeDefinition(position, value, tag, string.Empty, textStyleId)
    {
      Height = height,
      Layer = br.Layer
    };
    blockDef.UpgradeOpen();
    blockDef.AppendEntity(attDef);
    tr.AddNewlyCreatedDBObject(attDef, true);

    using var attRef = new AttributeReference
    {
      Position = position,
      Height = height,
      Layer = br.Layer,
      TextString = value
    };
    br.AttributeCollection.AppendAttribute(attRef);
    tr.AddNewlyCreatedDBObject(attRef, true);
  }

  // InsertMongRebarTag: Chèn block nhãn móng thép "IBR_Mong" tại điểm pt.
  //
  // Block IBR_Mong là hình chữ nhật có vòng tròn đồng tâm ở trên, hiển thị
  // đường kính thép (vd: phi18). Dùng để đánh dấu từng thanh thép trên mặt bằng.
  //
  // Logic tọa độ của block (gốc tọa độ tại 0,0 của block):
  //   - rw = 2.5*S (chiều rộng), rh = 0.6*S (chiều cao) → kích thước nhãn
  //   - 4 cạnh hình chữ nhật từ (0,0)→(rw,0)→(rw,rh)→(0,rh)→(0,0)
  //   - Vòng tròn đồng tâm tại (rw/2, rh + rh*0.5) bán kính rh*0.5 và rh*0.3
  //     → điểm trung tâm nằm trên cạnh trên của hình chữ nhật (điểm giữa cạnh trên + offset)
  //   - Attribute REBAR tại (rw*0.15, rh*0.15) → góc dưới-trái của hình chữ nhật
  //
  // Đăng ký block 1 lần: nếu bt đã có "IBR_Mong" → chỉ InsertBlockReference.
  private void InsertMongRebarTag(Point3d pt, double angle, string rebarSpec, Transaction tr, BlockTableRecord btr)
  {
    var blockName = "IBR_Mong";
    if (_db == null) return;

    var bt = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForRead);
    if (!bt.Has(blockName))
    {
      using var btrDef = new BlockTableRecord { Name = blockName };
      bt.UpgradeOpen();
      bt.Add(btrDef);
      tr.AddNewlyCreatedDBObject(btrDef, true);

      var rh = 0.6 * S;
      var rw = 2.5 * S;

      // Rectangle body
      AddLine(new Point3d(0, 0, 0), new Point3d(rw, 0, 0), Layer.Rebar, null, tr, btrDef);
      AddLine(new Point3d(rw, 0, 0), new Point3d(rw, rh, 0), Layer.Rebar, null, tr, btrDef);
      AddLine(new Point3d(rw, rh, 0), new Point3d(0, rh, 0), Layer.Rebar, null, tr, btrDef);
      AddLine(new Point3d(0, rh, 0), new Point3d(0, 0, 0), Layer.Rebar, null, tr, btrDef);

      // Circle at top
      var circCenter = new Point3d(rw / 2, rh + rh * 0.5, 0);
      AddCircle(circCenter, rh * 0.5, Layer.Rebar, tr, btrDef);
      AddCircle(circCenter, rh * 0.3, Layer.Rebar, tr, btrDef);

      // Attribute
      var attDef = new AttributeDefinition(new Point3d(rw * 0.15, rh * 0.15, 0), rebarSpec, "REBAR", string.Empty, _db.Textstyle)
      {
        Height = 0.3 * S,
        Layer = Layer.Rebar
      };
      btrDef.AppendEntity(attDef);
      tr.AddNewlyCreatedDBObject(attDef, true);
    }

    var br = InsertBlock(blockName, pt, angle, tr, btr);
  }

  // AddDonut: Vẽ hình vành khăn (vòng tròn đặc có lỗ rỗng) tại center.
  //
  // Kỹ thuật hatch vành khăn (island detection):
  //   HatchLoopTypes.External yêu cầu winding direction:
  //     - Outer boundary: duyệt COUNTER-CLOCKWISE (ngược chiều kim đồng hồ)
  //       → vùng bên TRÁI đường đi là vùng đặc.
  //     - Inner boundary (island): duyệt CLOCKWISE (cùng chiều kim đồng hồ)
  //       → vùng bên TRÁI đường đi là vùng rỗng (lỗ).
  //
  //   Polyline đỉnh 4 điểm vuông góc:
  //     - Outer (CCW): phải→trên→trái→dưới→phải  (tọa độ X dương→Y dương→X âm→Y âm)
  //     - Inner (CW):  phải→dưới→trái→trên→phải  (tọa độ X dương→Y âm→X âm→Y dương)
  //   Chỉ truyền outerPL vào Hatch → hatch đặc toàn bộ hình vuông.
  //   Chỉ truyền cả outerPL + innerPL → AutoCAD tự nhận inner là island (lỗ rỗng).
  private void AddDonut(Point3d center, double outerDia, Transaction tr, BlockTableRecord btr)
  {
    var innerDia = Math.Max(outerDia - 0.3 * S, outerDia * 0.5);
    double outerR = outerDia / 2;
    double innerR = innerDia / 2;

    // Outer boundary: 4 đỉnh vuông góc, thứ tự CCW → vùng đặc.
    using var outerPL = new Polyline();
    outerPL.AddVertexAt(0, new Point2d(center.X + outerR, center.Y), 0, 0, 0);
    outerPL.AddVertexAt(1, new Point2d(center.X, center.Y + outerR), 0, 0, 0);
    outerPL.AddVertexAt(2, new Point2d(center.X - outerR, center.Y), 0, 0, 0);
    outerPL.AddVertexAt(3, new Point2d(center.X, center.Y - outerR), 0, 0, 0);
    outerPL.Closed = true;
    btr.AppendEntity(outerPL);
    tr.AddNewlyCreatedDBObject(outerPL, true);

    // Inner boundary: 4 đỉnh vuông góc nhỏ hơn, thứ tự CW → tạo lỗ rỗng.
    // Đỉnh inner: phải(dương)→dưới(âm)→trái(âm)→trên(dương)→phải(dương)
    // → ngược chiều outer → AutoCAD nhận là island (lỗ).
    using var innerPL = new Polyline();
    innerPL.AddVertexAt(0, new Point2d(center.X + innerR, center.Y), 0, 0, 0);
    innerPL.AddVertexAt(1, new Point2d(center.X, center.Y - innerR), 0, 0, 0);
    innerPL.AddVertexAt(2, new Point2d(center.X - innerR, center.Y), 0, 0, 0);
    innerPL.AddVertexAt(3, new Point2d(center.X, center.Y + innerR), 0, 0, 0);
    innerPL.Closed = true;
    btr.AppendEntity(innerPL);
    tr.AddNewlyCreatedDBObject(innerPL, true);

    // Hatch với cả outerPL + innerPL → AutoCAD tự nhận inner là island.
    var ids = new ObjectIdCollection { outerPL.ObjectId, innerPL.ObjectId };
    AddHatch(ids, Hatch.Solid, 1, Layer.Outline, tr, btr);
  }

  // ParseRebarSpec: Parse chuỗi qui cách thép → (số thanh, đường kính, khoảng cách).
  //
  // Input: string spec - chuỗi qui cách thép (từ Model.RebarX, Model.RebarY...)
  //
  // Các format được hỗ trợ:
  //   "2a150" → 2 thanh, phi150mm, spacing=150mm  (thép đều cách)
  //   "2d25"  → 2 thanh, phi25mm,  spacing=25mm   (thép đều cách, d=dia)
  //   "10"    → 1 thanh,  phi10mm,  spacing=0       (thép đơn)
  //   "d10"   → 1 thanh,  phi10mm,  spacing=0       (thép đơn, có prefix d)
  //
  // Regex pattern 1: @"^(\d+)([a-zA-Z])(\d+)$"
  //   Group 1: số thanh (count)
  //   Group 2: đơn vị/ký tự phân cách (a=spacing mm, d=dia mm)
  //   Group 3: giá trị số
  //
  // Regex pattern 2: @"^(?:D)?(\d+)$"
  //   Optional prefix "D", lấy số cuối là đường kính.
  private (int count, double dia, double spacing) ParseRebarSpec(string spec)
  {
    if (string.IsNullOrWhiteSpace(spec)) return (0, 0, 0);
    spec = spec.Trim().ToUpperInvariant();

    // "2a150" = 2 rebars @ 150mm, "2d25" = 2 rebars phi25mm
    var m = Regex.Match(spec, @"^(\d+)([a-zA-Z])(\d+)$");
    if (m.Success)
    {
      int count = int.Parse(m.Groups[1].Value);
      string unit = m.Groups[2].Value;
      int value = int.Parse(m.Groups[3].Value);
      return (count, value, value);
    }

    // "10" or "d10" = phi10
    var m2 = Regex.Match(spec, @"^(?:D)?(\d+)$");
    if (m2.Success)
    {
      return (1, double.Parse(m2.Groups[1].Value), 0);
    }

    return (0, 0, 0);
  }
}
