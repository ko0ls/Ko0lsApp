using System.Windows.Media;
using AutoCADTools.Core._3D;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation._3D.Models
{
  public class Material3D : BindableObject, IMaterial
  {
    public Material3D() { }

    private Color _diffuseColor = Colors.LightGray;
    [ChangeTracker]
    public Color DiffuseColor
    {
      get => _diffuseColor;
      set => SetProperty(ref _diffuseColor, value);
    }

    private Color _emissiveColor = Colors.Black;
    [ChangeTracker]
    public Color EmissiveColor
    {
      get => _emissiveColor;
      set => SetProperty(ref _emissiveColor, value);
    }

    private Color _specularColor = Colors.White;
    [ChangeTracker]
    public Color SpecularColor
    {
      get => _specularColor;
      set => SetProperty(ref _specularColor, value);
    }

    private double _specularPower = 64.0;
    [ChangeTracker]
    public double SpecularPower
    {
      get => _specularPower;
      set => SetProperty(ref _specularPower, value);
    }

    private double _transparency = 0.0;
    [ChangeTracker]
    public double Transparency
    {
      get => _transparency;
      set => SetProperty(ref _transparency, value);
    }

    private string _texturePath = "";
    [ChangeTracker]
    public string TexturePath
    {
      get => _texturePath;
      set => SetProperty(ref _texturePath, value);
    }

    private bool _backFaceCulling = true;
    [ChangeTracker]
    public bool BackFaceCulling
    {
      get => _backFaceCulling;
      set => SetProperty(ref _backFaceCulling, value);
    }

    public Material3D Clone()
    {
      return ObjectCloner.Clone(this) ?? new Material3D();
    }
  }
}
