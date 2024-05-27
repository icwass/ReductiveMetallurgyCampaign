//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
//using System.Linq;
using System.Collections.Generic;
//using System.Globalization;
//using System.Reflection;

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

public static partial class CampaignLoader
{
	const string rejectionTutorialID = "rmc-ch1-practical-test";
	private static Campaign campaign_self;

	public const enum_129 typePuzzle = (enum_129)0;
	public const enum_129 typeCutscene = (enum_129)1;
	public const enum_129 typeDocument = (enum_129)2;
	public const enum_129 typeSolitaire = (enum_129)3;


	/////////////////////////////////////////////////////////////////////////////////////////////////
	// helpers
	public static bool currentCampaignIsRMC() => campaign_self == Campaigns.field_2330;
	public static void Load()
	{
		// hooking
		On.Solution.method_1958 += LookThroughModDirectoriesForTipSolutionFiles;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// main functions

	public static void modifyCampaign()
	{
		// find THIS campaign
		foreach (Campaign campaign in QuintessentialLoader.AllCampaigns)
		{
			if (campaign.QuintTitle == "Reductive Metallurgy")
			{
				campaign_self = campaign;
				break;
			}
		}

		// modify the campaign
		Logger.Log("[ReductiveMetallurgyCampaign] Modifying campaign items.");
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
					if (puzzleID == rejectionTutorialID) StoryPanelPatcher.setOptionsUnlock(puzzle);

					// modify using stuff in advanced.yaml
					MainClass.AdvancedContent.modifyCampaignItem(campaignItem);
				}
			}
		}
	}

	public static Maybe<Solution> LookThroughModDirectoriesForTipSolutionFiles(On.Solution.orig_method_1958 orig, string filePath)
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