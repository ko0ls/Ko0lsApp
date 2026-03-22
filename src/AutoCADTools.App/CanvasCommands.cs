using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(AutoCADTools.App.CanvasCommands))]

namespace AutoCADTools.App
{
  public class CanvasCommands
  {
    /// <summary>
    /// Creates a demo Canvas with sample annotations and registers it in AppEntry.
    /// Run from AutoCAD: NETLOAD the DLL, then type KO0LSDEMO.
    /// </summary>
    [CommandMethod("KO0LSDEMO")]
    public void Ko0lsDemo()
    {
      try {
        var (view, vm) = CanvasDemo.CreateDemoCanvas();
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage(
          "\n[OK] Ko0lsDemo: {0} annotations added. Access via AppEntry.CanvasViewModel.",
          vm.Canvas?.Children.Count ?? 0);
      }
      catch (System.Exception ex) {
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage(
          $"\n[Error] Ko0lsDemo failed: {ex.Message}");
      }
    }
  }
}
