using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using Quintessential.Settings;
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

public static class oldAmalgamate
{
	const string levelID = "rmc-metallurgist-opus";
	const string soundName = "rmc_sim_crash";
	const string AmalgamateTimerField = "ReductiveMetallurgyCampaign_AmalgamateTimerField";
	const float fadecrashLength = 6f;
	const float fadebackLength = 0.5f; // 3f
	static Sound sim_crash;
	static Texture texture_board => class_238.field_1989.field_82.field_283;
	static Texture texture_boardOverlay;

	static bool erroringOut = false;

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// public functions
	public static void PostLoad()
	{
		LoadCustomSound();
		string path = "amalgamate/";
		texture_boardOverlay = class_235.method_615(path + "board");
		InstructionJumbler.initializePool();

		On.class_135.method_263 += drawAmalgamatedBoard;
		IL.SolutionEditorScreen.method_50 += winscreenRedirect;
		On.SolutionEditorScreen.method_50 += drawErrors;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// hooking
	private static void drawAmalgamatedBoard(On.class_135.orig_method_263 orig, Texture texture, Color color, Vector2 position, Vector2 offset)
	{
		orig(texture, color, position, offset);
		if (erroringOut && texture == texture_board) orig(texture_boardOverlay, color, position, offset);
	}
	private static void LoadCustomSound()
	{
		// load the custom sound
		string path = "Content/amalgamate/" + soundName + ".wav";
		foreach (var dir in QuintessentialLoader.ModContentDirectories)
		{
			string filepath = Path.Combine(dir, path);
			if (File.Exists(filepath))
			{
				sim_crash = new Sound()
				{
					field_4060 = Path.GetFileNameWithoutExtension(filepath),
					field_4061 = class_158.method_375(filepath)
				};
				break;
			}
		}
		// add entry to the volume dictionary
		var field = typeof(class_11).GetField("field_52", BindingFlags.Static | BindingFlags.NonPublic);
		var dictionary = (Dictionary<string, float>)field.GetValue(null);
		dictionary.Add(soundName, 0.5f);

		// modify the method that reenables sounds after they are triggered
		void Method_540(On.class_201.orig_method_540 orig, class_201 class201_self)
		{
			orig(class201_self);
			sim_crash.field_4062 = false;
		}
		On.class_201.method_540 += Method_540;
	}
	private static void winscreenRedirect(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);
		// skip ahead to roughly where the "finished the level" code occurs
		cursor.Goto(220);

		// jump ahead to just after we start running the "finished the level" code
		if (!cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Brfalse))) return;

		// remove the code that used to be there
		cursor.RemoveRange(36);

		// load the SolutionEditorScreen self onto the stack so we can use it
		cursor.Emit(OpCodes.Ldarg_0);
		// then run the new code
		cursor.EmitDelegate<Action<SolutionEditorScreen>>((ses_self) =>
		{
			var sim = new DynamicData(ses_self).Get<Maybe<Sim>>("field_4022").method_1087();

			if (ses_self.method_503() == enum_128.Paused && !ses_self.field_4016 && sim.method_1825())
			{
				bool reverifyingSolutions = GameLogic.field_2434.field_2467.Count != 0;
				if (!reverifyingSolutions)
				{
					GameLogic.field_2434.field_2443.method_675(1f); // not sure what this is, but i'll leave it alone
					if (!isFinalePuzzle(ses_self))
					{
						new DynamicData(ses_self).Get<Sound>("field_4006").method_28(1f);// solve fanfare
					}
				}

				ses_self.field_4017 = (Maybe<int>)sim.method_1818();
				ses_self.field_4018 = (Maybe<int>)ses_self.method_2128();
				ses_self.field_4016 = true;
				if (isFinalePuzzle(ses_self) && !reverifyingSolutions)
				{
					triggerAmalgamateCrash(ses_self);
				}
				else
				{
					// run the original code
					GameLogic.field_2434.method_946(new class_257(ses_self, true));
					class_238.field_1991.field_1872.method_28(1f);
				}
			}
		});
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// helpers
	static bool isFinalePuzzle(SolutionEditorScreen ses_self) => ses_self.method_502().method_1934().field_2766 == levelID;
	static float fetchErroringTimer(SolutionEditorScreen ses_self)
	{
		var data = new DynamicData(ses_self).Get(AmalgamateTimerField);
		if (data == null)
		{
			erroringOut = false;
			return -1f;
		}
		else
		{
			return (float)data;
		}
	}
	static void setErroringTimer(SolutionEditorScreen ses_self, float f)
	{
		new DynamicData(ses_self).Set(AmalgamateTimerField, f);
		erroringOut = true;
	}
	private static void triggerAmalgamateCrash(SolutionEditorScreen ses_self)
	{
		//GameLogic.field_2434.field_2464 = false; // not sure what this does, but will include it
		// save solution stats
		new class_257(ses_self, true);

		// fetch resources
		var song_cruelty = class_238.field_1992.field_977;
		var vignette = new class_252(new class_264("rmc-cutscene-hubris"), song_cruelty);

		// start the erroring timer
		setErroringTimer(ses_self, 0f);

		////manually push and pop screens so the transition will go directly to the cutscene
		//var GAME = GameLogic.field_2434;
		//var editorScreen = GAME.method_938(); // PEEK
		//GAME.method_950(2); // POP 2 screens // DEBUG
		//GAME.method_946(vignette); // PUSH
		//GAME.method_946(editorScreen); // PUSH

		// create slow-transitions
		var field4109_slower = new class_124()
		{
			field_1458 = fadecrashLength,
			field_1459 = Transitions.field_4109.field_1459,
			field_1460 = Transitions.field_4109.field_1460
		};
		var field4108_slower = new class_124()
		{
			field_1458 = fadebackLength,
			field_1459 = Transitions.field_4108.field_1459,
			field_1460 = Transitions.field_4108.field_1460
		};

		// start transitioning from the editor to the vignette
		GameLogic.field_2434.method_947((Maybe<class_124>)field4109_slower, (Maybe<class_124>)field4108_slower);

		// play sound effect
		sim_crash.method_28(1f);
		GameLogic.field_2434.field_2443.method_673(song_cruelty);
	}
	/////////////////////////////////////////////////////////////////////////////////////////////////
	// instruction jumbling
	static void jumbleInstructions(SolutionEditorScreen ses_self, float errorPercent)
	{
		var maybeSim = new DynamicData(ses_self).Get<Maybe<Sim>>("field_4022");
		if (!maybeSim.method_1085()) return;
		var sim = maybeSim.method_1087();
		var solution = ses_self.method_502();
		var partList = solution.field_3919;
		var partSimStates = sim.field_3821;
		var compiledProgramGrid = sim.method_1820();

		var period = 0;
		if (partList.Count > 0) period = compiledProgramGrid.method_851(partList[0]).field_2367.Length;
		if (period == 0) return;

		foreach (var part in partList)
		{
			var program = compiledProgramGrid.method_851(part);

			// randomize instructions
			var upper = program.field_2367.Length * Math.Min(1, errorPercent * 1.2);
			upper = Math.Max(1, upper);

			for (int i = 0; i < upper; i++)
			{
				program.field_2367[i].field_2364 = InstructionJumbler.randomInstruction();
			}



		}
	}

	static class InstructionJumbler
	{
		static Random randomizer = new Random();
		static InstructionType[] instructionPool;

		public static void initializePool()
		{
			instructionPool = new InstructionType[]
			{
				class_169.field_1653, // blank
				class_169.field_1654, // blank2
				class_169.field_1655, // move +
				class_169.field_1656, // move -
				class_169.field_1657, // rotate cw
				class_169.field_1658, // rotate ccw
				class_169.field_1659, // extend
				class_169.field_1660, // retract
				class_169.field_1661, // pivot cw
				class_169.field_1662, // pivot ccw
				class_169.field_1663, // grab
				class_169.field_1664, // drop
			};
		}
		public static InstructionType randomInstruction() => instructionPool[randomizer.Next(instructionPool.Length)];
	}
	/////////////////////////////////////////////////////////////////////////////////////////////////
	// main functions
	private static void drawErrors(On.SolutionEditorScreen.orig_method_50 orig, SolutionEditorScreen ses_self, float timeDelta)
	{
		float errorTimer = fetchErroringTimer(ses_self);
		float errorPercent = Math.Max(0f, (errorTimer - 0.5f) / fadecrashLength);

		//var timeStamps = new List<float>() { 0f, 1f, 1.755f, 2.043f, 2.512f, 2.89f, 3.314f, 3.813f, 4.167f, 4.479f };


		if (erroringOut)
		{
			setErroringTimer(ses_self, errorTimer + timeDelta);

			if (errorTimer >= 1f) jumbleInstructions(ses_self, errorPercent);



		}
		//////////////////////////
		orig(ses_self, timeDelta);
		//////////////////////////
		if (erroringOut)
		{
			// blast error messages

			// 01
			new ErrorData(ses_self, "Unknown operating condition encountered.", new Vector2(0, 0), new Vector2[] { }).DrawSelf(errorPercent);


			// 02
			// "Atoms may not leave transmutation surfaces."
			void consumeAtomReference(/*AtomReference atomRef*/ float num)
			{
				// delete the input atom
				//atomRef.field_2277.method_1107(atomRef.field_2278);
				// draw input getting consumed
				AtomType air = class_175.field_1676;
				var hex = new HexIndex(0, 0);
				//ses_self.field_3937.Add(new class_286(ses_self, hex, air));

				Vector2 vector2 = class_187.field_1742.method_491(hex, ses_self.field_4009);
				Editor.method_927(air, vector2, 1f, 1f, num, 1f, -21f, 0.0f, null, null, false);
			}

			// this drawing needs to happen when other atoms in field_3937 get drawn
			// so they don't appear over arms
			// additionally, the atoms to draw should be determined when the error gets triggered
			var num = Math.Max(0.001f, Math.Min(1f, 1.6f - errorTimer));
			consumeAtomReference(num);



			if (errorTimer >= 1f) new ErrorData(ses_self, "Atoms may not leave transmutation surfaces.", new Vector2(600, 400), new Vector2[] { }).DrawSelf(errorPercent);

			// 03
			// "Arms cannot operate asynchronously."

			// 04
			// "Arms cannot move in two directions at once."

			// 05
			// "Arms cannot move in two directions at once."

			// 06


			// 07, 08, 09, 10
			// "This transmutation engine cannot amalgamate any further."
			// message is repeated, stacked like a Windows error
			if (errorTimer >= 3.314f) new ErrorData(ses_self, "Transmutation engines must not amalgamate.", new Vector2(40, -40), new Vector2[] { }).DrawSelf(errorPercent);
			if (errorTimer >= 3.813f) new ErrorData(ses_self, "Transmutation engines must not amalgamate.", new Vector2(80, -80), new Vector2[] { }).DrawSelf(errorPercent);
			if (errorTimer >= 4.167f) new ErrorData(ses_self, "Transmutation engines must not amalgamate.", new Vector2(120, -120), new Vector2[] { }).DrawSelf(errorPercent);
			if (errorTimer >= 4.479f) new ErrorData(ses_self, "Transmutation engines must not amalgamate.", new Vector2(160, -160), new Vector2[] { }).DrawSelf(errorPercent);



		}
	}


	struct ErrorData
	{
		const int maxErrorOffset = 5;
		static Random randomizer = new Random();

		SolutionEditorScreen ses;
		string message = string.Empty;
		Vector2 position = new Vector2(0, 0);
		Vector2[] errorSquareLocations = new Vector2[] { };

		public ErrorData(SolutionEditorScreen ses, string message, Vector2 position, Vector2[] errorSquareLocations)
		{
			this.ses = ses;
			this.message = message;
			this.position = position;
			this.errorSquareLocations = errorSquareLocations;
		}

		public void DrawSelf(float errorPercent = 0f)
		{
			float errorOffsetX = randomizer.Next(2 * maxErrorOffset) * errorPercent - maxErrorOffset;
			float errorOffsetY = randomizer.Next(2 * maxErrorOffset) * errorPercent - maxErrorOffset;
			var errorOffset = new Vector2(errorOffsetX, errorOffsetY);

			var errorSquareTexture = class_238.field_1989.field_101.field_795;
			var squareOffset = (errorSquareTexture.field_2056.ToVector2() / 2).Rounded();
			foreach (Vector2 errorSquarePos in errorSquareLocations)
			{
				class_135.method_272(errorSquareTexture, /*ses_self.field_4009 +*/ errorSquarePos - squareOffset + errorOffset);
			}

			var messageBoxTexture = class_238.field_1989.field_101.field_795;
			Bounds2 bounds2_2 = Bounds2.WithSize(ses.method_2122().BottomLeft + new Vector2(25f, -1f), new Vector2(messageBoxTexture.method_688(), 160f));
			if (class_115.field_1433.X < 1680.0) bounds2_2 = bounds2_2.Translated(new Vector2(0.0f, 35f));
			//bounds2_2 = bounds2_2.Translated(box_offset + new Vector2(randomizer.Next(10), randomizer.Next(10)));
			bounds2_2 = bounds2_2.Translated(position + errorOffset);

			class_135.method_275(class_238.field_1989.field_101.field_795, Color.White, bounds2_2);
			class_135.method_272(class_238.field_1989.field_101.field_794, bounds2_2.Min + new Vector2(21f, 120f));
			class_135.method_290(class_134.method_253("Fatal", string.Empty).method_1060().method_441(), bounds2_2.Center + new Vector2(-2f, 50f), class_238.field_1990.field_2143, Color.FromHex(10685952), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			class_135.method_292(message, bounds2_2.Center + new Vector2(-2f, 16f), class_238.field_1990.field_2143, class_181.field_1719, (enum_0)1, 1f, 0.6f, 300f, float.MaxValue, 0, new Color(), null, int.MaxValue);

			var method2094 = MainClass.PrivateMethod<SolutionEditorScreen>("method_2094");
			var replayButton = (ButtonDrawingLogic)method2094.Invoke(null, new object[] { class_134.method_253("Replay", string.Empty).method_1060(), bounds2_2.Min + new Vector2(22f, 20f), 158f });
			var okayButton = (ButtonDrawingLogic)method2094.Invoke(null, new object[] { class_134.method_253("Okay", string.Empty).method_1060(), bounds2_2.Min + new Vector2(191f, 20f), 158f });

			replayButton.method_824(false, true);
			okayButton.method_824(false, true);
		}
	}
}