using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ReductiveMetallurgyCampaign;

using PartTypes = class_191;
using Texture = class_256;
using Song = class_186;
using Tip = class_215;

public class PuzzleModelRMC
{
	public string ID, Music;
	public CabinetModelRMC Cabinet;
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
public class CabinetModelRMC
{
	public bool ExpandLeft, ExpandRight;
	public List<VialHolderModelRMC> VialHolders;
}

public class VialHolderModelRMC
{
	public string Position;
	public bool TopSide;
	public List<VialModelRMC> Vials;
}
public class VialModelRMC
{
	public string TextureSim, TextureGif;
}

public static class CampaignLoader
{
	const string rejectionTutorialID = "rmc-practical-test"; //"rmc-lesson-rejection";
	const string depositionTutorialID = "rmc-metal-deposition"; //"rmc-lesson-deposition";
	const string proliferationTutorialID = "rmc-ravari-renewal"; //"rmc-lesson-proliferation";
	const string ravariTutorialID_1 = "rmc-ravari-requiescence";
	const string ravariTutorialID_2 = "rmc-energetic-capacitor";
	const string polymerInputTutorialID = "rmc-synthesis-via-chain";
	const string oldPolymerInputTutorialID = "rmc-golden-thread-recycling";

	private static Campaign campaign_self;
	private static CampaignModelRMC campaign_model;

	public const enum_129 typePuzzle = (enum_129)0;
	public const enum_129 typeCutscene = (enum_129)1;
	public const enum_129 typeDocument = (enum_129)2;
	public const enum_129 typeSolitaire = (enum_129)3;




	public static bool currentCampaignIsRMC() => campaign_self == Campaigns.field_2330;
	public static CampaignModelRMC getModel() => campaign_model;

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// helpers
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
	public static void loadCampaignModel()
	{
		string filepath;
		if (!MainClass.findModMetaFilepath("ReductiveMetallurgyCampaign", out filepath) || !File.Exists(filepath + "/Puzzles/RMC.advanced.yaml"))
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find 'RMC.advanced.yaml' in the folder '" + filepath + "\\Puzzles\\'");
			throw new Exception("modifyCampaignRMC: Campaign data is missing.");
		}
		using (StreamReader streamReader = new StreamReader(filepath + "/Puzzles/RMC.advanced.yaml"))
		{
			campaign_model = YamlHelper.Deserializer.Deserialize<CampaignModelRMC>(streamReader);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// tips
	static Dictionary<string, Tip> tipDictionary = new();
	private static HashSet<string> TipPaths = new();
	static void initializeTips()
	{
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
			field_1901 = class_134.method_253("By using *Ravari's wheel* with the *glyph of proliferation*, quicksilver can be exchanged for any of the six planetary metals.\n\nOur current understanding of metallurgic theory will be completely revolutionized, once it is determined how this exchange actually occurs.", string.Empty),
			field_1902 = "RMCproliferation",
			field_1904 = new Vector2(0f, -40f)
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

		tipDictionary = new()
		{
			{rejectionTutorialID, tipRejection },
			{depositionTutorialID, tipDeposition },
			{proliferationTutorialID, tipProliferation },
			{ravariTutorialID_1, tipRavari1 },
			{ravariTutorialID_2, tipRavari2 },
			{oldPolymerInputTutorialID, tipPolymerInput },
			{polymerInputTutorialID, tipPolymerInput },
		};

		foreach (var tip in tipDictionary.Values)
		{
			TipPaths.Add("Content\\tips\\" + tip.field_1902 + ".solution");
		}

		// manually load the puzzle file needed for tips
		string subpath = "/Puzzles/rmc-sandbox.puzzle.yaml";
		string filepath;
		if (!MainClass.findModMetaFilepath("ReductiveMetallurgyCampaign", out filepath) || !File.Exists(filepath + subpath))
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find 'rmc-sandbox.puzzle.yaml' in the folder '" + filepath + "\\Puzzles\\'");
			throw new Exception("LoadPuzzleContent: Tip data is missing.");
		}
		var tipsPuzzle = PuzzleModel.FromModel(YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(filepath + subpath)));

		Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
		Puzzles.field_2816[Puzzles.field_2816.Length - 1] = tipsPuzzle;

