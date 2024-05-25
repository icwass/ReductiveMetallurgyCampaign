using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Globalization;
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
//using Font = class_1;

public static class Amalgamate
{
	private static IDetour hook_SolutionEditorBase_method_2021;
	public const string levelID = "rmc-ch3-metallurgist-opus";
	const string soundName = "rmc_sim_crash";
	const string Amalgamate_ErrorTimerField = "ReductiveMetallurgyCampaign_Amalgamate_ErrorTimerField";
	const string Amalgamate_ErrorGraphicsField = "ReductiveMetallurgyCampaign_Amalgamate_ErrorGraphicsField";
	const float fadecrashLength = 6f;
	const float fadebackLength = 3f;
	static Sound sim_crash;
	static Texture texture_board => class_238.field_1989.field_82.field_283;
	static Texture texture_boardOverlay;
	static Random randomizer = new Random();

	static bool erroringOut = false;
	/////////////////////////////////////////////////////////////////////////////////////////////////
	// helpers
	static bool isFinalePuzzle(SolutionEditorScreen ses_self) => ses_self.method_502().method_1934().field_2766 == levelID;
	static bool thereIsAtLeastOneOutput(SolutionEditorScreen ses_self) => ses_self.method_502().field_3919.Any(x => x.method_1159().method_309());
	private static void setFinaleSeen_RMC() => GameLogic.field_2434.field_2451.field_1929.method_858("RMC-FinaleSeen", true.method_453());
	private static bool getFinaleSeen_RMC() => GameLogic.field_2434.field_2451.field_1929.method_862(new delegate_384<bool>(bool.TryParse), "RMC-FinaleSeen").method_1090(false);

	static T fetchErrorData<T>(SolutionEditorBase seb_self, string field)
	{
		var data = new DynamicData(seb_self).Get(field);
		if (data == null)
		{
			erroringOut = false;
			return default(T);
		}
		else
		{
			return (T) data;
		}
	}
	static float fetchErroringTimer(SolutionEditorBase seb_self) => fetchErrorData<float>(seb_self, Amalgamate_ErrorTimerField);
	static ErrorGraphics fetchErrorGraphics(SolutionEditorBase seb_self) => fetchErrorData<ErrorGraphics>(seb_self, Amalgamate_ErrorGraphicsField);

	static void setErrorData<T>(SolutionEditorBase seb_self, string field, T data)
	{
		new DynamicData(seb_self).Set(field, data);
		erroringOut = true;
	}
	static void setErroringTimer(SolutionEditorBase seb_self, float f) => setErrorData(seb_self, Amalgamate_ErrorTimerField, f);
	static void setErrorGraphics(SolutionEditorBase seb_self, ErrorGraphics meg) => setErrorData(seb_self, Amalgamate_ErrorGraphicsField, meg);

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// public functions
	public static void PostLoad()
	{
		LoadCustomSound();
		texture_boardOverlay = class_235.method_615("amalgamate/board");

		On.class_135.method_263 += drawAmalgamatedBoard;
		IL.SolutionEditorScreen.method_50 += winscreenRedirect;
		On.SolutionEditorScreen.method_50 += drawErrors;

		//------------------------- HOOKING -------------------------//
		hook_SolutionEditorBase_method_2021 = new Hook(MainClass.PrivateMethod<SolutionEditorBase>("method_2021"), drawMoleculeErrorMolecules);
	}

