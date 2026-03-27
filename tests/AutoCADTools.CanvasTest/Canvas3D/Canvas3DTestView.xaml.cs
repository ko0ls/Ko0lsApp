using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Presentation._3D.Shapes3D;

namespace AutoCADTools.CanvasTest.Canvas3D;

public partial class Canvas3DTestView : UserControl
{
  private enum ViewportMode { Select, Orbit, Pan }

  private readonly Canvas3DTestViewModel _viewModel;
  private Point _lastMousePosition;
  private ViewportMode _currentMode = ViewportMode.Select;
  private Point3D _cameraTarget;

  public Canvas3DTestView()
  {
    InitializeComponent();

    _viewModel = new Canvas3DTestViewModel();
    DataContext = _viewModel;

    _cameraTarget = new Point3D(
      _viewModel.Camera.Position.X + _viewModel.Camera.LookDirection.X,
      _viewModel.Camera.Position.Y + _viewModel.Camera.LookDirection.Y,
      _viewModel.Camera.Position.Z + _viewModel.Camera.LookDirection.Z);

    MainViewport.Focus();
  }

  private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
  {
    MainViewport.Focus();
    _lastMousePosition = e.GetPosition(MainViewport);

    if (e.ChangedButton == MouseButton.Right)
    {
      _currentMode = ViewportMode.Orbit;
      RootGrid.CaptureMouse();
      return;
    }

    if (e.ChangedButton == MouseButton.Middle)
    {
      if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        _currentMode = ViewportMode.Orbit;
      else
        _currentMode = ViewportMode.Pan;
      RootGrid.CaptureMouse();
      return;
    }

    if (e.ChangedButton == MouseButton.Left)
    {
      var hit = HitTestTopShape(_lastMousePosition);
      _viewModel.SetSelectedShape(hit);
    }
  }

  private void Viewport_MouseMove(object sender, MouseEventArgs e)
  {
    var current = e.GetPosition(MainViewport);
    var deltaX = current.X - _lastMousePosition.X;
    var deltaY = current.Y - _lastMousePosition.Y;
    _lastMousePosition = current;

    if (_currentMode == ViewportMode.Orbit &&
        (Mouse.RightButton == MouseButtonState.Pressed || Mouse.MiddleButton == MouseButtonState.Pressed))
    {
      Orbit(deltaX, deltaY);
      return;
    }

    if (_currentMode == ViewportMode.Pan && Mouse.MiddleButton == MouseButtonState.Pressed)
    {
      Pan(deltaX, deltaY);
      return;
    }

    // Hover highlight khi đang ở Select mode
    if (_currentMode == ViewportMode.Select)
    {
      var hoverHit = HitTestTopShape(current);
      _viewModel.SetHoveredShape(hoverHit);
    }
  }

  private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
  {
    if (e.ChangedButton == MouseButton.Right || e.ChangedButton == MouseButton.Middle)
    {
      _currentMode = ViewportMode.Select;
      RootGrid.ReleaseMouseCapture();
    }
  }

  private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
  {
    var step = e.Delta > 0 ? 1.0 : -1.0;
    Zoom(step);
  }

  private void Viewport_MouseLeave(object sender, MouseEventArgs e)
  {
    if (_currentMode == ViewportMode.Select)
      _viewModel.ClearHover();
  }

