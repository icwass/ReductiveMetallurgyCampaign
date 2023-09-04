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
using Song = class_186;
//using Tip = class_215;
using Font = class_1;
public class JournalModelRMC
{
	public List<JournalVolumeModelRMC> Volumes;
}
public class JournalVolumeModelRMC
{
	public int FromChapter;
	public string Title, Description;
}

public sealed class JournalLoader
{
	private static JournalModelRMC journal_model;

	private static IDetour hook_JournalScreen_method_1040;

	private static List<CampaignItem> journal_items = new();



	/////////////////////////////////////////////////////////////////////////////////////////////////
	// journal stuff
	public static void loadJournalModel()
	{
		string filepath;
		if (!MainClass.findModMetaFilepath("ReductiveMetallurgyCampaign", out filepath) || !File.Exists(filepath + "/Puzzles/RMC.journal_RMC.yaml"))
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Could not find 'RMC.journal_RMC.yaml' in the folder '" + filepath + "\\Puzzles\\'");
			throw new Exception("modifyCampaignRMC: Journal data is missing.");
		}
		using (StreamReader streamReader = new StreamReader(filepath + "/Puzzles/RMC.journal_RMC.yaml"))
		{
			journal_model = YamlHelper.Deserializer.Deserialize<JournalModelRMC>(streamReader);
		}
	}

	public static void modifyJournals(Campaign campaign_self)
	{
		CampaignChapter[] campaignChapters = campaign_self.field_2309;
		int maxChapter = campaignChapters.Length-1;

		HashSet<int> chaptersToRemove = new();

		foreach (var volume in journal_model.Volumes)
		{
			var volumeIndex = volume.FromChapter;
			if (volumeIndex > maxChapter || volumeIndex < 0)
			{
				Logger.Log("[ReductiveMetallurgyCampaign] Invalid FromChapter value for journal page '" + volume.Title + "', ignoring.'");
				continue;
			}
			if (chaptersToRemove.Contains(volumeIndex))
			{
				Logger.Log("[ReductiveMetallurgyCampaign] Already consumed chapter '" + volumeIndex + "' for the journal, ignoring.'");
				continue;
			}

			var chapter = campaignChapters[volumeIndex];
			var items = chapter.field_2314;

			List<CampaignItem> itemForJournal = new();

			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (item.field_2324 == CampaignLoader.typePuzzle && item.field_2325.method_1085())
				{
					itemForJournal.Add(item);
				}
			}
			if (itemForJournal.Count < 5)
			{
				Logger.Log("[ReductiveMetallurgyCampaign] Insufficient puzzles in chapter '" + volumeIndex + "' to make a journal page, ignoring.'");
				continue;
			}

			var newJournalVolume = new JournalVolume()
			{
				field_2569 = volume.Title,
				field_2570 = volume.Description,
				field_2571 = new Puzzle[5]
			};

			for (int i = 0; i < 5; i++)
			{
				journal_items.Add(itemForJournal[i]);
				newJournalVolume.field_2571[i] = itemForJournal[i].field_2325.method_1087();
			}

			Array.Resize(ref JournalVolumes.field_2572, JournalVolumes.field_2572.Length + 1);
			JournalVolumes.field_2572[JournalVolumes.field_2572.Length - 1] = newJournalVolume;

			//chaptersToRemove.Add(volumeIndex);
			Logger.Log("[ReductiveMetallurgyCampaign] Converted chapter '" + volumeIndex + "' into a journal page.'");
		}

		CampaignChapter[] newCampaignChapters = new CampaignChapter[campaignChapters.Length - chaptersToRemove.Count];
		
		int j = 0;
		for (int i = 0; i < campaignChapters.Length; i++)
		{
			if (!chaptersToRemove.Contains(i))
			{
				newCampaignChapters[j] = campaignChapters[i];
				j++;
			}
		}
		
		campaign_self.field_2309 = newCampaignChapters;
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
		if (!journal_items.Select(x => x.field_2325.method_1087()).Contains(puzzle))
		{
			orig(screen_self, puzzle, basePosition, isLargePuzzle);
			return;
		}
		var item = journal_items.Where(x => x.field_2325.method_1087() == puzzle).First();
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
		var puzzleID = puzzle.field_2766;

		Vector2 moleculeOffset = isLargePuzzle ? new Vector2(470f, 365f) : new Vector2(280f, 200f);
		Texture textureFromMolecule(Molecule molecule, Vector2 offset) => Editor.method_928(molecule, false, mouseHover, offset, isLargePuzzle, (Maybe<float>)struct_18.field_1431).method_1351().field_937;
		Texture textureFromIndex(int i, Vector2 offset) => textureFromMolecule(puzzle.field_2771[i].field_2813, offset);

		if (puzzleID == "rmc-welded-elements") // use same layout as Proof of Completeness
		{
			for (int index = 0; index < 4; ++index)
				class_135.method_272(textureFromIndex(index, new Vector2(500f, 500f)), bounds2.Min + new Vector2(46f, 197f) + new Vector2(215 * (index % 2), -140 * (index / 2)));
		}
		/*
		else if (puzzleID == Puzzles.field_2911.field_2766) // Van Berlo's Pivots
		{
			moleculeOffset = new Vector2(500f, 500f);
			for (int index = 0; index < 4; ++index)
				class_135.method_272(textureFromIndex(index, moleculeOffset), bounds2.Min + new Vector2(72f, 187f) + new Vector2(195 * (index % 2), -160 * (index / 2)));
		}
		else if (puzzleID == Puzzles.field_2914.field_2766) // Alchemical Slag
		{
			for (int index = 0; index < 2; ++index)
				class_135.method_272(textureFromIndex(index, moleculeOffset), bounds2.Min + new Vector2(44f, 61f) + new Vector2(129 * index, 0.0f));
		}
		else if (puzzleID == Puzzles.field_2918.field_2766) // Electrum Separation
		{
			for (int index = 0; index < 2; ++index)
				class_135.method_272(textureFromIndex(index, moleculeOffset), bounds2.Min + new Vector2(34f, 119f) + new Vector2(0.0f, -89 * index));
		}
		*/
		else
		{
			var molecules = puzzle.field_2771.Select(x => x.field_2813).OrderByDescending(x => x.method_1100().Count);
			Texture moleculeTexture = textureFromMolecule(molecules.First(), moleculeOffset);
			Vector2 vector2_4 = (moleculeTexture.field_2056.ToVector2() / 2).Rounded();
			class_135.method_272(moleculeTexture, bounds2.Center.Rounded() - vector2_4 + new Vector2(2f, 2f));
		}
		if (mouseHover && Input.IsLeftClickPressed())
		{
			Song song = item.field_2328;
			Sound fanfare = item.field_2329;
			Maybe<class_264> maybeStoryPanel = item.field_2327;

			GameLogic.field_2434.method_946(new PuzzleInfoScreen(puzzle, song, fanfare, maybeStoryPanel));
			class_238.field_1991.field_1821.method_28(1f);
		}
		
	}
}



