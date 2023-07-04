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
	private static IDetour hook_SolitaireScreen_method_1889;
	private static IDetour hook_SolitaireScreen_method_1890;
	private static IDetour hook_SolitaireScreen_method_1893;
	private static IDetour hook_SolitaireScreen_method_1894;

	public static AtomType nullAtom;

	public static SolitaireState solitaireState_RMC;
	private static int sigmarWins_RMC = 0;
	private static bool isQuintessenceSigmarGarden(SolitaireScreen screen) => new DynamicData(screen).Get<bool>("field_3874");
	private static bool currentCampaignIsRMC(SolitaireScreen screen)
	{
		return MainClass.currentCampaignIsRMC() && !isQuintessenceSigmarGarden(screen);
	}
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
	public static void Load()
	{
		getSigmarWins_RMC();
		On.class_198.method_537 += Class198_Method_537;
		On.CampaignItem.method_825 += CampaignItem_Method_825;
		On.SolitaireGameState.method_1885 += SolitaireGameState_Method_1885;
		On.SolitaireScreen.method_50 += SolitaireScreen_Method_50;
		On.SolitaireGameState.class_301.method_1888 += Class301_Method_1888;
		On.class_16.method_50 += SolitaireRulesScreen_Method_50;

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
			return new DynamicData(screen_self).Get<StoryPanel>("field_3872").method_2170() >= 8 && !state.method_1922();
		}
		return orig(screen_self);
	}


	public static void Unload()
	{
		hook_SolitaireScreen_method_1889.Dispose();
		hook_SolitaireScreen_method_1890.Dispose();
		hook_SolitaireScreen_method_1894.Dispose();
	}



	public static SolitaireGameState Class198_Method_537(On.class_198.orig_method_537 orig, bool quintessenceSigmar)
	{
		if (!MainClass.currentCampaignIsRMC() || quintessenceSigmar) return orig(quintessenceSigmar);

		string path = "";
		string filePath = "Content/solitaire-rmc.dat";

		// find solitaire_rmc.dat
		foreach (var dir in QuintessentialLoader.ModContentDirectories)
		{
			if (File.Exists(Path.Combine(dir, filePath)))
			{
				path = Path.Combine(dir, filePath);
				break;
			}
		}

		if (path == "") return orig(quintessenceSigmar);

		using (BinaryReader binaryReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
			const int bytesPerBoard = 200;
			// 1 for leading 0xBB byte
			// 16 for settings
			// 182 (=91*2) for marbles
			// 1 for trailing 0xDD byte
			int num = binaryReader.ReadInt32();

			int boardID = class_269.field_2103.method_299(0, num);
			if (sigmarWins_RMC == 0)
			{
				//pick a specific board
				boardID = 0;
			}



			binaryReader.BaseStream.Seek(boardID * bytesPerBoard, SeekOrigin.Current);
			byte header = binaryReader.ReadByte();
			if (header != 0xBB) throw new Exception("[RMC:SigmarGardenPatcher] Invalid header byte for solitaire board: " + header);

			//no settings, currently - skip
			binaryReader.BaseStream.Seek(16, SeekOrigin.Current);

			HexRotation rotation = new HexRotation(0);

			SolitaireGameState solitaireGameState = new SolitaireGameState();
			for (int index = 0; index < 91; ++index)
			{
				// read in the next marble
				int atomInt = binaryReader.ReadByte() & 0x000F;
				byte pos = binaryReader.ReadByte();
				// if not a blank spot, add marble to board
				if (atomInt == 0) continue;
				int R = (pos & 0x000F) - 5;
				int Q = (pos >> 4) & 0x000F;
				HexIndex hex = new HexIndex(Q,R).RotatedAround(new HexIndex(5, 0), rotation);
				solitaireGameState.field_3864.Add(hex, getAtomType(atomInt));
			}

			byte trailing = binaryReader.ReadByte();
			if (trailing != 0xDD) throw new Exception("[RMC:SigmarGardenPatcher] Invalid trailing byte for solitaire board: " + trailing);

			return solitaireGameState;
		}
	}
	public static bool CampaignItem_Method_825(On.CampaignItem.orig_method_825 orig, CampaignItem item_self)
	{
		bool ret = orig(item_self);
		if (MainClass.currentCampaignIsRMC())
			ret = ret || (item_self.field_2324 == (enum_129)3 && sigmarWins_RMC > 0);
		return ret;
	}

	public static bool SolitaireGameState_Method_1885(On.SolitaireGameState.orig_method_1885 orig, SolitaireGameState state_self)
	{
		bool ret = orig(state_self);
		AtomType quintessence = class_175.field_1690;
		if (ret && MainClass.currentCampaignIsRMC() && !state_self.field_3864.ContainsValue(quintessence)) sigmarWins_RMC++;
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
				class264.field_2090 = "rmc-solitaire";
				screen_dyn.Set("field_3872", new StoryPanel((Maybe<class_264>)class264, true));
				// force a new game to start
				MainClass.PrivateMethod<SolitaireScreen>("method_1891").Invoke(screen_self, new object[] { });
			}
		}

		orig(screen_self, timeDelta);
	}

	public static bool Class301_Method_1888(On.SolitaireGameState.class_301.orig_method_1888 orig, SolitaireGameState.class_301 class301_self, AtomType param_5430, AtomType param_5431)
	{
		bool atomTypeIsMetal = param_5430.field_2297.method_1085();
		if (MainClass.currentCampaignIsRMC() && param_5430 == param_5431 && atomTypeIsMetal) return true;
		return orig(class301_self, param_5430, param_5431);
	}

	public static void SolitaireRulesScreen_Method_50(On.class_16.orig_method_50 orig, class_16 screen_self, float timeDelta)
	{
		if (MainClass.currentCampaignIsRMC())
		{
			var screen_dyn = new DynamicData(screen_self);
			string rule = "The metals match with themselves and with quicksilver, but only if there are no metals in the previous tier present.";
			screen_dyn.Set("field_70", class_134.method_253(rule, string.Empty));
		}

		orig(screen_self, timeDelta);
	}
}