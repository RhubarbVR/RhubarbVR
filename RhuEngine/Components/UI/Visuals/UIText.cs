using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;
using SixLabors.Fonts;

namespace RhuEngine.Components
{
	public enum EVerticalAlien
	{
		Bottom,
		Center,
		Top,
	}
	public enum EHorizontalAlien
	{
		Left,
		Middle,
		Right,
	}

	[Category(new string[] { "UI/Visuals" })]
	public class UIText : RenderUIComponent
	{

		[Default("<color=hsv(240,100,100)>Hello<color=blue><size14>World \n <size5>Trains \n are cool man<size10>\nHello ")]
		public readonly Sync<string> Text;
		[Default("")]
		public readonly Sync<string> EmptyString;
		[Default("<color=rgb(0.9,0.9,0.9)>null")]
		public readonly Sync<string> NullString;
		public readonly AssetRef<RFont> Font;
		public readonly Sync<Colorf> StartingColor;
		[Default(0.1f)]
		public readonly Sync<float> Leading;
		[Default(FontStyle.Regular)]
		public readonly Sync<FontStyle> StartingStyle;

		[Default(10f)]
		public readonly Sync<float> StatingSize;

		[Default(false)]
		public readonly Sync<bool> Password;

		public readonly Sync<Vector2f> MaxClamp;

		public readonly Sync<Vector2f> MinClamp;

		[Default(EVerticalAlien.Center)]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		public readonly Sync<EHorizontalAlien> HorizontalAlien;

		[Default(true)]
		public readonly Sync<bool> MiddleLines;

		public Matrix textOffset = Matrix.S(1);

		public override void CutElement(bool cut, bool update = true) {
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
			StartingColor.Value = Colorf.White;
			MinClamp.Value = Vector2f.MinValue;
			MaxClamp.Value = Vector2f.MaxValue;
		}

		public override void Render(Matrix matrix) {
		}

		public override void RenderTargetChange() {
		}
	}
}
