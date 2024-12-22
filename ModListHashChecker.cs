using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace ModListHashChecker
{
    [BepInPlugin("TeamMLC.ModlistHashChecker", "ModlistHashChecker", "0.1.2")]

    public class ModListHashChecker : BaseUnityPlugin
    {
        public static ModListHashChecker instance;
        public bool noHashFound = false;
        public bool hashMismatch = false;
        public bool clientMismatch = false;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "TeamMLC.ModlistHashChecker";
            public const string PLUGIN_NAME = "ModlistHashChecker";
            public const string PLUGIN_VERSION = "0.1.2";
        }

        internal static ManualLogSource Log;

        private void Awake()
        {
            ModListHashChecker.instance = this;
            ModListHashChecker.Log = base.Logger;
            ModListHashChecker.Log.LogInfo((object)"ModListHashChecker loaded with version 0.1.2!");
            ConfigManager.Init(Config);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
