//using Mono.Cecil.Cil;
//using MonoMod.Cil;
using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
using Quintessential.Serialization;
using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
//using System.Linq;
//using System.Collections.Generic;
//using System.Globalization;
using System.Reflection;

namespace ReductiveMetallurgyCampaign;

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
//using PartTypes = class_191;
//using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public class MainClass : QuintessentialMod
{
	public static MethodInfo PublicMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
	public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
	private static IDetour hook_Sim_method_1835, hook_QuintessentialLoader_LoadJournals;

	public static AdvancedContentModelRMC AdvancedContent;
	public static string FilePath = "";

	// settings
	public override Type SettingsType => typeof(MySettings);
	public class MySettings
	{
		[SettingsLabel("Display 'metals remaining' in the new Sigmar's Garden.")]
		public bool DisplayMetalsRemaining = true;
	}
	public override void ApplySettings()
	{
		base.ApplySettings();

		var SET = (MySettings)Settings;
		SigmarGardenPatcher.DisplayMetalsRemaining = SET.DisplayMetalsRemaining;
	}

	public static void checkIfFileExists(string subpath, string file, string error)
	{
		if (!File.Exists(FilePath + subpath + file))
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find '" + file + "' in the folder '" + FilePath + subpath + "'");
			throw new Exception(error);
		}
	}

	static void LoadAdvancedContent()
	{
		string subpath = "/Puzzles/";
		string file = "RMC.advanced.yaml";
		checkIfFileExists(subpath, file, "LoadAdvancedContent: Advanced content is missing.");
		using (StreamReader streamReader = new StreamReader(FilePath + subpath + file))
		{
			AdvancedContent = YamlHelper.Deserializer.Deserialize<AdvancedContentModelRMC>(streamReader);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	public override void Load()
	{
		// fetch the mod's filepath
		bool foundSelf = false;
		string name = "ReductiveMetallurgyCampaign";
		foreach (ModMeta mod in QuintessentialLoader.Mods)
		{
			if (mod.Name == name)
			{
				FilePath = mod.PathDirectory;
				foundSelf = true;
				break;
			}
		}
		if (!foundSelf)
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find... myself?' QuintessentialLoader.Mods doesn't have a mod called '" + name + "'.");
			throw new Exception("Load: Failed to find the expected mod.");
		}

		// load other stuff
		Settings = new MySettings();
		LoadAdvancedContent();
		CampaignLoader.Load();
		Document.Load();
		CutscenePatcher.Load();
		JournalLoader.Load();
		StoryPanelPatcher.Load();
	}

	public override void LoadPuzzleContent()
	{
		PolymerInput.LoadContent();
		StoryPanelPatcher.LoadContent();

		// manually load the puzzle file needed for tips
		string subpath = "/Puzzles/";
		string file = "rmc-sandbox.puzzle.yaml";
		checkIfFileExists(subpath, file, "LoadAdvancedContent: Tip data is missing.");
		var tipsPuzzle = PuzzleModel.FromModel(YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(FilePath + subpath + file)));

		Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
		Puzzles.field_2816[Puzzles.field_2816.Length - 1] = tipsPuzzle;

		//------------------------- HOOKING -------------------------//
		hook_Sim_method_1835 = new Hook(PrivateMethod<Sim>("method_1835"), OnSimMethod1835);
		hook_QuintessentialLoader_LoadJournals = new Hook(PublicMethod<QuintessentialLoader>("LoadJournals"), HotReloadCampaignAndJournal);
	}

	private delegate void orig_Sim_method_1835(Sim self);
	private static void OnSimMethod1835(orig_Sim_method_1835 orig, Sim sim_self)
	{
		PolymerInput.My_Method_1835(sim_self);
		orig(sim_self);
	}

	public delegate void orig_QuintessentialLoader_LoadJournals();
	public static void HotReloadCampaignAndJournal(orig_QuintessentialLoader_LoadJournals orig)
	{
		orig();
		LoadAdvancedContent();
		CampaignLoader.modifyCampaign();
		JournalLoader.modifyJournal();
		StoryPanelPatcher.CreateSigmarStoryUnlocks(AdvancedContent.SigmarStoryUnlocks);

	}

	public override void Unload()
	{
		hook_Sim_method_1835.Dispose();
		hook_QuintessentialLoader_LoadJournals.Dispose();
		SigmarGardenPatcher.Unload();
		Amalgamate.Unload();
		JournalLoader.Unload();
	}

	public override void PostLoad()
	{
		SigmarGardenPatcher.PostLoad();
		Amalgamate.PostLoad();
		ProductionManager.PostLoad();
		StoryPanelPatcher.PostLoad();
	}
}
