//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
//using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
//using System;
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
using PartTypes = class_191;
//using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public static partial class CampaignLoader
{
	static void LoadPolymerInputPuzzle(Puzzle puzzle)
	{
		HexIndex hexIndex1 = new HexIndex(1, 0);
		List<class_157> class157List = new List<class_157>();
		HexIndex[] hexIndexArray = new HexIndex[]
		{
			new HexIndex(0, 0),
			new HexIndex(4, 0)
		};
		foreach (var hexIndex2 in hexIndexArray)
		{
			class157List.Add(new class_157()
			{
				field_1597 = PartTypes.field_1760, // input
				field_1598 = hexIndex1 + hexIndex2
			});
		}
		class157List.Add(new class_157()
		{
			field_1597 = PolymerInput.partTypeBerloChain,
			field_1598 = hexIndex1 + new HexIndex(8, -2),
			field_1602 = new HexIndex[]
			{
				hexIndex1 + new HexIndex(8, -2),
				hexIndex1 + new HexIndex(7, -2),
				hexIndex1 + new HexIndex(6, -2),
				hexIndex1 + new HexIndex(5, -2),
				hexIndex1 + new HexIndex(4, -2),
				hexIndex1 + new HexIndex(3, -2),
				hexIndex1 + new HexIndex(2, -2),
				hexIndex1 + new HexIndex(1, -2),
				hexIndex1 + new HexIndex(0, -1),
				hexIndex1 + new HexIndex(-1, 0),
				hexIndex1 + new HexIndex(-1, 1),
				hexIndex1 + new HexIndex(-1, 2),
				hexIndex1 + new HexIndex(0, 2),
				hexIndex1 + new HexIndex(1, 2),
				hexIndex1 + new HexIndex(2, 2),
				hexIndex1 + new HexIndex(3, 2),
				hexIndex1 + new HexIndex(4, 2),
				hexIndex1 + new HexIndex(5, 2),
				hexIndex1 + new HexIndex(6, 2)
			}
		});
		puzzle.field_2772 = new class_157[class157List.Count];
		for (int index = 0; index < class157List.Count; ++index)
			puzzle.field_2772[index] = class157List[index];
	}
	static void LoadOldPolymerInputPuzzle(Puzzle puzzle)
	{
		HexIndex hexIndex1 = new HexIndex(1, 0);
		List<class_157> class157List = new List<class_157>();
		HexIndex[] hexIndexArray = new HexIndex[3]
		{
			new HexIndex(2, 0),
			new HexIndex(4, 0),
			new HexIndex(6, 0)
		};
		foreach (var hexIndex2 in hexIndexArray)
		{
			class157List.Add(new class_157()
			{
				field_1597 = PartTypes.field_1760, // input
				field_1598 = hexIndex1 + hexIndex2
			});
		}
		class157List.Add(new class_157()
		{
			field_1597 = PolymerInput.partTypeGoldenThread,
			field_1598 = hexIndex1 + new HexIndex(6, -2),
			field_1602 = new HexIndex[15]
			{
				hexIndex1 + new HexIndex(6, -2),
				hexIndex1 + new HexIndex(5, -2),
				hexIndex1 + new HexIndex(4, -2),
				hexIndex1 + new HexIndex(3, -2),
				hexIndex1 + new HexIndex(2, -2),
				hexIndex1 + new HexIndex(1, -2),
				hexIndex1 + new HexIndex(0, -1),
				hexIndex1 + new HexIndex(0, 0),
				hexIndex1 + new HexIndex(-1, 1),
				hexIndex1 + new HexIndex(-1, 2),
				hexIndex1 + new HexIndex(0, 2),
				hexIndex1 + new HexIndex(1, 2),
				hexIndex1 + new HexIndex(2, 2),
				hexIndex1 + new HexIndex(3, 2),
				hexIndex1 + new HexIndex(4, 2)
			}
		});
		puzzle.field_2772 = new class_157[class157List.Count];
		for (int index = 0; index < class157List.Count; ++index)
			puzzle.field_2772[index] = class157List[index];
	}
}