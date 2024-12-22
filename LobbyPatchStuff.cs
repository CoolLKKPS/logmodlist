using HarmonyLib;
using System.Threading;
using BepInEx.Bootstrap;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;


namespace ModListHashChecker
{

    [HarmonyPatch(typeof(GameNetworkManager))]
    public class LobbyCreatedPatch
    {

        [HarmonyPatch("SteamMatchmaking_OnLobbyCreated")]
        [HarmonyPostfix]
        static void Postfix(Result result, ref Lobby lobby)
        {
            if (result != Result.OK) return;

            lobby.SetData("ModListHash", HashGeneration.DictionaryHashGenerator.GenerateHash(Chainloader.PluginInfos));
            ModListHashChecker.Log.LogInfo($"Setting lobby ModHashList to {HashGeneration.generatedHash}");
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    public class StartRoundPatch
    {
        static int delayTask = ConfigManager.JoinWarningDelay.Value * 1000;
        public static void Postfix()
        {
            if (ModListHashChecker.instance.clientMismatch)
            {
                ModListHashChecker.Log.LogInfo($"hash mismatch detected");
                Task.Run(() =>
                {
                    Thread.Sleep(delayTask);
                    ModListHashChecker.Log.LogInfo($"display warning");
                    HUDManager.Instance.DisplayTip("Modlist Hash Mismatch", $"{ConfigManager.JoinWarningText.Value}", false, false, "clientHashMismatch");
                });
            }
            else
                ModListHashChecker.Log.LogInfo($"no hash mismatch");

        }
    }


    [HarmonyPatch(typeof(GameNetworkManager))]
    public class LobbyJoinPatch
    {

        [HarmonyPatch("StartClient")]
        [HarmonyPostfix]
        static void Postfix(SteamId id)
        {
            ModListHashChecker.Log.LogInfo("Comparing your modlist with the host's modlist.");

            var lobbyModList = GameNetworkManager.Instance.currentLobby?.GetData("ModListHash");
            if (lobbyModList == null)
            {
                ModListHashChecker.Log.LogWarning("Host does not have a modlist hash.");
                return;
            }
            else
            {
                ModListHashChecker.Log.LogInfo($"Host's modlist hash: {lobbyModList}");
                ModListHashChecker.Log.LogInfo($"Your modlist hash: {HashGeneration.generatedHash}");

                if (lobbyModList == HashGeneration.generatedHash)
                {
                    ModListHashChecker.Log.LogInfo("Your modlist matches the host's modlist.");
                }
                else
                {
                    ModListHashChecker.Log.LogWarning("Your modlist does not match the host's modlist.");
                    ModListHashChecker.Log.LogWarning("You may experience issues.");
                    ModListHashChecker.instance.clientMismatch = true;
                }
            }

        }
    }

}
