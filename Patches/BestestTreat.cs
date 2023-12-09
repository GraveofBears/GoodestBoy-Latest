using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace GoodestBoy.Patches;

public class BestestTreat
{
    internal static AssetBundle GetAssetBundle(string filename)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
        using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
        return AssetBundle.LoadFromStream(stream);
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
    static class Tameable_transform_Patch
    {
        static void Postfix(Tameable __instance, ref string __result)
        {
            if (__instance.m_character.IsTamed())
            {
                var currentWeapon = Player.m_localPlayer.GetCurrentWeapon();
                if (currentWeapon.m_dropPrefab?.name != "BestestTreat") return;
                __result += $"\n[<color=yellow><b>L-Ctrl + E</b></color>] Heal: {GoodestBoy._goodestHealAmount.Value} HP";
            }
        }
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
    static class Tameable_Interact_Patch
    {
        static bool Prefix(Tameable __instance)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                var lastTime = __instance.m_nview.m_zdo.GetInt("LastHealTime");
                if (lastTime + GoodestBoy._goodestHealCooldown.Value > EnvMan.instance.m_totalSeconds)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Cooldown not ready");
                    return false;
                }

                var currentWeapon = Player.m_localPlayer.GetCurrentWeapon();
                if (currentWeapon.m_dropPrefab?.name != "BestestTreat") return true;

                if (__instance.m_character.IsTamed())
                {
                    currentWeapon.m_durability -= GoodestBoy._goodestDurabilityCost.Value;
                    if (currentWeapon.m_durability <= 0)
                    {
                        Player.m_localPlayer.UnequipItem(currentWeapon, false);
                        Player.m_localPlayer.m_inventory.RemoveItem(currentWeapon);
                    }

                    Player.m_localPlayer.m_zanim.SetTrigger("interact");
                    __instance.m_nview.InvokeRPC("GoodestHealTameable");
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Awake))]
    static class Tameable_Awake_Patch
    {
        static void Postfix(Tameable __instance)
        {
            __instance.m_nview.Register("GoodestHealTameable", _ =>
            {
                var lastTime = __instance.m_nview.m_zdo.GetInt("LastHealTime");
                if (lastTime + GoodestBoy._goodestHealCooldown.Value > EnvMan.instance.m_totalSeconds)
                {
                    return;
                }

                if (__instance.m_nview.IsOwner())
                {
                    __instance.m_nview.m_zdo.Set("LastHealTime", (int)EnvMan.instance.m_totalSeconds);
                    __instance.m_character.Heal(GoodestBoy._goodestHealAmount.Value);
                    Object.Instantiate(ZNetScene.instance.GetPrefab("fx_creature_tamed"), __instance.transform.position, Quaternion.identity);
                }
            });
        }
    }
}