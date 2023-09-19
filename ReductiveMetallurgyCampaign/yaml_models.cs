//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
//using Quintessential;
//using Quintessential.Settings;
//using SDL2;
//using System;
//using System.IO;
//using System.Linq;
using System.Collections.Generic;
//using System.Reflection;

namespace ReductiveMetallurgyCampaign;

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
//using PartTypes = class_191;
//using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

/////////////////////////////////////////////////////////////////////////////////////////////////
// advanced.yaml
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
	public CabinetModelRMC Cabinet;
}
public class CabinetModelRMC
{
	public bool ExpandLeft, ExpandRight;
	public List<ConduitModelRMC> Conduits;
	public List<VialHolderModelRMC> VialHolders;
	public List<OverlayModelRMC> Overlays;
}
public class ConduitModelRMC
{
	public string Position1, Position2;
	public List<string> Hexes;
}
public class VialHolderModelRMC
{
	public string Position;
	public bool TopSide;
	public List<VialModelRMC> Vials;
}
public class VialModelRMC
{
	public string TextureSim, TextureGif;
}
public class OverlayModelRMC
{
	public string Texture, Position;
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