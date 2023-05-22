using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

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
		public bool ignoreAlpha = false;
#pragma warning restore CS0649

		private void Reset()
		{
			hideFlags = HideFlags.DontSaveInBuild;
			targetGraphic = GetComponent<Graphic>();
			targetLineRenderer = GetComponent<LineRenderer>();
			ignoreAlpha = false;

#if UNITY_EDITOR
			//get parent styler
			UIStyler styler = GetComponentInParent<UIStyler>();

			if (styler != null)
				styler.ApplyStyle();
			else
				Debug.LogError("UIStyleMarkupEditor: Could not find parent UIStyler component!");
#endif
		}
	}

	//add a basic custom inspector that detects when the style class is changed and updates the target graphic
#if UNITY_EDITOR
	[CustomEditor(typeof(UIStyleMarkup))]
	internal class UIStyleMarkupEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			//get parent styler
			UIStyler styler = (target as UIStyleMarkup).GetComponentInParent<UIStyler>();

			//display the default inspector
			base.OnInspectorGUI();

			if(EditorGUI.EndChangeCheck())
			{
				if (styler != null)
					styler.ApplyStyle();
				else
					Debug.LogError("UIStyleMarkupEditor: Could not find parent UIStyler component!");
			}
		}
	}
#endif
}
