using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
using SDL2;
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
//using PartTypes = class_191;
//using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public static class SigmarGardenPatcher
{
	private static int sigmarWins_RMC = 0;
	private static bool currentCampaignIsRMC = false;
	private static void setSigmarWins_RMC() => GameLogic.field_2434.field_2451.field_1929.method_858("RMC-SigmarWins", sigmarWins_RMC.method_453());
	private static void getSigmarWins_RMC() { sigmarWins_RMC = GameLogic.field_2434.field_2451.field_1929.method_862<int>(new delegate_384<int>(int.TryParse), "RMC-SigmarWins").method_1090(0); }

	public static void Load()
	{
		On.CampaignItem.method_825 += CampaignItem_Method_825;
		On.SolitaireGameState.method_1885 += SolitaireGameState_Method_1885;
		On.SolitaireScreen.method_50 += SolitaireScreen_Method_50;
		getSigmarWins_RMC();
	}
	public static bool CampaignItem_Method_825(On.CampaignItem.orig_method_825 orig, CampaignItem item_self)
	{
		bool ret = orig(item_self);

		if (currentCampaignIsRMC) ret = ret || (item_self.field_2324 == (enum_129)3 && sigmarWins_RMC > 0);

		return ret;
	}

	public static bool SolitaireGameState_Method_1885(On.SolitaireGameState.orig_method_1885 orig, SolitaireGameState state_self)
	{
		bool ret = orig(state_self);
		if (ret && currentCampaignIsRMC) sigmarWins_RMC++;
		setSigmarWins_RMC();
		return ret;
	}
	public static void SolitaireScreen_Method_50(On.SolitaireScreen.orig_method_50 orig, SolitaireScreen screen_self, float timeDelta)
	{

		currentCampaignIsRMC = MainClass.campaign_self == Campaigns.field_2330;

		if (currentCampaignIsRMC)
		{
			var screen_dyn = new DynamicData(screen_self);
			screen_dyn.Set("field_3871", sigmarWins_RMC);
			var currentStoryPanel = screen_dyn.Get<StoryPanel>("field_3872");
			var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("field_4093");
			if (!stringArray.Any(x => x.Contains("Saverio") || x.Contains("Pugano")))
			{
				currentCampaignIsRMC = true;
				var class264 = new class_264("solitaire-rmc");
				class264.field_2090 = "solitaire";
				screen_dyn.Set("field_3872", new StoryPanel((Maybe<class_264>)class264, true));
			}
		}



		orig(screen_self, timeDelta);
	}






}