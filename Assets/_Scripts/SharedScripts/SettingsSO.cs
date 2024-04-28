namespace SharedScripts {

    public class SettingsSO
    {
    public static SettingsSO Instance { get; private set; }
    public bool doNotDestroyOnLeave = true;

    static SettingsSO()
    {
            Instance = new SettingsSO();
    }

    private SettingsSO()
    {
            doNotDestroyOnLeave = true;
    }
    }
}