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

public static class Amalgamate
{
	private static IDetour hook_SolutionEditorBase_method_2021;
	const string levelID = "rmc-metallurgist-opus";
	const string soundName = "rmc_sim_crash";
	const string Amalgamate_ErrorTimerField = "ReductiveMetallurgyCampaign_Amalgamate_ErrorTimerField";
	const string Amalgamate_MoleculeErrorGraphicsField = "ReductiveMetallurgyCampaign_Amalgamate_MoleculeErrorGraphicsField";
	const float fadecrashLength = 6f;
	const float fadebackLength = 3f;
	static Sound sim_crash;
	static Texture texture_board => class_238.field_1989.field_82.field_283;
	static Texture texture_boardOverlay;
	static Random randomizer = new Random();

	static bool erroringOut = false;
	public static bool alwaysShowFinale = false; // DEBUG
	public static bool goToSolutionsMenu = false; // DEBUG
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
	static MoleculeErrorGraphics fetchMoleculeErrorGraphics(SolutionEditorBase seb_self) => fetchErrorData<MoleculeErrorGraphics>(seb_self, Amalgamate_MoleculeErrorGraphicsField);

	static void setErrorData<T>(SolutionEditorBase seb_self, string field, T data)
	{
		new DynamicData(seb_self).Set(field, data);
		erroringOut = true;
	}
	static void setErroringTimer(SolutionEditorBase seb_self, float f) => setErrorData(seb_self, Amalgamate_ErrorTimerField, f);
	static void setMoleculeErrorGraphics(SolutionEditorBase seb_self, MoleculeErrorGraphics meg) => setErrorData(seb_self, Amalgamate_MoleculeErrorGraphicsField, meg);

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// public functions
	public static void PostLoad()
	{
		LoadCustomSound();
		texture_boardOverlay = class_235.method_615("amalgamate/board");
		//InstructionJumbler.initializePool();

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

		var MoleculeErrorGraphics = fetchMoleculeErrorGraphics(seb_self);
		if (MoleculeErrorGraphics == null) return;
		var timer = fetchErroringTimer(seb_self);
		MoleculeErrorGraphics.drawMolecules(1.3f - timer);
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
				bool canTriggerAmalgamateCrash = isFinalePuzzle(ses_self) && thereIsAtLeastOneOutput(ses_self) && (!getFinaleSeen_RMC() || alwaysShowFinale);


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
	// instruction jumbling


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
		if (!goToSolutionsMenu)
		{
			var GAME = GameLogic.field_2434;
			var editorScreen = GAME.method_938(); // PEEK
			GAME.method_950(2); // POP 2 screens // DEBUG
			GAME.method_946(vignette); // PUSH
			GAME.method_946(editorScreen); // PUSH
		}

		// create slow-transitions
		var field4109_slower = new class_124()
		{
			field_1458 = fadecrashLength,
			field_1459 = Transitions.field_4109.field_1459,
			field_1460 = Transitions.field_4109.field_1460
		};
		var field4108_slower = new class_124()
		{
			field_1458 = goToSolutionsMenu ? 0.25f : fadebackLength,
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
		
		// find molecules that we could use for MoleculeErrorGraphics
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

		MoleculeErrorGraphics moleculeErrorGraphics;

		if (viableMolecules.Count > 0)
		{
			// make molecules sink into the board
			foreach (var molecule in viableMolecules)
			{
				moleculeList.Remove(molecule);
			}
			moleculeErrorGraphics = new MoleculeErrorGraphics(ses_self, "Molecules may not leave transmutation surfaces.", viableMolecules);
		}
		else if (inputReverseMolecules.Count > 0)
		{
			// make atoms go back through the inputs
			foreach (var molecule in inputReverseMolecules)
			{
				moleculeList.Remove(molecule);
			}
			moleculeErrorGraphics = new MoleculeErrorGraphics(ses_self, "Reagents may not backfeed into input vials.", inputReverseMolecules);
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
			moleculeErrorGraphics = new MoleculeErrorGraphics(ses_self, "Atoms may not leave transmutation surfaces.", partialMolecules);
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

			moleculeErrorGraphics = new MoleculeErrorGraphics(ses_self, "Products may not backfeed into the transmutation engine.", outputMolecules, true);
		}
		else
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Finale sequence was triggered, even though there are no output parts. How did that even happen?");
			throw new Exception("calculateFinaleData: Expected at lease one output part, found none.");
		}

		setMoleculeErrorGraphics(ses_self, moleculeErrorGraphics);
	}

