using System.Windows.Media;

namespace AutoCADTools.Core._3D
{
  public interface IMaterial
  {
    Color DiffuseColor { get; set; }
    Color EmissiveColor { get; set; }
    Color SpecularColor { get; set; }
    double SpecularPower { get; set; }
    double Transparency { get; set; }
    string TexturePath { get; set; }
    bool BackFaceCulling { get; set; }
  }
}
