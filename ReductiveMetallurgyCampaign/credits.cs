//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
using SDL2;
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
using Font = class_1;

public sealed class RMCCreditsScreen : IScreen
{
	private float timer;

	static class_124 transitionInstant = new class_124()
	{
		field_1458 = 0f,
		field_1459 = Transitions.field_4108.field_1459,
		field_1460 = Transitions.field_4108.field_1460
	};
	private static bool transitioningBackToMenu;
	public static Vector2 screenResolution => class_115.field_1433;
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
		Font crimson_21 = class_238.field_1990.field_2146;
		Font crimson_16_5 = class_238.field_1990.field_2145;
		Font font = bigFont ? crimson_21 : crimson_16_5;
		return class_135.method_290(str, pos, font, class_181.field_1718.WithAlpha(alpha), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
	}

	public void method_47(bool param_4523)
	{
		transitioningBackToMenu = false;
		var creditsSong = class_238.field_1992.field_973;
		creditsSong.field_1741 = 1.0;
		GameLogic.field_2434.field_2443.method_673(creditsSong);
	}

	public void method_48()	{}

	public bool method_1037() => true;

	public void method_50(float timeDelta)
	{
		timer += timeDelta;

		Texture background = CutscenePatcher.creditsBackground;
		float scalar = screenResolution.Y / background.field_2056.Y;
		Vector2 normedSize = background.field_2056.ToVector2() * scalar;
		class_135.method_279(Color.Black, Vector2.Zero, screenResolution);
		class_135.method_263(background, Color.White, screenResolution / 2 - normedSize / 2, normedSize);

		var creditsModel = MainClass.AdvancedContent.Credits;
		scalar = screenResolution.Y / 2160f;
		Vector2 textPosition = screenResolution / 2 + ModelHelpersRMC.Vector2FromString(creditsModel.PositionOffset) * scalar;

		class_310 class310 = new class_310();
		class310.incrementTimer(2f);
		var credits = creditsModel.Texts;
		foreach (var entry in credits)
		{
			addCreditFrame(class310, textPosition, entry);
		}
		class310.actions.ForEach(x => x());

		if (transitioningBackToMenu) return;

		if ((getCreditsSeen_RMC() && Input.IsSdlKeyPressed(SDL.enum_160.SDLK_ESCAPE)) || timer >= class310.time)
		{
			exitCredits();
		}
	}

	public sealed class class_310
	{
		public float time;
		public List<Action> actions = new List<Action>();
		public void incrementTimer(float amount) => this.time += amount;
	}
	private void addCreditFrame(class_310 class310, Vector2 position, List<string> credit)
	{
		float fadeTime = 0.5f;
		float gapTime = 0.5f;
		float drawfullTime = 5f;

		float num = fadeTime + drawfullTime + fadeTime + gapTime;
		float time = class310.time;
		if (timer >= time && timer < time + num)
		{
			float drawTime = 1f;
			if (timer < time + fadeTime)
				drawTime = class_162.method_416(timer, time, time + fadeTime, 0f, 1f);
			else if (timer > time + fadeTime + drawfullTime)
				drawTime = class_162.method_416(timer, time + fadeTime + drawfullTime, time + fadeTime + drawfullTime + fadeTime, 1f, 0f);
			class310.actions.Add(() => new creditFrame(position, credit).draw(drawTime * 0.9f));
		}
		class310.incrementTimer(num);
	}
	public sealed class creditFrame
	{
		public Vector2 origin;
		public List<string> texts;
		Vector2 nextLineOffset = new Vector2(0f, -32f);
		Vector2 initialOffset = new Vector2(0f, 13f);

		public creditFrame(Vector2 origin, List<string> texts)
		{
			this.origin = origin;
			this.texts = texts;
		}

		internal void draw(float alpha)
		{
			Vector2 pos = origin + initialOffset * (texts.Count - 1);
			for(int i = 0; i < texts.Count; i++)
			{
				drawCreditText(texts[i], pos, i == 0, alpha);
				pos += nextLineOffset;
			}
		}
	}
}