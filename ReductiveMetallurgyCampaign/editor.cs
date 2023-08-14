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
using System.Text;

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

public class BoardEditorScreen : IScreen
{
	//=========================//
	//boilerplate needed as a subclass of IScreen
	public bool method_1037() => false;
	public void method_47(bool param_5434) {
		GameLogic.field_2434.field_2443.method_673(class_238.field_1992.field_969);
		string testPath = "Content/solitaire-rmc.dat";
		foreach (var dir in QuintessentialLoader.ModContentDirectories.Where(dir => File.Exists(Path.Combine(dir, testPath))))
		{
			if (File.Exists(Path.Combine(dir, testPath)))
			{
				dirPath = dir;
				break;
			}
		}

		if (boards != null) return;

		boards = new List<editableBoard>();
		string fullFilePath = Path.Combine(dirPath, filePath);
		if (File.Exists(fullFilePath))
		{
			// load existing boards
			using (BinaryReader binaryReader = new BinaryReader(new FileStream(fullFilePath, FileMode.Open, FileAccess.Read), Encoding.ASCII))
			{
				long size = binaryReader.BaseStream.Length;
				string buffer = "";
				for (long i = 0; i < size; i++)
				{
					char c = binaryReader.ReadChar();
					if (c == ';')
					{
						//process board, open next one
						boards.Add(new editableBoard(buffer));
						buffer = "";
					}
					else
					{
						buffer += c;
					}
				}
				if (buffer != "") boards.Add(new editableBoard(buffer));
			}
		}
		if (boards.Count == 0) boards.Add(new editableBoard());
	}
	public void method_48() { }
	public void method_50(float timeDelta) { DrawFunction(); }
	//=========================//
	// helpers
	public static void Load()
	{
		On.class_178.method_50 += DrawButtonForSigmarBoardEditor;
	}
	public static bool AtomButtonClicked(Vector2 position, bool rightClick = false)
	{
		// check if clicked
		bool mouseClicked = rightClick ? Input.IsRightClickPressed() : Input.IsLeftClickPressed();
		bool inRange = Vector2.Distance(position, Input.MousePos()) < 28.0;
		return mouseClicked && inRange;
	}

	public static bool DrawAtomButton(Vector2 position, AtomType atomType, bool rightClick = false, bool bright = true)
	{
		bool inRange = Vector2.Distance(position, Input.MousePos()) < 28.0;
		// draw hover
		if (inRange)
		{
			Texture field551 = class_238.field_1989.field_98.field_551; // solitaire/hex_hover
			Color color = Color.White.WithAlpha(true ? 0.6f : 0.1f);
			Vector2 vec2 = position + new Vector2(-36f, -41f);
			class_135.method_271(field551, color, vec2);
		}
		// draw atom
		if (atomType != SigmarGardenPatcher.getAtomType(0))
		{
			float num2 = bright ? 1f : 0.2f;
			float num3 = bright ? 0.45f : 0.0f;
			Editor.method_927(atomType, position, 0.88f, num2, 1f, num3, -12f, 0.0f, null, null, false);
		}

		return AtomButtonClicked(position, rightClick);
	}

	//public static bool DrawCheckbox(Vector2 pos, string label, bool enabled)
	//{
	//	Bounds2 boxBounds = Bounds2.WithSize(pos, new Vector2(36f, 37f));
	//	Bounds2 labelBounds = UI.DrawText(label, pos + new Vector2(45f, 13f), UI.SubTitle, UI.TextColor, TextAlignment.Left);
	//	if (enabled)
	//		UI.DrawTexture(class_238.field_1989.field_101.field_773, boxBounds.Min);
	//	if (boxBounds.Contains(Input.MousePos()) || labelBounds.Contains(Input.MousePos()))
	//	{
	//		UI.DrawTexture(class_238.field_1989.field_101.field_774, boxBounds.Min);
	//		if (!Input.IsLeftClickPressed())
	//			return false;
	//		playButtonClick();
	//		return true;
	//	}
	//	UI.DrawTexture(class_238.field_1989.field_101.field_772, boxBounds.Min);
	//	return false;
	//}

	void playSound(Sound sound) => sound.method_28(1f);
	void playButtonClick() => playSound(class_238.field_1991.field_1821);


