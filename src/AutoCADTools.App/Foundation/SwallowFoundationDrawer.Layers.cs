#nullable enable

using System.IO;
using System.Linq;
using AutoCADTools.App.Const;
using AutoCADTools.App.Utils;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Color = AutoCADTools.App.Const.Color;

namespace AutoCADTools.App.Foundation;

public partial class SwallowFoundationDrawer
{
  private static readonly (string name, short colorIndex)[] LayerEntries =
  [
    (Layer.Outline, Color.White),
    (Layer.ConcreteHatch, Color.DarkGrey),
    (Layer.Axis, Color.Grey),
    (Layer.Column, Color.White),
    (Layer.Rebar, Color.Cyan),
    (Layer.Elevation, Color.Red),
    (Layer.Dimension, Color.Red),
    (Layer.LabelCyan, Color.Cyan),
    (Layer.LabelYellow, Color.Yellow),
    (Layer.LabelGreen, Color.Green),
    (Layer.Hidden, Color.Magenta)
  ];

  private void EnsureLayers(Transaction tr)
  {
    if (_db == null) return;
    var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);

    foreach (var entry in LayerEntries) {
      if (lt.Has(entry.name)) continue;
      using var ltr = new LayerTableRecord();
      ltr.Name = entry.name;
      ltr.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, entry.colorIndex);
      lt.UpgradeOpen();
      lt.Add(ltr);
      tr.AddNewlyCreatedDBObject(ltr, true);
    }
  }

  private bool EnsureDimensionStyle(Transaction tr)
  {
    if (_db == null) return false;
    var dst = (DimStyleTable)tr.GetObject(_db.DimStyleTableId, OpenMode.ForRead);
    if (dst.Has(DimStyle.Dim100)) return true;

    // UpgradeOpen immediately before the write, downgrade after
    dst.UpgradeOpen();
    try {
      using var dsr = new DimStyleTableRecord();
      dsr.Name = DimStyle.Dim100;

      // Copy settings from H100 style if available
      if (dst.Has("H100")) {
        var stdId = dst["H100"];
        var stdRec = (DimStyleTableRecord)tr.GetObject(stdId, OpenMode.ForRead);
        dsr.CopyFrom(stdRec);
        dsr.Name = DimStyle.Dim100;
      }

      dst.Add(dsr);
      tr.AddNewlyCreatedDBObject(dsr, true);
      return true;
    }
    finally {
      dst.DowngradeOpen();
    }
  }
}
