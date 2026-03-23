namespace AutoCADTools.Storage
{
  public interface ISettingsRepository
  {
    string GetLanguage();
    void SaveLanguage(string cultureName);
  }
}
