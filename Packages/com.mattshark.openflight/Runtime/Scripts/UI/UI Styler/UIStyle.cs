using UnityEngine;
using System;
using static OpenFlightVRC.UI.UIStyleMarkup;

namespace OpenFlightVRC.UI
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	internal class StyleMarkupLinkAttribute : Attribute
	{
		public StyleClass Class { get; private set; }

		private StyleMarkupLinkAttribute() { }

		public StyleMarkupLinkAttribute(StyleClass styleClass)
		{
			Class = styleClass;
		}
	}

	[CreateAssetMenu(fileName = "UIStyle", menuName = "VRC Packages/OpenFlight/UIStyle", order = 1)]
	internal class UIStyle : ScriptableObject
	{
		[StyleMarkupLink(StyleClass.Background)]
		public Color backgroundColor = Color.black;

		[StyleMarkupLink(StyleClass.FieldBackground)]
		public Color fieldBackgroundColor = Color.black;

		[StyleMarkupLink(StyleClass.ButtonBackground)]
		public Color buttonBackgroundColor = Color.black;

		[StyleMarkupLink(StyleClass.SliderHandle)]
		public Color sliderHandle = Color.black;

		[StyleMarkupLink(StyleClass.SliderProgress)]
		public Color sliderProgress = Color.black;

		[StyleMarkupLink(StyleClass.SliderBackground)]
		public Color sliderBackground = Color.black;

		[StyleMarkupLink(StyleClass.Icon)]
		public Color iconColor = Color.black;

		[StyleMarkupLink(StyleClass.IconDropShadow)]
		public Color iconDropShadowColor = Color.black;

		[StyleMarkupLink(StyleClass.HighlightedButton)]
		public Color highlightedButtonColor = Color.black;

		[StyleMarkupLink(StyleClass.PlaceholderText)]
		public Color placeholderTextColor = Color.black;

		[StyleMarkupLink(StyleClass.Text)]
		public Color textColor = Color.black;

		[StyleMarkupLink(StyleClass.TextDropShadow)]
		public Color textDropShadowColor = Color.black;

		[StyleMarkupLink(StyleClass.InvertedText)]
		public Color invertedTextColor = Color.black;

		[StyleMarkupLink(StyleClass.RedIcon)]
		public Color redIconColor = Color.black;

		[StyleMarkupLink(StyleClass.InvertedIcon)]
		public Color invertedIconColor = Color.black;

		[StyleMarkupLink(StyleClass.TextHighlight)]
		public Color textHighlightColor = Color.black;

		[StyleMarkupLink(StyleClass.TextCaret)]
		public Color textCaretColor = Color.white;

		[StyleMarkupLink(StyleClass.GraphLine)]
		public Color graphLine = Color.white;

		[StyleMarkupLink(StyleClass.GraphPoint)]
		public Color graphPoint = Color.white;

		[StyleMarkupLink(StyleClass.GraphBackground)]
		public Color graphBackground = Color.white;

		[StyleMarkupLink(StyleClass.ActiveTab)]
		public Color activeTab = Color.white;

		[StyleMarkupLink(StyleClass.InActiveTab)]
		public Color inActiveTab = Color.white;
	}
}
