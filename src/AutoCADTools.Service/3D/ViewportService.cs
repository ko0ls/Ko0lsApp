using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D.Enums;

namespace AutoCADTools.Service._3D
{
  public class ViewportService : IViewportService
  {
    private EnumViewportMode _currentMode = EnumViewportMode.Select;
    private EnumViewportMode _previousMode = EnumViewportMode.Select;
    private Point _lastMousePosition;
    private ICameraService _cameraService;

    public ICameraService CameraService
    {
      get => _cameraService ?? throw new InvalidOperationException("CameraService not set");
      set => _cameraService = value;
    }

    public EnumViewportMode CurrentMode
    {
      get => _currentMode;
      set
      {
        if (_currentMode != value)
        {
          _currentMode = value;
          ModeChanged?.Invoke(this, value);
        }
      }
    }

    public Point LastMousePosition => _lastMousePosition;

    public event EventHandler<EnumViewportMode> ModeChanged;

    public void OnMouseMove(Point screenPt, bool isLeftBtnDown, bool isRightBtnDown)
    {
      var deltaX = screenPt.X - _lastMousePosition.X;
      var deltaY = screenPt.Y - _lastMousePosition.Y;
      _lastMousePosition = screenPt;

      if (Math.Abs(deltaX) < 0.5 && Math.Abs(deltaY) < 0.5)
        return;

      if (_cameraService == null) return;

      switch (CurrentMode)
      {
        case EnumViewportMode.Orbit:
          if (isLeftBtnDown)
            _cameraService.Orbit(new Vector3D(0, 1, 0), deltaX * 0.5);
          if (isRightBtnDown)
            _cameraService.Orbit(new Vector3D(1, 0, 0), -deltaY * 0.5);
          break;

        case EnumViewportMode.Pan:
          if (isLeftBtnDown || isRightBtnDown)
            _cameraService.Pan(new Vector3D(-deltaX * 0.05, deltaY * 0.05, 0), 1);
          break;
      }
    }

    public void OnMouseWheel(int delta)
    {
      if (_cameraService == null) return;
      if (delta > 0)
        _cameraService.ZoomIn();
      else if (delta < 0)
        _cameraService.ZoomOut();
    }

    public void OnKeyDown(Key key)
    {
      switch (key)
      {
        case Key.Space:
          _previousMode = CurrentMode;
          CurrentMode = EnumViewportMode.Orbit;
          break;
        case Key.F:
          _cameraService.ZoomToFit(new Rect3D());
          break;
        case Key.Escape:
          CurrentMode = EnumViewportMode.Select;
          _cameraService.SetPresetView(EnumPresetView.Isometric);
          break;
        case Key.W:
          CurrentMode = EnumViewportMode.Select;
          break;
        case Key.E:
          CurrentMode = EnumViewportMode.Pan;
          break;
        case Key.R:
          CurrentMode = EnumViewportMode.Zoom;
          break;
        case Key.O:
          CurrentMode = EnumViewportMode.Orbit;
          break;
        case Key.D1:
          _cameraService.SetPresetView(EnumPresetView.Top);
          break;
        case Key.D2:
          _cameraService.SetPresetView(EnumPresetView.Front);
          break;
        case Key.D3:
          _cameraService.SetPresetView(EnumPresetView.Left);
          break;
        case Key.D4:
          _cameraService.SetPresetView(EnumPresetView.Isometric);
          break;
      }
    }
  }
}
