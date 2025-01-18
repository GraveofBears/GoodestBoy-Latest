using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CreatureManager;
using HarmonyLib;
using ItemManager;
using ServerSync;
using LocalizationManager;
using UnityEngine;
using GoodestBoy.Patches;



namespace GoodestBoy
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]


    public class GoodestBoy : BaseUnityPlugin
    {
        private const string ModName = "GoodestBoy";
        private const string ModVersion = "0.2.9";
        private const string ModGUID = "odinplus.plugins.goodestboy";



        private static readonly ConfigSync configSync = new(ModName) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private static ConfigEntry<Toggle> serverConfigLocked = null!;
        
        public static ConfigEntry<int> _goodestHealAmount = null!;
        public static ConfigEntry<int> _goodestHealCooldown = null!;
        public static ConfigEntry<int> _goodestDurabilityCost = null!;
        internal static ConfigEntry<int> dogHealth = null!;
        internal static ConfigEntry<int> dogFedDuration = null!;
        internal static ConfigEntry<int> dogTimeToTame = null!;
        internal static ConfigEntry<bool> dogStartsTamed = null!;
        internal static ConfigEntry<int> dogProcreationTime = null!;
        internal static ConfigEntry<Character.Faction> dogCharacterFaction = null!;
        internal static ConfigEntry<int> pupperHealth = null!;
        internal static ConfigEntry<int> pupperOffspringGrowupTime = null!;


        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }


        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

        private enum Toggle
        {
            On = 1,
            Off = 0
        }



        public void Awake()
        {
            Localizer.Load();

            dogHealth = config("Goodest Boy", "Health", 250, new ConfigDescription("Declare health points for BestestDog"));
            dogFedDuration = config("Goodest Boy", "Fed Duration", 600, new ConfigDescription("Declare how long BestestDog is fed"));
            dogTimeToTame = config("Goodest Boy", "Time Till Tame", 600, new ConfigDescription("Declare how long it takes to tame BestestDog"));
            dogStartsTamed = config("Goodest Boy", "Starts Tamed", false, new ConfigDescription("Declare if dog starts tamed"));
            dogProcreationTime = config("Goodest Boy", "Procreation Time", 600, new ConfigDescription("Declare how long dogs procreate"));
            dogCharacterFaction = config("Goodest Boy", "Character Faction", Character.Faction.Players, new ConfigDescription("Declare DestestDogs Faction"));

            pupperHealth = config("Bestest Pup", "Health", 50, new ConfigDescription("Declare health points for Pupper"));
            pupperOffspringGrowupTime = config("Bestest Pup", "Grow-Up Time", 2000, new ConfigDescription("Declare growup time needed to convert offspring into GoodestBoy.Time in seconds."));

            GameObject GSD_Dog_Attack1 = ItemManager.PrefabManager.RegisterPrefab("gsd", "GSD_Dog_Attack1");
            GameObject GSD_Dog_Attack2 = ItemManager.PrefabManager.RegisterPrefab("gsd", "GSD_Dog_Attack2");
            GameObject GSD_Dog_Attack3 = ItemManager.PrefabManager.RegisterPrefab("gsd", "GSD_Dog_Attack3");
            GameObject fx_dogg_backstab = ItemManager.PrefabManager.RegisterPrefab("gsd", "fx_dogg_backstab"); //register projectile
            GameObject fx_doggo_crit = ItemManager.PrefabManager.RegisterPrefab("gsd", "fx_doggo_crit"); //register projectile
            GameObject fx_doggo_footstep_jog = ItemManager.PrefabManager.RegisterPrefab("gsd", "fx_doggo_footstep_jog");
            GameObject fx_doggo_footstep_snow_run = ItemManager.PrefabManager.RegisterPrefab("gsd", "fx_doggo_footstep_snow_run");
            GameObject fx_doggo_footstep_water = ItemManager.PrefabManager.RegisterPrefab("gsd", "fx_doggo_footstep_water");
            GameObject fx_doggo_pet = ItemManager.PrefabManager.RegisterPrefab("gsd", "fx_doggo_pet");
            GameObject fx_doggo_tamed = ItemManager.PrefabManager.RegisterPrefab("gsd", "fx_doggo_tamed");            
            GameObject sfx_bestestball_hit = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_bestestball_hit");
            GameObject sfx_bestestball_throw = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_bestestball_throw");
            GameObject sfx_doggo_attack = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_attack");
            GameObject sfx_doggo_attack_hit = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_attack_hit");
            GameObject sfx_doggo_bark = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_bark");
            GameObject sfx_doggo_consume = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_consume");
            GameObject sfx_doggo_death = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_death");
            GameObject sfx_doggo_hit = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_hit");
            GameObject sfx_doggo_panting = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_panting");
            GameObject sfx_doggo_swim = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_doggo_swim");
            GameObject sfx_gb_whistle = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_gb_whistle");
            GameObject vfx_doggo_birth = ItemManager.PrefabManager.RegisterPrefab("gsd", "vfx_doggo_birth");
            GameObject vfx_doggo_death = ItemManager.PrefabManager.RegisterPrefab("gsd", "vfx_doggo_death");
            GameObject vfx_doggo_destruction = ItemManager.PrefabManager.RegisterPrefab("gsd", "vfx_doggo_destruction");
            GameObject vfx_doggo_hit = ItemManager.PrefabManager.RegisterPrefab("gsd", "vfx_doggo_hit");
            GameObject vfx_doggo_love = ItemManager.PrefabManager.RegisterPrefab("gsd", "vfx_doggo_love");
            GameObject vfx_doggo_soothed = ItemManager.PrefabManager.RegisterPrefab("gsd", "vfx_doggo_soothed");
            GameObject vfx_dogwater_surface = ItemManager.PrefabManager.RegisterPrefab("gsd", "vfx_dogwater_surface");
            GameObject GSD_Ragdoll = ItemManager.PrefabManager.RegisterPrefab("gsd", "GSD_Ragdoll");

            Item BestestTreat = new("gsd", "BestestTreat");
          //  BestestTreat.Name.English("Bestest Treat");
          //  BestestTreat.Description.English("A treat for your bested dog");
            BestestTreat.Crafting.Add(CraftingTable.Workbench, 1);
            BestestTreat.RequiredItems.Add("CookedMeat", 2);
            BestestTreat.CraftAmount = 1;

            Item StoneGSDPup = new("gsd", "StoneGSDPup");
           // StoneGSDPup.Name.English("Stone Pupper");
           // StoneGSDPup.Description.English("Throw me to spawn a pupper");
            StoneGSDPup.Crafting.Add(CraftingTable.Workbench, 2);
            StoneGSDPup.RequiredItems.Add("Stone", 5);
            StoneGSDPup.RequiredItems.Add("BoneFragments", 5);
            StoneGSDPup.CraftAmount = 1;
            GameObject StoneGSD_Projectile = ItemManager.PrefabManager.RegisterPrefab("gsd", "StoneGSD_Projectile");

            Item BestestBall = new("gsd", "BestestBall");
          //  BestestBall.Name.English("Bestest Ball");
         //   BestestBall.Description.English("A dog toy, sometimes he will fetch, sometimes he wont, and sometimes he wont give it back.");
            BestestBall.Crafting.Add(CraftingTable.Workbench, 1);
            BestestBall.RequiredItems.Add("LeatherScraps", 2);
            BestestBall.CraftAmount = 1;
            GameObject BestestBall_Projectile = ItemManager.PrefabManager.RegisterPrefab("gsd", "BestestBall_Projectile"); //register projectile

            Item GB_Whistle = new("gsd", "GB_Whistle");
         //   GB_Whistle.Name.English("Goodest Whistle");
         //   GB_Whistle.Description.English("A whistle that will call your friends");
            GB_Whistle.Crafting.Add(CraftingTable.Workbench, 1);
            GB_Whistle.RequiredItems.Add("FineWood", 1);
            GB_Whistle.CraftAmount = 1;
            var shared = GB_Whistle.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared;
            var statusEffect = ScriptableObject.CreateInstance<Recall>(); //sets the status effect script into this variable. 
            statusEffect.m_ttl = .25f; // duration of the status effect.
            shared.m_consumeStatusEffect = statusEffect; // the item = the status effect.

            Creature Bestest_Pup = new("gsd", "Bestest_Pup")            //add creature
            {
                Biome = Heightmap.Biome.None,
                CanSpawn = true,
                SpawnChance = 100,
                GroupSize = new Range(1, 2),
                Maximum = 1,

            };
            Bestest_Pup.Drops["BoneFragments"].Amount = new Range(1, 2);
            Bestest_Pup.Drops["BoneFragments"].DropChance = 50f;
            Bestest_Pup.Prefab.GetComponent<Humanoid>().m_health = pupperHealth.Value;
            Bestest_Pup.Prefab.GetComponent<Growup>().m_growTime = pupperOffspringGrowupTime.Value;



            Creature BestestDog = new("gsd", "BestestDog")            //add creature
            {
                Biome = Heightmap.Biome.BlackForest,
                CanBeTamed = true,
                FoodItems = "RawMeat, BestestTreat, CookedMeat, BoarJerky, NeckTail, DeerStew, NeckTailGrilled, DeerMeat, CookedDeerMeat, MinceMeatSauce, Sausages, Entrails, SerpentMeatCooked, SerpentStew, SerpentMeat, CookedWolfMeat, WolfMeat, Wolfjerky, LoxMeat, CookedLoxMeat, LoxPie,",
                SpawnChance = 10,
                RequiredWeather = Weather.BlackForestFog,
                GroupSize = new Range(1, 2),
                CheckSpawnInterval = 300,
                SpecificSpawnTime = SpawnTime.Always,                
                Maximum = 1
                
            };
            BestestDog.Drops["BoneFragments"].Amount = new Range(1, 2);
            BestestDog.Drops["BoneFragments"].DropChance = 15f;
            BestestDog.Drops["WolfMeat"].Amount = new Range(1, 2);
            BestestDog.Drops["WolfMeat"].DropChance = 50f;
            BestestDog.Drops["StoneGSDPup"].Amount = new Range(1, 1);
            BestestDog.Drops["StoneGSDPup"].DropChance = 1f;
            BestestDog.Prefab.GetComponent<Humanoid>().m_health = dogHealth.Value;
            BestestDog.Prefab.GetComponent<Tameable>().m_fedDuration = dogFedDuration.Value;
            BestestDog.Prefab.GetComponent<Tameable>().m_tamingTime = dogFedDuration.Value;
            BestestDog.Prefab.GetComponent<Tameable>().m_startsTamed = dogStartsTamed.Value;
            BestestDog.Prefab.GetComponent<Procreation>().m_pregnancyDuration = dogProcreationTime.Value;
            BestestDog.Prefab.GetComponent<Humanoid>().m_faction = dogCharacterFaction.Value;
            _goodestHealAmount = Config.Bind("Treat Healing", "Amount", 100, "How much health the treat heals.");
            _goodestHealCooldown = Config.Bind("Treat Cooldown", "Cooldown", 60, "How long the cooldown is in seconds.");
            _goodestDurabilityCost = Config.Bind("Treat Durability", "Durability", 1, "How much durability the treat loses when used.");
            
            Assembly assembly = Assembly.GetExecutingAssembly();

            Harmony harmony = new(ModGUID);
            harmony.PatchAll(assembly);


        }
    }
}