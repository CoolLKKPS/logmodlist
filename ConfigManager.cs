using BepInEx.Configuration;

namespace ModListHashChecker
{
    public class ConfigManager
    {
        public static ConfigManager Instance { get; private set; }
        public static ConfigFile config { get; private set; }

        public static void Init(ConfigFile config)
        {
            Instance = new ConfigManager(config);
        }

        public static ConfigEntry<string> ExpectedModListHash { get; private set; }
        public static ConfigEntry<bool> NoExpectedHashMessage { get; private set; }
        //public static ConfigEntry<bool> AutosetModListHash { get; private set; }
        public static ConfigEntry<bool> MenuWarning { get; private set; }
        public static ConfigEntry<bool> JoinWarning { get; private set; }
        public static ConfigEntry<string> WarningMessageText { get; private set; }
        public static ConfigEntry<string> JoinWarningText { get; private set; }
        public static ConfigEntry<int> JoinWarningDelay { get; private set; }
        public static ConfigEntry<string> WarningButtonIgnoreText { get; private set; }
        public static ConfigEntry<string> WarningButtonResetText { get; private set; }
        public static ConfigEntry<string> NoHashMessageText { get; private set; }
        public static ConfigEntry<string> NoHashRightButtonText { get; private set; }
        public static ConfigEntry<string> NoHashLeftButtonText { get; private set; }

        private ConfigManager(ConfigFile loadconfig)
        {
            config = loadconfig;
            ExpectedModListHash = config.Bind("General", "ExpectedModListHash", "", "The expected modlist hash for this modpack. Do not change this unless you know what you're doing.");
            //AutosetModListHash = config.Bind("General", "AutosetModListHash", false, "If true, override the current expected hash to the current one. Do not change this unless you know what you're doing.");
            NoExpectedHashMessage = config.Bind("General", "NoExpectedHashMessage", true, "Enable or Disable displaying a warning message in the menus when the expected hash is empty. Do not change this unless you know what you're doing.");
            NoHashMessageText = config.Bind("Menu Warning", "NoHashMessageText", "ExpectedModListHash configuration item is blank.\n\nWould you like to set it to the currently loaded list of mods?", "Menu Message to display when the expected hash is empty");
            NoHashRightButtonText = config.Bind("Menu Warning", "NoHashRightButtonText", "No", "Button text for leaving the ExpectedModListHash blank");
            NoHashLeftButtonText = config.Bind("Menu Warning", "NoHashLeftButtonText", "Yes", "Button text for setting the ExpectedModListHashto the detected hash");
            MenuWarning = config.Bind("General", "MenuWarning", true, "Enable or Disable displaying a warning message in the menus when the hash does not match the expected hash.");
            JoinWarning = config.Bind("General", "JoinWarning", true, "Enable or Disable displaying a warning message when a client joins and the hash does not match the host hash.");
            JoinWarningText = config.Bind("Join Warning", "JoinWarningText", "Your modlist does not match the expected modlist hash.\n\n You may experience issues.", "Message to display in Hash Mismatch Menu Warning Message");
            JoinWarningDelay = config.Bind("Join Warning", "JoinWarningDelay", 5, "Seconds delay until hud warning is displayed for mismatched hash on client join.");
            WarningMessageText = config.Bind("Menu Warning", "WarningMessageText", "Your modlist does not match the expected modlist hash.\n\n You may experience issues.", "Message to display in Hash Mismatch Menu Warning Message");
            WarningButtonIgnoreText = config.Bind("Menu Warning", "WarningButtonIgnoreText", "Okay", "Button text for ignoring the Hash Mismatch Menu Warning Message");
            WarningButtonResetText = config.Bind("Menu Warning", "WarningButtonResetText", "Reset", "Button text for reseting ExpectedModListHash to the detected hash in Hash Mismatch Menu Warning Message");
        }

    }
}
