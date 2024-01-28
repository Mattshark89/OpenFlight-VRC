using UnityEngine;
using UnityEngine.UI;
using UdonSharp;

#if UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using UnityEditor.Callbacks;
#endif

namespace OpenFlightVRC.UI
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

#if !COMPILER_UDONSHARP && UNITY_EDITOR
public class UIStyleMarkupScenePostProcessor {
	[PostProcessSceneAttribute]
	public static void OnPostProcessScene() {
		//find all the UIStyleMarkup scripts in the scene
		UIStyleMarkup[] styleMarkupScripts = Object.FindObjectsOfType<UIStyleMarkup>();
		foreach (UIStyleMarkup styleMarkupScript in styleMarkupScripts)
		{
			//remove them
			UdonSharpEditorUtility.DestroyImmediate(styleMarkupScript);
		}
	}
}
#endif

	[AddComponentMenu("Udon Sharp/Video/UI/Style Markup")]
	internal class UIStyleMarkup : UdonSharpBehaviour
	{

#pragma warning disable CS0649
		public StyleClass styleClass;
		public Graphic targetGraphic;
		public LineRenderer targetLineRenderer;
		public bool ignoreAlpha = false;
#pragma warning restore CS0649


#if !COMPILER_UDONSHARP && UNITY_EDITOR
		private void Reset()
		{
			hideFlags = HideFlags.DontSaveInBuild;
			targetGraphic = GetComponent<Graphic>();
			targetLineRenderer = GetComponent<LineRenderer>();
			ignoreAlpha = false;

			//get parent styler
			UIStyler styler = GetComponentInParent<UIStyler>();

			if (styler != null)
				styler.ApplyStyle();
			else
				Debug.LogError("UIStyleMarkupEditor: Could not find parent UIStyler component!");
		}
#endif
	}

	//add a basic custom inspector that detects when the style class is changed and updates the target graphic
#if !COMPILER_UDONSHARP && UNITY_EDITOR
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
