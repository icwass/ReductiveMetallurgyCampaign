//using Mono.Cecil.Cil;
//using MonoMod.Cil;
using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
//using System.Linq;
using System.Collections.Generic;
//using System.Globalization;
using System.Reflection;

namespace ReductiveMetallurgyCampaign;

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
//using PartTypes = class_191;
using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

// SOLITAIRE_ICON_TEMP - these lines will be removed once custom campaign icons are actually implemented in quintessential

public class MainClass : QuintessentialMod
{
	public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
	private static IDetour hook_Sim_method_1835;

	static Texture iconSolitaire, iconSolitaireSmall;
	public static List<class_259> customSolitaires = new(); // SOLITAIRE_ICON_DEBUG

	// settings
	public static bool DisplayMetalsRemaining = true;
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
		DisplayMetalsRemaining = SET.DisplayMetalsRemaining;
	}

	public static bool findModMetaFilepath(string name, out string filepath)
	{
		filepath = "<missing mod directory>";
		foreach (ModMeta mod in QuintessentialLoader.Mods)
		{
			if (mod.Name == name)
			{
				filepath = mod.PathDirectory;
				return true;
			}
		}
		return false;
	}

	public override void LoadPuzzleContent()
	{
		PolymerInput.LoadContent();
		StoryPanelPatcher.LoadContent();
		ProductionManager.initializeProductionTextureBank();
		CampaignLoader.modifyCampaign();

		string path = "textures/puzzle_select/"; // SOLITAIRE_ICON_TEMP
		iconSolitaire = class_235.method_615(path + "icon_rmc_solitaire"); // SOLITAIRE_ICON_TEMP
		iconSolitaireSmall = class_235.method_615(path + "icon_rmc_solitaire_small"); // SOLITAIRE_ICON_TEMP

		//------------------------- HOOKING -------------------------//
		hook_Sim_method_1835 = new Hook(PrivateMethod<Sim>("method_1835"), OnSimMethod1835);
	}

	private delegate void orig_Sim_method_1835(Sim self);
	private static void OnSimMethod1835(orig_Sim_method_1835 orig, Sim sim_self)
	{
		PolymerInput.My_Method_1835(sim_self);
		orig(sim_self);
	}

	public override void Unload() {
		hook_Sim_method_1835.Dispose();
		SigmarGardenPatcher.Unload();
		Amalgamate.Unload();
		JournalLoader.Unload();
	}

	public override void Load()
	{
		Settings = new MySettings();
		CampaignLoader.Load();
		JournalLoader.loadJournalModel();
		Document.Load();
		CutscenePatcher.Load();
		JournalLoader.Load();
		StoryPanelPatcher.Load();
	}

	public override void PostLoad()
	{
		SigmarGardenPatcher.PostLoad();
		Amalgamate.PostLoad();
		ProductionManager.PostLoad();
		StoryPanelPatcher.PostLoad();
		On.CampaignItem.method_826 += ChooseCustomIconLarge; // SOLITAIRE_ICON_TEMP
		On.CampaignItem.method_827 += ChooseCustomIconSmall; // SOLITAIRE_ICON_TEMP
	}

	public static Texture ChooseCustomIconLarge(On.CampaignItem.orig_method_826 orig, CampaignItem item_self) // SOLITAIRE_ICON_TEMP
	{
		return item_self.field_2324 == CampaignLoader.typeSolitaire && customSolitaires.Contains(item_self.field_2326) ? iconSolitaire : orig(item_self);
	}
	public static Texture ChooseCustomIconSmall(On.CampaignItem.orig_method_827 orig, CampaignItem item_self) // SOLITAIRE_ICON_TEMP
	{
		return item_self.field_2324 == CampaignLoader.typeSolitaire && customSolitaires.Contains(item_self.field_2326) ? iconSolitaireSmall : orig(item_self);
	}
}
