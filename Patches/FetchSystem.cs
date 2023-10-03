using System.Linq;
using HarmonyLib;
using GoodestBoy;
using UnityEngine;

namespace GoodestBoy.Patches;

[HarmonyPatch]
public class FetchSystem
{
    private static readonly Collider[] _sphereResults = new Collider[32]; // this is important dont forget this one and dont change it.
    private static ItemDrop _lastDroppedItem; // also this one it saves what item the player throws into the ground.

    // this patch saves the item drop by the player on the ground into the _lastDroppedItem variable.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.OnPlayerDrop))]
    public static void OnPlayerDrop_Postfix(ItemDrop __instance)
    {
        _lastDroppedItem = __instance;
    }

    // this patch is where you can setup what item to be fetch.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.DropItem))]
    public static void DropItem_Postfix(Inventory inventory, ItemDrop.ItemData item, int amount, Humanoid __instance, bool __result)
    {
        //item.m_shared.m_name != "$fr_bone" - means if not your item then ignores the patch.
        //change the "$fr_bone" with your items m_name
        //you can add more items by adding this line: || item.m_shared.m_name != "$youritemname"
        if (!__result || !__instance.IsPlayer() || !_lastDroppedItem || item.m_shared.m_name != "BestestBall") return;
        foreach (var pet in from pet in FetchAI._fetchAiList where !((pet.gameObject.transform.position - Player.m_localPlayer.transform.position).sqrMagnitude > 80f) let component = pet.GetComponent<Character>() where (bool)component && component.IsTamed() && !pet.GetComponent<Growup>() && !pet.m_monsterAI.IsAlerted() && pet.m_AiState == FetchAI.AIStates.BaseAI select pet)
        {
            pet.GetBall(_lastDroppedItem);
        }
    }

    //this is where fetch ai is added to creatures when they became tame.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.MakeTame))]
    public static void MakeTame_Postfix(MonsterAI __instance)
    {
        //__instance.gameObject.name.Replace("(Clone)", "") != "Wolf" -means if the creature is not a wolf then ignore the patch
        //change the "Wolf" with the name of your creature.
        //you can also this change that into this: or like this: !__instance.gameObject.name.ToLower().Contains("yourcreature")
        //you can add more creature by adding this: || __instance.gameObject.name.Replace("(Clone)", "") != "yourcreature" 
        if (!GoodestBoy._creatureList.Contains(__instance.gameObject.name.Replace("(Clone)", ""))) return;
        var component = __instance.GetComponent<Character>();
        if ((object)component != null && component.IsTamed())
        {
            NewTame(__instance.gameObject);
        }
    }

    //this patch also adds the fetch ai to creatures when they became tame.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Procreation), nameof(Procreation.Awake))]
    public static void Awake_Postfix(Procreation __instance)
    {
        //__instance.gameObject.name.Replace("(Clone)", "") != "Wolf" -means if the creature is not a wolf then ignore the patch
        //change the "Wolf" with the name of your creature.
        //you can also this change that into this: or like this: !__instance.gameObject.name.ToLower().Contains("yourcreature")
        //you can add more creature by adding this: || __instance.gameObject.name.Replace("(Clone)", "") != "yourcreature" 
        if (!GoodestBoy._creatureList.Contains(__instance.gameObject.name.Replace("(Clone)", ""))) return;
        var component = __instance.GetComponent<Character>();
        if ((object)component != null && component.IsTamed())
        {
            NewTame(__instance.gameObject);
        }
    }

    //this patch adds the fetch ai to already tamed creatures.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
    public static void Awake_Prefix(Character __instance)
    {
        //__instance.name.Replace("(Clone)", "") != "Wolf" -means if the creature is not a wolf then ignore the patch
        //change the "Wolf" with the name of your creature.
        //you can also this change that into this: or like this: !__instance.name.ToLower().Contains("yourcreature")
        //you can add more creature by adding this: || __instance.name.Replace("(Clone)", "") != "yourcreature" 
        if (__instance == null || __instance.IsPlayer() || __instance.IsBoss() || !__instance.m_nview.IsValid() || !__instance.IsTamed() || (bool)__instance.GetComponent<Growup>() || !GoodestBoy._creatureList.Contains(__instance.name.Replace("(Clone)", ""))) return;
        NewTame(__instance.gameObject);
    }

    //this method is important dont forget it.
    public static Rigidbody FindBall(Vector3 pos, float range)
    {
        var num = Physics.OverlapSphereNonAlloc(pos, range, _sphereResults, LayerMask.GetMask("item"));
        Rigidbody result = null;
        var num2 = 999999f;
        for (var i = 0; i < num; i++)
        {
            var collider = _sphereResults[i];
            if (!collider.attachedRigidbody) continue;
            var component = collider.attachedRigidbody.GetComponent<ItemDrop>();
            //component.m_itemData.m_shared.m_name != "$fr_bone" -change "$fr_bone" into your fetch item m_name
            //you can add more by adding this: || component.m_itemData.m_shared.m_name != "$yourfetchitem"
            if (component == null || !component.GetComponent<ZNetView>().IsValid() || component.m_itemData.m_shared.m_name != "BestestBall") continue;
            var component2 = component.GetComponent<Rigidbody>();
            if (!(bool)component2 || !component2.useGravity) continue;
            var sqrMagnitude = (component.transform.position - pos).sqrMagnitude;
            if (!(sqrMagnitude < num2)) continue;
            result = component2;
            num2 = sqrMagnitude;
        }

        return result;
    }

    //dont also forget this method
    //this method checks and adds the fetch ai script
    private static void NewTame(GameObject pet)
    {
        if (pet.GetComponent<FetchAI>()) return;
        pet.GetComponent<MonsterAI>().enabled = false;
        pet.AddComponent<FetchAI>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.DropItem))]
    public static void DropItem_Postfix(ItemDrop __result)
    {
        _lastDroppedItem = __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.SpawnOnHit))]
    public static void SpawnOnHit_Postfix(Projectile __instance, ref ItemDrop.ItemData ___m_spawnItem)
    {
        if (__instance.m_spawnItem == null || !__instance.m_respawnItemOnHit || !_lastDroppedItem || ___m_spawnItem.m_shared.m_name != "BestestBall" || ___m_spawnItem.m_shared.m_name != "BestestStick") return;
        foreach (var pet in from pet in FetchAI._fetchAiList where !((pet.gameObject.transform.position - Player.m_localPlayer.transform.position).sqrMagnitude > 80f) let component = pet.GetComponent<Character>() where (bool)component && component.IsTamed() && !pet.GetComponent<Growup>() && !pet.m_monsterAI.IsAlerted() && pet.m_AiState == FetchAI.AIStates.BaseAI select pet)
        {
            pet.GetBall(_lastDroppedItem);
        }
    }
}