	//=========================//
	// "real" stuff
	const string filePath = "Content/solitaire-rmc.txt";
	const string datPath = "Content/solitaire-rmc.dat";
	string dirPath = "";
	int atomSwatch = 1;
	List<editableBoard> boards;
	int currentBoardIndex = 0;
	Maybe<string> newBoardName = (Maybe<string>)struct_18.field_1431;
	public void DrawFunction()
	{
		//=========================//
		// draw frame, handle transition logic

		//if (GameLogic.field_2434.method_938() is class_16) return; // tutorial screen
		Vector2 windowFrameSize = new Vector2(1516f, 922f);
		Vector2 origin = (Input.ScreenSize() / 2 - windowFrameSize / 2 + new Vector2(-2f, -11f)).Rounded();

		//DEBUG helper to find the origin =============================================================================================================================//
		for (int i = 0; i < 5; i++)
		{
			UI.DrawUiBackground(origin, new Vector2(50f, 50f));
		}

		//class_135.method_272(...) == UI.DrawTexture(texture, pos);
		UI.DrawUiBackground(origin + new Vector2(81f, 89f), new Vector2(508f, 767f));
		class_140.method_322(origin);
		class_135.method_272(class_238.field_1989.field_98.field_545, origin + new Vector2(597f, 92f)); // solitaire/board
		class_135.method_276(class_238.field_1989.field_102.field_817, Color.White, origin, windowFrameSize); // window/frame
		class_135.method_272(class_238.field_1989.field_102.field_821, origin + new Vector2(105f, 859f)); // window/story_detail
		class_135.method_272(class_238.field_1989.field_98.field_546, origin + new Vector2(808f, 847f)); // solitaire/board_top
		class_135.method_272(class_238.field_1989.field_102.field_825, origin + new Vector2(66f, 76f)); // window/vertical_bar_left
		class_135.method_272(class_238.field_1989.field_102.field_824, origin + new Vector2(574f, 76f)); // window/vertical_bar_center

		if (UI.DrawAndCheckCloseButton(origin, windowFrameSize, new Vector2(104, 98))) closeScreen();

		//=========================//
		// choose/change current board

		bool inClickBox = Bounds2.WithSize(origin + new Vector2(1337, 98), new Vector2(96, 57)).Contains(Input.MousePos());

		bool tryToDecrementBoardIndex = Input.IsSdlKeyPressed(SDL.enum_160.SDLK_LEFT) || (Input.IsLeftClickPressed() && inClickBox);
		bool tryToIncrementBoardIndex = Input.IsSdlKeyPressed(SDL.enum_160.SDLK_RIGHT) || (Input.IsRightClickPressed() && inClickBox);

		if (tryToDecrementBoardIndex && currentBoardIndex > 0)
		{
			playButtonClick();
			currentBoardIndex--;
		}
		if (tryToIncrementBoardIndex && currentBoardIndex < boards.Count-1)
		{
			playButtonClick();
			currentBoardIndex++;
		}
		var board = boards[currentBoardIndex];
		class_135.method_290(class_134.method_253("Board #", string.Empty).method_1060().method_441(), origin + new Vector2(1384f, 137f), class_238.field_1990.field_2142, Color.White, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, -2, Color.Black, class_238.field_1989.field_99.field_706.field_751, int.MaxValue, false, true);
		class_135.method_290("" + (currentBoardIndex + 1) + "/" + boards.Count, origin + new Vector2(1385f, 106f), class_238.field_1990.field_2143, Color.FromHex(3483687), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);

		//=========================//
		// draw/change board name
		Texture nameBox = class_238.field_1989.field_100.field_126;
		Vector2 namePos = new Vector2(1018f, 874f);
		class_135.method_272(nameBox, origin + namePos - new Vector2(nameBox.field_2056.X/2, nameBox.field_2056.Y*0.25f));
		class_135.method_290(class_134.method_253(board.name, string.Empty).method_1058(), origin + namePos, class_238.field_1990.field_2146, Color.White, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, -2, Color.Black, class_238.field_1989.field_99.field_706.field_751, int.MaxValue, false, true);

		ButtonDrawingLogic buttonDrawingLogic = class_140.method_313((string)class_134.method_253("Edit Name", string.Empty), origin + new Vector2(604f, 99f), 132, 53);
		if (buttonDrawingLogic.method_824(true, true))
		{
			playButtonClick();
			var editBox = MessageBoxScreen.method_1096(Bounds2.WithSize(origin, windowFrameSize), false, (string)class_134.method_253("Please edit the board name:", string.Empty), board.name, (string)class_134.method_253("Save Changes", string.Empty), new Action<string>(x => { newBoardName = (Maybe<string>) x; }));
			GameLogic.field_2434.method_946(editBox);
		}

		if (newBoardName.method_1085())
		{
			board.name = newBoardName.method_1087();
			newBoardName = (Maybe<string>)struct_18.field_1431;
			return;
		}

		//=========================//
		// add/delete/export boards

		var buttonPosition = origin + new Vector2(746f, 99f);
		var buttonWidth = 132;
		var buttonOffset = new Vector2(buttonWidth + 2f, 0f);

		//add board
		Maybe<editableBoard> newBoard = (Maybe<editableBoard>)struct_18.field_1431;
		if (class_140.method_313((string)class_134.method_253("Add Board", string.Empty), buttonPosition, buttonWidth, 53).method_824(true, true))
		{
			newBoard = new editableBoard();
		}
		buttonPosition += buttonOffset;
		if (class_140.method_313((string)class_134.method_253("Copy Board", string.Empty), buttonPosition, buttonWidth, 53).method_824(true, true))
		{
			newBoard = new editableBoard(board);
		}
		if (newBoard.method_1085())
		{
			playButtonClick();
			//insert board into lineup
			boards.Add(new editableBoard()); // place holder
			int lastBoard = boards.Count - 1;
			for (int i = lastBoard - 1; i > currentBoardIndex; i--)
			{
				boards[i + 1] = boards[i];
			}
			currentBoardIndex++;
			boards[currentBoardIndex] = newBoard.method_1087();
			board = boards[currentBoardIndex];
		}

		//delete board
		buttonPosition += buttonOffset;
		if (class_140.method_313((string)class_134.method_253("Delete Board", string.Empty), origin + new Vector2(1014f, 99f), buttonWidth, 53).method_824(boards.Count > 1, true))
		{
			playButtonClick();
			boards.Remove(boards[currentBoardIndex]);
			currentBoardIndex = Math.Min(currentBoardIndex,boards.Count - 1);
		}

		buttonPosition += buttonOffset;
		if (class_140.method_313((string)class_134.method_253("Playtest", string.Empty), buttonPosition, buttonWidth, 53).method_824(CampaignLoader.currentCampaignIsRMC(), true))
		{
			playButtonClick();
			var solitaireGame = new SolitaireScreen(false);
			SolitaireGameState solitaireGameState = board.solitaireGameState();
			SigmarGardenPatcher.solitaireState_RMC = SolitaireState.method_1918(new SolitaireGameState(), solitaireGameState, new class_179<float>(-0.4f), new class_179<int>(0), new Dictionary<HexIndex, float>());
			GameLogic.field_2434.method_945(solitaireGame, (Maybe<class_124>)struct_18.field_1431, (Maybe<class_124>)struct_18.field_1431);
		}

		//=========================//
		// transform board

		buttonPosition = origin + new Vector2(90f, 99f);
		buttonWidth = 120;
		var buttonHeight = 60;
		buttonOffset = new Vector2(buttonWidth + 2f, 0f);
		if (class_140.method_313((string)class_134.method_253("Rotate CCW", string.Empty), buttonPosition, buttonWidth, 53).method_824(true, true))
		{
			playButtonClick();
			board.rotateBoard(1);
		}
		buttonPosition += buttonOffset;
		if (class_140.method_313((string)class_134.method_253("Rotate CW", string.Empty), buttonPosition, buttonWidth, 53).method_824(true, true))
		{
			playButtonClick();
			board.rotateBoard(-1);
		}
		buttonPosition += buttonOffset;
		if (class_140.method_313((string)class_134.method_253("Mirror Horz", string.Empty), buttonPosition, buttonWidth, 53).method_824(true, true))
		{
			playButtonClick();
			board.mirror60();
			board.rotateBoard(2);
		}
		buttonPosition += buttonOffset;
		if (class_140.method_313((string)class_134.method_253("Mirror Vert", string.Empty), buttonPosition, buttonWidth, 53).method_824(true, true))
		{
			playButtonClick();
			board.mirror60();
			board.rotateBoard(-1);
		}

		buttonPosition += new Vector2(0f, buttonHeight) - 3*buttonOffset;
		if (class_140.method_313((string)class_134.method_253("Randomize", string.Empty), buttonPosition, buttonWidth, 53).method_824(true, true))
		{
			playButtonClick();
			board.randomize();
		}

		if (Input.IsSdlKeyPressed(SDL.enum_160.SDLK_F3))
		{
			playButtonClick();
			var tempBoard = board;

			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(Path.Combine(dirPath, datPath), FileMode.Create, FileAccess.Write), Encoding.ASCII))
			{
				binaryWriter.Write(boards.Count);

				for (int i = 0; i < boards.Count; i++)
				{
					board = boards[i];
					//bytes used:
					// 1 for leading BB byte
					// 16 for setting
					// 182 (=91*2) for marbles
					// 1 for trailing DD byte
					binaryWriter.Write((byte) 0xBB);
					for (int b = 0; b < 16; b++)
					{
						binaryWriter.Write((byte)0x00);
					}
					foreach (var marble in board.marbles)
					{
						var hex = marble.Key;
						byte pos = (byte) (((hex.Q + 5) << 4) | (hex.R + 5));
						byte atom = (byte) (marble.Value + 0xF0);
						binaryWriter.Write(atom);
						binaryWriter.Write(pos);
					}
					binaryWriter.Write((byte)0xDD);
				}
			}