  private void Viewport_KeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Escape)
    {
      _viewModel.ClearSelection();
      _viewModel.ClearHover();
    }
  }

  private void Viewport_KeyUp(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
    {
      _currentMode = ViewportMode.Select;
      MainViewport.ReleaseMouseCapture();
    }
  }

  private Shape3DBase? HitTestTopShape(Point screenPoint)
  {
    var ray = ScreenPointToRay(screenPoint);
    Shape3DBase? topShape = null;
    var nearestDistance = double.MaxValue;

    foreach (var shape in _viewModel.Shapes)
    {
      if (!shape.IsVisible || shape.IsLocked)
        continue;

      if (!shape.HitTest(screenPoint, ray, 5.0))
        continue;

      var bb = shape.GetBoundingBox();
      var center = new Point3D(
        bb.X + bb.SizeX / 2,
        bb.Y + bb.SizeY / 2,
        bb.Z + bb.SizeZ / 2);

      var distance = (center - _viewModel.Camera.Position).Length;
      if (distance < nearestDistance)
      {
        nearestDistance = distance;
        topShape = shape;
      }
    }

    return topShape;
  }

  private Ray3D ScreenPointToRay(Point screenPoint)
  {
    var camera = _viewModel.Camera;
    var viewportWidth = MainViewport.ActualWidth;
    var viewportHeight = MainViewport.ActualHeight;

    if (viewportWidth <= 0 || viewportHeight <= 0)
      return new Ray3D(camera.Position, camera.LookDirection);

    var nx = (screenPoint.X / viewportWidth) * 2 - 1;
    var ny = -((screenPoint.Y / viewportHeight) * 2 - 1);

    var aspect = viewportWidth / viewportHeight;
    var fovRad = camera.FieldOfView * Math.PI / 180.0;
    var halfHeight = Math.Tan(fovRad / 2);
    var halfWidth = halfHeight * aspect;

    var lookDir = camera.LookDirection;
    lookDir.Normalize();

    var right = Vector3D.CrossProduct(lookDir, camera.UpDirection);
    right.Normalize();

    var up = Vector3D.CrossProduct(right, lookDir);
    up.Normalize();

    var dx = nx * halfWidth;
    var dy = ny * halfHeight;

    var worldDir = new Vector3D(
      right.X * dx + up.X * dy + lookDir.X,
      right.Y * dx + up.Y * dy + lookDir.Y,
      right.Z * dx + up.Z * dy + lookDir.Z);
    worldDir.Normalize();

    return new Ray3D(camera.Position, worldDir);
  }

  private void Orbit(double deltaX, double deltaY)
  {
    var camera = _viewModel.Camera;
    var toCamera = camera.Position - _cameraTarget;

    var yaw = new Quaternion(new Vector3D(0, 0, 1), deltaX * 0.35);

    var lookDir = camera.LookDirection;
    lookDir.Normalize();
    var right = Vector3D.CrossProduct(lookDir, camera.UpDirection);
    right.Normalize();
    var pitch = new Quaternion(right, -deltaY * 0.35);

    var rotated = RotateVector(RotateVector(toCamera, yaw), pitch);
    camera.Position = _cameraTarget + rotated;
    camera.UpDirection = RotateVector(RotateVector(camera.UpDirection, yaw), pitch);
    camera.LookDirection = _cameraTarget - camera.Position;
  }

  private void Pan(double deltaX, double deltaY)
  {
    var camera = _viewModel.Camera;

    var lookDir = camera.LookDirection;
    lookDir.Normalize();

    var right = Vector3D.CrossProduct(lookDir, camera.UpDirection);
    right.Normalize();

    var up = camera.UpDirection;
    up.Normalize();

    var speed = 0.03;
    var move = (-right * deltaX * speed) + (up * deltaY * speed);

    camera.Position += move;
    _cameraTarget += move;
    camera.LookDirection = _cameraTarget - camera.Position;
  }

  private void Zoom(double step)
  {
    var camera = _viewModel.Camera;
    var look = _cameraTarget - camera.Position;
    var length = look.Length;
    if (length < 0.001) return;

    look.Normalize();
    var amount = Math.Max(0.15, length * 0.08) * step;
    camera.Position += look * amount;
    camera.LookDirection = _cameraTarget - camera.Position;
  }

  private static Vector3D RotateVector(Vector3D vector, Quaternion quaternion)
  {
    var transform = new QuaternionRotation3D(quaternion);
    var matrix = Matrix3D.Identity;
    matrix.Rotate(transform.Quaternion);
    return matrix.Transform(vector);
  }
}
