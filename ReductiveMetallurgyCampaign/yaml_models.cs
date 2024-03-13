//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
//using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
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
using Tip = class_215;
//using Font = class_1;

/////////////////////////////////////////////////////////////////////////////////////////////////
// helpers

public abstract class ModelWithResourcesRMC
{
	internal static Dictionary<string, Texture> TextureBank;
	internal abstract Dictionary<string, Texture> initialTextureBank();
	public void ensureTextureBankExists() => TextureBank ??= this.initialTextureBank();
	internal static Texture fetchTexture(string filePath)
	{
		if (!TextureBank.ContainsKey(filePath))
		{
			TextureBank[filePath] = class_235.method_615(filePath);
		}
		return TextureBank[filePath];
	}
	//////////////////////////////////////////////////
	internal static Dictionary<string, Tuple<Song, Sound>> SongBank;
	public static void ensureSongListExists()
	{
		var song = class_238.field_1992;
		var fanfare = class_238.field_1991;
		SongBank ??= new()
		{
			{"Map",         Tuple.Create(song.field_968, fanfare.field_1832) },
			{"Solitaire",   Tuple.Create(song.field_969, fanfare.field_1832) },
			{"Solving1",    Tuple.Create(song.field_970, fanfare.field_1830) },
			{"Solving2",    Tuple.Create(song.field_971, fanfare.field_1831) },
			{"Solving3",    Tuple.Create(song.field_972, fanfare.field_1832) },
			{"Solving4",    Tuple.Create(song.field_973, fanfare.field_1833) },
			{"Solving5",    Tuple.Create(song.field_974, fanfare.field_1834) },
			{"Solving6",    Tuple.Create(song.field_975, fanfare.field_1835) },
			{"Story1",      Tuple.Create(song.field_976, fanfare.field_1832) },
			{"Story2",      Tuple.Create(song.field_977, fanfare.field_1832) },
			{"Title",       Tuple.Create(song.field_978, fanfare.field_1832) },
		};
	}
	public static Song fetchSong(string name) => SongBank.ContainsKey(name) ? SongBank[name].Item1 : SongBank["Solving3"].Item1;
	public static Sound fetchSound(string name) => SongBank.ContainsKey(name)? SongBank[name].Item2 : SongBank["Solving3"].Item2;
}

public static class ModelHelpersRMC
{
	static NumberStyles style = NumberStyles.Any;
	static NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;

	public static float FloatFromString(string str, float defaulF = 0f)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return float.Parse(str, style, format);
		}
		else
		{
			return defaulF;
		}
	}

	public static Vector2 Vector2FromString(string pos, float defaultX = 0f, float defaultY = 0f)
	{
		float x = FloatFromString(pos?.Split(',')[0], defaultX);
		float y = FloatFromString(pos?.Split(',')[1], defaultY);
		return new Vector2(x, y);
	}

	public static Color HexColor(int hex) => Color.FromHex(hex);
	public static Color ColorWhite => Color.White;
}

/////////////////////////////////////////////////////////////////////////////////////////////////
// advanced.yaml

public class AdvancedContentModelRMC
{
	public CreditsModelRMC Credits;
	public List<int> SigmarStoryUnlocks;
	public List<int> LeftHandedChapters;
	public Dictionary<string, string> JournalRemappings;
	public List<CharacterModelRMC> Characters;
	public List<CutsceneModelRMC> Cutscenes;
	public List<DocumentModelRMC> Documents;
	public List<PuzzleModelRMC> Puzzles;

	public void modifyCampaignItem(CampaignItem campaignItem)
	{
		if (!campaignItem.field_2325.method_1085()) return;
		string puzzleID = campaignItem.field_2325.method_1087().field_2766;

		foreach (var cutsceneM in this.Cutscenes.Where(x => x.ID == puzzleID))
		{
			cutsceneM.modifyCampaignItem(campaignItem);
			return;
		}

		foreach (var documentM in this.Documents.Where(x => x.ID == puzzleID))
		{
			documentM.modifyCampaignItem(campaignItem);
			return;
		}

		foreach (var puzzleM in this.Puzzles.Where(x => x.ID == puzzleID))
		{
			puzzleM.modifyCampaignItem(campaignItem);
			return;
		}
	}
}
public class CreditsModelRMC
{
	public string PositionOffset;
	public List<List<string>> Texts;
}

