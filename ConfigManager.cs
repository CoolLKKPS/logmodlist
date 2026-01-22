using BepInEx.Configuration;

namespace ModListHashChecker
{
    public class ConfigManager
    {
        public static ConfigEntry<string> ExpectedModListHash { get; private set; } = null!;
        public static ConfigEntry<bool> NoExpectedHashMessage { get; private set; } = null!;
        public static ConfigEntry<bool> MenuWarning { get; private set; } = null!;
        public static ConfigEntry<bool> JoinWarning { get; private set; } = null!;
        public static ConfigEntry<string> WarningMessageText { get; private set; } = null!;
        public static ConfigEntry<string> JoinWarningText { get; private set; } = null!;
        public static ConfigEntry<int> JoinWarningDelay { get; private set; } = null!;
        public static ConfigEntry<string> WarningButtonIgnoreText { get; private set; } = null!;
        public static ConfigEntry<string> WarningButtonResetText { get; private set; } = null!;
        public static ConfigEntry<string> NoHashMessageText { get; private set; } = null!;
        public static ConfigEntry<string> NoHashRightButtonText { get; private set; } = null!;
        public static ConfigEntry<string> NoHashLeftButtonText { get; private set; } = null!;
        public static ConfigEntry<bool> DisplayHashOnLevelLoad { get; private set; } = null!;
        public static ConfigEntry<bool> ChatHashMessageToAll { get; private set; } = null!;

        internal static void Init(ConfigFile config)
        {
            ExpectedModListHash = config.Bind("General", "ExpectedModListHash", "", "The expected modlist hash for this modpack. Do not change this unless you know what you're doing.");
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

            DisplayHashOnLevelLoad = config.Bind("General", "DisplayHashOnLevelLoad", true, "When enabled, will display the modlist hash in the chat on level load.");
            ChatHashMessageToAll = config.Bind("General", "ChatHashMessageToAll", false, "If DisplayHashOnLevelLoad is enabled, this will determine if your chat message is sent to all players (true) or just displayed for the local client (false)");
        }

    }
}