	public static void Unload()
	{
		hook_SolutionEditorBase_method_2021.Dispose();
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////
	// hooking
	private delegate void orig_SolutionEditorBase_method_2021(SolutionEditorBase seb_self, Vector2 param_5627);
	private static void drawMoleculeErrorMolecules(orig_SolutionEditorBase_method_2021 orig, SolutionEditorBase seb_self, Vector2 offset)
	{
		orig(seb_self, offset);

		var ErrorGraphics = fetchErrorGraphics(seb_self);
		if (!erroringOut || ErrorGraphics == null) return;
		var timer = fetchErroringTimer(seb_self);
		ErrorGraphics.drawMolecules(timer - 1f);
	}

	private static void drawAmalgamatedBoard(On.class_135.orig_method_263 orig, Texture texture, Color color, Vector2 position, Vector2 offset)
	{
		orig(texture, color, position, offset);
		if (erroringOut && texture == texture_board) orig(texture_boardOverlay, color, position, offset);
	}
	private static void LoadCustomSound()
	{
		// load the custom sound
		foreach (var dir in QuintessentialLoader.ModContentDirectories)
		{
			string filepath = Path.Combine(dir, "Content/amalgamate/" + soundName + ".wav");
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
		dictionary[soundName] = 0.5f;

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
				bool canTriggerAmalgamateCrash = isFinalePuzzle(ses_self) && thereIsAtLeastOneOutput(ses_self) && !getFinaleSeen_RMC();


				if (!reverifyingSolutions)
				{
					GameLogic.field_2434.field_2443.method_675(1f); // not sure what this is, but i'll leave it alone
					if (!canTriggerAmalgamateCrash)
					{
						new DynamicData(ses_self).Get<Sound>("field_4006").method_28(1f);// solve fanfare
					}
				}

				ses_self.field_4017 = (Maybe<int>)sim.method_1818();
				ses_self.field_4018 = (Maybe<int>)ses_self.method_2128();
				ses_self.field_4016 = true;
				if (canTriggerAmalgamateCrash && !reverifyingSolutions)
				{
					triggerAmalgamateCrash(ses_self);
					setFinaleSeen_RMC();
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
	// main functions
	private static void triggerAmalgamateCrash(SolutionEditorScreen ses_self)
	{
		//not sure if we need this - why did i have it, originally?
		//GameLogic.field_2434.field_2464 = false;

		// save solution stats
		new class_257(ses_self, true);

		// fetch resources, run calculations
		var song_cruelty = class_238.field_1992.field_977;
		var vignette = new class_252(new class_264("rmc-cutscene-hubris"), song_cruelty);
		calculateFinaleData(ses_self);

		// start the erroring timer
		setErroringTimer(ses_self, 0f);

		//manually push and pop screens so the next transition will go directly to the cutscene
		var GAME = GameLogic.field_2434;
		var editorScreen = GAME.method_938(); // PEEK
		GAME.method_950(2); // POP 2 screens
		GAME.method_946(vignette); // PUSH
		GAME.method_946(editorScreen); // PUSH

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
		sim_crash.method_28(1f);
		GameLogic.field_2434.field_2443.method_673(song_cruelty);
	}

	private static void calculateFinaleData(SolutionEditorScreen ses_self)
	{
		//
		var maybeSim = new DynamicData(ses_self).Get<Maybe<Sim>>("field_4022");
		if (!maybeSim.method_1085())
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Finale sequence was triggered, even though there is no Sim. How did that even happen?");
			throw new Exception("calculateFinaleData: Expected a Sim, found none.");
		}
		var sim = maybeSim.method_1087();
		var solution = ses_self.method_502();
		var partList = solution.field_3919;
		var partSimStates = sim.field_3821;
		var moleculeList = sim.field_3823;

		// fetch hexes that are NOT open to air
		HashSet<HexIndex> glyphHexes = new();
		HashSet<HexIndex> inputHexes = new();
		foreach (var part in partList)
		{
			var partSimState = partSimStates[part];

			// make all arms let go of their molecules
			foreach (var gripper in part.field_2696)
			{
				var gripperState = partSimStates[gripper];
				if (gripperState.field_2728)
				{
					// gripper is open now
					gripperState.field_2728 = false;
					// and no longer holding a molecule
					gripperState.field_2729 = (Maybe<Molecule>)struct_18.field_1431;
				}
			}
			bool isInput = part.method_1159().field_1541;
			HashSet<HexIndex> hexes = part.method_1186(solution);

			var masterSet = isInput ? inputHexes : glyphHexes;
			masterSet.UnionWith(hexes);
		}
		
		// find molecules that we could use for ErrorGraphics
		var viableMolecules = new List<Molecule>();
		var inputReverseMolecules = new List<Molecule>();
		var lessViableMolecules = new Dictionary<Molecule, List<HexIndex>>();
		foreach (var molecule in moleculeList)
		{
			var atomDict = molecule.method_1100();
			var atomHexes = atomDict.Keys;
			if (atomHexes.Count() == 1 && inputHexes.Contains(atomHexes.First()))
			{
				inputReverseMolecules.Add(molecule);
			}
			else
			{
				var viableHexes = new List<HexIndex>();
				foreach (var hex in atomHexes.Where(x => !glyphHexes.Contains(x) && !inputHexes.Contains(x)))
				{
					viableHexes.Add(hex);
				}

				if (viableHexes.Count == atomDict.Keys.Count())
				{
					viableMolecules.Add(molecule);
				}
				else if (viableHexes.Count > 0)
				{
					lessViableMolecules.Add(molecule, viableHexes);
				}
			}
		}

		ErrorGraphics errorGraphics;

		if (viableMolecules.Count > 0)
		{
			// make molecules sink into the board
			foreach (var molecule in viableMolecules)
			{
				moleculeList.Remove(molecule);
			}
			errorGraphics = new ErrorGraphics(ses_self, "Molecules may not sublimate through transmutation surfaces.", viableMolecules);
		}
		else if (lessViableMolecules.Count > 0)
		{
			//make parts of molecules sink into the board
			var partialMolecules = new List<Molecule>();
			foreach (var kvp in lessViableMolecules)
			{
				var molecule = kvp.Key;
				var viableHexes = kvp.Value;
				// clone molecule
				var newMolecule = molecule.method_1104();
				// remove unviable hexes from new molecule
				foreach (var hex in molecule.method_1100().Keys.Where(x => !viableHexes.Contains(x)))
				{
					newMolecule.method_1107(hex);
				}
				// remove viable hexes from original molecule
				foreach (var hex in viableHexes)
				{
					molecule.method_1107(hex);
				}
				partialMolecules.Add(newMolecule);
			}
			errorGraphics = new ErrorGraphics(ses_self, "Atoms may not sublimate through transmutation surfaces.", partialMolecules);
		}
		else if (inputReverseMolecules.Count > 0)
		{
			// make atoms go back through the inputs
			foreach (var molecule in inputReverseMolecules)
			{
				moleculeList.Remove(molecule);
			}
			errorGraphics = new ErrorGraphics(ses_self, "Reagents may not backfeed into input vials.", inputReverseMolecules);
		}
		else if (thereIsAtLeastOneOutput(ses_self))
		{
			// make atoms come back through the output
			var outputMolecules = new List<Molecule>();
			foreach (var part in partList.Where(x => x.method_1159().method_309()))
			{
				// clone molecule from puzzle definition
				var molecule = part.method_1185(solution).method_1104();
				// rotate
				molecule = molecule.method_1115(part.method_1163());
				// translate
				molecule = molecule.method_1117(part.method_1161());
				// and add to list
				outputMolecules.Add(molecule);
			}

			errorGraphics = new ErrorGraphics(ses_self, "Products may not backfeed into the transmutation engine.", outputMolecules, true);
		}
		else
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Finale sequence was triggered, even though there are no output parts. How did that even happen?");
			throw new Exception("calculateFinaleData: Expected at lease one output part, found none.");
		}

		setErrorGraphics(ses_self, errorGraphics);
	}

	class ErrorGraphics
	{
		SolutionEditorScreen ses;
		Vector2 ses_origin;
		Vector2 ses_target;
		bool drawMoleculeErrorBackwards;
		string errorMessageMolecule;
		string errorMessageInstruction;
		string errorMessageArmsPart1;
		string errorMessageArmsPart2;
		List<Molecule> molecules;
		List<HexIndex> armHexes = new();
		Dictionary<Part, CompiledProgram> programDict = null;
		HexIndex anchor(Molecule molecule) => molecule.method_1100().First().Key;
		Vector2 offset(HexIndex hex) => class_187.field_1742.method_491(hex, ses.field_4009);
		Vector2 offset(Molecule molecule) => offset(anchor(molecule));
		Vector2 resolution => class_115.field_1433;

		float clampTimer(float t) => Math.Max(0f, Math.Min(t, 1f));

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

		public ErrorGraphics(SolutionEditorScreen ses, string errorMessage, List<Molecule> molecules, bool drawBackwards = false)
		{
			InstructionJumbler.initializePool();
			this.ses = ses;
			this.ses_origin = ses.field_4009;
			this.ses_target = ses.field_4009;

			// we are given the molecule error information
			this.errorMessageMolecule = errorMessage;
			this.molecules = molecules;
			this.drawMoleculeErrorBackwards = drawBackwards;

			Vector2 moleculePosition = offset(molecules.First());
			this.ses_target = ses.field_4009 - moleculePosition + resolution * 0.5f;

			// easter egg error messages
			this.errorMessageInstruction = "Error 0x7880: TEngine.InstructionOutOfRangeException";
			this.errorMessageArmsPart1 = "Error 0x0243: TEngine.NullEssenceException";
			this.errorMessageArmsPart2 = "Error 0x3704: TEngine.VialOverflowException";

			//determine the instruction error information
			var maybeSim = new DynamicData(ses).Get<Maybe<Sim>>("field_4022");
			if (!maybeSim.method_1085())
			{
				// why would there be no sim? but just in case...
				// if no sim, then no instructions or arms - keep the easter egg messages
				return;
			}

			var sim = maybeSim.method_1087();
			var solution = ses.method_502();
			var partList = solution.field_3919;
			var partSimStates = sim.field_3821;
			var compiledProgramGrid = sim.method_1820();
			var period = 0;
			if (partList.Count > 0) period = compiledProgramGrid.method_851(partList[0]).field_2367.Length;
			if (period > 0)
			{
				this.programDict = new DynamicData(compiledProgramGrid).Get<Dictionary<Part, CompiledProgram>>("field_2368");
				this.errorMessageInstruction = "Instruction trays must not become corrupted.";
			}

			//determine the arm error information

			foreach (var arm in partList.Where(x => x.method_1159().field_1534.Length > 0))
			{
				this.armHexes.Add(partSimStates[arm].field_2734);
			}

			if (this.armHexes.Count > 0)
			{
				this.errorMessageArmsPart1 = "Arms may not rotate in two different directions at once.";
				this.errorMessageArmsPart2 = "Arms cannot operate asynchronously.";
			}
		}

		public void moveCamera(float t)
		{
			var timer = clampTimer(t);
			if (timer > 0f && timer < 1f)
			{
				float lerp = 1f - (float)Math.Pow(1 - timer, 2);
				ses.field_4009 = (1 - lerp) * ses_origin + lerp * ses_target;
			}
			else if (!ses_origin.Equals(ses_target) && timer > 1f)
			{
				ses.field_4009 = ses_target;
				ses_origin = ses_target;
			}
		}

		public void drawMolecules(float t)
		{
			var timer = clampTimer(t);
			var depth = 1 - timer;
			if (drawMoleculeErrorBackwards)
			{
				depth = timer;
				if (timer < 0.2f) return;
			}
			foreach (var molecule in molecules)
			{
				Editor.method_925(molecule, offset(molecule), anchor(molecule), 0f, 1f, depth, 1f, false, null);
			}
		}

		public void drawMoleculeError(float errorPercent)
		{
			List<Vector2> errorSquares = new();
			foreach (var molecule in molecules)
			{
				foreach (var hex in molecule.method_1100().Keys)
				{
					errorSquares.Add(offset(hex));
				}
			}

			drawErrorBox(ses, errorMessageMolecule, 0.5f, 0.5f, errorSquares.ToArray(), errorPercent);
		}

		public void drawInstructionError(float errorPercent)
		{
			drawErrorBox(ses, errorMessageInstruction, 0.73f, 0.08f, new Vector2[] { new Vector2(resolution.X * 0.7f, resolution.Y * 0.2f) }, errorPercent);

			if (programDict == null || programDict.Count == 0) return;

			var upper = programDict.First().Value.field_2367.Length;

			foreach (var program in programDict.Values)
			{
				for (int i = 0; i < upper; i++)
				{
					program.field_2367[i].field_2364 = InstructionJumbler.randomInstruction();
				}
			}
		}
		public void drawArmsError_part1(float errorPercent)
		{
			List<Vector2> errorSquares = new();
			foreach (var hex in armHexes)
			{
				errorSquares.Add(offset(hex));
			}
			drawErrorBox(ses, errorMessageArmsPart1, 0.7f, 0.9f, errorSquares.ToArray(), errorPercent);
		}
		public void drawArmsError_part2(float errorPercent)
		{
			drawErrorBox(ses, errorMessageArmsPart2, 0f, 0.68f, new Vector2[] { }, errorPercent);
		}
	}

	private static void drawErrorBox(SolutionEditorScreen ses, string message, float relativeX, float relativeY, Vector2[] errorSquareLocations, float errorPercent)
	{
		var messageBoxTexture = class_238.field_1989.field_101.field_795;
		Bounds2 boxBounds = Bounds2.WithSize(ses.method_2122().BottomLeft + new Vector2(25f, -1f), new Vector2(messageBoxTexture.method_688(), 160f));
		if (class_115.field_1433.X < 1680.0) boxBounds = boxBounds.Translated(new Vector2(0.0f, 35f));

		Vector2 drawSize = class_115.field_1433 - boxBounds.BottomLeft - boxBounds.Size;
		Vector2 position = new Vector2(relativeX * drawSize.X, relativeY * drawSize.Y);
		drawErrorBox(ses, message, position, errorSquareLocations, errorPercent);
	}

	private static void drawErrorBox(SolutionEditorScreen ses, string message, Vector2 position, Vector2[] errorSquareLocations, float errorPercent)
	{
		int maxErrorOffset = 3;
		float errorOffsetX = randomizer.Next(-maxErrorOffset, maxErrorOffset+1) * errorPercent;
		float errorOffsetY = randomizer.Next(-maxErrorOffset, maxErrorOffset + 1) * errorPercent;
		var errorOffset = new Vector2(errorOffsetX, errorOffsetY);

		var errorSquareTexture = class_238.field_1989.field_99.field_704.field_763;
		var squareOffset = (errorSquareTexture.field_2056.ToVector2() / 2).Rounded();
		foreach (Vector2 errorSquarePos in errorSquareLocations)
		{
			class_135.method_272(errorSquareTexture, errorSquarePos - squareOffset + errorOffset);
		}

		var messageBoxTexture = class_238.field_1989.field_101.field_795;
		var messageBoxDivider = class_238.field_1989.field_101.field_794;
		Bounds2 bounds2_2 = Bounds2.WithSize(ses.method_2122().BottomLeft + new Vector2(25f, -1f), new Vector2(messageBoxTexture.method_688(), 160f));
		if (class_115.field_1433.X < 1680.0) bounds2_2 = bounds2_2.Translated(new Vector2(0.0f, 35f));
		bounds2_2 = bounds2_2.Translated(position + errorOffset);
		class_135.method_275(messageBoxTexture, Color.White, bounds2_2);
		class_135.method_272(messageBoxDivider, bounds2_2.Min + new Vector2(21f, 120f));
		class_135.method_290(class_134.method_253("Fatal", string.Empty).method_1060().method_441(), bounds2_2.Center + new Vector2(-2f, 50f), class_238.field_1990.field_2143, Color.FromHex(10685952), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
		class_135.method_292(message, bounds2_2.Center + new Vector2(-2f, 16f), class_238.field_1990.field_2143, class_181.field_1719, (enum_0)1, 1f, 0.6f, 300f, float.MaxValue, 0, new Color(), null, int.MaxValue);

		var method2094 = MainClass.PrivateMethod<SolutionEditorScreen>("method_2094");
		((ButtonDrawingLogic)method2094.Invoke(null, new object[] { class_134.method_253("Replay", string.Empty).method_1060(), bounds2_2.Min + new Vector2(22f, 20f), 158f })).method_824(false, true);
		((ButtonDrawingLogic)method2094.Invoke(null, new object[] { class_134.method_253("Okay", string.Empty).method_1060(), bounds2_2.Min + new Vector2(191f, 20f), 158f })).method_824(false, true);
	}


	private static void drawErrors(On.SolutionEditorScreen.orig_method_50 orig, SolutionEditorScreen ses_self, float timeDelta)
	{
		float errorTimer = fetchErroringTimer(ses_self);
		float errorPercent = Math.Max(0f, (errorTimer - 0.5f) / fadecrashLength);

		if (erroringOut)
		{
			setErroringTimer(ses_self, errorTimer + timeDelta);
		}

		//////////////////////////
		orig(ses_self, timeDelta);
		//////////////////////////
		if (!erroringOut) return;

		// timeStamp 01
		drawErrorBox(ses_self, "Unknown operating condition encountered.", new Vector2(0, 0), new Vector2[] { }, errorPercent);

		// timeStamp 02
		if (errorTimer < 1f) return;

		var errorGraphics = fetchErrorGraphics(ses_self);
		errorGraphics.moveCamera(errorTimer - 1.3f);
		errorGraphics.drawMoleculeError(errorPercent);

		// timeStamp 03
		if (errorTimer < 1.755f) return;
		errorGraphics.drawArmsError_part1(errorPercent);

		// timeStamp 04
		if (errorTimer < 2.043f) return;
		errorGraphics.drawInstructionError(errorPercent);

		// timeStamp 05
		if (errorTimer < 2.512f) return;
		errorGraphics.drawArmsError_part2(errorPercent);

		// timeStamp 06
		if (errorTimer < 2.89f) return;
		drawErrorBox(ses_self, "Internal chambers must not rupture.", 0.98f, 0.64f, new Vector2[] { }, errorPercent);

		// timeStamps 07 - 10
		var timeStamps_07_10 = new List<float>() {3.314f, 3.813f, 4.167f, 4.479f};
		for (int i = 0; i < timeStamps_07_10.Count; i++)
		{
			if (errorTimer >= timeStamps_07_10[i]) drawErrorBox(ses_self, "Transmutation engines must not amalgamate.", new Vector2(40*(i+1), -40 * (i + 1)), new Vector2[] { }, errorPercent);
		}
	}
}