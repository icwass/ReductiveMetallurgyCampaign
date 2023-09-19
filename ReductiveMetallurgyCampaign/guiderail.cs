//using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Reflection;

namespace ReductiveMetallurgyCampaign;

using PartType = class_139;
using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
//using PartTypes = class_191;
using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;

public static class PolymerInput
{
	public static PartType partTypeGoldenThread, partTypeBerloChain;

	private static Vector2 hexGraphicalOffset(HexIndex hex) => class_187.field_1742.method_492(hex);


	private static bool contentLoaded = false;
	public static void LoadContent()
	{
		if (contentLoaded) return;
		contentLoaded = true;

		partTypeGoldenThread = new PartType()
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

		partTypeBerloChain = new PartType()
		{
			field_1528 = "rmc-berlo-chain-polymer-input",
			field_1529 = class_134.method_253("Guiding Rail", string.Empty),
			field_1530 = class_134.method_253("This rail helps guide the input polymer into the machine. Arms can be mounted on it, but atoms are not allowed to pass through it.", string.Empty),
			field_1532 = (enum_2)2,
			field_1538 = new class_222[]
			{
				new class_222(new HexIndex(-5, 2), new HexIndex(-4, 2), (enum_126) 1, (Maybe<AtomType>) struct_18.field_1431)
			},
			field_1539 = true,
			field_1542 = true,
			field_1551 = Permissions.None
		};

		string str = "textures/parts/guide_rail/";
		Texture tex_bend = class_235.method_615(str + "bend_hex");
		Texture tex_end = class_235.method_615(str + "end_hex");
		Texture tex_sharp = class_235.method_615(str + "sharp_hex");
		Texture tex_single = class_235.method_615(str + "single_hex");
		Texture tex_straight = class_235.method_615(str + "straight_hex");
		Texture tex_hole = class_235.method_615(str + "hole");
		Texture tex_bond = class_235.method_615(str + "bond");

		QApi.AddPartType(partTypeGoldenThread, (part, pos, editor, renderer) =>
		{
			foreach (var class222 in part.method_1159().field_1538)
			{
				float num = class_187.field_1742.method_492(class222.field_1921 - class222.field_1920).Angle();
				renderer.method_526(tex_bond, class222.field_1920, new Vector2(0.0f, 0.0f), new Vector2(-23f, 20f), num);
			}
			void drawVoidHex(HexIndex hex, float alpha)
			{
				Vector2 vec2 = class_187.field_1742.method_491(hex, renderer.field_1797) - tex_hole.field_2056.ToVector2() / 2;
				class_135.method_271(tex_hole, Color.White.WithAlpha(alpha), vec2.Rounded());
			}
			drawVoidHex(new HexIndex(-5, 2), 1f);
			drawVoidHex(new HexIndex(-3, 2), 0.75f);
			drawVoidHex(new HexIndex(-1, 2), 0.5f);

			drawRailHexes(renderer, part, pos);
		});

		QApi.AddPartType(partTypeBerloChain, (part, pos, editor, renderer) =>
		{
			foreach (var class222 in part.method_1159().field_1538)
			{
				float num = class_187.field_1742.method_492(class222.field_1921 - class222.field_1920).Angle();
				renderer.method_526(tex_bond, class222.field_1920, new Vector2(0.0f, 0.0f), new Vector2(-23f, 20f), num);
			}
			void drawVoidHex(HexIndex hex, float alpha)
			{
				Vector2 vec2 = class_187.field_1742.method_491(hex, renderer.field_1797) - tex_hole.field_2056.ToVector2() / 2;
				class_135.method_271(tex_hole, Color.White.WithAlpha(alpha), vec2.Rounded());
			}
			var holes = new Dictionary<HexIndex, float>() {
				{new HexIndex(-6,  1), 1f },
				{new HexIndex(-4,  1), 0.75f },
				{new HexIndex(-2,  1), 0.5f }
			};
			foreach (var hole in holes)
			{
				drawVoidHex(hole.Key, hole.Value);
				drawVoidHex(hole.Key + new HexIndex(-1, 2), hole.Value);
			}
			drawRailHexes(renderer, part, pos);
		});

		void drawRailHexes(class_195 renderer, Part part, Vector2 pos)
		{
			var trackList = part.method_1189();
			// note: these arrays have a specific order so that later code is simplified
			class_126[] lighting_trackMetal = new class_126[5]
			{
				class_238.field_1989.field_90.field_262, //	textures/parts/track_straight.lighting/...
				class_238.field_1989.field_90.field_258, //	textures/parts/track_bend.lighting/...
				class_238.field_1989.field_90.field_260, //	textures/parts/track_sharp.lighting/...
				class_238.field_1989.field_90.field_259, //	textures/parts/track_end.lighting/...
				class_238.field_1989.field_90.field_261, //	textures/parts/track_single.lighting/...
			};
			Texture[] textures_trackShadow = new Texture[5]
			{
				class_238.field_1989.field_90.field_227, // textures/parts/track_straight_shadow
				class_238.field_1989.field_90.field_215, // textures/parts/track_bend_shadow
				class_238.field_1989.field_90.field_223, // textures/parts/track_sharp_shadow
				class_238.field_1989.field_90.field_217, // textures/parts/track_end_shadow
				class_238.field_1989.field_90.field_225, // textures/parts/track_single_shadow
			};
			Texture[] textures_trackBack = new Texture[5]
			{
				tex_straight,	// textures/parts/track_straight_hex
				tex_bend,		// textures/parts/track_bend_hex
				tex_sharp,		// textures/parts/track_sharp_hex
				tex_end,		// textures/parts/track_end_hex
				tex_single,		// textures/parts/track_single_hex
			};
			Texture track_hexShadow = class_238.field_1989.field_90.field_218;	// textures/parts/track_hex_shadow
			Texture track_plus = class_238.field_1989.field_90.field_221;		// textures/parts/track_plus
			Texture track_minus = class_238.field_1989.field_90.field_219;		// textures/parts/track_minus

			////////////////////////////////
			// determine the track hex angles
			bool track_is_a_loop = trackList.Count > 2 && HexIndex.Distance(trackList.First(), trackList.Last()) == 1;
			int[] hexTexIndex = new int[trackList.Count];
			int[] hexTurns = new int[trackList.Count]; // in turns

			HexIndex getTrackHex(int num) // assumes trackList.Count >= 2
			{
				if (!track_is_a_loop && num < 0) num = 1;
				if (!track_is_a_loop && num >= trackList.Count) num = trackList.Count - 2;
				return trackList[(num % trackList.Count + trackList.Count) % trackList.Count];
			}
			int convertRadiansToTurns(float radians)
			{
				float degrees = radians * (float)(180 / Math.PI);
				int turns = (int)Math.Round(degrees / 60);
				return (turns % 6 + 6) % 6;
			}

			if (trackList.Count == 1)
			{
				hexTexIndex[0] = 4;
				hexTurns[0] = 0;
			}
			else
			{
				HexIndex hexPrev, hexNext, hexThis;
				Vector2 vecPrev, vecNext;
				int rotPrev, rotNext, rotDiff;
				for (int i = 0; i < trackList.Count; i++)
				{
					hexThis = getTrackHex(i);
					hexPrev = getTrackHex(i - 1);
					hexNext = getTrackHex(i + 1);
					// note: due to how getTrackHex is defined, we have hexPrev == hexNext whenever the following occurs:
					// - hexThis is the first or the last hex in the track
					// - the track is NOT a loop
					// this is done purposely so the end-of-track case is taken care of automatically
					vecPrev = hexGraphicalOffset(hexThis - hexPrev);
					vecNext = hexGraphicalOffset(hexNext - hexThis);

					rotPrev = convertRadiansToTurns(vecPrev.Angle());
					rotNext = convertRadiansToTurns(vecNext.Angle());
					rotDiff = (6 + rotNext - rotPrev) % 6;
					// by calculating in this way, and by ordering the graphic arrays in a clever way,
					// it's now really easy to determine the image and rotation we need to draw the hex.
					// in particular, rotDiff = 3 can occur only at the end of a non-loop track, and is the end-of-track case
					hexTexIndex[i] = Math.Min(rotDiff, 6 - rotDiff);
					hexTurns[i] = (rotDiff < 4) ? rotPrev : rotNext + 3;
				}
			}
			////////////////////////////////
			// draw using the previously-determined information
			Vector2 TextureCenter(Texture texture) => (texture.field_2056.ToVector2() / 2).Rounded();
			float degrees(int deg) => deg * (float)(Math.PI / 180f);

			for (int drawPass = 0; drawPass < 2; drawPass++)
			{
				for (int i = 0; i < trackList.Count; i++)
				{
					HexIndex hexThis = trackList[i];
					int index = hexTexIndex[i];
					float angle = hexTurns[i] * degrees(60);

					if (drawPass == 0) // first pass
					{
						Texture track_back = textures_trackBack[index];
						Texture track_shadow = textures_trackShadow[index];
						renderer.method_526(track_back, hexThis, new Vector2(-1f, -1f), TextureCenter(track_back), angle);
						renderer.method_526(track_shadow, hexThis, new Vector2(0f, -7f), TextureCenter(track_shadow), angle);
						renderer.method_526(track_hexShadow, hexThis, new Vector2(0f, 0f), TextureCenter(track_hexShadow), angle);
					}
					else // second pass
					{
						class_126 track_metal = lighting_trackMetal[index];
						renderer.method_527(track_metal, hexThis, new Vector2(0f, 0f), (track_metal.method_235().ToVector2() / 2).Rounded(), angle);

						if (i == 0 || i == trackList.Count - 1) // then draw plus/minus symbols
						{
							float symbolAngle = angle;
							if (track_is_a_loop)
							{
								//need to adjust the angle to place them well
								symbolAngle += (3 - index) * degrees(-30);
								// straight pieces have two good positions - we try to choose the "more open to air" position
								if (index == 0)
								{
									int counter = 0;
									HexIndex hex1 = new HexIndex(1, 0).Rotated(HexRotation.Rounded(angle - degrees(60)));
									HexIndex hex2 = new HexIndex(1, 0).Rotated(HexRotation.Rounded(angle - degrees(120)));
									if (trackList.Contains(hexThis + hex1)) ++counter;
									if (trackList.Contains(hexThis + hex2)) ++counter;
									if (trackList.Contains(hexThis - hex1)) --counter;
									if (trackList.Contains(hexThis - hex2)) --counter;
									if (counter > 0) symbolAngle += degrees(180);
								}
							}
							Vector2 vec = new Vector2(30f, 0f).Rotated(symbolAngle).Rounded();

							if (i == trackList.Count - 1)
							{
								renderer.method_526(track_plus, hexThis, vec, TextureCenter(track_plus), 0f);
							}
							if (i == 0)
							{
								if (trackList.Count == 1) vec = -vec;
								renderer.method_526(track_minus, hexThis, vec, TextureCenter(track_minus), 0f);
							}
						}
					}
				}
			}
		}
	}


	public static void My_Method_1835(Sim sim_self)
	{
		var sim_dyn = new DynamicData(sim_self);
		var solutionEditorBase = sim_self.field_3818;
		var solution = solutionEditorBase.method_502();
		var field3919 = solution.field_3919;
		var struct122List1 = sim_self.field_3826;

		List<Vector2> vector2List = new List<Vector2>();
		float num1 = 20f;
		List<Sim.struct_122> struct122List2 = new List<Sim.struct_122>();
		foreach (var part in field3919.Where(x => x.method_1159() == partTypeGoldenThread || x.method_1159() == partTypeBerloChain))
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

