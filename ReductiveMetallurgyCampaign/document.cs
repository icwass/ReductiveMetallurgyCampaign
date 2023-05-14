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
//using Song = class_186;
//using Tip = class_215;
using Font = class_1;

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

public class Document
{
	string ID;
	Texture baseTexture;
	Action<Language, string[]> drawFunction;

	//==================================================//
	// constructors
	public Document(string documentID, Texture documentBaseTexture, Action<Language, string[]> documentDrawFunction)
	{
		this.ID = documentID;
		this.baseTexture = documentBaseTexture;
		this.drawFunction = documentDrawFunction;
		documentDatabase.Add(this.ID, this);
	}
	public Document(string documentID, Texture documentBaseTexture, Action<Language, Vector2, string[]> draw)
	: this(documentID, documentBaseTexture, makeDocumentDrawFunction(documentBaseTexture, draw))
	{ }
	public Document(string documentID, Texture documentBaseTexture, List<DrawItem> drawItems)
	: this(documentID, documentBaseTexture, makeDocumentDrawFunction(drawItems))
	{ }

	private static Action<Language, string[]> makeDocumentDrawFunction(Texture baseTexture, Action<Language, Vector2, string[]> draw)
	{
		return (lang, textArray) => {
			Vector2 origin = class_115.field_1433 / 2 - baseTexture.field_2056.ToVector2() / 2;
			class_135.method_272(baseTexture, origin.Rounded()); //draw document
			draw(lang, origin, textArray);
		};
	}
	private static Action<Language, Vector2, string[]> makeDocumentDrawFunction(List<DrawItem> drawItems)
	{
		return (lang, origin, textArray) => {
			int maxIndex = textArray.Length;
			int index = 0;
			for (int i = 0; i < drawItems.Count; i++)
			{
				string text = index < maxIndex ? textArray[index] : null;
				index += drawItems[i].draw(origin, text) ? 1 : 0;
			}
		};
	}
	//==================================================//
	//global stuff
	private static Dictionary<string, Document> documentDatabase = new Dictionary<string, Document>();


	public static void LoadDocuments(List<DocumentModelRMC> Documents)
	{
		foreach (var d in Documents)
		{
			Texture base_texture = class_238.field_1989.field_85.field_570; // letter-5
			if (!string.IsNullOrEmpty(d.Texture))
			{
				base_texture = class_235.method_615(d.Texture);
			}
			List<DrawItem> drawItems = new();
			int maxIndex = d.DrawItems == null ? 0 : d.DrawItems.Count;
			for (int i = 0; i < maxIndex; i++)
			{
				drawItems.Add(new DrawItem(d.DrawItems[i]));
			}
			new Document(d.ID, base_texture, drawItems);
		}
	}

	public static void Load()
	{
		On.DocumentScreen.method_50 += DocumentScreen_Method_50;
	}

	private static void DocumentScreen_Method_50(On.DocumentScreen.orig_method_50 orig, DocumentScreen documentScreen_self, float timeDelta)
	{
		var documentScreen_dyn = new DynamicData(documentScreen_self);
		var class264 = documentScreen_dyn.Get<class_264>("field_2409");
		string documentID = class264.field_2090;
		if (!documentDatabase.ContainsKey(documentID))
		{
			orig(documentScreen_self, timeDelta);
			return;
		};
		//========================================================//
		Document document = documentDatabase[documentID];
		Language lang = class_134.field_1504;
		string[] textArray = documentScreen_dyn.Get<string[]>("field_2408");
		document.drawFunction(lang, textArray);

		// player input / scene transition
		if (Input.IsLeftClickPressed() || Input.IsSdlKeyPressed(SDL.enum_160.SDLK_ESCAPE))
		{
			GameLogic.field_2434.field_2451.method_574(class264);
			GameLogic.field_2434.method_949();
			class_238.field_1991.field_1875.method_28(1f); // sound effect: ui_paper_back
		}
	}


	//==================================================//
	// DrawItem helper class
	public sealed class DrawItem
	{
		Vector2 position = new Vector2(0f, 0f);
		Texture texture = null;
		float rotation = 0f;
		float scale = 1f;
		float alpha = 1f;
		Font font = class_238.field_1990.field_2150;
		Color color = DocumentScreen.field_2410;
		enum_0 alignment = (enum_0)0;
		float lineSpacing = 1f;
		float columnWidth = float.MaxValue;
		bool handwritten = false;
		public DrawItem(Vector2 position, Font font, Color color, enum_0 alignment, float lineSpacing, float columnWidth, bool handwritten)
		{
			this.position = position;
			this.font = font;
			this.color = color;
			this.alignment = alignment;
			this.lineSpacing = lineSpacing;
			this.columnWidth = columnWidth;
			this.handwritten = handwritten;
		}
		public DrawItem(Vector2 position, Texture texture, float rotation, float scale)
		{
			this.position = position;
			this.texture = texture;
			this.rotation = rotation;
			this.scale = scale;
		}

		//item.Position, item.Texture, item.Rotation, item.Scale, item.Alpha, item.Font, item.Color, item.Align, item.LineSpacing, item.ColumnWidth, item.Handwritten

