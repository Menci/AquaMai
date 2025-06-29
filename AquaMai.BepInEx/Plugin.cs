using System.Reflection;
using AquaMai.Common;
using BepInEx;
using BepInEx.Logging;

namespace AquaMai.BepInEx;

[BepInPlugin(PluginName, BuildInfo.Name, BuildInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginName = "net.aquadx.aquamai";

    public readonly static ManualLogSource LogSource = global::BepInEx.Logging.Logger.CreateLogSource(BuildInfo.Name);

    private static bool _isInitialized = false;

    public void Awake()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        var harmony = new HarmonyLib.Harmony(PluginName);

#if !NO_AMDAEMON
        // Early initialization of AMDaemon.NET
        try
        {
            Manager.AmManager.Instance.Initialize();
            harmony.PatchAll(typeof(Plugin));
        }
        catch (System.Exception ex)
        {
            LogSource.LogError($"AMDaemon.NET early initialization failed: {ex.Message}");
            return;
        }
#endif

        Common.AquaMai.Bootstrap(new BootstrapOptions
        {
            CurrentAssembly = Assembly.GetExecutingAssembly(),
            Harmony = harmony,
            MsgStringAction = LogSource.LogMessage,
            MsgObjectAction = LogSource.LogMessage,
            ErrorStringAction = LogSource.LogError,
            ErrorObjectAction = LogSource.LogError,
            WarningStringAction = LogSource.LogWarning,
            WarningObjectAction = LogSource.LogWarning,
        });
    }

    public void OnGUI()
    {
        Common.AquaMai.OnGUI();
    }

#if !NO_AMDAEMON
    [HarmonyLib.HarmonyPatch(typeof(Manager.AmManager), "Initialize")]
    [HarmonyLib.HarmonyPrefix]
    public static bool PreAmManagerInitialize()
    {
        return false; // Prevent AMDaemon.NET from initializing again
    }
#endif
}
