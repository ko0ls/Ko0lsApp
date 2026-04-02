using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutoCADTools.App.Utils;

public static class CloneAutocadStyle
{
  internal static bool ImportDimensionStyleFromFile(string sourceCadFilePath, string dimStyleName, Transaction acTrans)
  {
    if (!File.Exists(sourceCadFilePath)) return false;
    using var currentDoc = Application.DocumentManager.MdiActiveDocument;
    var currentDb = currentDoc.Database;
    var sourceDb = new Database(false, true);
    using (sourceDb) {
      var fileExtension = Path.GetExtension(sourceCadFilePath);
      try {
        switch (fileExtension) {
          case ".dxf":
            sourceDb.DxfIn(sourceCadFilePath, null);
            break;
          case ".dwg":
          case ".dwt":
            sourceDb.ReadDwgFile(sourceCadFilePath, FileOpenMode.OpenForReadAndReadShare, false, null);
            break;
          default:
            return false;
        }
      }
      catch (System.Exception) {
        return false;
      }

      var idsForInsert = new ObjectIdCollection();
      var acDimTable = acTrans.GetObject(currentDb.DimStyleTableId, OpenMode.ForWrite) as DimStyleTable;
      if (acDimTable == null) return false;
      using (var scTrans = sourceDb.TransactionManager.StartTransaction()) {
        var scDimStyleTable =
          scTrans.GetObject(sourceDb.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
        if (scDimStyleTable == null) return false;
        var scDimStyleTblRec =
          (DimStyleTableRecord) scTrans.GetObject(scDimStyleTable[dimStyleName], OpenMode.ForRead);
        if (scDimStyleTblRec == null) return false;
        idsForInsert.Add(scDimStyleTblRec.ObjectId);
      }

      if (idsForInsert.Count == 0) return false;
      var iMap = new IdMapping();
      currentDb.WblockCloneObjects(idsForInsert, currentDb.DimStyleTableId, iMap, DuplicateRecordCloning.Ignore,
        false);
    }

    return true;
  }
}