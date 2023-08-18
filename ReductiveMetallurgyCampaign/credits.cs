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
using Font = class_1;

public sealed class RMCCreditsScreen : IScreen
{
	private static readonly float field_2392 = 0.5f;
	private static readonly float field_2393 = 0.5f;
	private float timer;

	static class_124 transitionInstant = new class_124()
	{
		field_1458 = 0f,
		field_1459 = Transitions.field_4108.field_1459,
		field_1460 = Transitions.field_4108.field_1460
	};
	private static bool transitioningBackToMenu;
	private static void setCreditsSeen_RMC() => GameLogic.field_2434.field_2451.field_1929.method_858("RMC-CreditsSeen", true.method_453());
	private static bool getCreditsSeen_RMC() => GameLogic.field_2434.field_2451.field_1929.method_862(new delegate_384<bool>(bool.TryParse), "RMC-CreditsSeen").method_1090(false);
	private static void exitCredits()
	{
		setCreditsSeen_RMC();
		// get rid of all documents that might be waiting for us after we exit the credits - not sure if it's needed
		GameLogic.field_2434.method_951<DocumentScreen>();
		// transition out
		GameLogic.field_2434.method_947((Maybe<class_124>)transitionInstant, (Maybe<class_124>)transitionInstant);
		// only trigger this codeblock once
		transitioningBackToMenu = true;
	}


