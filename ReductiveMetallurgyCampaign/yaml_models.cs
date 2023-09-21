//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
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

	public static Vector2 Vector2FromString(string pos, float defaultX = 0f, float defaultY = 0f)
	{
		if (!string.IsNullOrEmpty(pos))
		{
			float x = float.Parse(pos.Split(',')[0], style, format);
			float y = float.Parse(pos.Split(',')[1], style, format);
			return new Vector2(x, y);
		}
		else
		{
			return new Vector2(defaultX,defaultY);
		}
	}
}

public class CampaignModelRMC
{
	public CreditsModelRMC Credits;
	public List<int> SigmarStoryUnlocks;
	public List<string> SigmarsGardens;
	public List<CharacterModelRMC> Characters;
	public List<CutsceneModelRMC> Cutscenes;
	public List<DocumentModelRMC> Documents;
	public List<PuzzleModelRMC> Puzzles;
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
}
public class CutsceneModelRMC
{
	public string ID, Location, Background, Music;
}
public class DocumentModelRMC
{
	public string ID, Texture;
	public List<DrawItemModelRMC> DrawItems;
}
public class DrawItemModelRMC
{
	public string Position, Texture, Rotation, Scale, Alpha, Font, Color, Align, LineSpacing, ColumnWidth;
	public bool Handwritten;
}
//////////////////////////////////////////////////
public class PuzzleModelRMC
{
	public string ID, Music;
	public bool NoStoryPanel = false;
	public TipModelRMC Tip = null;
	public CabinetModelRMC Cabinet;
}

public class TipModelRMC
{
	public string ID, Title, Description, Solution, Texture, SolutionOffset;
	Texture loadedTexture;

	static HashSet<string> TipPaths = new();

	public static bool IsCustomTipPath(string path) => TipPaths.Contains(path);

	public Tip FromModel()
	{
		Maybe<Texture> image = (Maybe<Texture>)struct_18.field_1431;

		if (!string.IsNullOrEmpty(this.Texture))
		{
			this.loadedTexture ??= class_235.method_615(this.Texture); // if null, load the texture
			image = this.loadedTexture;
		}

		if (!string.IsNullOrEmpty(this.Solution))
		{
			TipPaths.Add("Content\\tips\\" + this.Solution + ".solution");
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
	public bool ExpandLeft, ExpandRight;
	public List<ConduitModelRMC> Conduits;
	public List<VialHolderModelRMC> VialHolders;
	public List<OverlayModelRMC> Overlays;

	public void ModifyCabinet(Puzzle puzzle)
	{
		var puzzleID = puzzle.field_2766;
		if (!puzzle.field_2779.method_1085())
		{
			Logger.Log("[ReductiveMetallurgyCampaign] Puzzle '" + puzzleID + "' is not a production puzzle - ignoring the cabinet data.");
			return;
		}

		var productionData = puzzle.field_2779.method_1087();
		productionData.field_2075 = !this.ExpandLeft;
		productionData.field_2076 = !this.ExpandRight;

		if (this.Conduits != null)
		{
			productionData.field_2072 = new class_117[this.Conduits.Count];
			for (int i = 0; i < this.Conduits.Count; i++)
			{
				productionData.field_2072[i] = this.Conduits[i].FromModel();
			}
		}

		if (this.Overlays != null)
		{
			ProductionManager.AddOverlaysForPuzzle(puzzleID, this.Overlays);
		}

		if (this.VialHolders != null)
		{
			productionData.field_2073 = new class_128[this.VialHolders.Count];
			for (int i = 0; i < this.VialHolders.Count; i++)
			{
				productionData.field_2073[i] = this.VialHolders[i].FromModel();
			}
		}

		// fix and update the cabinet bounding box
		puzzle.method_1247();
	}


	public class ConduitModelRMC
	{
		public string Position1, Position2;
		public List<string> Hexes;

		public class_117 FromModel()
		{
			int Q1 = int.Parse(this.Position1.Split(',')[0]);
			int R1 = int.Parse(this.Position1.Split(',')[1]);
			int Q2 = int.Parse(this.Position2.Split(',')[0]);
			int R2 = int.Parse(this.Position2.Split(',')[1]);
			var hexList = new HexIndex[this.Hexes.Count];
			for (int j = 0; j < this.Hexes.Count; j++)
			{
				var hex = this.Hexes[j];
				hexList[j] = new HexIndex(int.Parse(hex.Split(',')[0]), int.Parse(hex.Split(',')[1]));
			}
			return new class_117(Q1, R1, Q2, R2, hexList);
		}
	}
	public class VialHolderModelRMC
	{
		public string Position;
		public bool TopSide;
		public List<VialModelRMC> Vials;

		public class_128 FromModel()
		{
			var vials = new Tuple<Texture, Texture>[this.Vials.Count];

			for (int j = 0; j < this.Vials.Count; j++)
			{
				var vial = this.Vials[j];
				vials[j] = Tuple.Create(ProductionManager.fetchTexture(vial.TextureSim), ProductionManager.fetchTexture(vial.TextureGif));
			}

			return new class_128(int.Parse(this.Position.Split(',')[0]), int.Parse(this.Position.Split(',')[1]), this.TopSide, vials);
		}
	}
	public class VialModelRMC
	{
		public string TextureSim, TextureGif;
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
}
public class JournalVolumeModelRMC
{
	public int FromChapter;
	public string Title, Description;
}