		public DrawItem(DrawItemModelRMC item)
		{
			void conditionalSet<T>(string input, ref T field, Func<string, T> func, bool check = true) { if (!string.IsNullOrEmpty(input) && check) field = func(input); };

			if (!string.IsNullOrEmpty(item.Position))
			{
				float x, y;
				string pos = item.Position;
				if (float.TryParse(pos.Split(',')[0], out x) && float.TryParse(pos.Split(',')[1], out y))
				{
					this.position = new Vector2(x, y);
				}
			}

			if (!string.IsNullOrEmpty(item.Texture))
			{
				//make graphic item
				conditionalSet(item.Texture, ref this.texture, x => class_235.method_615(item.Texture));
				this.color = Color.White;
				conditionalSet(item.Color, ref this.color, x => Color.FromHex(int.Parse(x)));
				float temp = 0f;
				conditionalSet(item.Rotation, ref this.rotation, x => temp, float.TryParse(item.Rotation, out temp));
				temp = 1f;
				conditionalSet(item.Scale, ref this.scale, x => temp, float.TryParse(item.Scale, out temp));
				temp = 1f;
				conditionalSet(item.Alpha, ref this.alpha, x => temp, float.TryParse(item.Alpha, out temp));
			}
			else
			{
				//make a text item
				if (!string.IsNullOrEmpty(item.Align))
				{
					if (item.Align.ToLower() == "center") this.alignment = (enum_0)1;
					if (item.Align.ToLower() == "right") this.alignment = (enum_0)2;
				}
				conditionalSet(item.Font, ref this.font, x => getFont(item.Font));
				conditionalSet(item.Color, ref this.color, x => Color.FromHex(int.Parse(x)));
				conditionalSet(item.LineSpacing, ref this.lineSpacing, x => float.Parse(x));
				conditionalSet(item.ColumnWidth, ref this.columnWidth, x => float.Parse(x));
				this.handwritten = item.Handwritten;
			}
		}

		public static Font getFont(string font)
		{
			Dictionary<string, Font> FontBank = new()
			{
				{"crimson 21", class_238.field_1990.field_2146},
				{"crimson 16.5", class_238.field_1990.field_2145},
				{"crimson 15", class_238.field_1990.field_2144},
				{"crimson 13", class_238.field_1990.field_2143},
				{"crimson 12", class_238.field_1990.field_2142},
				{"crimson 10.5", class_238.field_1990.field_2141},
				{"crimson 9.75", class_238.field_1990.field_2140},

				{"cinzel 21", class_238.field_1990.field_2147},
				{"cormorant 22.5", class_238.field_1990.field_2148},
				{"cormorant 18", class_238.field_1990.field_2149},
				{"cormorant 15", class_238.field_1990.field_2150},
				{"cormorant 12.75", class_238.field_1990.field_2151},
				{"cormorant 11", class_238.field_1990.field_2152},

				{"reenie 17.25", class_238.field_1990.field_2153},
				{"naver 17.25", class_238.field_1990.field_2154},
			};
			return FontBank.ContainsKey(font) ? FontBank[font] : class_238.field_1990.field_2150;
		}

		public static Font getHandwrittenFont() => getFont(class_134.field_1504 == Language.Korean ? "naver 17.25" : "reenie 17.25");

		public bool draw(Vector2 origin, string text)
		{
			// returns true if it drew as text
			if (this.texture != null)
			{
				Vector2 textureDimensions = this.texture.field_2056.ToVector2();
				Matrix4 Translation = Matrix4.method_1070((origin + this.position).ToVector3(0.0f));
				Matrix4 Rotation = Matrix4.method_1073(this.rotation);
				Matrix4 Scaling = Matrix4.method_1074((textureDimensions * this.scale).ToVector3(0.0f));
				Matrix4 Transformation = Translation * Rotation * Scaling; // order is important
				class_135.method_262(this.texture, this.color.WithAlpha(this.alpha), Transformation);
			}
			else if (!string.IsNullOrEmpty(text))
			{
				if (handwritten) font = getHandwrittenFont();
				drawText(text, origin + position, font, color, alignment, lineSpacing, columnWidth, float.MaxValue, int.MaxValue, true);
				return true;
			}
			return false;
		}

		public static Bounds2 drawText(
		string text,
		Vector2 position,
		Font font,
		Color color,
		enum_0 alignment = (enum_0)0,
		float lineSpacing = 1f,
		float columnWidth = float.MaxValue,
		float truncateWidth = float.MaxValue,
		int maxCharactersDrawn = int.MaxValue,
		bool returnBoundingBox = false)
		{
			return class_135.method_290(text, position, font, color, alignment, lineSpacing,
				0.6f/*default for documents, not sure what it does*/,
				columnWidth,
				truncateWidth,
				0/*default for documents, not sure what it does*/,
				new Color()/*default for documents, not sure what it does*/,
				null/*default texture for documents, changing it seems to affect the color somehow, not sure what it actually does*/,
				Math.Max(-1, maxCharactersDrawn - 1),
				returnBoundingBox,
				true/*false will hide the text - however, this can be done by setting maxCharactersDrawn == 0*/
			);
		}
	}
}