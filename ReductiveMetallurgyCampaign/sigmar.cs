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

	private static AtomType nullAtom;

	private static SolitaireState solitaireState_RMC;
	private static int sigmarWins_RMC = 0;
	private static bool currentCampaignIsRMC() => MainClass.campaign_self == Campaigns.field_2330;
	private static bool isQuintessenceSigmarGarden(SolitaireScreen screen) => new DynamicData(screen).Get<bool>("field_3874");
	private static bool currentCampaignIsRMC(SolitaireScreen screen)
	{
		return currentCampaignIsRMC() && !isQuintessenceSigmarGarden(screen);
	}
	private static void setSigmarWins_RMC() => GameLogic.field_2434.field_2451.field_1929.method_858("RMC-SigmarWins", sigmarWins_RMC.method_453());
	private static void getSigmarWins_RMC() { sigmarWins_RMC = GameLogic.field_2434.field_2451.field_1929.method_862<int>(new delegate_384<int>(int.TryParse), "RMC-SigmarWins").method_1090(0); }

	public static void Load()
	{
		getSigmarWins_RMC();
		On.class_198.method_537 += Class198_Method_537;
		On.CampaignItem.method_825 += CampaignItem_Method_825;
		On.SolitaireGameState.method_1885 += SolitaireGameState_Method_1885;
		On.SolitaireScreen.method_50 += SolitaireScreen_Method_50;
		On.SolitaireGameState.class_301.method_1888 += Class301_Method_1888;
		//BoardEditor stuff
		On.class_178.method_50 += Class178_Method_50;

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


		hook_SolitaireScreen_method_1889 = new Hook(
		typeof(SolitaireScreen).GetMethod("method_1889", BindingFlags.Instance | BindingFlags.NonPublic),
		typeof(SigmarGardenPatcher).GetMethod("OnSolitaireScreen_Method_1889", BindingFlags.Static | BindingFlags.NonPublic)
		);
		hook_SolitaireScreen_method_1890 = new Hook(
		typeof(SolitaireScreen).GetMethod("method_1890", BindingFlags.Instance | BindingFlags.NonPublic),
		typeof(SigmarGardenPatcher).GetMethod("OnSolitaireScreen_Method_1890", BindingFlags.Static | BindingFlags.NonPublic)
		);
	}
	private delegate SolitaireState orig_SolitaireScreen_method_1889(SolitaireScreen self);
	private delegate void orig_SolitaireScreen_method_1890(SolitaireScreen self, SolitaireState param_5433);
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


	public static void Unload()
	{
		hook_SolitaireScreen_method_1889.Dispose();
		hook_SolitaireScreen_method_1890.Dispose();
	}



	public static SolitaireGameState Class198_Method_537(On.class_198.orig_method_537 orig, bool quintessenceSigmar)
	{
		if (!currentCampaignIsRMC() || quintessenceSigmar) return orig(quintessenceSigmar);

		string path = "";
		string filePath = "Content/solitaire-rmc.dat";

		// find solitaire_rmc.dat
		foreach (var dir in QuintessentialLoader.ModContentDirectories)
		{
			Logger.Log(Path.Combine(dir, filePath));
			if (File.Exists(Path.Combine(dir, filePath)))
			{
				path = Path.Combine(dir, filePath);
				break;
			}
		}

		if (path == "") return orig(quintessenceSigmar);

		int field1811 = 55;
		int field1812 = field1811 * 3;

		using (BinaryReader binaryReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
			int num = binaryReader.ReadInt32();
			int boardID = class_269.field_2103.method_299(0, num);
			binaryReader.BaseStream.Seek(boardID * field1812, SeekOrigin.Current);
			HexRotation rotation = new HexRotation(class_269.field_2103.method_299(0, 6));
			SolitaireGameState solitaireGameState = new SolitaireGameState();
			AtomType[] atomTypes = new AtomType[17]
			{
				nullAtom, // 00 - filler
				class_175.field_1675, // 01 - salt
				class_175.field_1676, // 02 - air
				class_175.field_1677, // 03 - earth
				class_175.field_1678, // 04 - fire
				class_175.field_1679, // 05 - water
				class_175.field_1680, // 06 - quicksilver
				class_175.field_1686, // 07 - gold
				class_175.field_1685, // 08 - silver
				class_175.field_1682, // 09 - copper
				class_175.field_1684, // 10 - iron
				class_175.field_1683, // 11 - tin
				class_175.field_1681, // 12 - lead
				class_175.field_1687, // 13 - vitae
				class_175.field_1688, // 14 - mors
				class_175.field_1689, // 15 - repeat
				class_175.field_1690, // 16 - quintessence
			};
			for (int index = 0; index < field1811; ++index)
			{
				AtomType atomType = atomTypes[binaryReader.ReadByte()];
				HexIndex key = new HexIndex(binaryReader.ReadSByte(), binaryReader.ReadSByte());
				//key = key.RotatedAround(new HexIndex(5, 0), rotation);
				solitaireGameState.field_3864.Add(key, atomType);
			}
			return solitaireGameState;
		}
	}
	public static bool CampaignItem_Method_825(On.CampaignItem.orig_method_825 orig, CampaignItem item_self)
	{
		bool ret = orig(item_self);
		if (currentCampaignIsRMC())
			ret = ret || (item_self.field_2324 == (enum_129)3 && sigmarWins_RMC > 0);
		return ret;
	}

	public static bool SolitaireGameState_Method_1885(On.SolitaireGameState.orig_method_1885 orig, SolitaireGameState state_self)
	{
		bool ret = orig(state_self);
		AtomType quintessence = class_175.field_1690;
		if (ret && currentCampaignIsRMC() && !state_self.field_3864.ContainsValue(quintessence)) sigmarWins_RMC++;
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
				class264.field_2090 = "solitaire";
				screen_dyn.Set("field_3872", new StoryPanel((Maybe<class_264>)class264, true));
			}
		}
		orig(screen_self, timeDelta);
	}

	public static bool Class301_Method_1888(On.SolitaireGameState.class_301.orig_method_1888 orig, SolitaireGameState.class_301 class301_self, AtomType param_5430, AtomType param_5431)
	{
		bool atomTypeIsMetal = param_5430.field_2297.method_1085();
		if (currentCampaignIsRMC() && param_5430 == param_5431 && atomTypeIsMetal) return true;
		return orig(class301_self, param_5430, param_5431);
	}




	//=============================================================================================================================//

	//sigmar board editor
	private static bool showBoardEditorButton = true;

	public static void Class178_Method_50(On.class_178.orig_method_50 orig, class_178 class178_self, float timeDelta)
	{
		if (Input.IsSdlKeyHeld(SDL.enum_160.SDLK_F3))
		{
			showBoardEditorButton = !showBoardEditorButton;
			class_238.field_1991.field_1821.method_28(1f);
		}

		if (GameLogic.field_2434.method_938() is BoardEditorScreen)
			return;
		orig(class178_self, timeDelta);

		float num = 65f;
		Vector2 vector2_1 = new(570f, 440f);
		Vector2 vector2_2 = (class_115.field_1433 / 2 - vector2_1 / 2).Rounded();
		Vector2 vector2_3 = new(161f, 256f);
		Vector2 vector2_4 = vector2_3 + new Vector2(0.0f, -num);
		Vector2 vector2_5 = vector2_4 + new Vector2(0.0f, -num);
		Vector2 vector2_6 = vector2_5 + new Vector2(0.0f, -num * 3);
		if (showBoardEditorButton && class_140.method_314(class_134.method_253("[RMC] Board Editor", string.Empty), vector2_2 + vector2_6).method_824(true, true))
		{
			GameLogic.field_2434.method_945(new BoardEditorScreen(), (Maybe<class_124>)struct_18.field_1431, (Maybe<class_124>)Transitions.field_4104);
		}
	}

	class BoardEditorScreen : IScreen
	{
		//=========================//
		//boilerplate needed as a subclass of IScreen
		public bool method_1037() => false;
		public void method_47(bool param_5434) { GameLogic.field_2434.field_2443.method_673(class_238.field_1992.field_969); }
		public void method_48() { }
		public void method_50(float timeDelta) { DrawFunction(); }
		//=========================//
		// helpers
		public static AtomType getAtomType(int i)
		{
			return new AtomType[17]
			{
				nullAtom, // 00 - filler
				class_175.field_1675, // 01 - salt
				class_175.field_1676, // 02 - air
				class_175.field_1677, // 03 - earth
				class_175.field_1678, // 04 - fire
				class_175.field_1679, // 05 - water
				class_175.field_1680, // 06 - quicksilver
				class_175.field_1686, // 07 - gold
				class_175.field_1685, // 08 - silver
				class_175.field_1682, // 09 - copper
				class_175.field_1684, // 10 - iron
				class_175.field_1683, // 11 - tin
				class_175.field_1681, // 12 - lead
				class_175.field_1687, // 13 - vitae
				class_175.field_1688, // 14 - mors
				class_175.field_1689, // 15 - repeat
				class_175.field_1690, // 16 - quintessence
			}[i];
		}

		public static bool AtomButtonClicked(Vector2 position, bool rightClick = false)
		{
			// check if clicked
			bool mouseClicked = rightClick ? Input.IsRightClickPressed() : Input.IsLeftClickPressed();
			bool inRange = Vector2.Distance(position, Input.MousePos()) < 28.0;
			return mouseClicked && inRange;
		}

		public static bool DrawAtomButton(Vector2 position, AtomType atomType, bool rightClick = false, bool bright = true)
		{
			bool inRange = Vector2.Distance(position, Input.MousePos()) < 28.0;
			// draw hover
			if (inRange)
			{
				class_256 field551 = class_238.field_1989.field_98.field_551; // solitaire/hex_hover
				Color color = Color.White.WithAlpha(true ? 0.6f : 0.1f);
				Vector2 vec2 = position + new Vector2(-36f, -41f);
				class_135.method_271(field551, color, vec2);
			}
			// draw atom
			if (atomType != getAtomType(0))
			{
				float num2 = bright ? 1f : 0.2f;
				float num3 = bright ? 0.45f : 0.0f;
				Editor.method_927(atomType, position, 0.88f, num2, 1f, num3, -12f, 0.0f, null, null, false);
			}

			return AtomButtonClicked(position, rightClick);
		}

		//public static bool DrawCheckbox(Vector2 pos, string label, bool enabled)
		//{
		//	Bounds2 boxBounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
		//	Bounds2 labelBounds = UI.DrawText(label, pos + new Vector2(45f, 13f), UI.SubTitle, UI.TextColor, TextAlignment.Left);
		//	if (enabled)
		//		UI.DrawTexture(class_238.field_1989.field_101.field_773, boxBounds.Min);
		//	if (boxBounds.Contains(Input.MousePos()) || labelBounds.Contains(Input.MousePos()))
		//	{
		//		UI.DrawTexture(class_238.field_1989.field_101.field_774, boxBounds.Min);
		//		if (!Input.IsLeftClickPressed())
		//			return false;
		//		class_238.field_1991.field_1821.method_28(1f);
		//		return true;
		//	}
		//	UI.DrawTexture(class_238.field_1989.field_101.field_772, boxBounds.Min);
		//	return false;
		//}


		//=========================//
		// "real" stuff
		public void DrawFunction()
		{
			//=========================//
			// draw frame, handle transition logic

			//if (GameLogic.field_2434.method_938() is class_16) return; // tutorial screen
			Vector2 windowFrameSize = new Vector2(1516f, 922f);
			Vector2 origin = (Input.ScreenSize() / 2 - windowFrameSize / 2 + new Vector2(-2f, -11f)).Rounded();

			//DEBUG helper to find the origin =============================================================================================================================//
			for (int i = 0; i < 5; i++)
			{
				UI.DrawUiBackground(origin, new Vector2(50f, 50f));
			}

			//class_135.method_272(...) == UI.DrawTexture(texture, pos);
			UI.DrawUiBackground(origin + new Vector2(81f, 89f), new Vector2(508f, 767f));
			class_140.method_322(origin);
			class_135.method_272(class_238.field_1989.field_98.field_545, origin + new Vector2(597f, 92f)); // solitaire/board
			class_135.method_276(class_238.field_1989.field_102.field_817, Color.White, origin, windowFrameSize); // window/frame
			class_135.method_272(class_238.field_1989.field_102.field_821, origin + new Vector2(105f, 859f)); // window/story_detail
			class_135.method_272(class_238.field_1989.field_98.field_546, origin + new Vector2(808f, 847f)); // solitaire/board_top
			class_135.method_272(class_238.field_1989.field_102.field_825, origin + new Vector2(66f, 76f)); // window/vertical_bar_left
			class_135.method_272(class_238.field_1989.field_102.field_824, origin + new Vector2(574f, 76f)); // window/vertical_bar_center

			if (UI.DrawAndCheckCloseButton(origin, windowFrameSize, new Vector2(104, 98)))
			{
				GameLogic.field_2434.method_947((Maybe<class_124>)Transitions.field_4105, (Maybe<class_124>)struct_18.field_1431);
			}

			//=========================//
			// draw marbles on the board
			Vector2 boardCenter = origin + new Vector2(1017f, 507f);
			Vector2 marblePosition(int x, int y) => boardCenter + new Vector2(66f * (x + 0.5f * y), 57f * y);

			for (int i = -5; i <= 5; i++)
			{
				for (int j = -5; j <= 5; j++)
				{
					//
					if (i + j >= -5 && i + j <= 5)
					{
						if (DrawAtomButton(marblePosition(i, j), getAtomType((i + j + 11) % 11))) class_238.field_1991.field_1821.method_28(1f);

						class_135.method_290("" + i + "," + j, marblePosition(i, j) + new Vector2(0f, -35f), class_238.field_1990.field_2143, Color.FromHex(3483687), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
					}
				}
			}


			//=========================//
			// draw atom pallete

			boardCenter = origin + new Vector2(337f, 637f);
			int[][] swatches = new int[15][] {
				new int[3]{ 4, -3, 1},	new int[3]{ 9, 1, 1},	new int[3]{15, 0, 0},
				new int[3]{ 3, -2, 1},	new int[3]{10, 2, 1},
				new int[3]{13, -4, 2},	new int[3]{ 8, 0, 2},
				new int[3]{ 1, -3, 2},	new int[3]{ 6, 1, 2},
				new int[3]{14, -2, 2},	new int[3]{11, 2, 2},
				new int[3]{ 2, -4, 3},	new int[3]{ 7, 0, 3},
				new int[3]{ 5, -3, 3},	new int[3]{12, 1, 3},
			};

			foreach (var swatch in swatches)
			{
				if (DrawAtomButton(marblePosition(swatch[1], swatch[2]), getAtomType(swatch[0]))) class_238.field_1991.field_1821.method_28(1f);
			}




			/*


			this.field_3872.method_2172(timeDelta, origin + new Vector2(89f, 94f), this.field_3871, SolitaireScreen.field_3868);
			if (this.method_1889().method_1921() && this.method_1894())
				this.method_1891();
			if (class_140.method_313((string)class_134.method_253("New Game", string.Empty), origin + new Vector2(604f, 99f), 132, 53).method_824(this.method_1894(), true))
			{
				this.method_1891();
				GameLogic.field_2434.field_2459.method_37();
				class_238.field_1991.field_1821.method_28(1f);
			}
			if (SolitaireScreen.method_1895(origin + new Vector2(1292f, 99f), 37, 53).method_824(this.method_1893(), true))
			{
				GameLogic.field_2434.method_946((IScreen)new class_16());
				class_238.field_1991.field_1821.method_28(1f);
			}



			SolitaireScreen.class_412 class412 = new SolitaireScreen.class_412();

			class_135.method_290(class_134.method_253("Wins", string.Empty).method_1060().method_441(), origin + new Vector2(1384f, 137f), class_238.field_1990.field_2142, Color.White, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, -2, Color.Black, class_238.field_1989.field_99.field_706.field_751, int.MaxValue, false, true);
			class_135.method_290(this.field_3871.ToString(), origin + new Vector2(1385f, 106f), class_238.field_1990.field_2143, Color.FromHex(3483687), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), (class_256)null, int.MaxValue, false, true);
			Action<AtomType, Vector2> action = new Action<AtomType, Vector2>(class412.method_1901);
			this.field_3873 = (Maybe<AtomType>)struct_18.field_1431;
			Vector2 vector2_2 = origin + new Vector2(770f, (float)sbyte.MaxValue);
			action(class_175.field_1675, vector2_2);
			vector2_2.X += 25f;
			class_135.method_272(class_238.field_1989.field_98.field_549, vector2_2 + new Vector2(0.0f, -2f));
			vector2_2.X += 29f;
			action(class_175.field_1676, vector2_2);
			vector2_2.X += 42f;
			action(class_175.field_1678, vector2_2);
			vector2_2.X += 42f;
			action(class_175.field_1679, vector2_2);
			vector2_2.X += 42f;
			action(class_175.field_1677, vector2_2);
			vector2_2.X += 30f;
			if (this.field_3874)
			{
				vector2_2.X -= 5f;
				class_135.method_272(class_238.field_1989.field_98.field_549, vector2_2 + new Vector2(0.0f, -2f));
				vector2_2.X += 29f;
				action(class_175.field_1690, vector2_2);
				vector2_2.X += 30f;
				class_135.method_272(class_238.field_1989.field_98.field_550, vector2_2 + new Vector2(0.0f, -23f));
				vector2_2.X += 30f;
			}
			else
			{
				class_135.method_272(class_238.field_1989.field_98.field_550, vector2_2 + new Vector2(0.0f, -23f));
				vector2_2.X += 30f;
				action(class_175.field_1680, vector2_2);
				vector2_2.X += 25f;
				class_135.method_272(class_238.field_1989.field_98.field_549, vector2_2 + new Vector2(0.0f, -2f));
				vector2_2.X += 29f;
			}
			action(class_175.field_1681, vector2_2);
			vector2_2.X += 40f;
			action(class_175.field_1683, vector2_2);
			vector2_2.X += 40f;
			action(class_175.field_1684, vector2_2);
			vector2_2.X += 40f;
			action(class_175.field_1682, vector2_2);
			vector2_2.X += 40f;
			action(class_175.field_1685, vector2_2);
			vector2_2.X += 40f;
			action(class_175.field_1686, vector2_2);
			class412.field_3885 = new Func<HexIndex, Vector2>(class412.method_1902);
			class412.field_3887 = new Action<SolitaireGameState, AtomType, HexIndex>(class412.method_1903);
			this.method_1889().method_1933(SolitaireScreen.class_301.field_3897 ?? (SolitaireScreen.class_301.field_3897 = new Action<SolitaireState.WaitingForNewGameFields>(SolitaireScreen.class_301.field_2343.method_1915)), new Action<SolitaireState.struct_123>(class412.method_1904), new Action<SolitaireState.struct_124>(class412.method_1905), new Action<SolitaireState.WonLastGameFields>(class412.method_1906));

			*/






















		}



	}
}