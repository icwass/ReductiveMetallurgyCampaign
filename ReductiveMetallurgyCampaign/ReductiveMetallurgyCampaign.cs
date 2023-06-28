using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
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
	static Texture return_button, return_button_hover;

	private static bool findModMetaFilepath(string name, out string filepath)
	{
		filepath = "";
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
			field_1901 = class_134.method_253("The *glyph of deposition* transmutes one atom of metal into two atoms of a lower form.\n\nNote that the resulting metals need not be of the next lower form, nor need they be the same metal.", string.Empty),
			field_1902 = "RMCdeposition",
			field_1904 = new Vector2(0.0f, -40f)
		};

		Logger.Log("[ReductiveMetallurgyCampaign] Modifying campaign levels.");
		CampaignChapter[] field2309 = campaign_self.field_2309;
		foreach (var campaignChapter in field2309)
		{
			if (campaignChapter.field_2310 == 1) campaignChapter.field_2321 = true;
			foreach (var campaignItem in campaignChapter.field_2314)
			{
				if (campaignItem.field_2325.method_1085())
				{
					Puzzle puzzle = campaignItem.field_2325.method_1087();

					if (puzzle.field_2766 == "rmc-lesson-rejection")
					{
						puzzle.field_2769 = tipRejection;
					}
					else if (puzzle.field_2766 == "rmc-lesson-deposition")
					{
						puzzle.field_2769 = tipDeposition;
					}
					else if (puzzle.field_2766 == "rmc-golden-thread-recycling")
					{
						puzzle.field_2769 = tipPolymerInput;
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
					else if (puzzle.field_2766 == "rmc-synthesis-via-chain")
					{
						puzzle.field_2769 = tipPolymerInput;
						HexIndex hexIndex1 = new HexIndex(1, 0);
						List<class_157> class157List = new List<class_157>();
						HexIndex[] hexIndexArray = new HexIndex[]
						{
							new HexIndex(0, 0),
							new HexIndex(4, 0),
							new HexIndex(8, 0)
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
							field_1598 = hexIndex1 + new HexIndex(12, -2),
							field_1602 = new HexIndex[]
							{
								hexIndex1 + new HexIndex(12, -2),
								hexIndex1 + new HexIndex(11, -2),
								hexIndex1 + new HexIndex(10, -2),
								hexIndex1 + new HexIndex(9, -2),
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
								hexIndex1 + new HexIndex(6, 2),
								hexIndex1 + new HexIndex(7, 2),
								hexIndex1 + new HexIndex(8, 2),
								hexIndex1 + new HexIndex(9, 2),
								hexIndex1 + new HexIndex(10, 2)
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

		string path = "textures/story/";
		return_button = class_235.method_615(path + "return_button_rmc");
		return_button_hover = class_235.method_615(path + "return_button_hover_rmc");

		//------------------------- HOOKING -------------------------//
		hook_Sim_method_1835 = new Hook(
		typeof(Sim).GetMethod("method_1835", BindingFlags.Instance | BindingFlags.NonPublic),
		typeof(MainClass).GetMethod("OnSimMethod1835", BindingFlags.Static | BindingFlags.NonPublic)
		);
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
		Document.Load();
		BoardEditorScreen.Load();
		CutscenePatcher.Load();
		On.class_172.method_480 += new On.class_172.hook_method_480(Class172_Method_480);
		On.Solution.method_1958 += Solution_Method_1958;
		On.class_135.method_272 += Class135_Method_272;
	}

	public static void Class135_Method_272(On.class_135.orig_method_272 orig, Texture texture, Vector2 position)
	{
		if (MainClass.campaign_self == Campaigns.field_2330)
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
		if (filePath == "Content\\tips\\RMCrejection.solution" || filePath == "Content\\tips\\RMCdeposition.solution")
		{
			foreach (var dir in QuintessentialLoader.ModContentDirectories)
			{
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
		class_172.field_1670.Add("Verrin Ravari (Frustrated)", new class_230(class_134.method_253("Verrin Ravari", string.Empty), null, class_235.method_615("portraits/verrin_frustrated_small"), Color.FromHex(6691857), false));
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
	}
}
