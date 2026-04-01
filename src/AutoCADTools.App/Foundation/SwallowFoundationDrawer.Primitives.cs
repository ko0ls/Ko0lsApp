#nullable enable

using System;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  // ── Geometry primitives ─────────────────────────────────────────

  private Line AddLine(Point3d pt1, Point3d pt2, string layer, string? linetypeName, Transaction tr, BlockTableRecord btr)
  {
    var ln = new Line(pt1, pt2) { Layer = layer };
    if (!string.IsNullOrEmpty(linetypeName))
      ln.Linetype = linetypeName;
    btr.AppendEntity(ln);
    tr.AddNewlyCreatedDBObject(ln, true);
    return ln;
  }

  private Circle AddCircle(Point3d center, double radius, string layer, Transaction tr, BlockTableRecord btr)
  {
    var c = new Circle(center, Vector3d.ZAxis, radius) { Layer = layer };
    btr.AppendEntity(c);
    tr.AddNewlyCreatedDBObject(c, true);
    return c;
  }

  private Hatch AddHatch(ObjectIdCollection boundaryIds, string patName, double patScale, string layer, Transaction tr, BlockTableRecord btr)
  {
    var ht = new Hatch { Layer = layer, PatternScale = patScale, HatchStyle = HatchStyle.Normal };

    if (string.IsNullOrEmpty(patName))
    {
      ht.SetHatchPattern(HatchPatternType.PreDefined, "");
    }
    else
    {
      ht.SetHatchPattern(HatchPatternType.PreDefined, patName);
    }

    ht.AppendLoop(HatchLoopTypes.External, boundaryIds);
    ht.EvaluateHatch(false);

    btr.AppendEntity(ht);
    tr.AddNewlyCreatedDBObject(ht, true);
    return ht;
  }

  private Hatch AddSolidHatch(ObjectIdCollection boundaryIds, string layer, Transaction tr, BlockTableRecord btr)
  {
    var ht = new Hatch { Layer = layer, HatchStyle = HatchStyle.Normal };
    ht.SetHatchPattern(HatchPatternType.PreDefined, "");
    ht.AppendLoop(HatchLoopTypes.External, boundaryIds);
    ht.EvaluateHatch(false);
    btr.AppendEntity(ht);
    tr.AddNewlyCreatedDBObject(ht, true);
    return ht;
  }

  private void AddRotatedDimensionEntity(
    double angle,
    Point3d dimPoint,
    Point3d xLine1,
    Point3d xLine2,
    string layer,
    string? xdataKey,
    string? xdataValue,
    Transaction tr,
    BlockTableRecord btr)
  {
    var rd = new RotatedDimension(
      angle,
      xLine1,
      xLine2,
      dimPoint,
      "",
      ObjectId.Null)
    {
      TextRotation = angle,
      Layer = layer
    };

    rd.TextPosition = new Point3d(dimPoint.X + 0.5, dimPoint.Y + 0.5, 0);

    if (!string.IsNullOrEmpty(xdataKey!) && xdataValue != null)
      SetXDataString(rd, xdataKey!, xdataValue);

    btr.AppendEntity(rd);
    tr.AddNewlyCreatedDBObject(rd, true);
  }

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

  private DBText AddDbText(Point3d position, string text, double height, string layer, Transaction tr, BlockTableRecord btr)
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
      Justify = AttachmentPoint.MiddleCenter
    };
    btr.AppendEntity(dt);
    tr.AddNewlyCreatedDBObject(dt, true);
    return dt;
  }

  // ── XData ────────────────────────────────────────────────────────

  private void SetXDataString(Entity entity, string appName, string value)
  {
    var rb = new ResultBuffer(
      new TypedValue((int)DxfCode.ExtendedDataRegAppName, appName),
      new TypedValue((int)DxfCode.ExtendedDataAsciiString, value));
    entity.XData = rb;
  }

  // ── Polyline-based hatch region helper ──────────────────────────

  private Hatch HatchPolyline(Polyline pl, string patName, double patScale, string layer, Transaction tr, BlockTableRecord btr)
  {
    pl.Color = layer switch
    {
      LayerConcreteHatch => Autodesk.AutoCAD.Colors.Color.FromRgb(180, 180, 180),
      _ => Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0)
    };
    btr.AppendEntity(pl);
    tr.AddNewlyCreatedDBObject(pl, true);
    var plId = pl.ObjectId;
    var ids = new ObjectIdCollection { plId };
    return AddHatch(ids, patName, patScale, layer, tr, btr);
  }

  private Polyline CreateRectanglePolyline(Point3d p1, Point3d p2, Point3d p3, Point3d p4)
  {
    var pl = new Polyline();
    pl.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
    pl.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
    pl.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
    pl.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);
    pl.Closed = true;
    return pl;
  }

  // ── Block + attribute helpers ───────────────────────────────────

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

  // ── Rebar tag block (IBR_Mong) ───────────────────────────────────

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
      AddLine(new Point3d(0, 0, 0), new Point3d(rw, 0, 0), LayerRebar, null, tr, btrDef);
      AddLine(new Point3d(rw, 0, 0), new Point3d(rw, rh, 0), LayerRebar, null, tr, btrDef);
      AddLine(new Point3d(rw, rh, 0), new Point3d(0, rh, 0), LayerRebar, null, tr, btrDef);
      AddLine(new Point3d(0, rh, 0), new Point3d(0, 0, 0), LayerRebar, null, tr, btrDef);

      // Circle at top
      var circCenter = new Point3d(rw / 2, rh + rh * 0.5, 0);
      AddCircle(circCenter, rh * 0.5, LayerRebar, tr, btrDef);
      AddCircle(circCenter, rh * 0.3, LayerRebar, tr, btrDef);

      // Attribute
      var attDef = new AttributeDefinition(new Point3d(rw * 0.15, rh * 0.15, 0), rebarSpec, "REBAR", string.Empty, _db.Textstyle)
      {
        Height = 0.3 * S,
        Layer = LayerRebar
      };
      btrDef.AppendEntity(attDef);
      tr.AddNewlyCreatedDBObject(attDef, true);
    }

    var br = InsertBlock(blockName, pt, angle, tr, btr);
  }

  // ── Donut (solid ring circle) ────────────────────────────────────

  private void AddDonut(Point3d center, double outerDia, Transaction tr, BlockTableRecord btr)
  {
    var innerDia = Math.Max(outerDia - 0.3 * S, outerDia * 0.5);
    var outerCirc = new Circle(center, Vector3d.ZAxis, outerDia / 2) { Layer = LayerOutline };
    var innerCirc = new Circle(center, Vector3d.ZAxis, innerDia / 2) { Layer = LayerOutline };

    using var outerPL = new Polyline();
    outerPL.AddVertexAt(0, new Point2d(center.X + outerDia / 2, center.Y), 0, 0, 0);
    outerPL.AddVertexAt(1, new Point2d(center.X, center.Y + outerDia / 2), 0, 0, 0);
    outerPL.AddVertexAt(2, new Point2d(center.X - outerDia / 2, center.Y), 0, 0, 0);
    outerPL.AddVertexAt(3, new Point2d(center.X, center.Y - outerDia / 2), 0, 0, 0);
    outerPL.Closed = true;

    using var innerPL = new Polyline();
    innerPL.AddVertexAt(0, new Point2d(center.X + innerDia / 2, center.Y), 0, 0, 0);
    innerPL.AddVertexAt(1, new Point2d(center.X, center.Y + innerDia / 2), 0, 0, 0);
    innerPL.AddVertexAt(2, new Point2d(center.X - innerDia / 2, center.Y), 0, 0, 0);
    innerPL.AddVertexAt(3, new Point2d(center.X, center.Y - innerDia / 2), 0, 0, 0);
    innerPL.Closed = true;

    btr.AppendEntity(outerPL);
    tr.AddNewlyCreatedDBObject(outerPL, true);
    btr.AppendEntity(innerPL);
    tr.AddNewlyCreatedDBObject(innerPL, true);

    // Hatch the ring as a solid fill with the outer boundary
    var ids = new ObjectIdCollection { outerPL.ObjectId };
    AddSolidHatch(ids, LayerOutline, tr, btr);
  }

  // ── Rebar spec parser ────────────────────────────────────────────

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
