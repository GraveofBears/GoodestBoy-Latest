using System.Collections.Generic;
using BepInEx;
using ItemManager;
using CreatureManager;
using HarmonyLib;
using ServerSync;

namespace GoodestBoy
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class GoodestBoy : BaseUnityPlugin
	{
		private const string ModName = "GoodestBoy";
		private const string ModVersion = "0.1.4";
		private const string ModGUID = "org.bepinex.plugins.goodestboy";

		public void Awake()
		{

			Creature EvilBunny = new("gsd", "EvilBunny", "assets")              //add creature
			{
				Biome = Heightmap.Biome.Meadows,
				CanBeTamed = false,
				FoodItems = "Carrot",
				SpawnChance = 30,
				GroupSize = new Range(1, 2),
				CheckSpawnInterval = 600,
				SpecificSpawnTime = SpawnTime.Day,
				RequiredWeather = Weather.ClearSkies,
				Maximum = 1
			};
			EvilBunny.Drops["LeatherScraps"].Amount = new Range(1, 2);
			EvilBunny.Drops["LeatherScraps"].DropChance = 10f;
			EvilBunny.Drops["Carrot"].Amount = new Range(1, 2);
			EvilBunny.Drops["Carrot"].DropChance = 10f;


			Creature BrownEvilBunny = new("gsd", "BrownEvilBunny", "assets")              //add creature
			{
				Biome = Heightmap.Biome.Meadows,
				CanBeTamed = false,
				FoodItems = "Carrot",
				SpawnChance = 30,
				GroupSize = new Range(1, 2),
				CheckSpawnInterval = 600,
				SpecificSpawnTime = SpawnTime.Day,
				RequiredWeather = Weather.ClearSkies,
				Maximum = 1
			};
			BrownEvilBunny.Drops["LeatherScraps"].Amount = new Range(1, 2);
			BrownEvilBunny.Drops["LeatherScraps"].DropChance = 10f;
			BrownEvilBunny.Drops["Carrot"].Amount = new Range(1, 2);
			BrownEvilBunny.Drops["Carrot"].DropChance = 10f;


			Creature BestestDog = new("gsd", "BestestDog", "assets")            //add creature
			{
				Biome = Heightmap.Biome.BlackForest,
				CanBeTamed = true,
				FoodItems = "BestestStick, YummyBone, RawMeat, CookedMeat, BoarJerky, NeckTail, DeerStew, NeckTailGrilled, DeerMeat, CookedDeerMeat, MinceMeatSauce, Sausages, Entrails, SerpentMeatCooked, SerpentStew, SerpentMeat, CookedWolfMeat, WolfMeat, Wolfjerky, LoxMeat, CookedLoxMeat, LoxPie",
				SpawnChance = 10,
				GroupSize = new Range(1, 2),
				CheckSpawnInterval = 300,
				RequiredWeather = Weather.Rain,
				SpecificSpawnTime = SpawnTime.Night,
				Maximum = 1
			};
			BestestDog.Drops["BoneFragments"].Amount = new Range(1, 2);
			BestestDog.Drops["BoneFragments"].DropChance = 15f;
			BestestDog.Drops["WolfMeat"].Amount = new Range(1, 2);
			BestestDog.Drops["WolfMeat"].DropChance = 50f;

			Item BestestStick = new("gsd", "BestestStick", "assets");           //add item
			BestestStick.Crafting.Add(CraftingTable.Workbench, 1);
			BestestStick.RequiredItems.Add("Wood", 2);
			BestestStick.CraftAmount = 1;

			Item YummyBone = new("gsd", "YummyBone", "assets");                  //add item
			YummyBone.Crafting.Add(CraftingTable.Workbench, 1);
			YummyBone.RequiredItems.Add("BoneFragments", 4);
			YummyBone.CraftAmount = 1;

		}
	}
}
