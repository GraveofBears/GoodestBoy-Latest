using HarmonyLib;
using UnityEngine;

namespace GoodestBoy.Patches
{
    public class GoodestBoyWait : MonoBehaviour
    {
        private Tameable m_tameable;
        private ZNetView m_nview;
        private Animator m_animator;

        private static readonly int s_isWaitingHash = "gb_waiting".GetStableHashCode();

        private void Awake()
        {
            m_tameable = GetComponent<Tameable>();
            m_nview = GetComponent<ZNetView>();
            m_animator = GetComponentInChildren<Animator>();

            if (m_nview != null)
            {
                m_nview.Register("RPC_ToggleWait", RPC_ToggleWait);
                m_nview.Register<bool>("RPC_SetWait", RPC_SetWait);
            }
        }

        private void Update()
        {
            if (!m_tameable || !m_tameable.IsTamed()) return;
            if (!Player.m_localPlayer) return;

            // Hotkey pressed?
            if (!GoodestBoy.WaitKey.Value.IsDown()) return;

            // Must be looking at THIS creature
            if (Player.m_localPlayer.GetHoverObject() != gameObject) return;

            ToggleWait();

            Player.m_localPlayer.Message(
                MessageHud.MessageType.Center,
                IsWaiting() ? "I'll wait here." : "Let's go."
            );
        }

        public bool IsWaiting()
        {
            if (m_nview?.GetZDO() == null)
                return false;

            return m_nview.GetZDO().GetBool(s_isWaitingHash);
        }

        public void ToggleWait()
        {
            if (m_nview == null) return;

            m_nview.InvokeRPC("RPC_ToggleWait");
        }

        private void RPC_ToggleWait(long sender)
        {
            // Remove the IsOwner check here - let the sender toggle
            bool newState = !IsWaiting();

            // This RPC should be called on all clients
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetWait", newState);
        }

        private void RPC_SetWait(long sender, bool state)
        {
            // Only owner should modify ZDO
            if (!m_nview.IsOwner()) return;

            m_nview.GetZDO().Set(s_isWaitingHash, state);

            // But everyone should update the animation
            SetWaitAnimation(state);
        }

        private void SetWaitAnimation(bool waiting)
        {
            if (m_animator != null)
            {
                m_animator.SetBool("wait", waiting);
            }
        }
    }

    // ============================
    // AI BLOCK PATCH
    // ============================

    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
    class MonsterAI_UpdateAI_GoodestBoyWaitPatch
    {
        static bool Prefix(MonsterAI __instance, ref bool __result)
        {
            if (__instance == null) return true;

            GoodestBoyWait wait = __instance.GetComponent<GoodestBoyWait>();
            if (wait == null) return true;

            if (!wait.IsWaiting()) return true;

            __instance.StopMoving();

            __result = true;
            return false;
        }
    }

    // ============================
    // INTERACT PATCH (DISABLED)
    // ============================

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
    class Tameable_Interact_GoodestBoyWaitPatch
    {
        static bool Prefix(Tameable __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            // No Shift+E behavior anymore — allow normal interaction
            return true;
        }
    }

    // ============================
    // HOVER TEXT PATCH
    // ============================

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
    class Tameable_HoverText_GoodestBoyWaitPatch
    {
        static void Postfix(Tameable __instance, ref string __result)
        {
            GoodestBoyWait wait = __instance.GetComponent<GoodestBoyWait>();
            if (wait == null) return;
            if (!__instance.IsTamed()) return;

            string key = GoodestBoy.WaitKey.Value.MainKey.ToString();

            __result += wait.IsWaiting()
                ? $"\n[{key}] Follow Me"
                : $"\n[{key}] Wait Here";
        }
    }

    // ============================
    // RESUME ON DAMAGE
    // ============================

    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    class Character_Damage_GoodestBoyWaitPatch
    {
        static void Postfix(Character __instance)
        {
            GoodestBoyWait wait = __instance.GetComponent<GoodestBoyWait>();
            if (wait == null) return;

            if (wait.IsWaiting())
            {
                wait.ToggleWait();
            }
        }
    }
}
