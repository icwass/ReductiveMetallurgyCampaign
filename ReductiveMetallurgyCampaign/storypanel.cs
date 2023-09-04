using Mono.Cecil.Cil;
using MonoMod.Cil;
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
using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;



public static class StoryPanelPatcher
{
	static Texture return_button, return_button_hover;
	private static Puzzle optionsUnlock = null;
	public const string optionsID = "rmc-options";

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// helpers
	public static void setOptionsUnlock(Puzzle puzzle)
	{
		if (optionsUnlock == null) optionsUnlock = puzzle;
	}



	/////////////////////////////////////////////////////////////////////////////////////////////////
	// public functions
	public static void LoadContent()
	{
		string path = "textures/story/";
		return_button = class_235.method_615(path + "return_button_rmc");
		return_button_hover = class_235.method_615(path + "return_button_hover_rmc");
	}

	public static void Load()
	{
		On.class_172.method_480 += new On.class_172.hook_method_480(AddCharactersToDictionary);
	}

	public static void PostLoad()
	{
		IL.StoryPanel.method_2175 += skipDrawingTheReturnButton;
		On.class_135.method_272 += hotswapReturnButtonTexture;
		On.OptionsScreen.method_50 += hotswapOptionsStorypanel;
		On.StoryPanel.method_2172 += customStorypanelUnlocks;
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////
	// hooking
	private static void AddCharactersToDictionary(On.class_172.orig_method_480 orig)
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
	private static void skipDrawingTheReturnButton(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);
		// skip ahead to roughly where the "check if we need to draw the Return button" code occurs
		cursor.Goto(772);

		// jump ahead to just after the comparison to the string "options" was made
		if (!cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Call))) return;

		// load the StoryPanel self onto the stack so we can use it
		cursor.Emit(OpCodes.Ldarg_0);

		// then run the new code
		cursor.EmitDelegate<Func<bool, StoryPanel, bool>>((isOptionsScreen, panel_self) =>
		{
			// return TRUE if we need to draw the Return button
			string storyPanelID = new DynamicData(panel_self).Get<Maybe<class_264>>("field_4090").method_1087().field_2090;
			if (storyPanelID == SigmarGardenPatcher.solitaireID) return false;
			if (storyPanelID == optionsID) return false;
			if (JournalLoader.journalItems().Select(x => x.field_2322).Contains(storyPanelID))
			{
				return false;
			}

			return isOptionsScreen;
		});
	}








	private static void hotswapReturnButtonTexture(On.class_135.orig_method_272 orig, Texture texture, Vector2 position)
	{
		if (CampaignLoader.currentCampaignIsRMC())
		{
			if (texture == class_238.field_1989.field_100.field_134)
			{
				texture = return_button;
			}
			else if (texture == class_238.field_1989.field_100.field_135)
			{
				texture = return_button_hover;
			}
		}
		orig(texture, position);
		return;
	}
	public static void hotswapOptionsStorypanel(On.OptionsScreen.orig_method_50 orig, OptionsScreen screen_self, float timeDelta)
	{
		if (CampaignLoader.currentCampaignIsRMC())
		{
			var screen_dyn = new DynamicData(screen_self);
			var currentStoryPanel = screen_dyn.Get<StoryPanel>("field_2680");
			var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("field_4093");
			if (!stringArray.Any(x => x.Contains("Saverio") || x.Contains("Pugano")))
			{
				var class264 = new class_264("options-rmc");
				class264.field_2090 = optionsID;
				screen_dyn.Set("field_2680", new StoryPanel((Maybe<class_264>)class264, false));
			}
		}
		orig(screen_self, timeDelta);
	}
	public static void customStorypanelUnlocks(On.StoryPanel.orig_method_2172 orig, StoryPanel panel_self, float timeDelta, Vector2 pos, int index, Tuple<int, LocString>[] tuple)
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







}