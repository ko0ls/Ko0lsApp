#nullable enable

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AutoCADTools.Core;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
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
  private double S => Model.Scale;

  private SwallowFoundationModel Model { get; set; } = new SwallowFoundationModel();

  // ╔══════════════════════════════════════════════════════════════╗
  // ║                XDATA APP NAME REGISTRATION                 ║
  // ╚══════════════════════════════════════════════════════════════╝

  private void EnsureAppNames(Transaction tr)
  {
    if (_db == null) return;
    var rat = (RegAppTable)tr.GetObject(_db.RegAppTableId, OpenMode.ForRead);
    foreach (var name in new[] { "Length", "ColWidth", "AxisOffset", "StepHeight", "Elevation" }) {
      if (rat.Has(name)) continue;
      using var record = new RegAppTableRecord();
      record.Name = name;
      rat.UpgradeOpen();
      rat.Add(record);
      tr.AddNewlyCreatedDBObject(record, true);
    }
  }

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

      EnsureLayers(tr);
      EnsureAppNames(tr);

      DrawPlanAtPoint(basePoint, btr, tr);
      DrawPlanDimensionsAtPoint(basePoint, btr, tr);
      DrawPlanRebarAtPoint(basePoint, btr, tr);
      DrawSectionAtPoint(basePoint, btr, tr);
      DrawSectionDimensionsAtPoint(basePoint, btr, tr);
      DrawSectionRebarAtPoint(basePoint, btr, tr);

      tr.Commit();
    }
  }
}
