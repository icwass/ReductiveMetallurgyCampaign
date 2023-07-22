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

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
using PartTypes = class_191;
using Texture = class_256;
using Song = class_186;
using Tip = class_215;

public class PuzzleModelRMC
{
	public string ID, Music;
	public List<string> Inputs, Outputs;
}
public class CharacterModelRMC
{
	public string ID, Name, SmallPortrait, LargePortrait;
	public int Color;
	public bool IsOnLeft;
}

public class SigmarsGardenRMC
{
	public string ID;
}


public class CampaignModelRMC
{
	public List<PuzzleModelRMC> Puzzles;
	public List<CharacterModelRMC> Characters;
	public List<DocumentModelRMC> Documents;
	public List<CutsceneModelRMC> Cutscenes;
	public List<SigmarsGardenRMC> SigmarsGardens;
}

public class MainClass : QuintessentialMod
{
	private static IDetour hook_Sim_method_1835;
	public static Campaign campaign_self;
	private static Puzzle optionsUnlock;
	static Texture return_button, return_button_hover;

	public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

	// settings
	public static bool DisplayMetalsRemaining = true;
	public override Type SettingsType => typeof(MySettings);
	public class MySettings
	{
		[SettingsLabel("Display metals remaining in the new Sigmar's Garden.")]
		public bool DisplayMetalsRemaining = true;
	}
	public override void ApplySettings()
	{
		base.ApplySettings();

		var SET = (MySettings)Settings;
		DisplayMetalsRemaining = SET.DisplayMetalsRemaining;
	}

	public static bool currentCampaignIsRMC() => campaign_self == Campaigns.field_2330;

	string[] specialTipsPaths;
	string[] specialTips = new string[]{
		"RMCrejection",
		"RMCdeposition",
		"RMCproliferation",
		"RMCravari",
		"RMCravari2",
	};


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

