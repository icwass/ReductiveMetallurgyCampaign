﻿//using Mono.Cecil.Cil;
//using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
//using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
using System.Linq;
//using System.Collections.Generic;
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
using Font = class_1;

public static partial class SigmarGardenPatcher
{
	private static IDetour hook_SolitaireScreen_method_1889;
	private static IDetour hook_SolitaireScreen_method_1890;
	private static IDetour hook_SolitaireScreen_method_1893;
	private static IDetour hook_SolitaireScreen_method_1894;

	public static bool DisplayMetalsRemaining = true;
	public const string solitaireID = "rmc-solitaire";

	private static int sigmarWins_RMC = 0;
	public static AtomType nullAtom;
	public static SolitaireState solitaireState_RMC;

	private static bool isQuintessenceSigmarGarden(SolitaireScreen screen) => new DynamicData(screen).Get<bool>("field_3874");
	private static bool currentCampaignIsRMC(SolitaireScreen screen) => CampaignLoader.currentCampaignIsRMC() && !isQuintessenceSigmarGarden(screen);
	private static void setSigmarWins_RMC() => GameLogic.field_2434.field_2451.field_1929.method_858("RMC-SigmarWins", sigmarWins_RMC.method_453());
	private static void getSigmarWins_RMC() { sigmarWins_RMC = GameLogic.field_2434.field_2451.field_1929.method_862<int>(new delegate_384<int>(int.TryParse), "RMC-SigmarWins").method_1090(0); }
	public static AtomType getAtomType(int i)
	{
		return new AtomType[17]
		{
			SigmarGardenPatcher.nullAtom, // 00 - filler
			class_175.field_1681, // 01 - lead
			class_175.field_1683, // 02 - tin
			class_175.field_1684, // 03 - iron
			class_175.field_1682, // 04 - copper
			class_175.field_1685, // 05 - silver
			class_175.field_1686, // 06 - gold
			class_175.field_1680, // 07 - quicksilver
			class_175.field_1687, // 08 - vitae
			class_175.field_1688, // 09 - mors
			class_175.field_1675, // 10 - salt
			class_175.field_1676, // 11 - air
			class_175.field_1679, // 12 - water
			class_175.field_1678, // 13 - fire
			class_175.field_1677, // 14 - earth
			class_175.field_1689, // 15 - repeat
			class_175.field_1690, // 16 - quintessence
		}[i];
	}
	public static void PostLoad()
	{
		getSigmarWins_RMC();
		On.CampaignItem.method_825 += DetermineIfCampaignItemIsCompleted;
		On.SolitaireGameState.class_301.method_1888 += DetermineIfMatchIsValid;
		On.SolitaireGameState.method_1885 += DetermineIfSolitaireGameWasWon;
		On.SolitaireScreen.method_50 += SolitaireScreen_Method_50;
		On.class_16.method_50 += SolitaireRulesScreen_Method_50;
		On.class_198.method_537 += getRandomizedSolitaireBoard;

		nullAtom = new AtomType()
		{
			field_2283 = (byte)0,
			field_2284 = (string)class_134.method_254("Null"),
			field_2285 = class_134.method_253("Elemental Null", string.Empty),
			field_2286 = class_134.method_253("Null", string.Empty),
			field_2287 = class_238.field_1989.field_81.field_598,
			field_2288 = class_238.field_1989.field_81.field_599,
			field_2290 = new class_106()
			{
				field_994 = class_238.field_1989.field_81.field_596,
				field_995 = class_238.field_1989.field_81.field_597
			}
		};

		hook_SolitaireScreen_method_1889 = new Hook(MainClass.PrivateMethod<SolitaireScreen> ("method_1889"), OnSolitaireScreen_Method_1889);
		hook_SolitaireScreen_method_1890 = new Hook(MainClass.PrivateMethod<SolitaireScreen>("method_1890"), OnSolitaireScreen_Method_1890);
		hook_SolitaireScreen_method_1893 = new Hook(MainClass.PrivateMethod<SolitaireScreen>("method_1893"), OnSolitaireScreen_Method_1893);
		hook_SolitaireScreen_method_1894 = new Hook(MainClass.PrivateMethod<SolitaireScreen>("method_1894"), OnSolitaireScreen_Method_1894);
	}
	private delegate SolitaireState orig_SolitaireScreen_method_1889(SolitaireScreen self);
	private delegate void orig_SolitaireScreen_method_1890(SolitaireScreen self, SolitaireState param_5433);
	private delegate bool orig_SolitaireScreen_method_1893(SolitaireScreen self);
	private delegate bool orig_SolitaireScreen_method_1894(SolitaireScreen self);
	private static SolitaireState OnSolitaireScreen_Method_1889(orig_SolitaireScreen_method_1889 orig, SolitaireScreen screen_self)
	{
		if (currentCampaignIsRMC(screen_self)) return solitaireState_RMC;
		return orig(screen_self);
	}
	private static void OnSolitaireScreen_Method_1890(orig_SolitaireScreen_method_1890 orig, SolitaireScreen screen_self, SolitaireState param_5433)
	{
		if (currentCampaignIsRMC(screen_self))
		{
			solitaireState_RMC = param_5433;
			return;
		}
		orig(screen_self, param_5433);
	}
	private static bool OnSolitaireScreen_Method_1893(orig_SolitaireScreen_method_1894 orig, SolitaireScreen screen_self)
	{
		// used to show the rules button
		if (currentCampaignIsRMC(screen_self))
		{
			var state = (SolitaireState)MainClass.PrivateMethod<SolitaireScreen>("method_1889").Invoke(screen_self, new object[] { });
			return new DynamicData(screen_self).Get<StoryPanel>("field_3872").method_2170() >= 8;
		}
		return orig(screen_self);
	}
	private static bool OnSolitaireScreen_Method_1894(orig_SolitaireScreen_method_1894 orig, SolitaireScreen screen_self)
	{
		// used to enable the NEW GAME button
		if (currentCampaignIsRMC(screen_self))
		{
			var state = (SolitaireState)MainClass.PrivateMethod<SolitaireScreen>("method_1889").Invoke(screen_self, new object[] { });
			return new DynamicData(screen_self).Get<StoryPanel>("field_3872").method_2170() >= 1 && !state.method_1922();
		}
		return orig(screen_self);
	}

