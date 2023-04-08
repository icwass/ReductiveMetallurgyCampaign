using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ReductiveMetallurgyCampaign;

using PartType = class_139;
using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
using PartTypes = class_191;
using Texture = class_256;
using Song = class_186;
using Tip = class_215;

public static class GoldenThreadPolymerInput
{
	//
	public static PartType partType;


	private static Vector2 hexGraphicalOffset(HexIndex hex) => class_187.field_1742.method_492(hex);


	private static bool contentLoaded = false;
	public static void LoadContent()
	{
		if (contentLoaded) return;
		contentLoaded = true;

		partType = new PartType()
		{
			field_1528 = "rmc-golden-thread-polymer-input",
			field_1529 = class_134.method_253("Guiding Rail", string.Empty),
			field_1530 = class_134.method_253("This rail helps guide the input polymer into the machine. Arms can be mounted on it, but atoms are not allowed to pass through it.", string.Empty),
			field_1532 = (enum_2) 2,
			field_1538 = new class_222[4]
			{
				new class_222(new HexIndex(-4, 2), new HexIndex(-4, 3), (enum_126) 1, (Maybe<AtomType>) struct_18.field_1431),
				new class_222(new HexIndex(-4, 2), new HexIndex(-3, 1), (enum_126) 1, (Maybe<AtomType>) struct_18.field_1431),
				new class_222(new HexIndex(-2, 2), new HexIndex(-2, 3), (enum_126) 1, (Maybe<AtomType>) struct_18.field_1431),
				new class_222(new HexIndex(-2, 2), new HexIndex(-1, 1), (enum_126) 1, (Maybe<AtomType>) struct_18.field_1431)
			},
			field_1539 = true,
			field_1542 = true,
			field_1551 = Permissions.None
		};

		Texture tex_bend, tex_end, tex_sharp, tex_single, tex_straight, tex_hole;
		string str = "textures/parts/guide_rail/";
		tex_bend = class_235.method_615(str + "bend_hex");
		tex_end = class_235.method_615(str + "end_hex");
		tex_sharp = class_235.method_615(str + "sharp_hex");
		tex_single = class_235.method_615(str + "single_hex");
		tex_straight = class_235.method_615(str + "straight_hex");
		tex_hole = class_235.method_615(str + "hole");

		QApi.AddPartType(partType, (part, pos, editor, renderer) =>
		{
			void drawVoidHex(HexIndex hex, float alpha)
			{
				Vector2 vec2 = class_187.field_1742.method_491(hex, renderer.field_1797) - tex_hole.field_2056.ToVector2() / 2;
				class_135.method_271(tex_hole, Color.White.WithAlpha(alpha), vec2.Rounded());
			}
			drawVoidHex(new HexIndex(-5, 2), 1f);
			drawVoidHex(new HexIndex(-3, 2), 0.75f);
			drawVoidHex(new HexIndex(-1, 2), 0.5f);

			//copied from track-drawing code
			var trackList = part.method_1189();

			HexIndex func(int num)
			{
				return trackList[class_162.method_408(num, trackList.Count)]; //note: class_162.method_408(num, trackList.Count) == (num % trackList.Count + trackList.Count) % trackList.Count;
			}
			bool track_is_a_loop = HexIndex.Distance(trackList.First(), trackList.Last()) == 1 & trackList.Count > 2;
			for (int index1 = 0; index1 < 2; ++index1)
			{
				for (int index2 = 0; index2 < trackList.Count; ++index2)
				{
					HexIndex hexIndex1 = trackList[index2];
					Maybe<Vector2> maybe1 = (Maybe<Vector2>)struct_18.field_1431;
					Maybe<Vector2> maybe2 = (Maybe<Vector2>)struct_18.field_1431;
					class_126 class126;
					Texture class256_1;
					Texture class256_2;
					float num1;
					HexRotation hexRotation1;
					Vector2 vector2_3;
					if (trackList.Count == 1)
					{
						class126 = class_238.field_1989.field_90.field_261; // textures/parts/track_single.lighting/...
						class256_1 = class_238.field_1989.field_90.field_225; // "textures/parts/track_single_shadow"
						class256_2 = tex_single; // "textures/parts/track_single_hex"
						num1 = 0.0f;
						maybe1 = (Maybe<Vector2>)new Vector2(30f, 0.0f);
						maybe2 = (Maybe<Vector2>)new Vector2(-30f, 0.0f);
					}
					else if (!track_is_a_loop && (index2 == 0 || index2 == trackList.Count - 1))
					{
						class126 = class_238.field_1989.field_90.field_259; // textures/parts/track_end.lighting/...
						class256_1 = class_238.field_1989.field_90.field_217; // "textures/parts/track_end_shadow"
						class256_2 = tex_end; // "textures/parts/track_end_hex"
						if (index2 == 0)
						{
							HexRotation hexRotation2 = HexRotation.Rounded(class_187.field_1742.method_492(func(index2 + 1) - hexIndex1).Angle());
							hexRotation1 = hexRotation2.Opposite();
							num1 = hexRotation1.ToRadians();
							vector2_3 = new Vector2(30f, 0.0f).Rotated(hexRotation1.ToRadians());
							maybe2 = (Maybe<Vector2>)vector2_3.Rounded();
						}
						else // index2 == trackList.Count - 1
						{
							HexRotation hexRotation3 = HexRotation.Rounded(class_187.field_1742.method_492(hexIndex1 - func(index2 - 1)).Angle());
							num1 = hexRotation3.ToRadians();
							vector2_3 = new Vector2(30f, 0.0f).Rotated(hexRotation3.ToRadians());
							maybe1 = (Maybe<Vector2>)vector2_3.Rounded();
						}
					}
					else
					{
						Vector2 vector2_4 = class_187.field_1742.method_492(hexIndex1 - func(index2 - 1));
						Vector2 vector2_5 = class_187.field_1742.method_492(func(index2 + 1) - hexIndex1);
						HexRotation hexRotation4 = HexRotation.Rounded(vector2_4.Angle());
						HexRotation hexRotation5 = HexRotation.Rounded(vector2_5.Angle());
						hexRotation1 = hexRotation5 - hexRotation4;
						int numberOfTurns = hexRotation1.AsShortestAngle().GetNumberOfTurns();
						HexRotation hexRotation6 = hexRotation4;
						switch (numberOfTurns)
						{
							case -2:
							case 2:
								class126 = class_238.field_1989.field_90.field_260; // textures/parts/track_sharp.lighting/...
								class256_1 = class_238.field_1989.field_90.field_223; // "textures/parts/track_sharp_shadow"
								class256_2 = tex_sharp; // "textures/parts/track_sharp_hex"
								if (numberOfTurns == -2)
									hexRotation6 += HexRotation.R60;
								break;
							case -1:
							case 1:
								class126 = class_238.field_1989.field_90.field_258; // textures/parts/track_bend.lighting/...
								class256_1 = class_238.field_1989.field_90.field_215; // "textures/parts/track_bend_shadow"
								class256_2 = tex_bend; // "textures/parts/track_bend_hex"
								if (numberOfTurns == -1)
									hexRotation6 += HexRotation.R120;
								break;
							default:
								class126 = class_238.field_1989.field_90.field_262; // textures/parts/track_straight.lighting/...
								class256_1 = class_238.field_1989.field_90.field_227; // "textures/parts/track_straight_shadow"
								class256_2 = tex_straight; // "textures/parts/track_straight_hex"
								break;
						}
						num1 = hexRotation6.ToRadians();
						if (track_is_a_loop && (index2 == 0 || index2 == trackList.Count - 1))
						{
							hexRotation1 = hexRotation5.Opposite();
							double radians1 = (double)hexRotation1.ToRadians();
							float radians2 = hexRotation4.ToRadians();
							double num2 = (double)radians2;
							float radians3 = class_162.method_410((float)radians1, (float)num2) / 2f + radians2;
							if (numberOfTurns == 0)
							{
								int num3 = 0;
								HexRotation rotation1 = HexRotation.Rounded(radians3 + 0.1f);
								HexRotation rotation2 = HexRotation.Rounded(radians3 - 0.1f);
								if (trackList.Contains<HexIndex>(hexIndex1 + new HexIndex(1, 0).Rotated(rotation1)))
									++num3;
								if (trackList.Contains<HexIndex>(hexIndex1 + new HexIndex(1, 0).Rotated(rotation2)))
									++num3;
								if (trackList.Contains<HexIndex>(hexIndex1 + new HexIndex(1, 0).Rotated(rotation1.Opposite())))
									--num3;
								if (trackList.Contains<HexIndex>(hexIndex1 + new HexIndex(1, 0).Rotated(rotation2.Opposite())))
									--num3;
								if (num3 > 0)
									radians3 += 3.141593f;
							}
							vector2_3 = new Vector2(30f, 0.0f);
							vector2_3 = vector2_3.Rotated(radians3);
							Vector2 vector2_6 = vector2_3.Rounded();
							if (index2 == 0)
								maybe2 = (Maybe<Vector2>)vector2_6;
							else
								maybe1 = (Maybe<Vector2>)vector2_6;
						}
					}
					HexIndex hexIndex2 = part.method_1161() + hexIndex1;
					class_187.field_1742.method_491(hexIndex2, pos);
					Vector2 vector2_7;
					if (index1 == 0)
					{
						vector2_7 = new Vector2(42f, 49f);
						renderer.method_526(class256_2, hexIndex1, new Vector2(-1f, -1f), vector2_7, num1);
						vector2_3 = class256_1.field_2056.ToVector2() / 2;
						Vector2 vector2_8 = vector2_3.Rounded();
						renderer.method_526(class256_1, hexIndex1, new Vector2(0.0f, -7f), vector2_8, num1);
						Texture field218 = class_238.field_1989.field_90.field_218; // textures/parts/track_hex_shadow
						vector2_3 = field218.field_2056.ToVector2() / 2;
						vector2_7 = vector2_3.Rounded();
						renderer.method_526(field218, hexIndex1, new Vector2(0.0f, 0.0f), vector2_7, num1);
					}
					else
					{
						vector2_3 = class126.method_235().ToVector2() / 2;
						vector2_7 = vector2_3.Rounded();
						renderer.method_527(class126, hexIndex1, new Vector2(0.0f, 0.0f), vector2_7, num1);
						if (maybe1.method_1085())
						{
							Texture field221 = class_238.field_1989.field_90.field_221; // "textures/parts/track_plus"
							vector2_7 = field221.field_2056.ToVector2() / 2;
							renderer.method_526(field221, hexIndex1, maybe1.method_1087(), vector2_7, 0.0f);
						}
						if (maybe2.method_1085())
						{
							Texture field219 = class_238.field_1989.field_90.field_219; // "textures/parts/track_minus"
							vector2_7 = field219.field_2056.ToVector2() / 2;
							renderer.method_526(field219, hexIndex1, maybe2.method_1087(), vector2_7, 0.0f);
						}
					}
				}
			}
		});
	}


