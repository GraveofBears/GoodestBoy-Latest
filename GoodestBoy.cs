﻿using System;
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
using UnityEngine;
using GoodestBoy.Patches;



namespace GoodestBoy
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]


    public class GoodestBoy : BaseUnityPlugin
    {
        private const string ModName = "GoodestBoy";
        private const string ModVersion = "0.1.11";
        private const string ModGUID = "odinplus.plugins.goodestboy";



        private static readonly ConfigSync configSync = new(ModName) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private static ConfigEntry<Toggle> serverConfigLocked = null!;


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

            GameObject sfx_dog_bark = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_dog_bark"); //register projectile
            GameObject sfx_dog_panting = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_dog_panting"); //register projectile
            GameObject sfx_gb_whistle = ItemManager.PrefabManager.RegisterPrefab("gsd", "sfx_gb_whistle");

            Item BestestTreat = new("gsd", "BestestTreat");
            BestestTreat.Name.English("Bestest Treat");
            BestestTreat.Description.English("A treat for your best friend, doesn't it look delicious.");
            BestestTreat.Crafting.Add(CraftingTable.Workbench, 1);
            BestestTreat.RequiredItems.Add("CookedMeat", 2);
            BestestTreat.CraftAmount = 5;
            GameObject BestestTreat_Projectile = ItemManager.PrefabManager.RegisterPrefab("gsd", "BestestTreat_Projectile"); //register projectile

            Item BestestBall = new("gsd", "BestestBall");
            BestestBall.Name.English("Bestest Ball");
            BestestBall.Description.English("The only ball worthy of your bestest friend.");
            BestestBall.Crafting.Add(CraftingTable.Workbench, 1);
            BestestBall.RequiredItems.Add("LeatherScraps", 2);
            BestestBall.CraftAmount = 1;
            GameObject BestestBall_Projectile = ItemManager.PrefabManager.RegisterPrefab("gsd", "BestestBall_Projectile"); //register projectile

            Item GoodestWhistle = new("gsd", "GoodestWhistle");
            GoodestWhistle.Name.English("GoodestWhistle");
            GoodestWhistle.Description.English("A whistle to call your best friend.");
            GoodestWhistle.Crafting.Add(CraftingTable.Workbench, 1);
            GoodestWhistle.RequiredItems.Add("FineWood", 1);
            GoodestWhistle.CraftAmount = 1;
            var shared = GoodestWhistle.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared;
            var statusEffect = ScriptableObject.CreateInstance<Recall>(); //sets the status effect script into this variable. 
            statusEffect.m_ttl = 1f; // duration of the status effect.
            shared.m_consumeStatusEffect = statusEffect; // the item = the status effect.


            Creature BestestDog = new("gsd", "BestestDog")            //add creature
            {
                Biome = Heightmap.Biome.BlackForest,
                CanBeTamed = true,
                FoodItems = "RawMeat, BestestTreat, CookedMeat, BoarJerky, NeckTail, DeerStew, NeckTailGrilled, DeerMeat, CookedDeerMeat, MinceMeatSauce, Sausages, Entrails, SerpentMeatCooked, SerpentStew, SerpentMeat, CookedWolfMeat, WolfMeat, Wolfjerky, LoxMeat, CookedLoxMeat, LoxPie,",
                SpawnChance = 10,
                RequiredWeather = Weather.None,
                GroupSize = new Range(1, 2),
                CheckSpawnInterval = 300,
                SpecificSpawnTime = SpawnTime.Night,
                Maximum = 1
            };
            BestestDog.Drops["BoneFragments"].Amount = new Range(1, 2);
            BestestDog.Drops["BoneFragments"].DropChance = 15f;
            BestestDog.Drops["WolfMeat"].Amount = new Range(1, 2);
            BestestDog.Drops["WolfMeat"].DropChance = 50f;

            Creature BestestPup = new("gsd", "BestestPup")            //add creature
            {
                Biome = Heightmap.Biome.None,
                CanSpawn = true,
                SpawnChance = 100,
                GroupSize = new Range(1, 2),
                Maximum = 1

            };
            BestestPup.Drops["BoneFragments"].Amount = new Range(1, 2);
            BestestPup.Drops["BoneFragments"].DropChance = 50f;


            Assembly assembly = Assembly.GetExecutingAssembly();

            Harmony harmony = new(ModGUID);
            harmony.PatchAll(assembly);


        }

    }
}