			board = tempBoard;
		}

		if (Input.IsSdlKeyPressed(SDL.enum_160.SDLK_F5))
		{
			playButtonClick();
			editableBoard.extractBitboards();
			playButtonClick();
		}



		//=========================//
		// draw atom pallete
		Vector2 boardCenter = origin + new Vector2(73f, 637f);
		Vector2 marblePosition(float x, float y) => boardCenter + new Vector2(66f * (x + 0.5f * y), 57f * y);
		var swatches = new Dictionary<int, float[]>()
		{
			{11, new float[2]{ 0, 3} }, {12, new float[2]{ 1, 3} }, {13, new float[2]{ 2, 3} }, {14, new float[2]{ 3, 3} },
			{4, new float[2]{ 1, 2} }, {5, new float[2]{ 2, 2} }, {6, new float[2]{ 3, 2} }, {10, new float[2]{ 4, 2} },
			{1, new float[2]{ 2, 1} }, {2, new float[2]{ 3, 1} }, {3, new float[2]{ 4, 1} }, {7, new float[2]{ 5, 1} },
			{15, new float[2]{ 3, 0} }, {8, new float[2]{ 4, 0} }, {9, new float[2]{ 5, 0} },
		};
		var swatchKeys = new Dictionary<string, int>()
		{
			{"1", 11 },{"2", 12 },{"3", 13 },{"4", 14 },
			{"Q", 4 },{"W", 5 },{"E", 6 },{"R", 10 },
			{"A", 1 },{"S", 2 },{"D", 3 },{"F", 7 },
			{"Z", 15 },{"X", 8 },{"C", 9 },
		};

		foreach (var swatch in swatches)
		{
			var marblePos = marblePosition(swatch.Value[0], swatch.Value[1]);
			var marbleInt = swatch.Key;
			Texture selected = class_238.field_1989.field_98.field_543;
			if (marbleInt == atomSwatch) class_135.method_271(selected, Color.White.WithAlpha(1f), marblePos + new Vector2(-37f, -38f));
			if (DrawAtomButton(marblePos, SigmarGardenPatcher.getAtomType(marbleInt)))
			{
				atomSwatch = swatch.Key;
				playButtonClick();
			}
		}
		foreach (var swatch in swatchKeys.Where(x => Input.IsKeyPressed(x.Key)))
		{
			atomSwatch = swatch.Value;
			playButtonClick();
		}

		//=========================//
		// draw marbles on the board
		boardCenter = origin + new Vector2(1017f, 507f);

		for (int i = 0; i < board.marbles.Count; i++)
		{
			var hex = board.marbles.ToArray()[i].Key;
			var Q = hex.Q;
			var R = hex.R;
			if (AtomButtonClicked(marblePosition(Q, R), true))
			{
				board.marbles[hex] = 0; playButtonClick();
			}
			else if (DrawAtomButton(marblePosition(Q, R), SigmarGardenPatcher.getAtomType(board.marbles[hex])))
			{
				board.marbles[hex] = atomSwatch;
				playButtonClick();
			}
			//DEBUG: draw coordinates
			//class_135.method_290("" + Q + "," + R, marblePosition(Q, R) + new Vector2(0f, -35f), class_238.field_1990.field_2143, Color.FromHex(3483687), (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
		}

		/*


		this.field_3872.method_2172(timeDelta, origin + new Vector2(89f, 94f), this.field_3871, SolitaireScreen.field_3868);
		if (this.method_1889().method_1921() && this.method_1894())
			this.method_1891();
		if (class_140.method_313((string)class_134.method_253("New Game", string.Empty), origin + new Vector2(604f, 99f), 132, 53).method_824(this.method_1894(), true))
		{
			this.method_1891();
			GameLogic.field_2434.field_2459.method_37();
			playButtonClick();
		}
		if (SolitaireScreen.method_1895(origin + new Vector2(1292f, 99f), 37, 53).method_824(this.method_1893(), true))
		{
			GameLogic.field_2434.method_946((IScreen)new class_16());
			playButtonClick();
		}



		SolitaireScreen.class_412 class412 = new SolitaireScreen.class_412();

		
		Action<AtomType, Vector2> action = new Action<AtomType, Vector2>(class412.method_1901);
		this.field_3873 = (Maybe<AtomType>)struct_18.field_1431;
		Vector2 vector2_2 = origin + new Vector2(770f, (float)sbyte.MaxValue);
		action(class_175.field_1675, vector2_2);
		vector2_2.X += 25f;
		class_135.method_272(class_238.field_1989.field_98.field_549, vector2_2 + new Vector2(0.0f, -2f));
		vector2_2.X += 29f;
		action(class_175.field_1676, vector2_2);
		vector2_2.X += 42f;
		action(class_175.field_1678, vector2_2);
		vector2_2.X += 42f;
		action(class_175.field_1679, vector2_2);
		vector2_2.X += 42f;
		action(class_175.field_1677, vector2_2);
		vector2_2.X += 30f;
		if (this.field_3874)
		{
			vector2_2.X -= 5f;
			class_135.method_272(class_238.field_1989.field_98.field_549, vector2_2 + new Vector2(0.0f, -2f));
			vector2_2.X += 29f;
			action(class_175.field_1690, vector2_2);
			vector2_2.X += 30f;
			class_135.method_272(class_238.field_1989.field_98.field_550, vector2_2 + new Vector2(0.0f, -23f));
			vector2_2.X += 30f;
		}
		else
		{
			class_135.method_272(class_238.field_1989.field_98.field_550, vector2_2 + new Vector2(0.0f, -23f));
			vector2_2.X += 30f;
			action(class_175.field_1680, vector2_2);
			vector2_2.X += 25f;
			class_135.method_272(class_238.field_1989.field_98.field_549, vector2_2 + new Vector2(0.0f, -2f));
			vector2_2.X += 29f;
		}
		action(class_175.field_1681, vector2_2);
		vector2_2.X += 40f;
		action(class_175.field_1683, vector2_2);
		vector2_2.X += 40f;
		action(class_175.field_1684, vector2_2);
		vector2_2.X += 40f;
		action(class_175.field_1682, vector2_2);
		vector2_2.X += 40f;
		action(class_175.field_1685, vector2_2);
		vector2_2.X += 40f;
		action(class_175.field_1686, vector2_2);
		class412.field_3885 = new Func<HexIndex, Vector2>(class412.method_1902);
		class412.field_3887 = new Action<SolitaireGameState, AtomType, HexIndex>(class412.method_1903);
		this.method_1889().method_1933(SolitaireScreen.class_301.field_3897 ?? (SolitaireScreen.class_301.field_3897 = new Action<SolitaireState.WaitingForNewGameFields>(SolitaireScreen.class_301.field_2343.method_1915)), new Action<SolitaireState.struct_123>(class412.method_1904), new Action<SolitaireState.struct_124>(class412.method_1905), new Action<SolitaireState.WonLastGameFields>(class412.method_1906));

		*/

	}

	void closeScreen()
	{
		GameLogic.field_2434.method_947((Maybe<class_124>)Transitions.field_4105, (Maybe<class_124>)struct_18.field_1431);
		// save boards
		if (dirPath == "") return;
		using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(Path.Combine(dirPath, filePath), FileMode.Create), Encoding.ASCII))
		{
			bool firstBoard = true;
			string boardStr;
			for (int i = 0; i < boards.Count; i++)
			{
				if (!boards[i].marbles.Keys.Any(x => boards[i].marbles[x] != 0)) continue;
				boardStr = boards[i].saveBoard();
				if (!firstBoard) boardStr = ";\n" + boardStr;
				binaryWriter.Write(boardStr.ToCharArray());
				firstBoard = false;
			}
		}
	}

	//=============================================================================================================================//
	// hooking
	public static void DrawButtonForSigmarBoardEditor(On.class_178.orig_method_50 orig, class_178 class178_self, float timeDelta)
	{
		if (GameLogic.field_2434.method_938() is BoardEditorScreen)
			return;
		orig(class178_self, timeDelta);

		float num = 65f;
		Vector2 vector2_1 = new(570f, 440f);
		Vector2 vector2_2 = (class_115.field_1433 / 2 - vector2_1 / 2).Rounded();
		Vector2 vector2_3 = new(161f, 256f);
		Vector2 vector2_4 = vector2_3 + new Vector2(0.0f, -num);
		Vector2 vector2_5 = vector2_4 + new Vector2(0.0f, -num);
		Vector2 vector2_6 = vector2_5 + new Vector2(0.0f, -num * 3);

		if (UI.DrawAndCheckBoxButton("[RMC] Board Editor", vector2_2 + vector2_6))
		{
			GameLogic.field_2434.method_945(new BoardEditorScreen(), (Maybe<class_124>)struct_18.field_1431, (Maybe<class_124>)Transitions.field_4104);
		}
	}
}