		// manually load the journal puzzles
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// puzzle-loader functions
	static Dictionary<string, Action<Puzzle>> LevelLoaders = new Dictionary<string, Action<Puzzle>>()
	{
		{rejectionTutorialID, LoadRejectionTutorialPuzzle },
		{polymerInputTutorialID, LoadPolymerInputPuzzle },
		{oldPolymerInputTutorialID, LoadOldPolymerInputPuzzle },
	};

	static void LoadRejectionTutorialPuzzle(Puzzle puzzle) => StoryPanelPatcher.setOptionsUnlock(puzzle);

	#region polymer input/output puzzle loaders

	static void LoadPolymerOutputs(Puzzle puzzle) // REMOVE THIS ONCE QUINTESSENTIAL FIXES THE BUG
	{
		for (int i = 0; i < puzzle.field_2771.Length; i++)
		{
			var output = puzzle.field_2771[i];
			output.field_2813 = MoleculeEditorScreen.method_1133(output.field_2813, class_181.field_1716);
		}
	}

	static void LoadPolymerInputPuzzle(Puzzle puzzle)
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
	static void LoadOldPolymerInputPuzzle(Puzzle puzzle)
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
	#endregion

	#region vial textures
	static Dictionary<string, Texture> vialTextureBank;
	static Texture fetchVialTexture(string filePath)
	{
		Texture ret = class_238.field_1989.field_92.field_401.field_418.field_501; // generic
		if (string.IsNullOrEmpty(filePath)) return ret;
		if (vialTextureBank.ContainsKey(filePath)) return vialTextureBank[filePath];
		try
		{
			ret = class_235.method_615(filePath);
		}
		catch
		{
			Logger.Log("[ReductiveMetallurgyCampaign] fetchVialTexture: Couldn't load '" + filePath + ".png', will use the generic vial texture instead.");
		}
		vialTextureBank.Add(filePath, ret);
		return ret;
	}

