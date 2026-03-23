using AcDb = Autodesk.AutoCAD.DatabaseServices;
using HostApp = Autodesk.AutoCAD.DatabaseServices.HostApplicationServices;

namespace AutoCADTools.App
{
  public class SettingsRepository : Storage.ISettingsRepository
  {
    private const string AppDictName = "Ko0lsSettings";
    private const string LangFieldName = "Language";
    private const string DefaultLang = "en";

    public string GetLanguage()
    {
      var db = HostApp.WorkingDatabase;
      if (db == null) return DefaultLang;
      using var tr = db.TransactionManager.StartOpenCloseTransaction();
      var nod = (AcDb.DBDictionary) tr.GetObject(
        db.NamedObjectsDictionaryId, AcDb.OpenMode.ForRead);

      if (!nod.Contains(AppDictName)) return DefaultLang;
      var appDictId = nod.GetAt(AppDictName);
      var appDict = (AcDb.DBDictionary) tr.GetObject(appDictId, AcDb.OpenMode.ForRead);

      if (!appDict.Contains(LangFieldName)) return DefaultLang;
      var xr = (AcDb.Xrecord) tr.GetObject(appDict.GetAt(LangFieldName), AcDb.OpenMode.ForRead);
      if (xr?.Data == null) return DefaultLang;

      foreach (var tv in xr.Data) {
        if (tv.TypeCode != (short) AcDb.DxfCode.Text) continue;
        var val = tv.Value as string;
        if (!string.IsNullOrEmpty(val)) return val;
      }
      tr.Commit();
      return DefaultLang;
    }

    public void SaveLanguage(string cultureName)
    {
      var db = HostApp.WorkingDatabase;
      if (db == null) return;
      using var tr = db.TransactionManager.StartOpenCloseTransaction();
      var nod = (AcDb.DBDictionary) tr.GetObject(
        db.NamedObjectsDictionaryId, AcDb.OpenMode.ForRead);

      AcDb.DBDictionary appDict;

      if (nod.Contains(AppDictName)) {
        var appDictId = nod.GetAt(AppDictName);
        appDict = (AcDb.DBDictionary) tr.GetObject(appDictId, AcDb.OpenMode.ForWrite);
      }
      else {
        nod.UpgradeOpen();
        appDict = new AcDb.DBDictionary();
        nod.SetAt(AppDictName, appDict);
        tr.AddNewlyCreatedDBObject(appDict, true);
      }

      var buf = new AcDb.ResultBuffer(
        new AcDb.TypedValue((short) AcDb.DxfCode.Text, cultureName));

      if (appDict.Contains(LangFieldName)) {
        var xr = (AcDb.Xrecord) tr.GetObject(appDict.GetAt(LangFieldName), AcDb.OpenMode.ForWrite);
        xr.Data = buf;
      }
      else {
        // appDict is already open ForWrite if it existed; newly created dict is also valid
        var xr = new AcDb.Xrecord { Data = buf };
        appDict.SetAt(LangFieldName, xr);
        tr.AddNewlyCreatedDBObject(xr, true);
      }

      tr.Commit();
    }
  }
}