public class editableBoard
{
	const string atomChars = "?LTICSGQVMOAWFER!";
	const string posChars = "0123456789x";

	public string name;
	public string settingsString;
	public Dictionary<HexIndex, int> marbles;

	public editableBoard(string boardString = "ExampleBoard|settings|G46,L56,S45,Q55,T65,C54,I64")
	{
		marbles = new Dictionary<HexIndex, int>();
		var data = boardString.Split('|');
		if (data.Length != 3)
		{
			data = new string[3]
			{
				"MALFORMED BOARD",
				"settings",
				"R46,R65,R54",
			};
		}
		name = filterDelimiters(data[0]);
		settingsString = data[1];
		var marbleStrings = data[2].Split(',');
		for (int i = 0; i < marbleStrings.Length; i++)
		{
			string str = marbleStrings[i];
			if (str.Length != 3) continue;
			int atomInt, Q, R;
			atomInt = 0;
			for (int j = 0; j < atomChars.Length; j++)
			{
				if (atomChars[j] == str[0])
				{
					atomInt = j;
					break;
				}
			}
			Q = Math.Min(Math.Max(str[1] - '0', 0), 10);
			R = Math.Min(Math.Max(str[2] - '0', 0), 10);
			var hex = new HexIndex(Q - 5, R - 5);
			if (!marbles.ContainsKey(hex)) marbles.Add(hex, atomInt);
		}

		// fill the rest of the board with blanks
		fillInHolesWithBlanks();
	}
	public editableBoard(editableBoard boardToCopy)
	{
		name = boardToCopy.name + " (Copy)";
		settingsString = boardToCopy.settingsString;
		marbles = new Dictionary<HexIndex, int>();
		foreach (var marble in boardToCopy.marbles)
		{
			marbles.Add(marble.Key, marble.Value);
		}
	}
	void fillInHolesWithBlanks()
	{
		for (int i = -5; i <= 5; i++)
		{
			for (int j = -5; j <= 5; j++)
			{
				var hex = new HexIndex(i, j);
				if (-5 <= i + j && i + j <= 5 && !marbles.ContainsKey(hex))
				{
					marbles.Add(hex, 0);
				}
			}
		}
	}

