#nullable enable

using AutoCADTools.App.Utils;
using AutoCADTools.Core.Localization;
using AutoCADTools.Core.Utils;
using AutoCADTools.Storage;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Microsoft.Extensions.DependencyInjection;

[assembly: ExtensionApplication(typeof(AutoCADTools.App.AppEntry))]

namespace AutoCADTools.App
{
  public class AppEntry : IExtensionApplication
  {
    private static ServiceProvider? _serviceProvider;
    private static ISettingsRepository? _settingsRepository;
    private static Presentation.Canvas.CanvasViewModel? _instanceVm;

    public static void RegisterViewModel(Presentation.Canvas.CanvasViewModel vm)
    {
      _instanceVm = vm;
    }

    public static Presentation.Canvas.CanvasViewModel? CanvasViewModel => _instanceVm;

    public void Initialize()
    {
      try {
        var services = new ServiceCollection();
        services.AddTransient<Presentation.Canvas.CanvasViewModel>();
        _serviceProvider = services.BuildServiceProvider();

        _settingsRepository = new SettingsRepository();
        try {
          var lang = _settingsRepository.GetLanguage();
          LocalizationManager.SetLanguage(lang);
        }
        catch {
          LocalizationManager.SetLanguage("en");
        }

        if (ComponentManager.Ribbon == null)
          ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        else {
          var editor = Application.DocumentManager.MdiActiveDocument.Editor;
          CreatePanel();
          editor.WriteMessage($"\n{"App.Loaded".GetString()}");
        }
      }
      catch (System.Exception ex) {
        Application.DocumentManager.MdiActiveDocument?.Editor
          .WriteMessage($"\n{"App.InitializationFailed".GetString().Replace("{0}", ex.Message)}");
      }
    }

    public void Terminate()
    {
      _serviceProvider?.Dispose();
    }

    private static void ComponentManager_ItemInitialized(object sender, RibbonItemEventArgs e)
    {
      try {
        if (ComponentManager.Ribbon == null) return;

        CreatePanel();

        ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
      }
      catch (System.Exception ex) {
        MessageUtils.Error(ex.Message);
      }
    }

    private static void CreatePanel()
    {
      RibbonUtils.CreatePanel("App.Title".GetString(), "Ko0ls Tab")
        .AddButton("Command.Settings".GetString(), "KOOLS_CMD_SETTINGS", "Command.Settings".GetString(), iconKey: "settings")
        .Build();
    }

    // ── Stub command methods ────────────────────────────────────────

    [CommandMethod("KOOLS_CMD_LINE")]
    public void CmdLine()
    {
      // TODO: implement
    }

    [CommandMethod("KOOLS_CMD_CIRCLE")]
    public void CmdCircle()
    {
      // TODO: implement
    }

    [CommandMethod("KOOLS_CMD_ARC")]
    public void CmdArc()
    {
      // TODO: implement
    }

    [CommandMethod("KOOLS_CMD_SETTINGS")]
    public void CmdSettings()
    {
      var vm = new Presentation.ViewModels.SettingViewModel(_settingsRepository!);
      var window = new Presentation.Views.SettingsWindow(vm);
      Application.ShowModalWindow(window);
    }
  }
}