public class CharacterModelRMC : ModelWithResourcesRMC
{
	public string ID, Name, SmallPortrait, LargePortrait;
	public int Color;
	public bool IsOnLeft;
	internal override Dictionary<string, Texture> initialTextureBank()
	{
		string path = "textures/portraits/";
		var portrait = class_238.field_1989.field_93;
		return new()
		{
			{"",								null},
			{path + "anataeus_alt_large",       portrait.field_670},
			{path + "anataeus_large",			portrait.field_671},
			{path + "anataeus_shabby_large",	portrait.field_672},
			{path + "anataeus_shabby_small",	portrait.field_673},
			{path + "anataeus_small",			portrait.field_674},
			{path + "anataeus_student_small",	portrait.field_675},
			{path + "armand_large",				portrait.field_676},
			{path + "clara_large",				portrait.field_677},
			{path + "clara_small",				portrait.field_678},
			{path + "clara_tiara_small",		portrait.field_679},
			{path + "concordia_large",			portrait.field_680},
			{path + "concordia_shabby_large",	portrait.field_681},
			{path + "concordia_shabby_small",	portrait.field_682},
			{path + "concordia_small",			portrait.field_683},
			{path + "gelt_armor_small",			portrait.field_684},
			{path + "gelt_large",				portrait.field_685},
			{path + "gelt_small",				portrait.field_686},
			{path + "henley_small",				portrait.field_687},
			{path + "nils_cloak_large",			portrait.field_688},
			{path + "nils_cloak_small",			portrait.field_689},
			{path + "nils_large",				portrait.field_690},
			{path + "nils_small",				portrait.field_691},
			{path + "taros_large",				portrait.field_692},
			{path + "verrin_large",				portrait.field_693},
			{path + "verrin_shabby_large",		portrait.field_694},
		};
	}

