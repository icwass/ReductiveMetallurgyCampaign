//using Mono.Cecil.Cil;
//using MonoMod.Cil;
using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
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
using Song = class_186;
//using Tip = class_215;
using Font = class_1;

public sealed class JournalLoader
{
	private static IDetour hook_JournalScreen_method_1040;

	public static List<PuzzleModelRMC> journal_puzzles = new();

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// journal stuff

	public static void modifyJournal()
	{
		// find THIS campaign
		var thing1 = QuintessentialLoader.AllJournals;
		var thing2 = QuintessentialLoader.ModJournalModels;

		Quintessential.Serialization.JournalChapterModel comparandModel = null;

		foreach (var JModel in QuintessentialLoader.ModJournalModels)
		{
			if (File.Exists(JModel.Path + "/RMC.journal.yaml"))
			{
				comparandModel = JModel.Chapters.First();
				break;
			}
		}

		if (comparandModel == null)
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find the 'RMC.journal.yaml' filepath amongst the loaded ModJournalModels:");
			foreach (var JModel in QuintessentialLoader.ModJournalModels)
			{
				Logger.Log("    " + JModel.Path);
			}
			throw new Exception("modifyJournalRMC: Content is missing.");
		}

		List<JournalVolume> journal_self = new();

		foreach (var Journal in QuintessentialLoader.AllJournals)
		{
			if (Journal.First().field_2569 == comparandModel.Title && Journal.First().field_2570 == comparandModel.Description)
			{
				journal_self = Journal;
				break;
			}
		}
		if (journal_self.Count() == 0) return;

		// modify the journal puzzles
		Logger.Log("[ReductiveMetallurgyCampaign] Modifying journal items.");
		foreach (var volume in journal_self)
		{
			foreach (var puzzle in volume.field_2571)
			{
				// journal puzzles were manually added before, so Quintessential loaded them like campaign puzzles
				// however, Quintessential loads journal puzzles differently, it seems
				// (that, or campaign puzzles are inherently different from journal puzzles)
				// *shrug*
				// to maintain consistency with the campaign puzzles
				// (and to not force everyone to merge their solution files),
				// we rewrite the puzzleID
				var puzzleID = puzzle.field_2766;
				if (MainClass.AdvancedContent.JournalRemappings.ContainsKey(puzzleID))
				{
					puzzleID = MainClass.AdvancedContent.JournalRemappings[puzzleID];
					puzzle.field_2766 = puzzleID;
				}

				// Quintessential doesn't load journal puzzles correctly yet,
				// so we manually add the puzzles to the collection of campaign puzzles
				// this way, solutions for these puzzles will be loaded correctly
				Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
				Puzzles.field_2816[Puzzles.field_2816.Length - 1] = puzzle;

				// run hard-coded stuff
				// NO HARD-CODED STUFF TO RUN

				// add to journalPuzzle list, for other classes to use
				foreach (var puzzleModel in MainClass.AdvancedContent.Puzzles.Where(x => x.ID == puzzleID))
				{
					puzzleModel.modifyCampaignItem(puzzle);
					journal_puzzles.Add(puzzleModel);
					break;
				}
			}
		}
	}

	public static void Load()
	{
		hook_JournalScreen_method_1040 = new Hook(MainClass.PrivateMethod<JournalScreen>("method_1040"), OnJournalScreen_Method_1040);
	}
	private delegate void orig_JournalScreen_method_1040(JournalScreen screen_self, Puzzle puzzle, Vector2 basePosition, bool isLargePuzzle);


	public static void Unload()
	{
		hook_JournalScreen_method_1040.Dispose();
	}

	private static void OnJournalScreen_Method_1040(orig_JournalScreen_method_1040 orig, JournalScreen screen_self, Puzzle puzzle, Vector2 basePosition, bool isLargePuzzle)
	{
		var puzzleID = puzzle.field_2766;
		if (!MainClass.AdvancedContent.Puzzles.Any(x => x.ID == puzzleID))
		{
			orig(screen_self, puzzle, basePosition, isLargePuzzle);
			return;
		}
		var puzzleModel = MainClass.AdvancedContent.Puzzles.Where(x => x.ID == puzzleID).First();

		bool puzzleSolved = GameLogic.field_2434.field_2451.method_573(puzzle);
		Font crimson_15 = class_238.field_1990.field_2144;
		bool authorExists = puzzle.field_2768.method_1085();
		string authorName() => puzzle.field_2768.method_1087();
		string displayString = authorExists ? string.Format("{0} ({1})", puzzle.field_2767, authorName()) : (string)puzzle.field_2767;

		Texture moleculeBackdrop = isLargePuzzle ? class_238.field_1989.field_88.field_894 : class_238.field_1989.field_88.field_895;
		Texture divider = isLargePuzzle ? class_238.field_1989.field_88.field_892 : class_238.field_1989.field_88.field_893;
		Texture solvedCheckbox = puzzleSolved ? class_238.field_1989.field_96.field_879 : class_238.field_1989.field_96.field_882;
		class_135.method_290(displayString, basePosition + new Vector2(9f, -19f), crimson_15, class_181.field_1718, (enum_0)0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
		Vector2 vector2_1 = basePosition + new Vector2(moleculeBackdrop.field_2056.X - 27, -23f);
		class_135.method_272(solvedCheckbox, vector2_1);
		class_135.method_272(divider, basePosition + new Vector2(isLargePuzzle ? 7f : 7f, -34f));
		class_135.method_272(moleculeBackdrop, basePosition);

		Bounds2 bounds2 = Bounds2.WithSize(basePosition, moleculeBackdrop.field_2056.ToVector2());
		bool mouseHover = bounds2.Contains(Input.MousePos());

		Vector2 moleculeOffset = isLargePuzzle ? new Vector2(470f, 365f) : new Vector2(280f, 200f);
		Texture textureFromMolecule(Molecule molecule, Vector2 offset) => Editor.method_928(molecule, false, mouseHover, offset, isLargePuzzle, (Maybe<float>)struct_18.field_1431).method_1351().field_937;
		Texture textureFromIndex(int i, Vector2 offset) => textureFromMolecule(puzzle.field_2771[i].field_2813, offset);

		if (puzzleModel.getJournalPreview().Count() > 0)
		{
			foreach (var kvp in puzzleModel.getJournalPreview())
			{
				class_135.method_272(textureFromIndex(kvp.Key, moleculeOffset), bounds2.Min + kvp.Value);
			}
		}
		else
		{
			var molecules = puzzle.field_2771.Select(x => x.field_2813).OrderByDescending(x => x.method_1100().Count);
			Texture moleculeTexture = textureFromMolecule(molecules.First(), moleculeOffset);
			Vector2 vector2_4 = (moleculeTexture.field_2056.ToVector2() / 2).Rounded();
			class_135.method_272(moleculeTexture, bounds2.Center.Rounded() - vector2_4 + new Vector2(2f, 2f));
		}
		if (mouseHover && Input.IsLeftClickPressed())
		{
			Song song = ModelWithResourcesRMC.fetchSong(puzzleModel.Music);
			Sound fanfare = ModelWithResourcesRMC.fetchSound(puzzleModel.Music);
			Maybe<class_264> maybeStoryPanel = puzzleModel.NoStoryPanel ? struct_18.field_1431 : new class_264(puzzleModel.ID);

			GameLogic.field_2434.method_946(new PuzzleInfoScreen(puzzle, song, fanfare, maybeStoryPanel));
			class_238.field_1991.field_1821.method_28(1f);
		}
	}
}



