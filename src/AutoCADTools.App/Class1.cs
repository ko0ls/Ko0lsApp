#nullable enable

using Autodesk.AutoCAD.ApplicationServices.Core;
using Microsoft.Extensions.DependencyInjection;

[assembly: Autodesk.AutoCAD.Runtime.ExtensionApplication(typeof(AutoCADTools.App.AppEntry))]

namespace AutoCADTools.App
{
  public class AppEntry : Autodesk.AutoCAD.Runtime.IExtensionApplication
  {
    private static ServiceProvider? _serviceProvider;
    private static Presentation.Canvas.CanvasViewModel? _instanceVm;

    public static void RegisterViewModel(Presentation.Canvas.CanvasViewModel vm)
    {
      _instanceVm = vm;
    }

    public static Presentation.Canvas.CanvasViewModel? CanvasViewModel => _instanceVm;

    public void Initialize()
    {
      try
      {
        var services = new ServiceCollection();
        services.AddTransient<Presentation.Canvas.CanvasViewModel>();
        _serviceProvider = services.BuildServiceProvider();
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\nAutoCADTools loaded successfully.");
      }
      catch (System.Exception ex)
      {
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\nAutoCADTools initialization failed: {ex.Message}");
      }
    }

    public void Terminate()
    {
      _serviceProvider?.Dispose();
    }
  }
}
