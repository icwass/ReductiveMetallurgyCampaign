//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
//using System.Linq;
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
//using Song = class_186;
using Tip = class_215;
//using Font = class_1;

/////////////////////////////////////////////////////////////////////////////////////////////////
// advanced.yaml

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

	public static Color HexColor(int hex)
	{
		return Color.FromHex(hex);
	}

	public static Color ColorWhite => Color.White;
}

public class CampaignModelRMC
{
	public CreditsModelRMC Credits;
	public List<int> SigmarStoryUnlocks;
	public List<CharacterModelRMC> Characters;
	public List<CutsceneModelRMC> Cutscenes;
	public List<DocumentModelRMC> Documents;
	public List<PuzzleModelRMC> Puzzles;

	public void LoadDocuments()
	{
		foreach (var document in this.Documents)
		{
			document.AddDocumentFromModel();
		}
	}
}
public class CreditsModelRMC
{
	public string PositionOffset;
	public List<List<string>> Texts;
}
public class CharacterModelRMC
{
	public string ID, Name, SmallPortrait, LargePortrait;
	public int Color;
	public bool IsOnLeft;

	Texture actorSmall, actorLarge;

	public class_230 FromModel()
	{
		if (!string.IsNullOrEmpty(this.SmallPortrait))
		{
			this.actorSmall ??= class_235.method_615(this.SmallPortrait); // if null, load the texture
		}
		if (!string.IsNullOrEmpty(this.LargePortrait))
		{
			this.actorLarge ??= class_235.method_615(this.LargePortrait); // if null, load the texture
		}

		if (!string.IsNullOrEmpty(this.SmallPortrait))
			actorSmall = class_235.method_615(this.SmallPortrait);
		if (!string.IsNullOrEmpty(this.LargePortrait))
			actorLarge = class_235.method_615(this.LargePortrait);

		return new class_230(class_134.method_253(this.Name, string.Empty), actorLarge, actorSmall, ModelHelpersRMC.HexColor(this.Color), this.IsOnLeft);
	}
}
public class CutsceneModelRMC
{
	public string ID, Location, Background, Music;
}
public class DocumentModelRMC
{
	public string ID, Texture;
	public List<DrawItemModelRMC> DrawItems;

	public void AddDocumentFromModel()
	{
		Texture base_texture = class_238.field_1989.field_85.field_570; // letter-5
		if (!string.IsNullOrEmpty(this.Texture))
		{
			base_texture = class_235.method_615(this.Texture);
		}
		List<Document.DrawItem> drawItems = new();

		if (this.DrawItems != null)
		{
			foreach (var drawItem in this.DrawItems)
			{
				drawItems.Add(drawItem.FromModel());
			}
		}
		new Document(this.ID, base_texture, drawItems);
	}
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
				class_235.method_615(this.Texture),
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
//////////////////////////////////////////////////
public class PuzzleModelRMC
{
	public string ID, Music;
	public TipModelRMC Tip = null;
	public CabinetModelRMC Cabinet;
}

public class TipModelRMC
{
	public string ID, Title, Description, Solution, Texture, SolutionOffset;
	Texture loadedTexture;

	public Tip FromModel()
	{
		Maybe<Texture> image = (Maybe<Texture>)struct_18.field_1431;

		if (!string.IsNullOrEmpty(this.Texture))
		{
			this.loadedTexture ??= class_235.method_615(this.Texture); // if null, load the texture
			image = this.loadedTexture;
		}

		return new Tip()
		{
			field_1899 = this.ID,
			field_1900 = class_134.method_253(this.Title ?? "<Untitled Tip>", string.Empty),
			field_1901 = class_134.method_253(this.Description ?? "<Description Missing>", string.Empty),
			field_1902 = this.Solution ?? "speedbonder",
			field_1903 = image,
			field_1904 = ModelHelpersRMC.Vector2FromString(this.SolutionOffset),
		};
	}


}

public class CabinetModelRMC
{
	public List<OverlayModelRMC> Overlays;

	public void ModifyCabinet(Puzzle puzzle)
	{
		var puzzleID = puzzle.field_2766;
		if (!puzzle.field_2779.method_1085())
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Puzzle '" + puzzleID + "' is not a production puzzle - ignoring the cabinet data.");
			return;
		}

		if (this.Overlays != null)
		{
			ProductionManager.AddOverlaysForPuzzle(puzzleID, this.Overlays);
		}
	}

	public class OverlayModelRMC
	{
		public string Texture, Position;
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////
// journal.yaml

public class JournalModelRMC
{
	public List<JournalVolumeModelRMC> Volumes;
	public List<JournalPreviewModelRMC> Previews;

	public Dictionary<string, Dictionary<int, Vector2>> GetPreviewPositions()
	{
		Dictionary<string, Dictionary<int, Vector2>> dict = new();

		foreach (var preview in Previews)
		{
			var tuple = preview.FromModel();
			dict.Add(tuple.Item1, tuple.Item2);
		}

		return dict;
	}
}
public class JournalVolumeModelRMC
{
	public int FromChapter;
	public string Title, Description;
}

public class JournalPreviewModelRMC
{
	public string ID;
	public List<JournalPreviewItemModelRMC> Items;

	public Tuple<string, Dictionary<int, Vector2>> FromModel()
	{
		Dictionary<int, Vector2> items = new();
		foreach (var item in Items)
		{
			var tuple = item.FromModel();
			items.Add(tuple.Item1, tuple.Item2);
		}
		
		return Tuple.Create(ID, items);
	}
}

public class JournalPreviewItemModelRMC
{
	public int Index;
	public string Position;

	public Tuple<int, Vector2> FromModel()
	{
		return Tuple.Create(Index, ModelHelpersRMC.Vector2FromString(Position));
	}
}