using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace ModListHashChecker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

    public class ModListHashChecker : BaseUnityPlugin
    {
        public static ModListHashChecker instance = null!;
        public bool NoHashFound { get; internal set; } = false;
        public bool HashMismatch { get; internal set; } = false;
        public bool ClientMismatch { get; internal set; } = false;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "TeamMLC.ModlistHashChecker";
            public const string PLUGIN_NAME = "ModlistHashChecker";
            public const string PLUGIN_VERSION = "0.2.2";
        }

        internal static ManualLogSource Log = null!;

        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} loaded with version {PluginInfo.PLUGIN_VERSION}!");
            ConfigManager.Init(this.Config);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
