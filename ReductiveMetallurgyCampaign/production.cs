using Mono.Cecil.Cil;
using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
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
using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public static class ProductionManager
{
	static Dictionary<string, List<Tuple<string, Vector2>>> productionOverlays = new();
	static Dictionary<string, Texture> productionTextureBank = null;

	public static IReadOnlyDictionary<string, Texture> ProductionTextures => productionTextureBank;

	public static void PostLoad()
	{
		IL.SolutionEditorBase.method_1984 += drawCustomProductionOverlays;
	}

	public static void DrawProductionOverlays(string puzzleID, Vector2 class423_field_3959)
	{
		if (!productionOverlays.ContainsKey(puzzleID)) return;
		
		foreach (var overlay in productionOverlays[puzzleID])
		{
			class_135.method_272(fetchTexture(overlay.Item1), class423_field_3959 + overlay.Item2);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////

	public static Texture fetchTexture(string filePath)
	{
		Texture ret = class_238.field_1989.field_92.field_391; // aether_overlay_middle
		if (string.IsNullOrEmpty(filePath)) return ret;
		if (productionTextureBank.ContainsKey(filePath)) return productionTextureBank[filePath];
		try
		{
			ret = class_235.method_615(filePath);
		}
		catch
		{
			Logger.Log("[ReductiveMetallurgyCampaign] fetchProductionOverlayTexture: Couldn't load '" + filePath + ".png', will use 'aether_overlay_middle' instead.");
		}
		productionTextureBank[filePath] = ret;
		return ret;
	}

	public static void AddOverlaysForPuzzle(string puzzleID, List<CabinetModelRMC.OverlayModelRMC> overlays)
	{
		List<Tuple<string, Vector2>> list = new();
		foreach (var overlay in overlays)
		{
			if (!string.IsNullOrEmpty(overlay.Texture))
			{
				var position = ModelHelpersRMC.Vector2FromString(overlay.Position);
				list.Add(Tuple.Create(overlay.Texture, position));
			}
		}
		productionOverlays[puzzleID] = list;
		Logger.Log("Added overlays for " + puzzleID);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////

	private static void drawCustomProductionOverlays(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);
		// skip ahead to roughly where the vanilla-overlay-drawing code occurs
		cursor.Goto(1277);

		// jump ahead to just before the vanilla-overlay-drawing
		if (!cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Blt))) return;

		// load the class423 object and the SolutionEditorBase self onto the stack so we can use it
		cursor.Emit(OpCodes.Ldarg_0);
		cursor.Emit(OpCodes.Ldloc_0);
		// then run the new code
		cursor.EmitDelegate<Action<SolutionEditorBase, SolutionEditorBase.class_423>>((seb_self, class423) =>
		{
			var puzzleID = seb_self.method_502().method_1934().field_2766;
			DrawProductionOverlays(puzzleID, class423.field_3959);
		});
	}

	public static void initializeProductionTextureBank()
	{
		string prodPath = "textures/pipelines/";
		var prods = class_238.field_1989.field_92;
		productionTextureBank ??= new()
		{
			{prodPath + "aether_overlay_bottom",    prods.field_390},
			{prodPath + "aether_overlay_middle",    prods.field_391},
			{prodPath + "aether_overlay_top",       prods.field_392},
			{prodPath + "amaro_overlay_bottom",     prods.field_393},
			{prodPath + "amaro_overlay_top",        prods.field_394},
			{prodPath + "edge_overlay_left",        prods.field_395},
			{prodPath + "edge_overlay_right",       prods.field_396},
			{prodPath + "solvent_overlay",          prods.field_397},
		};
	}
}