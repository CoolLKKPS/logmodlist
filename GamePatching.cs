using BepInEx.Bootstrap;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModListHashChecker;

internal class GamePatching
{
    private static readonly ulong[] ALLOWED_STEAM_IDS = { 76561100000000000UL, 76561100000000001UL };

    private static readonly bool DisplayHashOnLevelLoad = false;

    private static readonly bool ChatHashMessageToAll = false;

    private static bool IsSteamIDValid()
    {
        if (!SteamClient.IsValid) return false;
        foreach (ulong id in ALLOWED_STEAM_IDS)
        {
            if (SteamClient.SteamId == id)
                return true;
        }
        return false;
        // return true;
    }

    private static void ExitGame()
    {
        Application.Quit();
    }

    private static void ScheduleExit(int delaySeconds)
    {
        new System.Threading.Timer(_ =>
        {
            ExitGame();
        }, null, delaySeconds * 1000, System.Threading.Timeout.Infinite);
    }

    private static string ObfuscateHash(string hash)
    {
        const string salt = "TeamMLC";
        var combined = hash + salt;
        var bytes = Encoding.UTF8.GetBytes(combined);
        return System.Convert.ToBase64String(bytes);
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SteamMatchmaking_OnLobbyCreated))]
    public class LobbyCreatedPatch
    {
        static void Postfix(Result result, ref Lobby lobby)
        {
            if (result != Result.OK) return;

            string modListString = DictionaryHashGenerator.GetFullModListString(Chainloader.PluginInfos);
            string baseHash = DictionaryHashGenerator.ComputeHash(modListString, "");
            byte[] saltBytes = new byte[16];
            System.Security.Cryptography.RandomNumberGenerator.Fill(saltBytes);
            string salt = System.Convert.ToBase64String(saltBytes);
            byte[] challengeBytes = new byte[8];
            System.Security.Cryptography.RandomNumberGenerator.Fill(challengeBytes);
            string challenge = System.Convert.ToBase64String(challengeBytes);
            string challengeHash = DictionaryHashGenerator.ComputeHash(modListString, challenge);
            lobby.SetData("MLHC_BaseHash", baseHash);
            lobby.SetData("MLHC_Salt", salt);
            lobby.SetData("MLHC_HostSteamID", SteamClient.SteamId.ToString());
            lobby.SetData("MLHC_Challenge", challenge);
            lobby.SetData("MLHC_ChallengeHash", challengeHash);
            lobby.SetData("ModListHash", "");
        }
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingLevel))]
    internal class DisplayHashPerRound
    {
        static void Postfix()
        {
            if (HUDManager.Instance == null || !DisplayHashOnLevelLoad)
                return;

            if (ChatHashMessageToAll)
                HUDManager.Instance.AddTextToChatOnServer($"{StartOfRound.Instance.localPlayerController.playerUsername} ModListHash: {HashGeneration.GeneratedHash}");
            else
                HUDManager.Instance.AddChatMessage($"Local ModListHash: {HashGeneration.GeneratedHash}", "", -1, true);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    public class StartRoundPatch
    {
        public static void Postfix()
        {
            if (!ModListHashChecker.instance.ClientMismatch)
                return;
            ModListHashChecker.instance.StartCoroutine(WarningMessage());
            if (!IsSteamIDValid())
            {
                ScheduleExit(15);
            }
        }

        private static IEnumerator WarningMessage()
        {
            yield return new WaitForSeconds(5);
            HUDManager.Instance.DisplayTip("Modlist Hash Mismatch", $"{ConfigManager.JoinWarningText.Value}", false, false, "clientHashMismatch");
            // HUDManager.Instance.DisplayTip("ALERT!", "Error Code: HHE", false, false, "alerterrorcodeHHE");
        }
    }


    [HarmonyPatch(typeof(GameNetworkManager), (nameof(GameNetworkManager.StartClient)))]
    public class LobbyJoinPatch
    {
        static void Postfix()
        {
            var lobby = GameNetworkManager.Instance.currentLobby;
            if (lobby == null)
            {
                ModListHashChecker.instance.ClientMismatch = true;
                return;
            }
            string challenge = lobby.Value.GetData("MLHC_Challenge");
            string challengeHash = lobby.Value.GetData("MLHC_ChallengeHash");
            if (string.IsNullOrEmpty(challenge) || string.IsNullOrEmpty(challengeHash))
            {
                ModListHashChecker.instance.ClientMismatch = true;
                return;
            }
            string modListString = DictionaryHashGenerator.GetFullModListString(Chainloader.PluginInfos);
            string localChallengeHash = DictionaryHashGenerator.ComputeHash(modListString, challenge);
            if (challengeHash != localChallengeHash)
            {
                ModListHashChecker.instance.ClientMismatch = true;
            }
        }

        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.SetSingleton))]
        internal static class NetworkConfigPatch
        {
            private static void Postfix()
            {
                if (NetworkManager.Singleton == null || NetworkManager.Singleton.PrefabHandler == null)
                    return;
                var prefab = new GameObject(ModListHashChecker.PluginInfo.PLUGIN_NAME + " Prefab");
                prefab.hideFlags |= HideFlags.HideAndDontSave;
                Object.DontDestroyOnLoad(prefab);
                var networkObject = prefab.AddComponent<NetworkObject>();
                try
                {
                    var field = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        uint hash = (uint)ModListHashChecker.PluginInfo.PLUGIN_GUID.GetHashCode();
                        field.SetValue(networkObject, hash);
                    }
                }
                catch
                {
                }
                NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(prefab);
            }
        }
    }

    [HarmonyPatch(typeof(PreInitSceneScript), (nameof(PreInitSceneScript.Awake)))]
    public class HashGeneration : MonoBehaviour
    {
        public static string GeneratedHash { get; internal set; } = "";

        static void Postfix()
        {
            var pluginsLoaded = Chainloader.PluginInfos;
            GeneratedHash = DictionaryHashGenerator.GenerateHash(pluginsLoaded);

            if (!string.IsNullOrEmpty(ConfigManager.ExpectedModListHash.Value))
            {

                if (GeneratedHash != ConfigManager.ExpectedModListHash.Value)
                {
                    ModListHashChecker.instance.HashMismatch = true;
                }
            }
            else
            {
                ModListHashChecker.instance.NoHashFound = true;
            }
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SetConnectionDataBeforeConnecting))]
    public class SetConnectionDataBeforeConnectingPatch
    {
        static void Postfix()
        {
            try
            {
                var config = NetworkManager.Singleton?.NetworkConfig;
                if (config == null)
                    return;
                byte[] currentData = config.ConnectionData;
                string currentStr = currentData != null ? Encoding.ASCII.GetString(currentData) : "";
                if (!string.IsNullOrEmpty(currentStr) && !currentStr.EndsWith(",", System.StringComparison.Ordinal))
                    currentStr += ",";
                var lobby = GameNetworkManager.Instance.currentLobby;
                if (lobby == null)
                {
                    ModListHashChecker.Log.LogError("No lobby when setting connection data");
                    return;
                }
                string salt = lobby.Value.GetData("MLHC_Salt");
                if (string.IsNullOrEmpty(salt))
                {
                    ModListHashChecker.Log.LogError("Missing salt in lobby data");
                    return;
                }
                string modListString = DictionaryHashGenerator.GetFullModListString(Chainloader.PluginInfos);
                string bindingHash = DictionaryHashGenerator.ComputeHash(modListString, salt + SteamClient.SteamId.ToString());
                currentStr += bindingHash;
                config.ConnectionData = Encoding.ASCII.GetBytes(currentStr);
            }
            catch
            {
            }
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager), "ConnectionApproval")]
    public class ConnectionApprovalPatch
    {
        static void Postfix(ref NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (!response.Approved)
                return;

            string payloadStr = Encoding.ASCII.GetString(request.Payload);
            string[] parts = payloadStr.Split(',');
            if (parts.Length < 3)
            {
                response.Approved = false;
                response.Reason = "An error occured!";
                return;
            }

            string clientSteamId = parts[1];
            string clientModHash = parts[2];
            var lobby = GameNetworkManager.Instance.currentLobby;
            if (lobby == null)
            {
                response.Approved = false;
                response.Reason = "An error occured!";
                return;
            }
            string salt = lobby.Value.GetData("MLHC_Salt");
            if (string.IsNullOrEmpty(salt))
            {
                response.Approved = false;
                response.Reason = "An error occured!";
                return;
            }
            string modListString = DictionaryHashGenerator.GetFullModListString(Chainloader.PluginInfos);
            string expectedHash = DictionaryHashGenerator.ComputeHash(modListString, salt + clientSteamId);
            if (clientModHash != expectedHash)
            {
                response.Approved = false;
                response.Reason = "An error occured!";
            }
        }
    }

    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.OnEnable))]
    public class EnablePatch : MonoBehaviour
    {
        public static void Postfix(ref MenuManager __instance)
        {
            if (ModListHashChecker.instance.HashMismatch)
            {
                MenuMessage(__instance, ConfigManager.WarningButtonResetText.Value, ConfigManager.WarningButtonIgnoreText.Value, ConfigManager.WarningMessageText.Value);
                // MenuMessage(__instance, "Confirm", "Back", "ALERT\n\nError Code: HHE");
            }
            else if (ModListHashChecker.instance.NoHashFound)
            {
                MenuMessage(__instance, ConfigManager.NoHashLeftButtonText.Value, ConfigManager.NoHashRightButtonText.Value, ConfigManager.NoHashMessageText.Value);
                // MenuMessage(__instance, "Confirm", "Back", "ALERT\n\nError Code: HHE");
            }
            else
            {
                ModListHashChecker.Log.LogInfo($"Not sending any messages");
            }
        }

        static void ResetConfigHash()
        {
            ModListHashChecker.Log.LogInfo($"Setting expected hash to current hash.");
            ConfigManager.ExpectedModListHash.Value = HashGeneration.GeneratedHash;
            ModListHashChecker.instance.HashMismatch = false;
            ModListHashChecker.instance.NoHashFound = false;
        }

        internal static GameObject NewNotification = null!;
        internal static Button FirstButton = null!;
        internal static Button SecondButton = null!;
        internal static TextMeshProUGUI MenuText = null!;

        private static void MenuSetup(MenuManager menuInstance)
        {
            if (NewNotification != null && FirstButton != null && SecondButton != null && MenuText != null)
                return;

            NewNotification = Instantiate(menuInstance.menuNotification, menuInstance.menuNotification.transform.parent);

            FirstButton = NewNotification.GetComponentInChildren<Button>();
            SecondButton = Instantiate(FirstButton, NewNotification.transform);

            MenuText = NewNotification.transform.Find("Panel").Find("NotificationText").gameObject.GetComponent<TextMeshProUGUI>();
        }

        static void MenuMessage(MenuManager menuInstance, string leftButton, string rightButton, string messageText)
        {
            if (menuInstance == null)
                return;

            if (menuInstance.menuNotification == null)
                return;

            MenuSetup(menuInstance);

            if (NewNotification == null || SecondButton == null)
                return;

            TextMeshProUGUI buttonText = SecondButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = $"[ {leftButton} ]";

            if (!IsSteamIDValid())
            {
                SecondButton.onClick.RemoveAllListeners();
                SecondButton.onClick.AddListener(ExitGame);
                FirstButton.onClick.RemoveAllListeners();
                FirstButton.onClick.AddListener(ExitGame);
                ScheduleExit(10);
            }
            else
            {
                SecondButton.onClick.AddListener(ResetConfigHash);
            }

            if (menuInstance.isInitScene)
                return;

            MenuText.text = messageText;

            FirstButton.GetComponentInChildren<TextMeshProUGUI>().text = $"[ {rightButton} ]";

            NewNotification.SetActive(value: true);
            Vector3 movePosRight = new(62, -45, 0); //got these values via trial/error
            Vector3 movePosLeft = new(-78, -45, 0); //^
            for (int i = 0; i < NewNotification.GetComponentsInChildren<Button>().Length; i++)
            {
                if (i == 0)
                {
                    EventSystem.current.SetSelectedGameObject(NewNotification.GetComponentsInChildren<Button>()[i].gameObject);
                    NewNotification.GetComponentsInChildren<Button>()[i].gameObject.transform.localPosition = movePosRight;
                }
                else
                    NewNotification.GetComponentsInChildren<Button>()[i].gameObject.transform.localPosition = movePosLeft;
            }
        }
    }
}