	static void initializeVialTextureBank()
	{
		vialTextureBank = new()
		{
			{"textures/pipelines/vials/abrasive_powder/empty",                  class_238.field_1989.field_92.field_401.field_408.field_444},
			{"textures/pipelines/vials/abrasive_powder/filling",                class_238.field_1989.field_92.field_401.field_408.field_445},
			{"textures/pipelines/vials/aether_detector/draining",               class_238.field_1989.field_92.field_401.field_409.field_456},
			{"textures/pipelines/vials/aether_detector/empty",                  class_238.field_1989.field_92.field_401.field_409.field_457},
			{"textures/pipelines/vials/aether_detector/filling",                class_238.field_1989.field_92.field_401.field_409.field_458},
			{"textures/pipelines/vials/aether_detector/full",                   class_238.field_1989.field_92.field_401.field_409.field_459},
			{"textures/pipelines/vials/conductive_enamel/empty",                class_238.field_1989.field_92.field_401.field_410.field_462},
			{"textures/pipelines/vials/conductive_enamel/filling",              class_238.field_1989.field_92.field_401.field_410.field_463},
			{"textures/pipelines/vials/elemental_iron/draining",                class_238.field_1989.field_92.field_401.field_411.field_483},
			{"textures/pipelines/vials/elemental_iron/full",                    class_238.field_1989.field_92.field_401.field_411.field_484},
			{"textures/pipelines/vials/elemental_lead/draining",                class_238.field_1989.field_92.field_401.field_412.field_454},
			{"textures/pipelines/vials/elemental_lead/full",                    class_238.field_1989.field_92.field_401.field_412.field_455},
			{"textures/pipelines/vials/elemental_quicksilver/draining",         class_238.field_1989.field_92.field_401.field_413.field_464},
			{"textures/pipelines/vials/elemental_quicksilver/full",             class_238.field_1989.field_92.field_401.field_413.field_465},
			{"textures/pipelines/vials/elemental_salt/draining",                class_238.field_1989.field_92.field_401.field_414.field_502},
			{"textures/pipelines/vials/elemental_salt/full",                    class_238.field_1989.field_92.field_401.field_414.field_503},
			{"textures/pipelines/vials/elemental_tin/draining",                 class_238.field_1989.field_92.field_401.field_415.field_446},
			{"textures/pipelines/vials/elemental_tin/full",                     class_238.field_1989.field_92.field_401.field_415.field_447},
			{"textures/pipelines/vials/eyedrops/mors_empty",                    class_238.field_1989.field_92.field_401.field_416.field_485},
			{"textures/pipelines/vials/eyedrops/mors_filling",                  class_238.field_1989.field_92.field_401.field_416.field_486},
			{"textures/pipelines/vials/eyedrops/vitae_empty",                   class_238.field_1989.field_92.field_401.field_416.field_487},
			{"textures/pipelines/vials/eyedrops/vitae_filling",                 class_238.field_1989.field_92.field_401.field_416.field_488},
			{"textures/pipelines/vials/fragrant_powder/brook_empty",            class_238.field_1989.field_92.field_401.field_417.field_495},
			{"textures/pipelines/vials/fragrant_powder/brook_filling",          class_238.field_1989.field_92.field_401.field_417.field_496},
			{"textures/pipelines/vials/fragrant_powder/forest_empty",           class_238.field_1989.field_92.field_401.field_417.field_497},
			{"textures/pipelines/vials/fragrant_powder/forest_filling",         class_238.field_1989.field_92.field_401.field_417.field_498},
			{"textures/pipelines/vials/fragrant_powder/meadow_empty",           class_238.field_1989.field_92.field_401.field_417.field_499},
			{"textures/pipelines/vials/fragrant_powder/meadow_filling",         class_238.field_1989.field_92.field_401.field_417.field_500},
			{"textures/pipelines/vials/generic/empty",                          class_238.field_1989.field_92.field_401.field_418.field_501},
			{"textures/pipelines/vials/hexstabilized_salt/empty",               class_238.field_1989.field_92.field_401.field_419.field_491},
			{"textures/pipelines/vials/hexstabilized_salt/filling",             class_238.field_1989.field_92.field_401.field_419.field_492},
			{"textures/pipelines/vials/hydrated_hexstabilized_salt/draining",   class_238.field_1989.field_92.field_401.field_420.field_468},
			{"textures/pipelines/vials/hydrated_hexstabilized_salt/full",       class_238.field_1989.field_92.field_401.field_420.field_469},
			{"textures/pipelines/vials/lamplight_gas/empty",                    class_238.field_1989.field_92.field_401.field_421.field_474},
			{"textures/pipelines/vials/lamplight_gas_precursor/full",           class_238.field_1989.field_92.field_401.field_422.field_477},
			{"textures/pipelines/vials/lustre/empty",                           class_238.field_1989.field_92.field_401.field_423.field_460},
			{"textures/pipelines/vials/lustre/filling",                         class_238.field_1989.field_92.field_401.field_423.field_461},
			{"textures/pipelines/vials/metallic_cinnabar/draining",             class_238.field_1989.field_92.field_401.field_424.field_507},
			{"textures/pipelines/vials/metallic_cinnabar/full",                 class_238.field_1989.field_92.field_401.field_424.field_508},
			{"textures/pipelines/vials/rat_poison/empty",                       class_238.field_1989.field_92.field_401.field_425.field_448},
			{"textures/pipelines/vials/rat_poison/filling",                     class_238.field_1989.field_92.field_401.field_425.field_449},
			{"textures/pipelines/vials/reactive_earth/draining",                class_238.field_1989.field_92.field_401.field_426.field_504},
			{"textures/pipelines/vials/reactive_earth/full",                    class_238.field_1989.field_92.field_401.field_426.field_505},
			{"textures/pipelines/vials/refined_hematite/draining",              class_238.field_1989.field_92.field_401.field_427.field_452},
			{"textures/pipelines/vials/refined_hematite/full",                  class_238.field_1989.field_92.field_401.field_427.field_453},
			{"textures/pipelines/vials/rocket_fuel/empty",                      class_238.field_1989.field_92.field_401.field_428.field_478},
			{"textures/pipelines/vials/rocket_fuel/filling",                    class_238.field_1989.field_92.field_401.field_428.field_479},
			{"textures/pipelines/vials/silver_paint/empty",                     class_238.field_1989.field_92.field_401.field_429.field_442},
			{"textures/pipelines/vials/silver_paint/filling",                   class_238.field_1989.field_92.field_401.field_429.field_443},
			{"textures/pipelines/vials/solvent/empty",                          class_238.field_1989.field_92.field_401.field_430.field_466},
			{"textures/pipelines/vials/solvent/filling",                        class_238.field_1989.field_92.field_401.field_430.field_467},
			{"textures/pipelines/vials/special_amaro/draining",                 class_238.field_1989.field_92.field_401.field_431.field_470},
			{"textures/pipelines/vials/special_amaro/empty",                    class_238.field_1989.field_92.field_401.field_431.field_471},
			{"textures/pipelines/vials/special_amaro/filling",                  class_238.field_1989.field_92.field_401.field_431.field_472},
			{"textures/pipelines/vials/special_amaro/full",                     class_238.field_1989.field_92.field_401.field_431.field_473},
			{"textures/pipelines/vials/stabilized_air/full",                    class_238.field_1989.field_92.field_401.field_432.field_506},
			{"textures/pipelines/vials/stabilized_earth/draining",              class_238.field_1989.field_92.field_401.field_433.field_489},
			{"textures/pipelines/vials/stabilized_earth/full",                  class_238.field_1989.field_92.field_401.field_433.field_490},
			{"textures/pipelines/vials/stabilized_fire/draining",               class_238.field_1989.field_92.field_401.field_434.field_481},
			{"textures/pipelines/vials/stabilized_fire/full",                   class_238.field_1989.field_92.field_401.field_434.field_482},
			{"textures/pipelines/vials/stabilized_water/draining",              class_238.field_1989.field_92.field_401.field_435.field_440},
			{"textures/pipelines/vials/stabilized_water/full",                  class_238.field_1989.field_92.field_401.field_435.field_441},
			{"textures/pipelines/vials/tinstone/draining",                      class_238.field_1989.field_92.field_401.field_436.field_475},
			{"textures/pipelines/vials/tinstone/full",                          class_238.field_1989.field_92.field_401.field_436.field_476},
			{"textures/pipelines/vials/vapor_of_levity/empty",                  class_238.field_1989.field_92.field_401.field_437.field_480},
			{"textures/pipelines/vials/viscous_sludge/empty",                   class_238.field_1989.field_92.field_401.field_438.field_450},
			{"textures/pipelines/vials/viscous_sludge/filling",                 class_238.field_1989.field_92.field_401.field_438.field_451},
			{"textures/pipelines/vials/welding_thermite/empty",                 class_238.field_1989.field_92.field_401.field_439.field_493},
			{"textures/pipelines/vials/welding_thermite/filling",               class_238.field_1989.field_92.field_401.field_439.field_494},
		};
	}
	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// main functions

