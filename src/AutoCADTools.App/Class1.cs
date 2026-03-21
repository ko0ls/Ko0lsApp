using Autodesk.AutoCAD.Runtime;

[assembly: ExtensionApplication(typeof(AutoCADTools.App.AppEntry))]

namespace AutoCADTools.App
{
  public class AppEntry : IExtensionApplication
  {
    public void Initialize()
    {
      var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
      if (doc != null) {
        var ed = doc.Editor;
        ed.WriteMessage("\nAutoCADTools loaded successfully.\n");
      }
    }

    public void Terminate()
    {
    }
  }
}