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
//using PartTypes = class_191;
using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public static class ProductionManager
{
	public static void PostLoad()
	{
		IL.SolutionEditorBase.method_1984 += drawCustomProductionOverlays;
	}

	public static void DrawProductionOverlays(string puzzleID, Vector2 class423_field_3959)
	{
		foreach (var puzzleM in MainClass.AdvancedContent.Puzzles.Where(x => x.ID == puzzleID))
		{
			puzzleM.ensureTextureBankExists();

			if (puzzleM.Cabinet == null) continue;
			foreach (var overlay in puzzleM.Cabinet.fetchOverlays())
			{
				class_135.method_272(overlay.Item1, class423_field_3959 + overlay.Item2);
			}
		}
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
}