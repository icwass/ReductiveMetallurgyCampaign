//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
using SDL2;
using System;
//using System.IO;
//using System.Linq;
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
//using Song = class_186;
//using Tip = class_215;
using Font = class_1;

public class Document
{
	string ID;
	Texture baseTexture = null;
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

			if (documentID == "rmc-document-epilogue")
			{
				GameLogic.field_2434.method_945(new RMCCreditsScreen(), (Maybe<class_124>)Transitions.field_4109, (Maybe<class_124>)Transitions.field_4108);
			}
			else
			{
				class_238.field_1991.field_1875.method_28(1f); // ui_paper_back
				GameLogic.field_2434.method_949();
			}
		}
	}


	//==================================================//
	// DrawItem helper class
	public sealed class DrawItem
	{
		static Color textColor => DocumentScreen.field_2410;
		Vector2 position = new Vector2(0f, 0f);
		Color color = textColor;

		Texture texture = null;
		float rotation = 0f;
		float scale = 1f;
		float alpha = 1f;

		Font font = getFont(null);
		enum_0 alignment = (enum_0)0;
		float lineSpacing = 1f;
		float columnWidth = float.MaxValue;
		bool handwritten = false;

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// constructors
		public DrawItem() { }

		public DrawItem(Vector2 position, Font font, Color color, enum_0 alignment = 0, float lineSpacing = 1f, float columnWidth = float.MaxValue, bool handwritten = false)
		{
			this.position = position;
			this.font = font;
			this.color = color;
			this.alignment = alignment;
			this.lineSpacing = lineSpacing;
			this.columnWidth = columnWidth;
			this.handwritten = handwritten;
		}
		public DrawItem(Vector2 position, Font font, enum_0 alignment = 0, float lineSpacing = 1f, float columnWidth = float.MaxValue, bool handwritten = false)
		: this(position, font, textColor, alignment, lineSpacing, columnWidth, handwritten)
		{ }

		public DrawItem(Vector2 position, Texture texture, Color color, float scale = 1f, float rotation = 0f, float alpha = 1f)
		{
			this.position = position;
			this.texture = texture;
			this.color = color;
			this.rotation = rotation;
			this.scale = scale;
			this.alpha = alpha;
		}
		public DrawItem(Vector2 position, Texture texture, float scale = 1f, float rotation = 0f, float alpha = 1f)
		: this(position, texture, Color.White, scale, rotation, alpha)
		{ }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// public helpers
		public static enum_0 getAlignment(string align)
		{
			align ??= "left";
			switch (align.ToLower())
			{
				default:		return (enum_0) 0;
				case "center":	return (enum_0) 1;
				case "right":	return (enum_0) 2;
			}
		}

		public static Font getFont(string font)
		{
			string defaultFont = "cormorant 15";
			font ??= defaultFont;
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
			return FontBank.ContainsKey(font) ? FontBank[font] : FontBank[defaultFont];
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