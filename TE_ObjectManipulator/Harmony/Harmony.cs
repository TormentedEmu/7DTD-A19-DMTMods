using DMT;
using HarmonyLib;
using System.Reflection;

namespace TormentedEmu_Mods_A19
{
  public class Harmony_TE_ObjectManipulator : IHarmony
  {
    public void Start()
    {
      var harmony = new Harmony("TormentedEmu.Mods.A19");
      harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(GameManager), "SetCursorEnabledOverride")]
    public class GameManager_SetCursorEnabledOverride
    {
      static bool Prefix(GameManager __instance, ref bool ___bCursorVisibleOverrideState)
      {
        if (!___bCursorVisibleOverrideState)
        {
          ___bCursorVisibleOverrideState = true;
        }

        return true;
      }
    }
  }

}
