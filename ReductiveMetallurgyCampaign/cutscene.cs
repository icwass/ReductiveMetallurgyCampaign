//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
using MonoMod.Utils;
//using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
using SDL2;
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

public class CutscenePatcher
{
	static readonly string[] vanillaCutscenes = new string[] {
		"middle-3",
		"outro-5",
		"outro-4",
		"outro-3",
		"outro-2",
		"outro-1",
		"intro-4",
		"intro-5",
		"intro-1",
		"intro-2",
		"intro-3",
	};

	public static Texture creditsBackground => MainClass.AdvancedContent.Cutscenes.Where(x => x.ID == "rmc-cutscene-hubris").First().FromModel().Item2;


	public static void Load()
	{
		On.class_252.method_50 += class_252_Method_50;
	}

	static bool method_678() => class_115.field_1433.X < 1600f || class_115.field_1433.Y < 900f;

	static private float method_679() => !method_678() ? 1f : 0.76f;

	static private int method_680() => (int)(383f * method_679());

	static private int method_681() => (int)(561f * method_679());
	static private void method_684(class_264 field2038)
	{
		GameLogic.field_2434.field_2451.method_574(field2038);
		if (field2038.field_2090 == "outro-5" || field2038.field_2090 == "rmc-cutscene-hubris")
			GameLogic.field_2434.method_947((Maybe<class_124>)Transitions.field_4109, (Maybe<class_124>)Transitions.field_4108);
		else
			GameLogic.field_2434.method_947((Maybe<class_124>)Transitions.field_4107, (Maybe<class_124>)Transitions.field_4106);
	}
	static private Vector2 method_683(int param_4174, int param_4175)
	{
		int field_2044 = -10; // static readonly variable from class_252
		return new Vector2((float)(class_115.field_1433.X / 2 - ((param_4175 * method_680() + (param_4175 - 1) * field_2044) / 2) + (double)(param_4174 * (method_680() + field_2044))), (float)(!method_678() ? (class_115.field_1433.Y >= 1000 ? class_115.field_1433.Y / 2 - 175 : class_115.field_1433.Y / 2 - 200) : class_115.field_1433.Y / 2 - 140));
	}


