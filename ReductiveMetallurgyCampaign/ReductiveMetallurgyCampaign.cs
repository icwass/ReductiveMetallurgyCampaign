using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ReductiveMetallurgyCampaign;

// SOLITAIRE_ICON_TEMP - these lines will be removed once custom campaign icons are actually implemented in quintessential

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
using PartTypes = class_191;
using Texture = class_256;
using Song = class_186;
using Tip = class_215;

public class MainClass : QuintessentialMod
{
	private static IDetour hook_Sim_method_1835;

	static Texture iconSolitaire, iconSolitaireSmall;
	public static List<class_259> customSolitaires = new(); // SOLITAIRE_ICON_DEBUG


	public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

	// settings
	public static bool DisplayMetalsRemaining = true;
	public override Type SettingsType => typeof(MySettings);
	public class MySettings
	{
		[SettingsLabel("Display 'metals remaining' in the new Sigmar's Garden.")]
		public bool DisplayMetalsRemaining = true;
		[SettingsLabel("[DEBUG] Show the finale, even if already seen.")]
		public bool AlwaysShowFinale = false;
		[SettingsLabel("[DEBUG] Have the finale go to the solutions menu, for fast testing.")]
		public bool TestingMode = false;
	}
	public override void ApplySettings()
	{
		base.ApplySettings();

		var SET = (MySettings)Settings;
		DisplayMetalsRemaining = SET.DisplayMetalsRemaining;
		Amalgamate.alwaysShowFinale = SET.AlwaysShowFinale;
		Amalgamate.TestingMode = SET.TestingMode;
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
		CampaignLoader.loadCampaignModel();
		JournalLoader.loadJournalModel();
		Document.Load();
		//BoardEditorScreen.Load();
		CutscenePatcher.Load();
		JournalLoader.Load();
		StoryPanelPatcher.Load();
		On.Solution.method_1958 += CampaignLoader.Solution_Method_1958;
	}




	public override void PostLoad()
	{
		SigmarGardenPatcher.PostLoad();
		Amalgamate.PostLoad();
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














	//---------------------------------------------------//
	//debug helpers

	private static string atomtypeToChar(AtomType type)
	{
		string[] table = new string[17] { "…", "🜔", "🜁", "🜃", "🜂", "🜄", "☿", "☉", "☽", "♀", "♂", "♃", "♄", "🜍", "🜞", "…", "✶" };
		if (type.field_2283 < 17 && type.field_2283 >= 0) return table[type.field_2283];
		Logger.Log("printMolecule: Unknown atom type '" + type.field_2284 + "' (byteID: " + type.field_2283 + ")");
		return "?";
	}
	private static char bondToChar(enum_126 type, Pair<int, int> location)
	{
		int index = (location.Right % 2 == 0) ? 0 : ((location.Right - location.Left) % 4 == 0) ? 1 : 2;
		string table = "—\\/~}{";
		if (type == (enum_126)0)
		{
			return ' ';
		}
		else if (type == (enum_126)1)
		{
			return table[index];
		}
		else if (((int)type & 14) == (int)type)
		{
			return table[index + 3];
		}
		Logger.Log("printMolecule: Unknown bond type '" + type + "'");
		return '#';
	}
	private static Pair<int, int> hexToPair(HexIndex hex) => new Pair<int, int>(4 * hex.Q + 2 * hex.R, -2 * hex.R);
	private static Pair<int, int> bondToPair(class_277 bond)
	{
		//assumes bonds are between adjacent atoms only
		var pair1 = hexToPair(bond.field_2187);
		var pair2 = hexToPair(bond.field_2188);
		return new Pair<int, int>((pair1.Left + pair2.Left) / 2, (pair1.Right + pair2.Right) / 2);
	}
	public static void printMolecule(Molecule molecule)
	{
		var moleculeDyn = new DynamicData(molecule);
		var atomDict = moleculeDyn.Get<Dictionary<HexIndex, Atom>>("field_2642");
		if (atomDict.Count == 0)
		{
			Logger.Log("<empty molecule>");
			return;
		}
		var bondList = moleculeDyn.Get<List<class_277>>("field_2643");
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		Dictionary<Pair<int, int>, string> charDict = new();
		foreach (var pair in atomDict)
		{
			Pair<int, int> location = hexToPair(pair.Key);
			charDict.Add(location, atomtypeToChar(pair.Value.field_2275));
			minX = Math.Min(minX, location.Left);
			maxX = Math.Max(maxX, location.Left + 4);
			minY = Math.Min(minY, location.Right - 1);
			maxY = Math.Max(maxY, location.Right + 2);
		}
		foreach (var bond in bondList)
		{
			Pair<int, int> location = bondToPair(bond);
			charDict.Add(location, "" + bondToChar(bond.field_2186, location));
		}
		string[,] vram = new string[maxX - minX, maxY - minY];
		for (int i = 0; i < maxX - minX; i++)
		{
			for (int j = 0; j < maxY - minY; j++)
			{
				vram[i, j] = (i % 4 == 3) ? "	" : " ";
			}
		}
		foreach (var pair in charDict)
		{
			vram[pair.Key.Left - minX, pair.Key.Right - minY] = pair.Value;
		}
		for (int j = 0; j < maxY - minY; j++)
		{
			string str = "";
			for (int i = 0; i < maxX - minX; i++)
			{
				str = str + vram[i, j];
			}
			if (j == 0)
			{
				Logger.Log("/	" + str + "\\");
			}
			else if (j == maxY - minY - 1)
			{
				Logger.Log("\\	" + str + "/");
			}
			else
			{
				Logger.Log("|	" + str + "|");
			}
		}
	}
}
