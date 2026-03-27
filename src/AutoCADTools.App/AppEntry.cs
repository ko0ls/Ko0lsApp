#nullable enable

using System;
using System.Linq;
using AutoCADTools.App.Utils;
using AutoCADTools.Core.Localization;
using AutoCADTools.Core.Utils;
using AutoCADTools.Presentation.Utils;
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

        // 3D system
        services.AddSingleton<Service._3D.IObjectManager3D, Service._3D.ObjectManager3D>();
        services.AddSingleton<Service._3D.ICameraService, Service._3D.CameraService>();
        services.AddSingleton<Service._3D.IViewportService, Service._3D.ViewportService>();
        services.AddSingleton<Presentation._3D.Viewport3D.Selection.SelectionService>();
        services.AddTransient<Presentation._3D.Viewport3D.Viewport3DViewModel>();
        services.AddTransient<Presentation._3D.Properties3D.Properties3DViewModel>();
        services.AddTransient<Presentation._3D.ObjectManager3D.ObjectManager3DViewModel>();
        services.AddTransient<Presentation._3D.Viewport3DWindowViewModel>();
        services.AddTransient<Presentation._3D.MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _settingsRepository = new SettingsRepository();
        try {
          var lang = _settingsRepository.GetLanguage();
          LocalizationManager.SetLanguage(lang);
        }
        catch {
          LocalizationManager.SetLanguage("en");
        }

        // Trigger LocalizationService singleton init so LanguageChanged is subscribed
        _ = LocalizationService.Instance;

        if (ComponentManager.Ribbon == null)
          ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        else {
          var editor = Application.DocumentManager.MdiActiveDocument.Editor;
          CreatePanel();
          LocalizationManager.LanguageChanged += OnLanguageChanged;
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
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
      }
      catch (System.Exception ex) {
        MessageUtils.Error(ex.Message);
      }
    }

    private static void OnLanguageChanged(object? sender, EventArgs e)
    {
      RebuildRibbon();
    }

    private static void RebuildRibbon()
    {
      if (ComponentManager.Ribbon == null) return;
      const string tabName = "Ko0ls Tab";
      foreach (var tab in ComponentManager.Ribbon.Tabs) {
        if (tab.Title == tabName) {
          foreach (var panel in tab.Panels.ToList()) {
            tab.Panels.Remove(panel);
          }
          break;
        }
      }
      CreatePanel();
    }

    private static void CreatePanel()
    {
      RibbonUtils.CreatePanel("Panel.Settings.Title".GetString(), "Ko0ls Tab")
        .AddButton("Command.Settings".GetString(), "KOOLS_CMD_SETTINGS", "Command.Settings".GetString(), iconKey: "settings")
        .AddButton("3D View", "KOOLS_CMD_3DVIEW", "Open 3D Viewport", iconKey: "")
        .Build();
    }

    // ── Stub command methods ────────────────────────────────────────

    [CommandMethod("KOOLS_CMD_SETTINGS")]
    public void CmdSettings()
    {
      var vm = new Presentation.ViewModels.SettingViewModel(_settingsRepository!);
      var window = new Presentation.Views.SettingsWindow(vm);
      Application.ShowModalWindow(window);
    }

    [CommandMethod("KOOLS_CMD_3DVIEW")]
    public void Cmd3DView()
    {
      if (_serviceProvider == null) return;
      try {
        var mainVm = new Presentation._3D.MainViewModel(
          _serviceProvider.GetRequiredService<Presentation.Canvas.CanvasViewModel>(),
          _serviceProvider.GetRequiredService<Presentation._3D.Viewport3DWindowViewModel>());
        var window = new Presentation._3D.MainView(mainVm);
        Application.ShowModalWindow(window);
      }
      catch (System.Exception ex) {
        Application.DocumentManager.MdiActiveDocument?.Editor
          .WriteMessage($"\nMain View error: {ex.Message}");
      }
    }

    [CommandMethod("vmd1")]
    public void CmdSwallowFoundation()
    {
      try {
        var window = new Presentation.Views.SwallowFoundationWindow();
        Application.ShowModalWindow(window);
      }
      catch (System.Exception ex) {
        Application.DocumentManager.MdiActiveDocument?.Editor
          .WriteMessage($"\nSwallow foundation error: {ex.Message}");
      }
    }
  }
}