using HarmonyLib;
using UnityEngine;

namespace GoodestBoy.Patches;

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
public class HumanoidPatch
{
    public static bool Prefix(Humanoid __instance, Inventory inventory, ItemDrop.ItemData item, bool fromInventoryGui)
    {
        inventory ??= __instance.m_inventory;
        if (!inventory.ContainsItem(item)) return true;
        var hoverObject = __instance.GetHoverObject();
        var hoverable = hoverObject ? hoverObject.GetComponentInParent<Hoverable>() : null;
        if (hoverable != null && !fromInventoryGui)
        {
            var monsterAI = hoverObject.GetComponent<MonsterAI>();
            if (monsterAI != null && GoodestBoy._creatureList.Contains(monsterAI.gameObject.name.Replace("(Clone)", "")) &&
                monsterAI.CanConsume(item))
            {
                var tameable = hoverObject.GetComponent<Tameable>();
                var name = tameable?.GetText() == "" ? tameable.m_character.m_name : tameable.GetText();
                if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
                {
                    if (tameable != null && tameable.IsHungry())
                    {
                        __instance.DoInteractAnimation(hoverObject.transform.position);
                        monsterAI.m_onConsumedItem(item.m_dropPrefab.GetComponent<ItemDrop>());
                        var humanoid = hoverObject.GetComponent<Humanoid>();
                        humanoid.m_consumeItemEffects.Create(humanoid.transform.position, Quaternion.identity);
                        var anim = hoverObject.GetComponentInChildren<Animator>();
                        anim.SetTrigger("consume");
                        inventory.RemoveOneItem(item);
                        __instance.Message(MessageHud.MessageType.Center,
                            $"{name} is very happy.");
                        return false;
                    }

                    if (!tameable.IsHungry())
                    {
                        __instance.Message(MessageHud.MessageType.Center,
                            $"{name} is not yet hungry.");
                        return false;
                    }

                    return false;
                }
            }
        }

        return true;
    }
}