	public static void Unload()
	{
		hook_SolitaireScreen_method_1889.Dispose();
		hook_SolitaireScreen_method_1890.Dispose();
		hook_SolitaireScreen_method_1893.Dispose();
		hook_SolitaireScreen_method_1894.Dispose();
	}

	public static bool DetermineIfCampaignItemIsCompleted(On.CampaignItem.orig_method_825 orig, CampaignItem item_self)
	{
		bool ret = orig(item_self);
		if (CampaignLoader.currentCampaignIsRMC())
			ret = ret || (item_self.field_2324 == CampaignLoader.typeSolitaire && sigmarWins_RMC > 0);
		return ret;
	}

	public static bool DetermineIfMatchIsValid(On.SolitaireGameState.class_301.orig_method_1888 orig, SolitaireGameState.class_301 class301_self, AtomType param_5430, AtomType param_5431)
	{
		bool atomTypeIsMetal = param_5430.field_2297.method_1085();
		if (CampaignLoader.currentCampaignIsRMC() && param_5430 == param_5431 && atomTypeIsMetal) return true;
		return orig(class301_self, param_5430, param_5431);
	}

	public static bool DetermineIfSolitaireGameWasWon(On.SolitaireGameState.orig_method_1885 orig, SolitaireGameState state_self)
	{
		bool ret = orig(state_self);
		AtomType quintessence = class_175.field_1690;
		if (ret && CampaignLoader.currentCampaignIsRMC() && !state_self.field_3864.ContainsValue(quintessence)) sigmarWins_RMC++;
		setSigmarWins_RMC();
		return ret;
	}

