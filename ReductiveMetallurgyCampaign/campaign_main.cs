//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Globalization;
//using System.Reflection;

namespace ReductiveMetallurgyCampaign;

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
using PartTypes = class_191;
//using Texture = class_256;
using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public static partial class CampaignLoader
{
	const string rejectionTutorialID = "rmc-ch1-practical-test";
	const string polymerInputTutorialID = "rmc-ch3-synthesis-via-chain";
	const string oldPolymerInputTutorialID = "rmc-j01-golden-thread-recycling";

	private static Campaign campaign_self;

	public const enum_129 typePuzzle = (enum_129)0;
	public const enum_129 typeCutscene = (enum_129)1;
	public const enum_129 typeDocument = (enum_129)2;
	public const enum_129 typeSolitaire = (enum_129)3;

	public static bool currentCampaignIsRMC() => campaign_self == Campaigns.field_2330;

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
	public static void Load()
	{
		// hooking
		On.Solution.method_1958 += Solution_Method_1958;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// tips
	static void initializeTips()
	{
		// manually load the puzzle file needed for tips
		string subpath = "/Puzzles/rmc-sandbox.puzzle.yaml";
		string filepath;
		if (!MainClass.findModMetaFilepath("ReductiveMetallurgyCampaign", out filepath) || !File.Exists(filepath + subpath))
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find 'rmc-sandbox.puzzle.yaml' in the folder '" + filepath + "/Puzzles/'");
			throw new Exception("LoadPuzzleContent: Tip data is missing.");
		}
		var tipsPuzzle = PuzzleModel.FromModel(YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(filepath + subpath)));

		Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
		Puzzles.field_2816[Puzzles.field_2816.Length - 1] = tipsPuzzle;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// puzzle-loader functions
	static Dictionary<string, Action<Puzzle>> LevelLoaders = new ()
	{
		{rejectionTutorialID, LoadRejectionTutorialPuzzle },
		{polymerInputTutorialID, LoadPolymerInputPuzzle },
		{oldPolymerInputTutorialID, LoadOldPolymerInputPuzzle },
	};

	static void LoadRejectionTutorialPuzzle(Puzzle puzzle) => StoryPanelPatcher.setOptionsUnlock(puzzle);

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// main functions

	public static void modifyCampaign()
	{
		// initialize tip resources
		initializeTips();

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

		StoryPanelPatcher.CreateSigmarStoryUnlocks(MainClass.AdvancedContent.SigmarStoryUnlocks);

		////////////////////////////////////////
		// modify the campaign using the data //
		////////////////////////////////////////
		Logger.Log("[ReductiveMetallurgyCampaign] Modifying campaign levels.");
		CampaignChapter[] campaignChapters = campaign_self.field_2309;
		foreach (var campaignChapter in campaignChapters)
		{
			if (MainClass.AdvancedContent.LeftHandedChapters.Contains(campaignChapter.field_2310)) campaignChapter.field_2321 = true;

			foreach (var campaignItem in campaignChapter.field_2314)
			{
				// convert unlock requirement as necessary
				if (campaignItem.field_2326.GetType() == typeof(class_243))
				{
					class_243 stringUnlockRequirement = (class_243)campaignItem.field_2326;
					if (int.TryParse(stringUnlockRequirement.field_2005, out int n))
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
					if (LevelLoaders.ContainsKey(puzzleID)) LevelLoaders[puzzleID](puzzle);

					// modify using stuff in advanced.yaml
					MainClass.AdvancedContent.modifyCampaignItem(campaignItem);
				}
			}
		}

		JournalLoader.modifyJournals(campaign_self);
	}

	public static Maybe<Solution> Solution_Method_1958(On.Solution.orig_method_1958 orig, string filePath)
	{
		foreach (var dir in QuintessentialLoader.ModContentDirectories)
		{
			try
			{
				return orig(Path.Combine(dir, filePath));
			}
			catch (Exception) { }
		}

		return orig(filePath);
	}
}