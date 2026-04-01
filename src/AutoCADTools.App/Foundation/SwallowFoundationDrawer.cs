#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AutoCADTools.Core;

namespace AutoCADTools.App.Foundation;

public class SwallowFoundationDrawer
{
  // ── Layer names ──────────────────────────────────────────────────
  private const string LayerOutline = "THAY";
  private const string LayerConcreteHatch = "HATCH";
  private const string LayerAxis = "TIM";
  private const string LayerColumn = "COT";
  private const string LayerRebar = "THEP";
  private const string LayerElevation = "CDA";
  private const string LayerDimension = "DIM";
  private const string LayerLabel = "TEXT";

  private Document? _doc;
  private Database? _db;

  // Model scale: model values are in mm; S = 1/unit_scale so drawing is in metres
  private double S => 1.0 / Model.Scale;

  private SwallowFoundationModel Model { get; set; } = new SwallowFoundationModel();

  // ╔══════════════════════════════════════════════════════════════╗
  // ║                     PUBLIC  ENTRY  POINTS                     ║
  // ╚══════════════════════════════════════════════════════════════╝

  public void Draw(SwallowFoundationModel model)
  {
    var doc = Application.DocumentManager.MdiActiveDocument;
    if (doc == null) return;

    var ed = doc.Editor;
    var ppr = ed.GetPoint(new PromptPointOptions("\nPick a base point: "));
    if (ppr.Status != PromptStatus.OK) return;

    DrawAtPoint(model, ppr.Value);
  }

  public void DrawAtPoint(SwallowFoundationModel model, Point3d basePoint)
  {
    Model = model;
    _doc = Application.DocumentManager.MdiActiveDocument;
    if (_doc == null) return;
    _db = _doc.Database;

    using (_doc.LockDocument())
    using (var tr = _db.TransactionManager.StartTransaction())
    {
      var btr = (BlockTableRecord)tr.GetObject(
        SymbolUtilityServices.GetBlockModelSpaceId(_db), OpenMode.ForWrite);

      EnsureLayers();

      DrawPlanAtPoint(basePoint, btr, tr);
      DrawPlanDimensionsAtPoint(basePoint, btr, tr);
      DrawPlanRebarAtPoint(basePoint, btr, tr);
      DrawSectionAtPoint(basePoint, btr, tr);
      DrawSectionDimensionsAtPoint(basePoint, btr, tr);
      DrawSectionRebarAtPoint(basePoint, btr, tr);

      tr.Commit();
    }
  }

  // ╔══════════════════════════════════════════════════════════════╗
  // ║                     PRIVATE  HELPERS                          ║
  // ╚══════════════════════════════════════════════════════════════╝

  private void EnsureLayers()
  {
    if (_db == null) return;
    using var tr = _db.TransactionManager.StartTransaction();
    var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);