	public static void My_Method_1835(Sim sim_self)
	{
		var sim_dyn = new DynamicData(sim_self);
		var solutionEditorBase = sim_dyn.Get<SolutionEditorBase>("field_3818");
		var solution = solutionEditorBase.method_502();
		var field3919 = solution.field_3919;
		var struct122List1 = sim_dyn.Get<List<Sim.struct_122>>("field_3826");

		List<Vector2> vector2List = new List<Vector2>();
		float num1 = 20f;
		List<Sim.struct_122> struct122List2 = new List<Sim.struct_122>();
		foreach (var part in field3919.Where(x => x.method_1159() == GoldenThreadPolymerInput.partType))
		{
			foreach (HexIndex hexIndex in (IEnumerable<HexIndex>)part.method_1189())
				vector2List.Add(hexGraphicalOffset(part.method_1184(hexIndex)));
		}
		if (vector2List.Count == 0)
			return;
		float num2 = sim_dyn.Get<float>("field_3827");
		for (float num3 = num2; num3 <= 1f; num3 += num2)
		{
			List<Sim.struct_122> struct122List3 = new List<Sim.struct_122>();
			struct122List3.AddRange(struct122List1.Where(x => x.field_3850 == 0));
			foreach (var molecule in solutionEditorBase.method_507().method_483())
			{
				Part part;
				Vector2 vector2_1;
				HexIndex hexIndex;
				float radians;
				if (class_96.method_99<Part>(solutionEditorBase.method_1985(molecule), out part))
				{
					PartSimState partSimState = solutionEditorBase.method_507().method_481(part);
					class_236 class236 = solutionEditorBase.method_1990(part, Vector2.Zero, num3);
					vector2_1 = (Vector2)class236.field_1984;
					hexIndex = partSimState.field_2724;
					radians = (float)class236.field_1987;
				}
				else
				{
					vector2_1 = Vector2.Zero;
					hexIndex = new HexIndex(0, 0);
					radians = 0.0f;
				}
				foreach (var keyValuePair in (IEnumerable<KeyValuePair<HexIndex, Atom>>)molecule.method_1100())
				{
					HexIndex key = keyValuePair.Key;
					Vector2 vector2_2 = vector2_1 + hexGraphicalOffset(key - hexIndex).Rotated(radians);
					struct122List3.Add(new Sim.struct_122(0, vector2_2));
				}
			}
			foreach (var part in solution.method_1937().Where(x => x.method_1202()))
			{
				class_236 class236 = solutionEditorBase.method_1990(part, Vector2.Zero, num3);
				foreach (HexIndex key in part.method_1159().field_1544.Keys)
				{
					Vector2 vector2_3 = hexGraphicalOffset(key);
					Vector2 vector2_4 = class236.field_1984 + vector2_3.Rotated(class236.field_1985);
					struct122List3.Add(new Sim.struct_122(0, vector2_4));
				}
			}
			foreach (var a in vector2List)
			{
				foreach (Sim.struct_122 struct122 in struct122List3)
				{
					if (Vector2.Distance(a, struct122.field_3851) < struct122.field_3852 + num1)
					{
						float num4 = class_162.method_405(num3, 0.0f, 0.999f);
						solutionEditorBase.method_518(num4, (string)class_134.method_253("Atoms cannot pass through the guiding rail.", string.Empty), new Vector2[1]
						{
							struct122.field_3851
						});
						return;
					}
				}
			}
		}
	}


}

