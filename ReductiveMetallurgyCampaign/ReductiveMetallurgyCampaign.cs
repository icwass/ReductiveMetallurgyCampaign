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
	private static Puzzle optionsUnlock = null;
	static Texture return_button, return_button_hover;

	static Texture iconSolitaire, iconSolitaireSmall;
	public static List<class_259> customSolitaires = new(); // SOLITAIRE_ICON_DEBUG

	public static void setOptionsUnlock(Puzzle puzzle)
	{
		if (optionsUnlock == null) optionsUnlock = puzzle;
	}

	public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

	// settings
	public static bool DisplayMetalsRemaining = true;
	public override Type SettingsType => typeof(MySettings);
	public class MySettings
	{
		[SettingsLabel("Display metals remaining in the new Sigmar's Garden.")]
		public bool DisplayMetalsRemaining = true;
		[SettingsLabel("[DEBUG] Show the finale, even if already seen.")]
		public bool AlwaysShowFinale = false;
		[SettingsLabel("[DEBUG] Have the finale go to the solutions menu, for fast testing.")]
		public bool GoToSolutionsMenu = false;
	}
	public override void ApplySettings()
	{
		base.ApplySettings();

		var SET = (MySettings)Settings;
		DisplayMetalsRemaining = SET.DisplayMetalsRemaining;
		Amalgamate.alwaysShowFinale = SET.AlwaysShowFinale;
		Amalgamate.goToSolutionsMenu = SET.GoToSolutionsMenu;
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
		CampaignLoader.modifyCampaign();

		string path = "textures/story/";
		return_button = class_235.method_615(path + "return_button_rmc");
		return_button_hover = class_235.method_615(path + "return_button_hover_rmc");

		path = "textures/puzzle_select/"; // SOLITAIRE_ICON_TEMP
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
	}


	public override void Load()
	{
		Settings = new MySettings();
		CampaignLoader.loadCampaignModel();
		Document.Load();
		//BoardEditorScreen.Load();
		CutscenePatcher.Load();
		On.class_172.method_480 += new On.class_172.hook_method_480(AddCharactersToDictionary);
		On.Solution.method_1958 += CampaignLoader.Solution_Method_1958;
		On.class_135.method_272 += Class135_Method_272;
	}

	public static void Class135_Method_272(On.class_135.orig_method_272 orig, Texture texture, Vector2 position)
	{
		if (CampaignLoader.currentCampaignIsRMC())
		{
			if (texture == class_238.field_1989.field_100.field_134)
			{
				texture = return_button;
			} else if (texture == class_238.field_1989.field_100.field_135)
			{
				texture = return_button_hover;
			}
		}
		orig(texture, position);
		return;
	}

	public static void AddCharactersToDictionary(On.class_172.orig_method_480 orig)
	{
		orig();
		Logger.Log("[ReductiveMetallurgyCampaign] Adding vignette actors.");
		// hardcode the characters that are already present
		class_172.field_1670["Verrin Ravari"] = new class_230(class_134.method_253("Verrin Ravari", string.Empty), class_238.field_1989.field_93.field_693, class_235.method_615("portraits/verrin_small"), Color.FromHex(6691857), false);
		class_172.field_1670["Verrin Ravari (Shabby)"] = new class_230(class_134.method_253("Verrin Ravari", string.Empty), class_238.field_1989.field_93.field_694, class_235.method_615("portraits/verrin_shabby_small"), Color.FromHex(6691857), false);
		class_172.field_1670["Taros Colvan"] = new class_230(class_134.method_253("Taros Colvan", string.Empty), class_238.field_1989.field_93.field_692, class_235.method_615("portraits/taros_small"), Color.FromHex(7873302), false);
		class_172.field_1670["Armand Van Tassen"] = new class_230(class_134.method_253("Armand Van Tassen", string.Empty), class_238.field_1989.field_93.field_676, class_235.method_615("portraits/armand_small"), Color.FromHex(6434368), false);
		// add the new characters
		foreach (CharacterModelRMC character in CampaignLoader.getModel().Characters)
		{
			Texture class256_1 = null;
			Texture class256_2 = null;
			if (!string.IsNullOrEmpty(character.SmallPortrait))
				class256_1 = class_235.method_615(character.SmallPortrait);
			if (!string.IsNullOrEmpty(character.LargePortrait))
				class256_2 = class_235.method_615(character.LargePortrait);
			class_230 class230 = new class_230(class_134.method_253(character.Name, string.Empty), class256_2, class256_1, Color.FromHex(character.Color), character.IsOnLeft);
			class_172.field_1670.Add(character.ID, class230);
		}
	}


	public override void PostLoad()
	{
		SigmarGardenPatcher.PostLoad();
		//oldAmalgamate.PostLoad();
		Amalgamate.PostLoad();
		On.OptionsScreen.method_50 += OptionsScreen_Method_50;
		On.StoryPanel.method_2172 += StoryPanel_Method_2172;
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

	public static void OptionsScreen_Method_50(On.OptionsScreen.orig_method_50 orig, OptionsScreen screen_self, float timeDelta)
	{
		if (CampaignLoader.currentCampaignIsRMC())
		{
			var screen_dyn = new DynamicData(screen_self);
			var currentStoryPanel = screen_dyn.Get<StoryPanel>("field_2680");
			var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("field_4093");
			if (!stringArray.Any(x => x.Contains("Saverio") || x.Contains("Pugano")))
			{
				var class264 = new class_264("options-rmc");
				class264.field_2090 = "rmc-options";
				screen_dyn.Set("field_2680", new StoryPanel((Maybe<class_264>)class264, false));
			}
		}
		orig(screen_self, timeDelta);
	}

	public static void StoryPanel_Method_2172(On.StoryPanel.orig_method_2172 orig, StoryPanel panel_self, float timeDelta, Vector2 pos, int index, Tuple<int, LocString>[] tuple)
	{
		if (CampaignLoader.currentCampaignIsRMC() && tuple.Length == 2 && tuple[0].Item2 == class_134.method_253("Complete the prologue", string.Empty))
		{
			// then we're doing the options code while in the RMC campaign
			// hijack the inputs so we draw it our way
			bool flag = GameLogic.field_2434.field_2451.method_573(optionsUnlock);
			index = flag ? 1 : 0;
			tuple = new Tuple<int, LocString>[2]
			{
				Tuple.Create(1, class_134.method_253("Complete the practical interview", string.Empty)),
				Tuple.Create(int.MaxValue, LocString.field_2597)
			};
		}
		else if (CampaignLoader.currentCampaignIsRMC() && tuple.Length == 7 && tuple[0].Item2 == class_134.method_253("Win 1 game", string.Empty))
		{
			// then we're doing the solitaire code while in the RMC campaign
			// hijack the inputs so we draw it our way
			tuple = new Tuple<int, LocString>[]
			{
				Tuple.Create(1, class_134.method_253("Win 1 game", string.Empty)),
				Tuple.Create(10, class_134.method_253("Win 10 games", string.Empty)),
				Tuple.Create(25, class_134.method_253("Win 25 games", string.Empty)),
				Tuple.Create(50, class_134.method_253("Win 50 games", string.Empty)),
				Tuple.Create(75, class_134.method_253("Win 75 games", string.Empty)),
				Tuple.Create(99, class_134.method_253("Win 99 games", string.Empty)),
				Tuple.Create(100, class_134.method_253("Win 100 games", string.Empty)),
				Tuple.Create(int.MaxValue, LocString.field_2597)
			};
		}

		orig(panel_self, timeDelta, pos, index, tuple);
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