	public static void modifyCampaign()
	{
		// get song resources
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

		// initialize tip and vial resources
		initializeTips();
		initializeVialTextureBank();

		// fetch campaign data
		foreach (Campaign campaign in QuintessentialLoader.AllCampaigns)
		{
			if (campaign.QuintTitle == "Reductive Metallurgy")
			{
				campaign_self = campaign;
				patchCampaign(campaign_self);
				break;
			}
		}
		Dictionary<string, PuzzleModelRMC> puzzleDictionary = new Dictionary<string, PuzzleModelRMC>();
		List<string> documentIDList = new List<string>();
		Dictionary<string, string> cutsceneIDList = new Dictionary<string, string>();
		List<string> sigmarsGardensIDList = new List<string>();
		foreach (PuzzleModelRMC puzzle in campaign_model.Puzzles)
		{
			puzzleDictionary.Add(puzzle.ID, puzzle);
		}
		var DocumentModels = campaign_model.Documents;
		Document.LoadDocuments(DocumentModels);
		foreach (var document in DocumentModels)
		{
			documentIDList.Add(document.ID);
		}
		var CutsceneModels = campaign_model.Cutscenes;
		CutscenePatcher.LoadCutscenes(campaign_model.Cutscenes);
		foreach (var cutscene in campaign_model.Cutscenes)
		{
			bool blank = string.IsNullOrEmpty(cutscene.Music);
			cutsceneIDList.Add(cutscene.ID, blank ? "Story1" : cutscene.Music);
		}
		foreach (var garden in campaign_model.SigmarsGardens)
		{
			sigmarsGardensIDList.Add(garden.ID);
		}
		////////////////////////////////////////
		// modify the campaign using the data //
		////////////////////////////////////////
		Logger.Log("[ReductiveMetallurgyCampaign] Modifying campaign levels.");
		CampaignChapter[] campaignChapters = campaign_self.field_2309;
		foreach (var campaignChapter in campaignChapters)
		{
			if (campaignChapter.field_2310 == 1) campaignChapter.field_2321 = true;

			foreach (var campaignItem in campaignChapter.field_2314)
			{
				// convert unlock requirement as necessary
				if (campaignItem.field_2326.GetType() == typeof(class_243))
				{
					class_243 stringUnlockRequirement = (class_243)campaignItem.field_2326;
					int n;
					if (int.TryParse(stringUnlockRequirement.field_2005, out n))
					{
						campaignItem.field_2326 = new class_265(n);
					}
				}

				// modifiy puzzle data as necessary
				if (campaignItem.field_2325.method_1085())
				{
					Puzzle puzzle = campaignItem.field_2325.method_1087();
					string puzzleID = puzzle.field_2766;


					// run hard-coded stuff
					if (tipDictionary.ContainsKey(puzzleID)) puzzle.field_2769 = tipDictionary[puzzleID];
					if (LevelLoaders.ContainsKey(puzzleID)) LevelLoaders[puzzleID](puzzle);

					// modify using stuff in advanced.yaml

					//campaignItem.field_2324 defaults to (enum_129)0, i.e. a puzzle
					if (cutsceneIDList.Keys.Contains(puzzleID))
					{
						// change item into a cutscene
						campaignItem.field_2324 = typeCutscene;
						campaignItem.field_2328 = songList[cutsceneIDList[puzzleID]];
					}
					else if (documentIDList.Contains(puzzleID))
					{
						// change item into a document
						campaignItem.field_2324 = typeDocument;
					}
					else if (sigmarsGardensIDList.Contains(puzzleID))
					{
						// change item into a Sigmars Garden
						campaignItem.field_2324 = typeSolitaire;
						MainClass.customSolitaires.Add(campaignItem.field_2326); // SOLITAIRE_ICON_TEMP
					}
					else // keep it as a puzzle
					{
						if (puzzleDictionary.ContainsKey(puzzleID))
						{
							LoadPolymerOutputs(puzzle);
							PuzzleModelRMC puzzleModel = puzzleDictionary[puzzleID];

							if (!string.IsNullOrEmpty(puzzleModel.Music) && songList.Keys.Contains(puzzleModel.Music))
							{
								campaignItem.field_2328 = songList[puzzleModel.Music];
								campaignItem.field_2329 = fanfareList[puzzleModel.Music];
							}

							if (puzzleModel.Cabinet != null)
							{
								if (!puzzle.field_2779.method_1085())
								{
									Logger.Log("[ReductiveMetallurgyCampaign] Puzzle '" + puzzleID + "' is not a production puzzle - ignoring the cabinet data.");
								}
								else
								{
									var cabinet = puzzleModel.Cabinet;
									var productionData = puzzle.field_2779.method_1087();
									productionData.field_2075 = !cabinet.ExpandLeft;
									productionData.field_2076 = !cabinet.ExpandRight;
									productionData.field_2073 = new class_128[cabinet.VialHolders.Count];
									for (int i = 0; i < cabinet.VialHolders.Count; i++)
									{
										var holder = cabinet.VialHolders[i];
										var vials = new Tuple<Texture, Texture>[holder.Vials.Count];
										for (int j = 0; j < holder.Vials.Count; j++)
										{
											var vial = holder.Vials[j];
											vials[j] = Tuple.Create(fetchVialTexture(vial.TextureSim), fetchVialTexture(vial.TextureGif));
										}
										productionData.field_2073[i] = new class_128(int.Parse(holder.Position.Split(',')[0]), int.Parse(holder.Position.Split(',')[1]), holder.TopSide, vials);
									}
									// fix and update the cabinet bounding box
									puzzle.method_1247();
								}
							}
						}
					}
				}
			}
		}

		JournalLoader.modifyJournals(campaign_self);
	}

	public static Maybe<Solution> Solution_Method_1958(On.Solution.orig_method_1958 orig, string filePath)
	{
		if (TipPaths.Contains(filePath))
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
}