#nullable enable

using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  private void EnsureLayers(Transaction tr)
  {
    if (_db == null) return;
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
  }
}