	string filterDelimiters(string str)
	{
		if (str == "") return "";
		string ret = "";

		for (int i = 0; i < str.Length; i++)
		{
			char c = str[i];

			if (c == ';' || c == '|') c = ':';
			if (c != '\n' && c != '\r') ret += c;
		}
		return ret;
	}
	public string saveBoard()
	{
		string ret = filterDelimiters(name) + "|";
		ret += "000" + "|";
		bool firstMarble = true;
		foreach (var hex in marbles.Keys.Where(x => marbles[x] != 0))
		{
			if (!firstMarble) ret += ",";
			ret += atomChars[marbles[hex]];
			ret += posChars[hex.Q + 5];
			ret += posChars[hex.R + 5];
			firstMarble = false;
		}
		return ret;
	}
	public void rotateBoard(int turns)
	{
		var newMarbles = new Dictionary<HexIndex, int>();
		foreach (var marble in marbles)
		{
			newMarbles.Add(marble.Key.Rotated(new HexRotation(turns)), marble.Value);
		}
		marbles = newMarbles;
	}
	public void mirror60()
	{
		var newMarbles = new Dictionary<HexIndex, int>();
		foreach (var marble in marbles)
		{
			newMarbles.Add(new HexIndex(marble.Key.R, marble.Key.Q), marble.Value);
		}
		marbles = newMarbles;
	}

