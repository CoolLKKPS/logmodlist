using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace ModListHashChecker
{
    public class MenuMessagesPatch
    {
        [HarmonyPatch(typeof(MenuManager), "OnEnable")]
        public class EnablePatch : MonoBehaviour
        {
            public static void Postfix(ref MenuManager __instance)
            {
                if (ModListHashChecker.instance.hashMismatch && ConfigManager.MenuWarning.Value)
                {
                    MenuMessage(__instance, ConfigManager.WarningButtonResetText.Value, ConfigManager.WarningButtonIgnoreText.Value, ConfigManager.WarningMessageText.Value);
                }
                else if (ModListHashChecker.instance.noHashFound && ConfigManager.NoExpectedHashMessage.Value)
                {
                    MenuMessage(__instance, ConfigManager.NoHashLeftButtonText.Value, ConfigManager.NoHashRightButtonText.Value, ConfigManager.NoHashMessageText.Value);
                }
                else
                    ModListHashChecker.Log.LogInfo($"Not sending any messages");
            }

            static void ResetConfigHash()
            {
                ModListHashChecker.Log.LogInfo($"Setting expected hash to current hash.");
                ConfigManager.ExpectedModListHash.Value = HashGeneration.generatedHash;
                ModListHashChecker.instance.hashMismatch = false;
                ModListHashChecker.instance.noHashFound = false;
            }

            static void MenuMessage(MenuManager menuInstance, string leftButton, string rightButton, string messageText)
            {
                if (menuInstance != null && menuInstance.menuNotification != null)
                {
                    // cloning the one button from notification
                    Button setNewHash = Instantiate(menuInstance.menuNotification.GetComponentInChildren<Button>());

                    // set parent of cloned button to menuNotif
                    setNewHash.transform.SetParent(menuInstance.menuNotification.transform, false);
                    TextMeshProUGUI clonedText = setNewHash.GetComponentInChildren<TextMeshProUGUI>();
                    clonedText.text = $"[ {leftButton} ]";

                    setNewHash.onClick.AddListener(ResetConfigHash);

                    if (!menuInstance.isInitScene)
                    {
                        Debug.Log("Displaying menu notification: " + messageText);
                        menuInstance.menuNotificationText.text = messageText;
                        menuInstance.menuNotificationButtonText.text = $"[ {rightButton} ]";

                        menuInstance.menuNotification.SetActive(value: true);
                        Vector3 movePosRight = new Vector3(62, -45, 0); //got these values via trial/error
                        Vector3 movePosLeft = new Vector3(-78, -45, 0); //^
                        for (int i = 0; i < menuInstance.menuNotification.GetComponentsInChildren<Button>().Length; i++)
                        {
                            if (i == 0)
                            {
                                EventSystem.current.SetSelectedGameObject(menuInstance.menuNotification.GetComponentsInChildren<Button>()[i].gameObject);
                                menuInstance.menuNotification.GetComponentsInChildren<Button>()[i].gameObject.transform.localPosition = movePosRight;
                            }
                            else
                                menuInstance.menuNotification.GetComponentsInChildren<Button>()[i].gameObject.transform.localPosition = movePosLeft;

                        }
                    }
                    //__instance.DisplayMenuNotification(ConfigManager.WarningMessageText.Value, ConfigManager.WarningButtonText.Value);
                    ModListHashChecker.Log.LogInfo($"menu message sent");

                    return;
                }
                else
                    ModListHashChecker.Log.LogInfo($"handling null menuNotification");
            }
        }
    }
}
