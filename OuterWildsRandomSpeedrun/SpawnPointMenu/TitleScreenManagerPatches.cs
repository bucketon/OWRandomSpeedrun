using HarmonyLib;
using OuterWildsRandomSpeedrun;
using UnityEngine;

[HarmonyPatch]
public class TitleScreenManagerPatches {
  [HarmonyPostfix]
  [HarmonyPatch(typeof(TitleScreenManager), nameof(TitleScreenManager.OnMenuPush))]
  public static void TitleScreenManager_OnMenuPush_Postfix(TitleScreenManager __instance, Menu pushedMenu) {
    if (pushedMenu is SpawnPointMenu)
    {
      EnableTitleScreen(__instance, false);
    }
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(TitleScreenManager), nameof(TitleScreenManager.OnMenuPop))]
  public static void TitleScreenManager_OnMenuPop_Postfix(TitleScreenManager __instance, Menu poppedMenu)
  {
    if (poppedMenu is SpawnPointMenu)
    {
      EnableTitleScreen(__instance, true);
    }
  }

  private static void EnableTitleScreen(TitleScreenManager manager, bool shouldEnable)
  {
    manager._copyrightTextDisplay.enabled = shouldEnable;
    manager._gameVersionTextDisplay.enabled = shouldEnable;
    manager._gamertagDisplay.enabled = shouldEnable;
    manager.EnableMainMenuTextFields(shouldEnable);
    // We're setting this on TitleMenu > TitleCanvas 
    manager._mainMenu.gameObject.transform.parent.parent.GetComponentInParent<CanvasGroup>().interactable = shouldEnable;

    if (shouldEnable)
    {
      manager.SelectDefaultMainMenuSelection();
    }
  }
}