	public class_230 FromModel()
	{
		this.ensureTextureBankExists();
		return new class_230(
			class_134.method_253(this.Name, string.Empty),
			fetchTexture(this.LargePortrait ?? ""),
			fetchTexture(this.SmallPortrait ?? ""),
			ModelHelpersRMC.HexColor(this.Color),
			this.IsOnLeft
		);
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////
public class CutsceneModelRMC : ModelWithResourcesRMC
{
	public string ID, Location, Background, Music;
	internal override Dictionary<string, Texture> initialTextureBank()
	{
		string path1 = "textures/cinematic/backgrounds/";
		string path2 = "textures/puzzle_select/";
		var cinematic = class_238.field_1989.field_84.field_535;
		var puzzleSelect = class_238.field_1989.field_96;
		return new()
		{
			{path1 + "greathall_a",	cinematic.field_536},
			{path1 + "greathall_b",	cinematic.field_537},
			{path1 + "greathall_c",	cinematic.field_538},
			{path1 + "tailor_a",	cinematic.field_539},
			{path1 + "tailor_b",	cinematic.field_540},
			{path1 + "tailor_c",	cinematic.field_541},
			{path1 + "workshop",	cinematic.field_542},
			{path2 + "background_0",    puzzleSelect.field_826},
			{path2 + "background_1",    puzzleSelect.field_827},
			{path2 + "background_2",    puzzleSelect.field_828},
			{path2 + "background_3",    puzzleSelect.field_829},
			{path2 + "background_4",    puzzleSelect.field_830},
			{path2 + "background_5",    puzzleSelect.field_831},
			{path2 + "background_6",    puzzleSelect.field_832},
		};
	}
	public Tuple<string, Texture> FromModel() => Tuple.Create(this.Location, fetchTexture(this.Background));
	public void modifyCampaignItem(CampaignItem campaignItem)
	{
		ensureSongListExists();
		campaignItem.field_2324 = CampaignLoader.typeCutscene;
		campaignItem.field_2328 = fetchSong(this.Music);
	}
}

//////////////////////////////////////////////////
public class DocumentModelRMC : ModelWithResourcesRMC
{
	public string ID, Texture;
	public List<DrawItemModelRMC> DrawItems;

	internal override Dictionary<string, Texture> initialTextureBank()
	{
		string path = "textures/documents/";
		var docs = class_238.field_1989.field_85;
		return new()
		{
			{"",                        docs.field_570},
			{path + "letter_0",         docs.field_563},
			{path + "letter_0_bar",     docs.field_564},
			{path + "letter_1",         docs.field_565},
			{path + "letter_2",         docs.field_566},
			{path + "letter_3",         docs.field_567},
			{path + "letter_4",         docs.field_568},
			{path + "letter_4_overlay", docs.field_569},
			{path + "letter_5",         docs.field_570},
			{path + "letter_6",         docs.field_571},
			{path + "letter_6_overlay", docs.field_572},
			{path + "letter_7",         docs.field_573},
			{path + "letter_9",         docs.field_574},
			{path + "letter_response",  docs.field_575},
			{path + "pip",              docs.field_576},
		};
	}

	public void modifyCampaignItem(CampaignItem campaignItem)
	{
		ensureTextureBankExists();
		campaignItem.field_2324 = CampaignLoader.typeDocument;
		this.AddDocumentFromModel();
	}

	public void AddDocumentFromModel()
	{
		this.ensureTextureBankExists();

		List<Document.DrawItem> drawItems = new();
		if (this.DrawItems != null)
		{
			foreach (var drawItem in this.DrawItems)
			{
				drawItems.Add(drawItem.FromModel());
			}
		}
		new Document(this.ID, fetchTexture(this.Texture ?? ""), drawItems);
	}



	public class DrawItemModelRMC
	{
		public string Position, Texture, Rotation, Scale, Alpha, Font, Color, Align, LineSpacing, ColumnWidth;
		public bool Handwritten;

		public Document.DrawItem FromModel()
		{
			bool isImageItem = !string.IsNullOrEmpty(this.Texture);

			// image AND text properties
			Color color = isImageItem ? ModelHelpersRMC.ColorWhite : DocumentScreen.field_2410;
			if (!string.IsNullOrEmpty(this.Color)) color = ModelHelpersRMC.HexColor(int.Parse(this.Color));
			Vector2 position = ModelHelpersRMC.Vector2FromString(this.Position);

			if (isImageItem)
			{
				return new Document.DrawItem(
					position,
					fetchTexture(this.Texture),
					color,
					ModelHelpersRMC.FloatFromString(this.Scale, 1f),
					ModelHelpersRMC.FloatFromString(this.Rotation),
					ModelHelpersRMC.FloatFromString(this.Alpha, 1f)
				);
			}
			else // isTextItem
			{
				return new Document.DrawItem(
					position,
					Document.DrawItem.getFont(this.Font),
					color,
					Document.DrawItem.getAlignment(this.Align),
					ModelHelpersRMC.FloatFromString(this.LineSpacing, 1f),
					ModelHelpersRMC.FloatFromString(this.ColumnWidth, float.MaxValue),
					this.Handwritten
				);
			}
		}
	}
}
/////////////////////////////////////////////////////////////////////////////////////////////////

public class PuzzleModelRMC : ModelWithResourcesRMC
{
	public string ID, Music;
	public TipModelRMC Tip = null;
	public CabinetModelRMC Cabinet;
	public bool NoStoryPanel = false;
	public Dictionary<int, string> JournalPreview;

	internal override Dictionary<string, Texture> initialTextureBank()
	{
		string prodPath = "textures/pipelines/";
		var prods = class_238.field_1989.field_92;
		return new()
		{
			{prodPath + "aether_overlay_bottom",    prods.field_390},
			{prodPath + "aether_overlay_middle",    prods.field_391},
			{prodPath + "aether_overlay_top",       prods.field_392},
			{prodPath + "amaro_overlay_bottom",     prods.field_393},
			{prodPath + "amaro_overlay_top",        prods.field_394},
			{prodPath + "edge_overlay_left",        prods.field_395},
			{prodPath + "edge_overlay_right",       prods.field_396},
			{prodPath + "solvent_overlay",          prods.field_397},
		};
	}
	public void modifyCampaignItem(CampaignItem campaignItem)
	{
		this.ensureTextureBankExists();
		ensureSongListExists();
		campaignItem.field_2328 = fetchSong(this.Music);
		campaignItem.field_2329 = fetchSound(this.Music);

		Puzzle puzzle = campaignItem.field_2325.method_1087();
		modifyCampaignItem(puzzle);
	}
	public void modifyCampaignItem(Puzzle puzzle)
	{
		if (this.Tip != null) puzzle.field_2769 = this.Tip.FromModel();
	}

	public Dictionary<int, Vector2> getJournalPreview()
	{
		Dictionary<int, Vector2> ret = new();
		if (this.JournalPreview != null)
		{
			foreach (var kvp in this.JournalPreview)
			{
				ret.Add(kvp.Key, ModelHelpersRMC.Vector2FromString(kvp.Value));
			}
		}
		return ret;
	}

	//////////////////////////////////////////////////
	public class TipModelRMC
	{
#pragma warning disable CS0649
		public string ID, Title, Description, Texture, Solution, SolutionOffset;
		public Tip FromModel()
		{
			Maybe<Texture> maybeImage = !string.IsNullOrEmpty(this.Texture) ? fetchTexture(this.Texture) : (Maybe<Texture>)struct_18.field_1431;

			return new Tip()
			{
				field_1899 = this.ID,
				field_1900 = class_134.method_253(this.Title ?? "<Untitled Tip>", string.Empty),
				field_1901 = class_134.method_253(this.Description ?? "<Description Missing>", string.Empty),
				field_1902 = this.Solution ?? "speedbonder",
				field_1903 = maybeImage,
				field_1904 = ModelHelpersRMC.Vector2FromString(this.SolutionOffset),
			};
		}
	}

	public class CabinetModelRMC
	{
		public List<OverlayModelRMC> Overlays;

		public List<Tuple<Texture, Vector2>> fetchOverlays()
		{
			List<Tuple<Texture, Vector2>> ret = new();
			foreach (var overlay in Overlays)
			{
				ret.Add(overlay.FromModel());
			}
			return ret;
		}

		public class OverlayModelRMC
		{
#pragma warning disable CS0649
			public string Texture, Position;
			public Tuple<Texture, Vector2> FromModel()
			{
				return Tuple.Create(fetchTexture(this.Texture), ModelHelpersRMC.Vector2FromString(this.Position));
			}
		}
	}
}