	private static void class_252_Method_50(On.class_252.orig_method_50 orig, class_252 class252_self, float timeDelta)
	{
		var class252_dyn = new DynamicData(class252_self);
		var class264 = class252_dyn.Get<class_264>("field_2038");
		string cutsceneID = class264.field_2090;
		if (vanillaCutscenes.Contains(cutsceneID))
		{
			orig(class252_self, timeDelta);
			return;
		}
		//========================================================//
		List<class_252.class_253> getField2039() => class252_dyn.Get<List<class_252.class_253>>("field_2039");
		int getField2040() => class252_dyn.Get<int>("field_2040");
		void incrementField2040() => class252_dyn.Set("field_2040", getField2040() + 1);
		float getField2041() => class252_dyn.Get<float>("field_2041");
		void setField2041(float f) => class252_dyn.Set("field_2041", f);
		Maybe<float> getField2042() => class252_dyn.Get<Maybe<float>>("field_2042");
		void setField2042(Maybe<float> f) => class252_dyn.Set("field_2042", f);
		float getField2043() => class252_dyn.Get<float>("field_2043");
		void setField2043(float f) => class252_dyn.Set("field_2043", f);

		// reimplements the method
		Vignette vignette = class264.method_712();
		VignetteEvent vignetteEvent1 = vignette.field_4125[0][getField2040()];
		
		float a = 0.0f;
		if (getField2042().method_1085())
		{
			setField2041(getField2041() + timeDelta);
			a = class_162.method_406(getField2041() / getField2042().method_1087());
			if ((double)a >= 1.0)
			{
				setField2042((Maybe<float>)struct_18.field_1431);
				foreach (var class253 in getField2039())
				{
					if (class253.field_2048.method_1085())
					{
						class253.field_2046 = class253.field_2048.method_1087().field_2053;
						class253.field_2047 = class253.field_2048.method_1087().field_2055;
						class253.field_2048 = (Maybe<class_252.class_255>)struct_18.field_1431;
					}
				}
			}
		}

		setField2043(getField2043() + timeDelta);

		Texture background = class_238.field_1989.field_73; // transparent background
		string location = string.Empty;

		foreach (var cutscene in MainClass.AdvancedContent.Cutscenes.Where(x => x.ID == cutsceneID))
		{
			location = cutscene.FromModel().Item1;
			background = cutscene.FromModel().Item2;
		}

		Texture class256_1 = background;
		string str = location;

		class_135.method_279(Color.Black, Vector2.Zero, class_115.field_1433);
		float num = class_115.field_1433.Y / class256_1.field_2056.Y;
		Vector2 vector2_1 = class256_1.field_2056.ToVector2() * num;
		class_135.method_263(class256_1, Color.White, class_115.field_1433 / 2 - vector2_1 / 2, vector2_1);
		Vector2 vector2_2 = new Vector2(class_115.field_1433.X / 2f - (class_238.field_1989.field_84.field_531.field_2056.X / 2), class_115.field_1433.Y - 87f);
		class_135.method_272(class_238.field_1989.field_84.field_531, vector2_2.Rounded());
		class_135.method_290(str, new Vector2(class_115.field_1433.X / 2f, class_115.field_1433.Y - 68f), class_238.field_1990.field_2146, class_181.field_1718, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);

		class_252.class_254 class254 = new class_252.class_254();
		class254.field_2050 = class252_self;
		class254.field_2051 = true;
		foreach (class_252.class_253 class253 in getField2039())
		{
			Vector2 vector2_3 = ((class253.field_2048.method_1085() ? class_162.method_413(class253.field_2048.method_1087().field_2054, class253.field_2048.method_1087().field_2055, class_162.method_417(0.0f, 1f, a)) : class253.field_2047) + new Vector2(method_680(), method_681()) / 2).Rounded();
			bool flag = vignetteEvent1.method_2215() && vignetteEvent1.method_2218().field_4136 == class253.field_2046;
			enum_1 enum1 = class253.field_2049 ? (enum_1)1 : (enum_1)0;
			Vector2 vector2_4 = flag ? new Vector2(260f, 395f) : new Vector2(245f, 372f);
			Vector2 vector2_5 = flag ? new Vector2(-132f, -174f) : new Vector2(-124f, -163f);
			if (method_678())
			{
				vector2_4 = flag ? new Vector2(197f, 300f) : new Vector2(185f, 281f);
				vector2_5 = flag ? new Vector2(-100f, -132f) : new Vector2(-94f, -123f);
			}
			Vector2 vector2_6 = new Vector2(2f, 2f);
			class_135.method_266(class253.field_2046.field_1955, Color.White, vector2_3 + vector2_5 - vector2_6, vector2_4 + vector2_6 * 2, Bounds2.WithSize(0.0f, 0.0f, 1f, 1f), (enum_130)0, enum1);
			if (class253.field_2048.method_1085())
				class_135.method_266(class253.field_2048.method_1087().field_2053.field_1955, Color.White.WithAlpha(a), vector2_3 + vector2_5 - vector2_6, vector2_4 + vector2_6 * 2, Bounds2.WithSize(0.0f, 0.0f, 1f, 1f), (enum_130)0, enum1);
			class_256 class256_2 = flag ? class_238.field_1989.field_84.field_532 : class_238.field_1989.field_84.field_533;
			Vector2 vector2_7 = new Vector2(method_680(), method_681());
			Vector2 vector2_8 = vector2_3 - vector2_7 / 2;
			Color white = Color.White;
			Vector2 vector2_9 = vector2_8.Rounded();
			Vector2 vector2_10 = vector2_7;
			class_135.method_263(class256_2, white, vector2_9, vector2_10);
			float y = flag ? -206f : -193f;
			class_1 class1 = class_238.field_1990.field_2144;
			if (method_678())
			{
				y = flag ? -157f : -148f;
				class1 = class_238.field_1990.field_2143;
			}
			class_135.method_290(class253.field_2046.field_1954.method_1060(), vector2_3 + new Vector2(0.0f, y), class1, Color.White, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, -2, Color.Black, class_238.field_1989.field_99.field_706.field_751, int.MaxValue, false, true);
		}

		void method_685(VignetteEvent.LineFields param_4177)
		{
			float field_2045 = 0.005f; // static readonly float in class_252
			Vector2 vector2 = class_115.field_1433 / 2 + new Vector2((-class_238.field_1989.field_84.field_534.field_2056.X / 2), -424f);
			if (method_678())
				vector2.Y += 58f;
			int num = (int)(getField2043() / field_2045);
			class_135.method_272(class_238.field_1989.field_84.field_534, vector2.Rounded());
			class_135.method_290(param_4177.field_4136.field_1954.method_1060(), vector2 + new Vector2(254f, 172f), class_238.field_1990.field_2144, class_181.field_1719, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			class_135.method_292(param_4177.field_4093, vector2 + new Vector2(98f, 109f), class_238.field_1990.field_2145, class_181.field_1719, (enum_0)0, 1f, 0.6f, 780f, float.MaxValue, 0, new Color(), null, num);
			class254.field_2051 = getField2043() > field_2045 * (double)param_4177.field_4093.Length + 0.100000001490116;
		}

		vignetteEvent1.method_2221(new Action<VignetteEvent.LineFields>(method_685));

		bool ESCPressed = class_115.method_198(SDL.enum_160.SDLK_ESCAPE);
		if (GameLogic.field_2434.field_2451.method_575(class264) && ESCPressed)
		{
			method_684(class264);
		}

		//redid the early-return logic so it's easier to read
		bool leftClickPressed = class_115.method_206((enum_142)1);
		bool tabPressed = class_115.method_198(SDL.enum_160.SDLK_TAB);
		bool spacePressed = class_115.method_198(SDL.enum_160.SDLK_SPACE);
		bool flagAdvanceDialogue = leftClickPressed || tabPressed || spacePressed;

		bool flag2042 = getField2042().method_1085();
		bool flag254 = !class254.field_2051;
		if (flag254 || flag2042 || !flagAdvanceDialogue)
		{
			return;
		}
		//otherwise, we can advance the cutscene
		void method_686(VignetteEvent.struct_131 param_4178)
		{
			setField2041(0f);
			setField2042((Maybe<float>)0.5f);
			foreach (class_252.class_253 class253 in getField2039())
				class253.field_2048 = (Maybe<class_252.class_255>)new class_252.class_255()
				{
					field_2052 = class253.field_2046,
					field_2053 = class253.field_2046,
					field_2054 = class253.field_2047
				};
			Vector2 vector2 = method_683(0, 1);
			vector2.X = class_115.field_1433.X;
			getField2039().Add(new class_252.class_253()
			{
				field_2046 = param_4178.field_4133,
				field_2047 = vector2,
				field_2048 = (Maybe<class_252.class_255>)new class_252.class_255()
				{
					field_2052 = param_4178.field_4133,
					field_2053 = param_4178.field_4133,
					field_2054 = vector2
				}
			});
			for (int index = 0; index < getField2039().Count; ++index)
			{
				getField2039()[index].field_2048.method_1087().field_2055 = method_683(index, getField2039().Count);
			}
		}
		void method_687(VignetteEvent.struct_132 param_4179)
		{
			setField2041(0f);
			setField2042((Maybe<float>)0.5f);
			foreach (class_252.class_253 class253 in getField2039())
			{
				if (class253.field_2046 == param_4179.field_4134)
				{
					class253.field_2048 = (Maybe<class_252.class_255>)new class_252.class_255()
					{
						field_2052 = param_4179.field_4134,
						field_2053 = param_4179.field_4135,
						field_2054 = class253.field_2047,
						field_2055 = class253.field_2047
					};
				}
			}
		}

		if (getField2040() == vignette.field_4125[0].Count - 1)
		{
			method_684(class264);
		}
		else
		{
			incrementField2040();
			setField2043(0f);
			VignetteEvent vignetteEvent2 = vignette.field_4125[0][getField2040()];
			vignetteEvent2.method_2222(new Action<VignetteEvent.struct_131>(method_686));
			vignetteEvent2.method_2223(new Action<VignetteEvent.struct_132>(method_687));
			if (getField2042().method_1085())
			{
				incrementField2040();
				class_162.method_403(getField2040() < vignette.field_4125[0].Count && vignette.field_4125[0][getField2040()].method_2215(), "There must be a line after a non-line vignette event.");
			}
		}
		class_238.field_1991.field_1824.method_28(1f);
	}
}