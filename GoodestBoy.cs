using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using ServerSync;
using HarmonyLib;
using ItemManager;
using CreatureManager;


namespace GoodestBoy
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]


    public class OdinsKingdom : BaseUnityPlugin
    {
        private const string ModName = "GoodestBoy";
        private const string ModVersion = "0.1.6";
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

            Item BestestStick = new("gsd", "BestestStick");
            BestestStick.Name.English("Bestest Stick");
            BestestStick.Description.English("A stick for your bestest friend");
            BestestStick.Crafting.Add(CraftingTable.Workbench, 1);
            BestestStick.RequiredItems.Add("Wood", 2);
            BestestStick.CraftAmount = 1;
            GameObject BestestStick_Projectile = ItemManager.PrefabManager.RegisterPrefab("gsd", "BestestStick_Projectile"); //register projectile

            Item YummyBone = new("gsd", "YummyBone");
            YummyBone.Name.English("YummyBone");
            YummyBone.Description.English("A bone for your bestest friend");
            YummyBone.Crafting.Add(CraftingTable.Workbench, 1);
            YummyBone.RequiredItems.Add("BoneFragments", 4);
            YummyBone.CraftAmount = 1;

            Creature BestestDog = new("gsd", "BestestDog")            //add creature
            {
                Biome = Heightmap.Biome.BlackForest,
                CanBeTamed = true,
                FoodItems = "BestestStick, YummyBone, RawMeat, CookedMeat, BoarJerky, NeckTail, DeerStew, NeckTailGrilled, DeerMeat, CookedDeerMeat, MinceMeatSauce, Sausages, Entrails, SerpentMeatCooked, SerpentStew, SerpentMeat, CookedWolfMeat, WolfMeat, Wolfjerky, LoxMeat, CookedLoxMeat, LoxPie",
                SpawnChance = 10,
                GroupSize = new Range(1, 2),
                CheckSpawnInterval = 300,
                RequiredWeather = Weather.None,
                SpecificSpawnTime = SpawnTime.Night,
                Maximum = 1
            };
            BestestDog.Drops["BoneFragments"].Amount = new Range(1, 2);
            BestestDog.Drops["BoneFragments"].DropChance = 15f;
            BestestDog.Drops["WolfMeat"].Amount = new Range(1, 2);
            BestestDog.Drops["WolfMeat"].DropChance = 50f;

            Creature Bestest_Pup = new("gsd", "Bestest_Pup")            //add creature
            {
                Biome = Heightmap.Biome.None,
                CanSpawn = true,
                SpawnChance = 100,
                GroupSize = new Range(1, 2),
                Maximum = 1

            };
            Bestest_Pup.Drops["BoneFragments"].Amount = new Range(1, 2);
            Bestest_Pup.Drops["BoneFragments"].DropChance = 50f;


        }
    }
}