	private static CampaignModelRMC fetchCampaignModel()
	{
		string filepath;
		if (!findModMetaFilepath("ReductiveMetallurgyCampaign", out filepath) || !File.Exists(filepath + "/Puzzles/RMC.advanced.yaml"))
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find 'RMC.advanced.yaml' in the folder '" + filepath + "\\Puzzles\\'");
			throw new Exception("modifyCampaignRMC: Campaign data is missing.");
		}
		using (StreamReader streamReader = new StreamReader(filepath + "/Puzzles/RMC.advanced.yaml"))
			return YamlHelper.Deserializer.Deserialize<CampaignModelRMC>(streamReader);
	}

	private static void patchCampaign(Campaign campaign)
	{
		foreach (CampaignChapter campaignChapter in campaign.field_2309)
		{
			foreach (CampaignItem campaignItem in campaignChapter.field_2314)
			{
				string field2322 = campaignItem.field_2322;
				if (campaignItem.field_2325.method_1085())
				{
					Puzzle puzzle = campaignItem.field_2325.method_1087();
					puzzle.field_2766 = field2322;
					Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
					Puzzles.field_2816[Puzzles.field_2816.Length - 1] = puzzle;
					foreach (PuzzleInputOutput puzzleInputOutput in (puzzle.field_2770).Union(puzzle.field_2771))
					{
						if (!puzzleInputOutput.field_2813.field_2639.method_1085())
							puzzleInputOutput.field_2813.field_2639 = (Maybe<LocString>)class_134.method_253("Molecule", string.Empty);
					}
				}
			}
		}
	}

	private static void modifyCampaignLevelsRMC()
	{
		foreach (Campaign allCampaign in QuintessentialLoader.AllCampaigns)
		{
			if (allCampaign.QuintTitle == "Reductive Metallurgy")
			{
				campaign_self = allCampaign;
				patchCampaign(campaign_self);
				break;
			}
		}

		var songList = new Dictionary<string, Song>()
		{
			{"Map", class_238.field_1992.field_968},
			{"Solitaire", class_238.field_1992.field_969},
			{"Solving1", class_238.field_1992.field_970},
			{"Solving2", class_238.field_1992.field_971},
			{"Solving3", class_238.field_1992.field_972},
			{"Solving4", class_238.field_1992.field_973},
			{"Solving5", class_238.field_1992.field_974},
			{"Solving6", class_238.field_1992.field_975},
			{"Story1", class_238.field_1992.field_976},
			{"Story2", class_238.field_1992.field_977},
			{"Title", class_238.field_1992.field_978},
		};
		var fanfareList = new Dictionary<string, Sound>()
		{
			{"Map", class_238.field_1991.field_1832},
			{"Solitaire", class_238.field_1991.field_1832},
			{"Solving1", class_238.field_1991.field_1830},
			{"Solving2", class_238.field_1991.field_1831},
			{"Solving3", class_238.field_1991.field_1832},
			{"Solving4", class_238.field_1991.field_1833},
			{"Solving5", class_238.field_1991.field_1834},
			{"Solving6", class_238.field_1991.field_1835},
			{"Story1", class_238.field_1991.field_1832},
			{"Story2", class_238.field_1991.field_1832},
			{"Title", class_238.field_1991.field_1832},
		};
		Tip tipPolymerInput = new Tip()
		{
			field_1899 = "RMCT000",
			field_1900 = class_134.method_253("Repeating Reagents", string.Empty),
			field_1901 = class_134.method_253("Some reagents repeat infinitely, and must be fed into the transmutation engine with the help of a guiding rail.\n\nArms can be mounted on the rail, but atoms are not allowed to pass through it.", string.Empty),
			field_1902 = "RMCrejection",
			field_1903 = class_235.method_615("tips/polymer_inputs")
		};
		Tip tipRejection = new Tip()
		{
			field_1899 = "RMCT001",
			field_1900 = class_134.method_253("Glyph of Rejection", string.Empty),
			field_1901 = class_134.method_253("The *glyph of rejection* can demote an atom of metal to its next lower form, producing an atom of quicksilver.\n\nBy doing this repeatedly, even gold — the finest metal — can be transmuted into base lead.", string.Empty),
			field_1902 = "RMCrejection",
			field_1904 = new Vector2(-42f, 0f)
		};
		Tip tipDeposition = new Tip()
		{
			field_1899 = "RMCT002",
			field_1900 = class_134.method_253("Glyph of Deposition", string.Empty),
			field_1901 = class_134.method_253("The *glyph of deposition* transmutes one atom of metal into two atoms of a lower form.\n\nNote that the resulting metals together have the same 'metallicity' as the original.", string.Empty),
			field_1902 = "RMCdeposition",
			field_1904 = new Vector2(0f, 0f)
		};
		Tip tipProliferation = new Tip()
		{
			field_1899 = "RMCT003",
			field_1900 = class_134.method_253("Glyph of Proliferation", string.Empty),
			field_1901 = class_134.method_253("The *glyph of proliferation* does some amazing shit. Wowzers!", string.Empty),
			field_1902 = "RMCproliferation",
			field_1904 = new Vector2(-42f, 0f)
		};
		Tip tipRavari1 = new Tip()
		{
			field_1899 = "RMCT004",
			field_1900 = class_134.method_253("Ravari's Wheel", string.Empty),
			field_1901 = class_134.method_253("By using *Ravari's wheel* with the glyphs of projection and rejection, quicksilver can be stored or discharged.\n\nBecause it has metals with differing amounts of available quicksilver, Ravari's wheel could be called \"the metallurgist’s buffer.\"", string.Empty),
			field_1902 = "RMCravari",
			field_1904 = new Vector2(126f, 0f)
		};
		Tip tipRavari2 = new Tip()
		{
			field_1899 = "RMCT005",
			field_1900 = class_134.method_253("Direct Quicksilver Transfer", string.Empty),
			field_1901 = class_134.method_253("It is possible to use *Ravari's wheel* without manually handling the quicksilver.\n\nBy placing the wheel above the _quicksilver_ port of the glyphs of projection or rejection, one can directly transfer the quicksilver in and out of the wheel.", string.Empty),
			field_1902 = "RMCravari2",
			field_1904 = new Vector2(-126f, 0f)
		};

		Dictionary<string, Tip> tipDict = new()
		{
			{"rmc-lesson-rejection", tipRejection },
			{"rmc-lesson-deposition", tipDeposition },
			{"rmc-lesson-proliferation", tipProliferation },
			{"rmc-ravari-requiescence", tipRavari1 },
			{"rmc-energetic-capacitor", tipRavari2 },
			{"rmc-golden-thread-recycling", tipPolymerInput },
			{"rmc-synthesis-via-chain", tipPolymerInput },
		};
		
		Logger.Log("[ReductiveMetallurgyCampaign] Modifying campaign levels.");
		CampaignChapter[] field2309 = campaign_self.field_2309;
		foreach (var campaignChapter in field2309)
		{
			if (campaignChapter.field_2310 == 1) campaignChapter.field_2321 = true;
			foreach (var campaignItem in campaignChapter.field_2314)
			{
				// convert unlock requirement if needed
				if (campaignItem.field_2326.GetType() == typeof(class_243))
				{
					class_243 stringUnlockRequirement = (class_243) campaignItem.field_2326;
					int n;
					if (int.TryParse(stringUnlockRequirement.field_2005, out n))
					{
						campaignItem.field_2326 = new class_265(n);
					}
				}

				// process the puzzle, if it exists
				if (campaignItem.field_2325.method_1085())
				{
					Puzzle puzzle = campaignItem.field_2325.method_1087();
					string puzzleID = puzzle.field_2766;

					if (tipDict.ContainsKey(puzzleID)) puzzle.field_2769 = tipDict[puzzleID];

					if (puzzleID == "rmc-lesson-rejection")
					{
						optionsUnlock = puzzle;
					}

					if (puzzleID == "rmc-golden-thread-recycling")
					{
						HexIndex hexIndex1 = new HexIndex(1, 0);
						List<class_157> class157List = new List<class_157>();
						HexIndex[] hexIndexArray = new HexIndex[3]
						{
							new HexIndex(2, 0),
							new HexIndex(4, 0),
							new HexIndex(6, 0)
						};
						foreach (var hexIndex2 in hexIndexArray)
						{
							class157List.Add(new class_157()
							{
								field_1597 = PartTypes.field_1760, // input
								field_1598 = hexIndex1 + hexIndex2
							});
						}
						class157List.Add(new class_157()
						{
							field_1597 = PolymerInput.partTypeGoldenThread,
							field_1598 = hexIndex1 + new HexIndex(6, -2),
							field_1602 = new HexIndex[15]
							{
								hexIndex1 + new HexIndex(6, -2),
								hexIndex1 + new HexIndex(5, -2),
								hexIndex1 + new HexIndex(4, -2),
								hexIndex1 + new HexIndex(3, -2),
								hexIndex1 + new HexIndex(2, -2),
								hexIndex1 + new HexIndex(1, -2),
								hexIndex1 + new HexIndex(0, -1),
								hexIndex1 + new HexIndex(0, 0),
								hexIndex1 + new HexIndex(-1, 1),
								hexIndex1 + new HexIndex(-1, 2),
								hexIndex1 + new HexIndex(0, 2),
								hexIndex1 + new HexIndex(1, 2),
								hexIndex1 + new HexIndex(2, 2),
								hexIndex1 + new HexIndex(3, 2),
								hexIndex1 + new HexIndex(4, 2)
							}
						});
						puzzle.field_2772 = new class_157[class157List.Count];
						for (int index = 0; index < class157List.Count; ++index)
							puzzle.field_2772[index] = class157List[index];
					}
					else if (puzzleID == "rmc-synthesis-via-chain")
					{
						HexIndex hexIndex1 = new HexIndex(1, 0);
						List<class_157> class157List = new List<class_157>();
						HexIndex[] hexIndexArray = new HexIndex[]
						{
							new HexIndex(0, 0),
							new HexIndex(4, 0)
						};
						foreach (var hexIndex2 in hexIndexArray)
						{
							class157List.Add(new class_157()
							{
								field_1597 = PartTypes.field_1760, // input
								field_1598 = hexIndex1 + hexIndex2
							});
						}
						class157List.Add(new class_157()
						{
							field_1597 = PolymerInput.partTypeBerloChain,
							field_1598 = hexIndex1 + new HexIndex(8, -2),
							field_1602 = new HexIndex[]
							{
								hexIndex1 + new HexIndex(8, -2),
								hexIndex1 + new HexIndex(7, -2),
								hexIndex1 + new HexIndex(6, -2),
								hexIndex1 + new HexIndex(5, -2),
								hexIndex1 + new HexIndex(4, -2),
								hexIndex1 + new HexIndex(3, -2),
								hexIndex1 + new HexIndex(2, -2),
								hexIndex1 + new HexIndex(1, -2),
								hexIndex1 + new HexIndex(0, -1),
								hexIndex1 + new HexIndex(-1, 0),
								hexIndex1 + new HexIndex(-1, 1),
								hexIndex1 + new HexIndex(-1, 2),
								hexIndex1 + new HexIndex(0, 2),
								hexIndex1 + new HexIndex(1, 2),
								hexIndex1 + new HexIndex(2, 2),
								hexIndex1 + new HexIndex(3, 2),
								hexIndex1 + new HexIndex(4, 2),
								hexIndex1 + new HexIndex(5, 2),
								hexIndex1 + new HexIndex(6, 2)
							}
						});
						puzzle.field_2772 = new class_157[class157List.Count];
						for (int index = 0; index < class157List.Count; ++index)
							puzzle.field_2772[index] = class157List[index];
					}
				}
			}
		}
		Dictionary<string, PuzzleModelRMC> puzzleDictionary = new Dictionary<string, PuzzleModelRMC>();
		List<string> documentIDList = new List<string>();
		Dictionary<string, string> cutsceneIDList = new Dictionary<string, string>();
		List<string> sigmarsGardensIDList = new List<string>();
		foreach (PuzzleModelRMC puzzle in fetchCampaignModel().Puzzles)
		{
			puzzleDictionary.Add(puzzle.ID, puzzle);
		}
		var DocumentModels = fetchCampaignModel().Documents;
		Document.LoadDocuments(DocumentModels);
		foreach (var document in DocumentModels)
		{
			documentIDList.Add(document.ID);
		}
		var CutsceneModels = fetchCampaignModel().Cutscenes;
		CutscenePatcher.LoadCutscenes(CutsceneModels);
		foreach (var cutscene in CutsceneModels)
		{
			if (string.IsNullOrEmpty(cutscene.Music))
			{
				cutsceneIDList.Add(cutscene.ID, "Story1");
			}
			else
			{
				cutsceneIDList.Add(cutscene.ID, cutscene.Music);
			}
		}
		foreach (var garden in fetchCampaignModel().SigmarsGardens)
		{
			sigmarsGardensIDList.Add(garden.ID);
		}
		foreach (var campaignChapter in field2309)
		{
			foreach (var campaignItem in campaignChapter.field_2314)
			{
				if (campaignItem.field_2325.method_1085())
				{
					Puzzle puzzle = campaignItem.field_2325.method_1087();
					//campaignItem.field_2324 defaults to (enum_129)0, i.e. a puzzle
					string field2766 = puzzle.field_2766;
					if (cutsceneIDList.Keys.Contains(field2766))
					{
						// change item into a cutscene
						campaignItem.field_2324 = (enum_129)1;
						campaignItem.field_2328 = songList[cutsceneIDList[field2766]];
					}
					if (documentIDList.Contains(field2766))
					{
						// change item into a document
						campaignItem.field_2324 = (enum_129)2;
					}
					else if (sigmarsGardensIDList.Contains(field2766))
					{
						// change item into a Sigmars Garden
						campaignItem.field_2324 = (enum_129)3;
					}
					else if (puzzleDictionary.ContainsKey(field2766))
					{
						PuzzleModelRMC puzzleModel = puzzleDictionary[field2766];
						void processIO(bool doInputs)
						{
							List<string> stringList = doInputs ? puzzleModel.Inputs : puzzleModel.Outputs;
							if (stringList == null) return;

							PuzzleInputOutput[] puzzleInputOutputArray = doInputs ? puzzle.field_2770 : puzzle.field_2771;
							int num = Math.Min(puzzleInputOutputArray.Length, stringList.Count);
							for (int index = 0; index < num; ++index)
								puzzleInputOutputArray[index].field_2813.field_2639 = (Maybe<LocString>)class_134.method_253(stringList[index], string.Empty);
						}

						processIO(true);
						processIO(false);

						if (!string.IsNullOrEmpty(puzzleModel.Music) && songList.Keys.Contains(puzzleModel.Music))
						{
							campaignItem.field_2328 = songList[puzzleModel.Music];
							campaignItem.field_2329 = fanfareList[puzzleModel.Music];
						}
					}
				}
			}
		}
	}

	public override void LoadPuzzleContent()
	{
		PolymerInput.LoadContent();
		modifyCampaignLevelsRMC();

		
		specialTipsPaths = new string[specialTips.Length];
		for(int i=0; i < specialTips.Length; i++)
		{
			specialTipsPaths[i] = "Content\\tips\\" + specialTips[i] + ".solution";
		}

		// manually load the puzzle file needed for tips
		string subpath = "/Puzzles/rmc-sandbox.puzzle";
		string filepath;
		if (!findModMetaFilepath("ReductiveMetallurgyCampaign", out filepath) || !File.Exists(filepath + subpath))
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find 'rmc-sandbox.puzzle' in the folder '" + filepath + "\\Puzzles\\'");
			throw new Exception("LoadPuzzleContent: Tip data is missing.");
		}
		var tipsPuzzle = Puzzle.method_1249(filepath + subpath);
		Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
		Puzzles.field_2816[Puzzles.field_2816.Length - 1] = tipsPuzzle;


		string path = "textures/story/";
		return_button = class_235.method_615(path + "return_button_rmc");
		return_button_hover = class_235.method_615(path + "return_button_hover_rmc");

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
	}


	public override void Load()
	{
		Settings = new MySettings();
		Document.Load();
		//BoardEditorScreen.Load();
		CutscenePatcher.Load();
		On.class_172.method_480 += new On.class_172.hook_method_480(Class172_Method_480);
		On.Solution.method_1958 += Solution_Method_1958;
		On.class_135.method_272 += Class135_Method_272;
	}

	public static void Class135_Method_272(On.class_135.orig_method_272 orig, Texture texture, Vector2 position)
	{
		if (currentCampaignIsRMC())
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

	public Maybe<Solution> Solution_Method_1958(On.Solution.orig_method_1958 orig, string filePath)
	{
		if (specialTipsPaths.Contains(filePath))
		{
			foreach (var dir in QuintessentialLoader.ModContentDirectories)
			{
				Logger.Log("    " + dir);
				try
				{
					return orig(Path.Combine(dir, filePath));
				}
				catch (Exception) { }
			}
		}

		return orig(filePath);
	}

	public static void Class172_Method_480(On.class_172.orig_method_480 orig)
	{
		orig();
		Logger.Log("[ReductiveMetallurgyCampaign] Adding vignette actors.");
		class_172.field_1670["Verrin Ravari"] = new class_230(class_134.method_253("Verrin Ravari", string.Empty), class_238.field_1989.field_93.field_693, class_235.method_615("portraits/verrin_small"), Color.FromHex(6691857), false);
		class_172.field_1670["Verrin Ravari (Shabby)"] = new class_230(class_134.method_253("Verrin Ravari", string.Empty), class_238.field_1989.field_93.field_694, class_235.method_615("portraits/verrin_shabby_small"), Color.FromHex(6691857), false);
		class_172.field_1670["Taros Colvan"] = new class_230(class_134.method_253("Taros Colvan", string.Empty), class_238.field_1989.field_93.field_692, class_235.method_615("portraits/taros_small"), Color.FromHex(7873302), false);
		class_172.field_1670["Armand Van Tassen"] = new class_230(class_134.method_253("Armand Van Tassen", string.Empty), class_238.field_1989.field_93.field_676, class_235.method_615("portraits/armand_small"), Color.FromHex(6434368), false);
		foreach (CharacterModelRMC character in fetchCampaignModel().Characters)
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
		SigmarGardenPatcher.Load();
		On.OptionsScreen.method_50 += OptionsScreen_Method_50;
		On.StoryPanel.method_2172 += StoryPanel_Method_2172;
	}
	public static void OptionsScreen_Method_50(On.OptionsScreen.orig_method_50 orig, OptionsScreen screen_self, float timeDelta)
	{
		if (currentCampaignIsRMC())
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
		if (currentCampaignIsRMC() && tuple.Length == 2 && tuple[0].Item2 == class_134.method_253("Complete the prologue", string.Empty))
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
		else if (currentCampaignIsRMC() && tuple.Length == 7 && tuple[0].Item2 == class_134.method_253("Win 1 game", string.Empty))
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
}