	public static void SolitaireScreen_Method_50(On.SolitaireScreen.orig_method_50 orig, SolitaireScreen screen_self, float timeDelta)
	{
		if (currentCampaignIsRMC(screen_self))
		{
			var screen_dyn = new DynamicData(screen_self);
			screen_dyn.Set("field_3871", sigmarWins_RMC);
			var currentStoryPanel = screen_dyn.Get<StoryPanel>("field_3872");
			var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("field_4093");
			if (!stringArray.Any(x => x.Contains("Saverio") || x.Contains("Pugano")))
			{
				var class264 = new class_264("solitaire-rmc");
				class264.field_2090 = solitaireID;
				screen_dyn.Set("field_3872", new StoryPanel((Maybe<class_264>)class264, true));
			}
		}

		orig(screen_self, timeDelta);

		if (!currentCampaignIsRMC(screen_self)) return;

		//draw atoms remaining for each metal
		SolitaireScreen.class_412 class412 = new SolitaireScreen.class_412();
		class412.field_3883 = screen_self;
		class412.field_3886 = timeDelta;
		if (GameLogic.field_2434.method_938() is class_16) return;

		Vector2 vector2_1 = new Vector2(1516f, 922f);
		class412.field_3884 = (class_115.field_1433 / 2 - vector2_1 / 2 + new Vector2(-2f, -11f)).Rounded();


		int Method_1901(AtomType atomType, Vector2 pos)
		{
			SolitaireScreen.class_413 class413 = new SolitaireScreen.class_413();
			class413.field_3889 = atomType;
			class413.field_3888 = 0;
			class413.field_3890 = 0;

			var class301 = SolitaireScreen.class_301.field_2343;
			void Method_1907(SolitaireState.struct_123 param_5448) => MainClass.PrivateMethod<SolitaireScreen.class_413>("method_1907").Invoke(class413, new object[] { param_5448 });
			void Method_1909(SolitaireState.struct_124 param_5449) => MainClass.PrivateMethod<SolitaireScreen.class_413>("method_1909").Invoke(class413, new object[] { param_5449 });
			void Method_1911(SolitaireState.WaitingForNewGameFields param_5451) => MainClass.PrivateMethod<SolitaireScreen.class_301>("method_1907").Invoke(class301, new object[] { param_5451 });
			void Method_1914(SolitaireState.WonLastGameFields param_5452) => MainClass.PrivateMethod<SolitaireScreen.class_301>("method_1907").Invoke(class301, new object[] { param_5452 });
			var state = (SolitaireState)MainClass.PrivateMethod<SolitaireScreen>("method_1889").Invoke(class412.field_3883, new object[] { });
			state.method_1933(SolitaireScreen.class_301.field_3893 ?? (SolitaireScreen.class_301.field_3893 = new Action<SolitaireState.WaitingForNewGameFields>(Method_1911)), new Action<SolitaireState.struct_123>(Method_1907), new Action<SolitaireState.struct_124>(Method_1909), SolitaireScreen.class_301.field_3896 ?? (SolitaireScreen.class_301.field_3896 = new Action<SolitaireState.WonLastGameFields>(Method_1914)));

			// draw the number of atoms remaining for that atomType
			int count = class413.field_3888;
			Color color = count == 0 ? class_181.field_1718.WithAlpha(0.2f) : class_181.field_1718;
			if (count % 2 == 1) color = class_181.field_1720;
			string total = count.ToString();
			Font crimson_10_5 = class_238.field_1990.field_2141;
			pos += new Vector2(19f, 12f);
			class_135.method_290(total, pos, crimson_10_5, color, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			return count;
		}

		int metalsLeft = 0;

		Vector2 vector2_2 = class412.field_3884 + new Vector2(980f, sbyte.MaxValue);
		vector2_2.X += 30f;
		//draw quicksilver
		Vector2 quicksilverPos = vector2_2;
		vector2_2.X += 25f;
		//draw pip
		vector2_2.X += 29f;
		metalsLeft += Method_1901(class_175.field_1681, vector2_2); // lead
		vector2_2.X += 40f;
		metalsLeft += Method_1901(class_175.field_1683, vector2_2); // tin
		vector2_2.X += 40f;
		metalsLeft += Method_1901(class_175.field_1684, vector2_2); // iron
		vector2_2.X += 40f;
		metalsLeft += Method_1901(class_175.field_1682, vector2_2); // copper
		vector2_2.X += 40f;
		metalsLeft += Method_1901(class_175.field_1685, vector2_2); // silver

		if (DisplayMetalsRemaining)
		{
			// draw metalsLeft above quicksilver
			Color color = metalsLeft == 0 ? class_181.field_1718.WithAlpha(0.2f) : class_181.field_1718;
			string total = "(" + metalsLeft.ToString() + ")";
			Font crimson_9_75 = class_238.field_1990.field_2140;
			quicksilverPos += new Vector2(-19f, 15f);
			class_135.method_290(total, quicksilverPos, crimson_9_75, color, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
		}
	}

	public static void SolitaireRulesScreen_Method_50(On.class_16.orig_method_50 orig, class_16 screen_self, float timeDelta)
	{
		if (CampaignLoader.currentCampaignIsRMC())
		{
			var screen_dyn = new DynamicData(screen_self);
			string rule = "The metals match with themselves and with quicksilver, but only if there are no metals in the previous tier present.";
			screen_dyn.Set("field_70", class_134.method_253(rule, string.Empty));
		}

		orig(screen_self, timeDelta);
	}
}