	public void randomize()
	{
		// implement later?


		// randomize the board with whatever i want









	}


	public SolitaireGameState solitaireGameState()
	{
		var ret = new SolitaireGameState();

		foreach (var marble in marbles.Where(marble => marble.Value != 0))
		{
			ret.field_3864.Add(marble.Key + new HexIndex(5, 0), SigmarGardenPatcher.getAtomType(marble.Value));
		}
		return ret;
	}



	public static void extractBitboards()
	{
		List<string> bitboards = new();
		// read in the boards from solitaire.dat
		// and remove duplicates that are isomorphics up to rotation and mirroring
		// total number of isomorphic boards: 16968
		using (BinaryReader binaryReader = new BinaryReader(new FileStream(Path.Combine("Content", "solitaire.dat"), FileMode.Open, FileAccess.Read)))
		{
			int boardCount = binaryReader.ReadInt32();
			Logger.Log("Extracting from solitaire.dat");
			Logger.Log("0 %");
			for (int b = 0; b < boardCount; b++)
			{
				if (b % 500 == 499) Logger.Log(((b+1)/500) + " %");
				char[,] boards = new char[12, 121];
				for (int i = 0; i < 12; i++)
				{
					for (int j = 0; j < 121; j++)
					{
						boards[i, j] = '.';
					}
				}
				for (int a = 0; a < 55; a++)
				{
					var uselessAtomByte = binaryReader.ReadByte();
					int hexQ = binaryReader.ReadByte();
					int hexR = binaryReader.ReadSByte();
					var hexA = new HexIndex(hexQ, hexR);
					var hexB = new HexIndex(hexQ + hexR, -hexR);
					for (int r = 0; r < 6; r++)
					{
						boards[r, hexA.Q * 11 + hexA.R + 5] = 'X';
						boards[r + 6, hexB.Q * 11 + hexB.R + 5] = 'X';
						hexA = hexA.RotatedAround(new HexIndex(5, 0), new HexRotation(1));
						hexB = hexB.RotatedAround(new HexIndex(5, 0), new HexRotation(1));
					}
				}
				string[] boardstrings = new string[12];
				bool boardAlreadyExists = false;
				for (int i = 0; i < 12; i++)
				{
					boardstrings[i] = "";
					for (int j = 0; j < 121; j++)
					{
						boardstrings[i] += boards[i, j];
					}
					if (bitboards.Contains(boardstrings[i]))
					{
						boardAlreadyExists = true;
						continue;
					}
				}
				if (!boardAlreadyExists) bitboards.Add(boardstrings[0]);
			}
		}

		using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(Path.Combine("Content", "peebus.dat"), FileMode.Create, FileAccess.Write), Encoding.ASCII))
		{
			int boardCount = bitboards.Count;
			Logger.Log("Found " + boardCount + " non-isomorphic boards - writing to peebus.dat");

			binaryWriter.Write(boardCount);
			foreach (var board in bitboards)
			{
				for (int i = 0; i < 16; i++)
				{
					int boardInt = 0;
					for (int j = 0; j < 8; j++)
					{
						if ((i * 8 + j < 121) && board[i * 8 + j] == 'X')
						{
							boardInt += 1 << j;
						}
					}
					binaryWriter.Write((byte) (boardInt & 0xFF));
				}
			}
		}
	}
}