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
		Texture ret = class_238.field_1989.field_92.field_401.field_418.field_501; // generic vial
		if (string.IsNullOrEmpty(filePath)) return ret;
		if (productionTextureBank.ContainsKey(filePath)) return productionTextureBank[filePath];
		try
		{
			ret = class_235.method_615(filePath);
		}
		catch
		{
			Logger.Log("[ReductiveMetallurgyCampaign] fetchProductionTexture: Couldn't load '" + filePath + ".png', will use the generic vial texture instead.");
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

		string vialPath = prodPath + "vials/";
		var vials = prods.field_401;

		productionTextureBank ??= new()
		{
			{vialPath + "abrasive_powder/empty",                  vials.field_408.field_444},
			{vialPath + "abrasive_powder/filling",                vials.field_408.field_445},
			{vialPath + "aether_detector/draining",               vials.field_409.field_456},
			{vialPath + "aether_detector/empty",                  vials.field_409.field_457},
			{vialPath + "aether_detector/filling",                vials.field_409.field_458},
			{vialPath + "aether_detector/full",                   vials.field_409.field_459},
			{vialPath + "conductive_enamel/empty",                vials.field_410.field_462},
			{vialPath + "conductive_enamel/filling",              vials.field_410.field_463},
			{vialPath + "elemental_iron/draining",                vials.field_411.field_483},
			{vialPath + "elemental_iron/full",                    vials.field_411.field_484},
			{vialPath + "elemental_lead/draining",                vials.field_412.field_454},
			{vialPath + "elemental_lead/full",                    vials.field_412.field_455},
			{vialPath + "elemental_quicksilver/draining",         vials.field_413.field_464},
			{vialPath + "elemental_quicksilver/full",             vials.field_413.field_465},
			{vialPath + "elemental_salt/draining",                vials.field_414.field_502},
			{vialPath + "elemental_salt/full",                    vials.field_414.field_503},
			{vialPath + "elemental_tin/draining",                 vials.field_415.field_446},
			{vialPath + "elemental_tin/full",                     vials.field_415.field_447},
			{vialPath + "eyedrops/mors_empty",                    vials.field_416.field_485},
			{vialPath + "eyedrops/mors_filling",                  vials.field_416.field_486},
			{vialPath + "eyedrops/vitae_empty",                   vials.field_416.field_487},
			{vialPath + "eyedrops/vitae_filling",                 vials.field_416.field_488},
			{vialPath + "fragrant_powder/brook_empty",            vials.field_417.field_495},
			{vialPath + "fragrant_powder/brook_filling",          vials.field_417.field_496},
			{vialPath + "fragrant_powder/forest_empty",           vials.field_417.field_497},
			{vialPath + "fragrant_powder/forest_filling",         vials.field_417.field_498},
			{vialPath + "fragrant_powder/meadow_empty",           vials.field_417.field_499},
			{vialPath + "fragrant_powder/meadow_filling",         vials.field_417.field_500},
			{vialPath + "generic/empty",                          vials.field_418.field_501},
			{vialPath + "hexstabilized_salt/empty",               vials.field_419.field_491},
			{vialPath + "hexstabilized_salt/filling",             vials.field_419.field_492},
			{vialPath + "hydrated_hexstabilized_salt/draining",   vials.field_420.field_468},
			{vialPath + "hydrated_hexstabilized_salt/full",       vials.field_420.field_469},
			{vialPath + "lamplight_gas/empty",                    vials.field_421.field_474},
			{vialPath + "lamplight_gas_precursor/full",           vials.field_422.field_477},
			{vialPath + "lustre/empty",                           vials.field_423.field_460},
			{vialPath + "lustre/filling",                         vials.field_423.field_461},
			{vialPath + "metallic_cinnabar/draining",             vials.field_424.field_507},
			{vialPath + "metallic_cinnabar/full",                 vials.field_424.field_508},
			{vialPath + "rat_poison/empty",                       vials.field_425.field_448},
			{vialPath + "rat_poison/filling",                     vials.field_425.field_449},
			{vialPath + "reactive_earth/draining",                vials.field_426.field_504},
			{vialPath + "reactive_earth/full",                    vials.field_426.field_505},
			{vialPath + "refined_hematite/draining",              vials.field_427.field_452},
			{vialPath + "refined_hematite/full",                  vials.field_427.field_453},
			{vialPath + "rocket_fuel/empty",                      vials.field_428.field_478},
			{vialPath + "rocket_fuel/filling",                    vials.field_428.field_479},
			{vialPath + "silver_paint/empty",                     vials.field_429.field_442},
			{vialPath + "silver_paint/filling",                   vials.field_429.field_443},
			{vialPath + "solvent/empty",                          vials.field_430.field_466},
			{vialPath + "solvent/filling",                        vials.field_430.field_467},
			{vialPath + "special_amaro/draining",                 vials.field_431.field_470},
			{vialPath + "special_amaro/empty",                    vials.field_431.field_471},
			{vialPath + "special_amaro/filling",                  vials.field_431.field_472},
			{vialPath + "special_amaro/full",                     vials.field_431.field_473},
			{vialPath + "stabilized_air/full",                    vials.field_432.field_506},
			{vialPath + "stabilized_earth/draining",              vials.field_433.field_489},
			{vialPath + "stabilized_earth/full",                  vials.field_433.field_490},
			{vialPath + "stabilized_fire/draining",               vials.field_434.field_481},
			{vialPath + "stabilized_fire/full",                   vials.field_434.field_482},
			{vialPath + "stabilized_water/draining",              vials.field_435.field_440},
			{vialPath + "stabilized_water/full",                  vials.field_435.field_441},
			{vialPath + "tinstone/draining",                      vials.field_436.field_475},
			{vialPath + "tinstone/full",                          vials.field_436.field_476},
			{vialPath + "vapor_of_levity/empty",                  vials.field_437.field_480},
			{vialPath + "viscous_sludge/empty",                   vials.field_438.field_450},
			{vialPath + "viscous_sludge/filling",                 vials.field_438.field_451},
			{vialPath + "welding_thermite/empty",                 vials.field_439.field_493},
			{vialPath + "welding_thermite/filling",               vials.field_439.field_494},

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