	class MoleculeErrorGraphics
	{
		SolutionEditorScreen ses;
		string errorMessage;
		List<Molecule> molecules;
		bool drawBackwards;
		HexIndex anchor(Molecule molecule) => molecule.method_1100().First().Key;
		Vector2 offset(HexIndex hex) => class_187.field_1742.method_491(hex, ses.field_4009);
		Vector2 offset(Molecule molecule) => offset(anchor(molecule));

		public MoleculeErrorGraphics(SolutionEditorScreen ses, string errorMessage, List<Molecule> molecules, bool drawBackwards = false)
		{
			this.ses = ses;
			this.errorMessage = errorMessage;
			this.molecules = molecules;
			this.drawBackwards = drawBackwards;
		}

		public void drawMolecules(float timer)
		{
			var num = Math.Min(1f, Math.Max(0f, timer));
			if (drawBackwards)
			{
				num = 1f - num;
				if (num < 0.2f) return;
			}

			foreach (var molecule in molecules)
			{
				Editor.method_925(molecule, offset(molecule), anchor(molecule), 0f, 1f, num, 1f, false, null);
			}
		}

		public void drawError(float errorPercent)
		{
			List<Vector2> errorSquares = new();
			foreach (var molecule in molecules)
			{
				//
				
				
				
				
				errorSquares.Add(offset(molecule));




			}

			drawErrorBox(ses, errorMessage, new Vector2(600, 400), errorSquares.ToArray(), errorPercent);
		}
	}


	private static void drawErrorBox(SolutionEditorScreen ses, string message, Vector2 position, Vector2[] errorSquareLocations, float errorPercent)
	{
		int maxErrorOffset = 5;
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
		var replayButton = (ButtonDrawingLogic)method2094.Invoke(null, new object[] { class_134.method_253("Replay", string.Empty).method_1060(), bounds2_2.Min + new Vector2(22f, 20f), 158f });
		var okayButton = (ButtonDrawingLogic)method2094.Invoke(null, new object[] { class_134.method_253("Okay", string.Empty).method_1060(), bounds2_2.Min + new Vector2(191f, 20f), 158f });

		replayButton.method_824(false, true);
		okayButton.method_824(false, true);
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


		var timeStamps_01_06 = new List<float>() { 0f, 1f, 1.755f, 2.043f, 2.512f, 2.89f };

		// timeStamp 01
		drawErrorBox(ses_self, "Unknown operating condition encountered.", new Vector2(0, 0), new Vector2[] { }, errorPercent);

		// timeStamp 02
		if (errorTimer < 1f) return;

		var moleculeErrorGraphics = fetchMoleculeErrorGraphics(ses_self);
		moleculeErrorGraphics.drawError(errorPercent);


		// timeStamp 03
		// timeStamp 04
		// timeStamp 05
		// timeStamp 06

		drawErrorBox(ses_self, "Unknown operating condition encountered.", new Vector2(0, 0), new Vector2[] { }, errorPercent);

		// timeStamps 07 - 10
		var timeStamps_07_10 = new List<float>() {3.314f, 3.813f, 4.167f, 4.479f};
		for (int i = 0; i < timeStamps_07_10.Count; i++)
		{
			if (errorTimer >= timeStamps_07_10[i]) drawErrorBox(ses_self, "Transmutation engines must not amalgamate.", new Vector2(40*(i+1), -40 * (i + 1)), new Vector2[] { }, errorPercent);
		}
	}
}