	static Bounds2 drawCreditText(string str, Vector2 pos, bool bigFont, float alpha)
	{
		Font font = bigFont ? class_238.field_1990.field_2146 : class_238.field_1990.field_2145;
		return class_135.method_290(str, pos, font, class_181.field_1718.WithAlpha(alpha), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
	}

	public void method_47(bool param_4523)
	{
		transitioningBackToMenu = false;
		var creditsSong = class_238.field_1992.field_973;
		creditsSong.field_1741 = 1.0;
		GameLogic.field_2434.field_2443.method_673(creditsSong);
	}

	public void method_48()
	{
	}

	public bool method_1037() => true;

	public void method_50(float timeDelta)
	{
		timer += timeDelta;
		Texture field541 = CutscenePatcher.creditsBackground;
		float num = class_115.field_1433.Y / field541.field_2056.Y;
		Vector2 vector2_1 = field541.field_2056.ToVector2() * num;
		class_135.method_279(Color.Black, Vector2.Zero, class_115.field_1433);
		class_135.method_263(field541, Color.White, class_115.field_1433 / 2 - vector2_1 / 2, vector2_1);
		Vector2 vector2_2 = class_115.field_1433 / 2 + new Vector2(-946f, -710f) * num;
		class_310 class310 = new class_310();
		method_887(class310, 2f);
		method_882(class310, vector2_2);
		method_883(class310, vector2_2, "RP0", "Writing, level design");
		method_883(class310, vector2_2, "mr\\_puzzel", "Programming, writing");
		method_883(class310, vector2_2, "zorflax", "Playtesting, level design");
		//method_883(class310, vector2_2, "Zach Barth", "Game design, programming");
		//method_883(class310, vector2_2, "Keith Holman", "Programming, game design");
		//method_883(class310, vector2_2, "Kyle Steed", "Art, graphic design");
		//method_883(class310, vector2_2, "Jonathan Stroh", "Art, graphic design");
		//method_883(class310, vector2_2, "Steffani Charano", "Concept art");
		//method_883(class310, vector2_2, "Matthew S. Burns", "Writing, music");
		method_884(class310, vector2_2);
		method_885(class310, vector2_2);
		method_887(class310, 1f);
		class310.field_2398.ForEach(class_301.field_2407 ?? (class_301.field_2407 = new Action<Action>(class_301.field_2343.method_893)));

		if (transitioningBackToMenu) return;

		if ((getCreditsSeen_RMC() && Input.IsSdlKeyPressed(SDL.enum_160.SDLK_ESCAPE)) || timer >= class310.field_2397)
		{
			exitCredits();
		}
	}


	public sealed class class_310
	{
		public float field_2397;
		public List<Action> field_2398 = new List<Action>();
	}
	private void method_882(class_310 param_4525, Vector2 param_4526) => method_886(param_4525, 5f, new Action<float>(new class_309()
	{
		field_2396 = param_4526
	}.method_888));

	private void method_883(
	  class_310 param_4527,
	  Vector2 param_4528,
	  string param_4529,
	  string param_4530)
	{
		method_886(param_4527, 3.5f, new Action<float>(new class_315()
		{
			field_2404 = param_4529,
			field_2405 = param_4528,
			field_2406 = param_4530
		}.method_892));
	}

	private void method_884(class_310 param_4531, Vector2 param_4532) => method_886(param_4531, 3.5f, new Action<float>(new class_314()
	{
		field_2403 = param_4532
	}.method_891));

	private void method_885(class_310 param_4533, Vector2 param_4534) => method_886(param_4533, 5f, new Action<float>(new class_313()
	{
		field_2402 = param_4534
	}.method_890));

	private void method_886(
	  class_310 param_4535,
	  float param_4536,
	  Action<float> param_4537)
	{
		class_312 class312 = new class_312();
		class312.field_2401 = param_4537;
		float num = field_2392 + param_4536 + field_2392 + field_2393;
		if (timer >= param_4535.field_2397 && timer < param_4535.field_2397 + num)
		{
			class_311 class311 = new class_311();
			class311.field_2400 = class312;
			class311.field_2399 = 1f;
			if (timer < param_4535.field_2397 + field_2392)
				class311.field_2399 = class_162.method_416(timer, param_4535.field_2397, param_4535.field_2397 + field_2392, 0f, 1f);
			else if (timer > param_4535.field_2397 + field_2392 + param_4536)
				class311.field_2399 = class_162.method_416(timer, param_4535.field_2397 + field_2392 + param_4536, param_4535.field_2397 + field_2392 + param_4536 + field_2392, 1f, 0f);
			param_4535.field_2398.Add(new Action(class311.method_889));
		}
		param_4535.field_2397 += num;
	}

	private void method_887(class_310 param_4538, float param_4539) => param_4538.field_2397 += param_4539;

	public sealed class class_309
	{
		public Vector2 field_2396;
		internal void method_888(float alpha) => drawCreditText("*Reductive Metallurgy*", field_2396, true, alpha);
	}

	public sealed class class_311
	{
		public float field_2399;
		public class_312 field_2400;

		internal void method_889() => field_2400.field_2401(field_2399 * 0.9f);
	}

	public sealed class class_312
	{
		public Action<float> field_2401;
	}

	public sealed class class_313
	{
		public Vector2 field_2402;

		internal void method_890(float alpha) => drawCreditText("And thanks to you, for playing!", field_2402, true, alpha);
	}

	public sealed class class_314
	{
		public Vector2 field_2403;

		internal void method_891(float alpha)
		{
			drawCreditText("Special Thanks", field_2403 + new Vector2(0f, 27f), true, alpha);
			drawCreditText("Zachtronics, for this wondrous game", field_2403 + new Vector2(0f, -5f), false, alpha);
			drawCreditText("Luna, for the Quintessential mod loader", field_2403 + new Vector2(0f, -37f), false, alpha);
		}
	}

	public sealed class class_315
	{
		public string field_2404;
		public Vector2 field_2405;
		public string field_2406;

		internal void method_892(float alpha)
		{
			drawCreditText(field_2404, field_2405 + new Vector2(0f, 12f), true, alpha);
			drawCreditText(field_2406, field_2405 + new Vector2(0f, -20f), false, alpha);
		}
	}

	[Serializable]
	public sealed class class_301
	{
		public static readonly class_301 field_2343 = new class_301();
		public static Action<Action> field_2407;

		internal void method_893(Action param_4543) => param_4543();
	}
}