    foreach (var entry in new[]
    {
      (LayerOutline, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0)),
      (LayerConcreteHatch, Autodesk.AutoCAD.Colors.Color.FromRgb(180, 180, 180)),
      (LayerAxis, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 255)),
      (LayerColumn, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0)),
      (LayerRebar, Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0)),
      (LayerElevation, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 176, 80)),
      (LayerDimension, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0)),
      (LayerLabel, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0)),
    })
    {
      if (!lt.Has(entry.Item1))
      {
        using var ltr = new LayerTableRecord { Name = entry.Item1, Color = entry.Item2 };
        lt.UpgradeOpen();
        lt.Add(ltr);
        tr.AddNewlyCreatedDBObject(ltr, true);
      }
    }
    tr.Commit();
  }

  // ── Geometry primitives ─────────────────────────────────────────

  private Line AddLine(Point3d pt1, Point3d pt2, string layer, Transaction tr, BlockTableRecord btr)
  {
    var ln = new Line(pt1, pt2) { Layer = layer };
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
      AddLine(new Point3d(0, 0, 0), new Point3d(rw, 0, 0), LayerRebar, tr, btrDef);
      AddLine(new Point3d(rw, 0, 0), new Point3d(rw, rh, 0), LayerRebar, tr, btrDef);
      AddLine(new Point3d(rw, rh, 0), new Point3d(0, rh, 0), LayerRebar, tr, btrDef);
      AddLine(new Point3d(0, rh, 0), new Point3d(0, 0, 0), LayerRebar, tr, btrDef);

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
    br.TransformBy(Matrix3d.Rotation(angle, Vector3d.ZAxis, pt));
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

  // ── Line style helper ─────────────────────────────────────────────

  private void SetLinetype(Entity entity, string linetypeName)
  {
    if (_db == null) return;
    using var tr = _db.TransactionManager.StartTransaction();
    var lt = (LinetypeTable)tr.GetObject(_db.LinetypeTableId, OpenMode.ForRead);
    if (lt.Has(linetypeName))
    {
      entity.Linetype = linetypeName;
    }
    tr.Commit();
  }

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
    AddLine(p1, p2, LayerOutline, tr, btr);
    AddLine(p2, p3, LayerOutline, tr, btr);
    AddLine(p3, p4, LayerOutline, tr, btr);
    AddLine(p4, p1, LayerOutline, tr, btr);

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
      LayerAxis, tr, btr);
    SetLinetype(lnAxisX, "DASHED");

    var lnAxisY = AddLine(
      new Point3d(bp.X + cpX, bp.Y - Ly, bp.Z),
      new Point3d(bp.X + cpX, bp.Y + Ly, bp.Z),
      LayerAxis, tr, btr);
    SetLinetype(lnAxisY, "DASHED");

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

      AddLine(sp1, sp2, LayerOutline, tr, btr);
      AddLine(sp2, sp3, LayerOutline, tr, btr);
      AddLine(sp3, sp4, LayerOutline, tr, btr);
      AddLine(sp4, sp1, LayerOutline, tr, btr);
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
          LayerRebar, tr, btr);
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
          LayerRebar, tr, btr);
        ln.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);

        var tagPt = new Point3d(x + 0.5 * S, bp.Y + 0.5 * S, bp.Z);
        InsertMongRebarTag(tagPt, Math.PI / 2, $"phi{(int)ryDia}", tr, btr);
      }
    }
  }

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
      AddLine(new Point3d(sec.X - Lx / 2, step1Top, sec.Z), new Point3d(sec.X + Lx / 2, step1Top, sec.Z), LayerOutline, tr, btr);
      AddLine(new Point3d(sec.X + Lx / 2, step1Top, sec.Z), new Point3d(sec.X + cwX / 2, step1Top + 0.3 * S, sec.Z), LayerOutline, tr, btr);
      AddLine(new Point3d(sec.X + cwX / 2, step1Top + 0.3 * S, sec.Z), new Point3d(sec.X - cwX / 2, step1Top + 0.3 * S, sec.Z), LayerOutline, tr, btr);
      AddLine(new Point3d(sec.X - cwX / 2, step1Top + 0.3 * S, sec.Z), new Point3d(sec.X - Lx / 2, step1Top, sec.Z), LayerOutline, tr, btr);
    }
    else
    {
      AddLine(new Point3d(sec.X - Lx / 2, step1Top, sec.Z), new Point3d(sec.X + Lx / 2, step1Top, sec.Z), LayerOutline, tr, btr);
    }

    // ── Column stub ─────────────────────────────────────────────────
    double colStubH = 0.5 * S;
    double colTopZ = colBaseZ + colStubH;
    AddLine(new Point3d(sec.X - cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X + cwX / 2, colBaseZ, sec.Z), LayerColumn, tr, btr);
    AddLine(new Point3d(sec.X - cwX / 2, colTopZ, sec.Z), new Point3d(sec.X + cwX / 2, colTopZ, sec.Z), LayerColumn, tr, btr);
    AddLine(new Point3d(sec.X - cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X - cwX / 2, colTopZ, sec.Z), LayerColumn, tr, btr);
    AddLine(new Point3d(sec.X + cwX / 2, colBaseZ, sec.Z), new Point3d(sec.X + cwX / 2, colTopZ, sec.Z), LayerColumn, tr, btr);

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
        var ln = AddLine(new Point3d(x, rebarY, sec.Z), new Point3d(x, rebarY + 0.1, sec.Z), LayerRebar, tr, btr);
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
        LayerRebar, tr, btr);
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
            LayerRebar, tr, btr);
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

        AddLine(new Point3d(sX1, sY1, sec.Z), new Point3d(sX2, sY1, sec.Z), LayerRebar, tr, btr);
        AddLine(new Point3d(sX2, sY1, sec.Z), new Point3d(sX2, sY2, sec.Z), LayerRebar, tr, btr);
        AddLine(new Point3d(sX2, sY2, sec.Z), new Point3d(sX1, sY2, sec.Z), LayerRebar, tr, btr);
        AddLine(new Point3d(sX1, sY2, sec.Z), new Point3d(sX1, sY1, sec.Z), LayerRebar, tr, btr);

        AddMText(new Point3d(sec.X + cwX / 2 + 0.2 * S, sY1 + 0.2 * S, sec.Z),
          $"phi{(int)stDia}@{(int)stSpacing}", tagH * 0.7, LayerLabel, tr, btr);
      }
    }

    // ── Column cross-section cut marker (IBMcCot) ──────────────────
    double mcX = sec.X + Lx / 2 + 0.5 * S;
    double mcY = colBaseZ + 0.3 * S;
    AddCircle(new Point3d(mcX, mcY, sec.Z), 0.2 * S, LayerColumn, tr, btr);
    AddCircle(new Point3d(mcX, mcY - 0.4 * S, sec.Z), 0.2 * S, LayerColumn, tr, btr);
    AddLine(new Point3d(mcX, mcY - 0.2 * S, sec.Z), new Point3d(mcX, mcY - 0.2 * S, sec.Z), LayerColumn, tr, btr);
    AddMText(new Point3d(mcX + 0.3 * S, mcY - 0.2 * S, sec.Z), "MC", tagH, LayerLabel, tr, btr);
  }
}
