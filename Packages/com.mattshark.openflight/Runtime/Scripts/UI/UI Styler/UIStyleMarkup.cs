using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenFlightVRC.UI
{
	[AddComponentMenu("Udon Sharp/Video/UI/Style Markup")]
	internal class UIStyleMarkup : MonoBehaviour
	{
		public enum StyleClass
		{
			Background,
			FieldBackground,
			ButtonBackground,
			SliderHandle,
			SliderProgress,
			SliderBackground,
			Icon,
			IconDropShadow,
			HighlightedButton,
			PlaceholderText,
			Text,
			TextDropShadow,
			InvertedText,
			RedIcon,
			InvertedIcon,
			TextHighlight,
			TextCaret,
			GraphLine,
			GraphPoint,
			GraphBackground,
			ActiveTab,
			InActiveTab,
		}

#pragma warning disable CS0649
		public StyleClass styleClass;
		public Graphic targetGraphic;
		public LineRenderer targetLineRenderer;
#pragma warning restore CS0649

		private void Reset()
		{
			hideFlags = HideFlags.DontSaveInBuild;
			targetGraphic = GetComponent<Graphic>();
			targetLineRenderer = GetComponent<LineRenderer>();
		}
	}
}
