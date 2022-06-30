using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using RhuEngine.Linker;

using RNumerics;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RhuEngine
{
	public class RText : IDisposable
	{
		public RText(RFont rFont) {
			TargetFont = rFont;
			TextOptions = rFont.MakeTextOptions(96,HorizontalAlignment.Left, VerticalAlignment.Top,TextAlignment.Center);
		}

		public RFont TargetFont;

		public TextOptions TextOptions;

		public RTexture2D texture2D;

		public event Action UpdatedTexture;

		public FontRectangle FontRectangle;

		public float AspectRatio;


		private string _text;

		public string Text
		{
			get => _text;
			set {
				if (_text != value) {
					_text = value;
					TextUpdated();
				}
			}
		}

		private void TextUpdated() {
			if (TargetFont is null) {
				throw new Exception("Need a font to Make text");
			}
			texture2D = TargetFont?.RenderText(_text,TextOptions);
			FontRectangle = TargetFont?.GetSizeOfText(_text, TextOptions) ?? new FontRectangle();
			AspectRatio = FontRectangle.Width / FontRectangle.Height;
			UpdatedTexture?.Invoke();
		}

		public void Dispose() {
			TargetFont = null;
			UpdatedTexture = null;
			texture2D?.Dispose();
			texture2D = null;
		}
	}


	public class RFont
	{
		public Font MainFont { get; }
		public FontCollection FallBacks { get; }

		public RFont(Font mainFont, FontCollection fallBacks) {
			MainFont = mainFont;
			FallBacks = fallBacks;
		}


		public RFont(Font mainFont) {
			MainFont = mainFont;
		}

		public TextOptions MakeTextOptions(float dpi = 96f, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, VerticalAlignment VerticalAlignment = VerticalAlignment.Top, TextAlignment TextAlignment = TextAlignment.Start, TextDirection TextDirection = TextDirection.Auto, WordBreaking WordBreaking = WordBreaking.Normal,float WrappingLength = -1f, Vector2f? Origin = null, float LineSpacing = 1f, HintingMode hintingMode = HintingMode.None, float TabWidth = 4f, LayoutMode LayoutMode = LayoutMode.HorizontalTopBottom, KerningMode KerningMode = KerningMode.Normal) {
			return FallBacks is null
				? new TextOptions(MainFont) {
					Dpi = dpi,
					TabWidth = TabWidth,
					HintingMode = hintingMode,
					LayoutMode = LayoutMode,
					KerningMode = KerningMode,
					LineSpacing = LineSpacing,
					Origin = (Vector2)(Origin??Vector2f.Zero),
					WrappingLength = WrappingLength,
					WordBreaking = WordBreaking,
					TextAlignment = TextAlignment,
					TextDirection = TextDirection,
					HorizontalAlignment = horizontalAlignment,
					VerticalAlignment = VerticalAlignment,
				}
				: new TextOptions(MainFont) {
				Dpi = dpi,
				FallbackFontFamilies = FallBacks.Families.ToArray(),
					TabWidth = TabWidth,
					HintingMode = hintingMode,
					LayoutMode = LayoutMode,
					KerningMode = KerningMode,
					LineSpacing = LineSpacing,
					Origin = (Vector2)(Origin??Vector2f.Zero),
					WrappingLength = WrappingLength,
					WordBreaking = WordBreaking,
					TextAlignment = TextAlignment,
					TextDirection = TextDirection,
					HorizontalAlignment = horizontalAlignment,
					VerticalAlignment = VerticalAlignment,
				};
		}

		public RTexture2D RenderText(string text,TextOptions textOptions) {
			var size = TextMeasurer.Measure(text, textOptions);
			if (size == FontRectangle.Empty) {
				return RTexture2D.White;
			}
			using var img = new Image<Rgba32>((int)size.Width, (int)size.Height);
			img.Mutate(x => x.DrawText(textOptions, text, Color.White));
			return new ImageSharpTexture(img).CreateTextureAndDisposes();
		}

		public FontRectangle GetSizeOfText(string text, TextOptions textOptions) {
			return TextMeasurer.Measure(text, textOptions);
		}
	}
}
