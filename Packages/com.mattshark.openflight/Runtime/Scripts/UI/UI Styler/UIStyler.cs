using UnityEngine;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using VRC.SDK3.Components;

#if UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace OpenFlightVRC.UI
{
	[AddComponentMenu("OpenFlight/UI/Styler")]
	internal class UIStyler : MonoBehaviour
	{
#pragma warning disable CS0649
		public UIStyle uiStyle;
#pragma warning restore CS0649

		private void Reset()
		{
			hideFlags = HideFlags.DontSaveInBuild;
		}

		private static Dictionary<UIStyleMarkup.StyleClass, FieldInfo> GetStyleFieldMap()
		{
			Dictionary<UIStyleMarkup.StyleClass, FieldInfo> fieldLookup = new Dictionary<UIStyleMarkup.StyleClass, FieldInfo>();

			foreach (FieldInfo field in typeof(UIStyle).GetFields(BindingFlags.Public | BindingFlags.Instance))
			{
				if (field.FieldType == typeof(Color))
				{
					StyleMarkupLinkAttribute markupAttr = field.GetCustomAttribute<StyleMarkupLinkAttribute>();

					if (markupAttr != null)
					{
						fieldLookup.Add(markupAttr.Class, field);
					}
				}
			}

			return fieldLookup;
		}

#if UNITY_EDITOR
    private Color GetColor(FieldInfo field)
        {
            return (Color)field.GetValue(uiStyle);
        }

        public void ApplyStyle()
        {
            if (uiStyle == null)
                return;

            var lookup = GetStyleFieldMap();

            UIStyleMarkup[] markups = GetComponentsInChildren<UIStyleMarkup>(true);

            foreach (UIStyleMarkup markup in markups)
            {
                Color graphicColor = GetColor(lookup[markup.styleClass]);

                if (markup.styleClass == UIStyleMarkup.StyleClass.TextHighlight)
                {
                    InputField input = markup.GetComponent<InputField>();

                    if (input != null)
                    {
                        Undo.RecordObject(input, "Apply UI Style");
                        input.selectionColor = graphicColor;
                        RecordObject(input);
                    }
                    else
                    {
                        VRCUrlInputField vrcInput = markup.GetComponent<VRCUrlInputField>();
                        Undo.RecordObject(vrcInput, "Apply UI Style");
                        vrcInput.selectionColor = graphicColor;
                        RecordObject(vrcInput);
                    }

                }
                else if (markup.styleClass == UIStyleMarkup.StyleClass.TextCaret)
                {
                    InputField input = markup.GetComponent<InputField>();

                    if (input != null)
                    {
                        Undo.RecordObject(input, "Apply UI Style");
                        input.caretColor = graphicColor;
                        RecordObject(input);
                    }
                    else
                    {
                        VRCUrlInputField vrcInput = markup.GetComponent<VRCUrlInputField>();
                        Undo.RecordObject(vrcInput, "Apply UI Style");
                        vrcInput.caretColor = graphicColor;
                        RecordObject(vrcInput);
                    }
                }
                else if (markup.styleClass == UIStyleMarkup.StyleClass.ActiveTab)
                {
                    Button button = markup.GetComponent<Button>();

                    if (button != null)
                    {
                        Undo.RecordObject(button, "Apply UI Style");
                        button.colors = new ColorBlock()
                        {
                            colorMultiplier = 1,
                            disabledColor = button.colors.disabledColor,
                            fadeDuration = button.colors.fadeDuration,
                            highlightedColor = button.colors.highlightedColor,
                            normalColor = button.colors.normalColor,
                            pressedColor = graphicColor,
                            selectedColor = graphicColor
                        };
                        RecordObject(button);
                    }
                }
                else if (markup.styleClass == UIStyleMarkup.StyleClass.InActiveTab)
                {
                    Button button = markup.GetComponent<Button>();

                    if (button != null)
                    {
                        Undo.RecordObject(button, "Apply UI Style");
                        button.colors = new ColorBlock()
                        {
                            colorMultiplier = 1,
                            disabledColor = button.colors.disabledColor,
                            fadeDuration = button.colors.fadeDuration,
                            highlightedColor = button.colors.highlightedColor,
                            normalColor = graphicColor,
                            pressedColor = button.colors.pressedColor,
                            selectedColor = button.colors.selectedColor
                        };
                        RecordObject(button);
                    }
                }
                else if (markup.styleClass == UIStyleMarkup.StyleClass.HighlightedButton)
                {
                    Button button = markup.GetComponent<Button>();

                    if (button != null)
                    {
                        Undo.RecordObject(button, "Apply UI Style");
                        button.colors = new ColorBlock()
                        {
                            colorMultiplier = 1,
                            disabledColor = button.colors.disabledColor,
                            fadeDuration = button.colors.fadeDuration,
                            highlightedColor = graphicColor,
                            normalColor = button.colors.normalColor,
                            pressedColor = button.colors.pressedColor,
                            selectedColor = button.colors.selectedColor
                        };
                        RecordObject(button);
                    }
                }
                else if (markup.targetGraphic != null)
                {
                    Undo.RecordObject(markup.targetGraphic, "Apply UI Style");
                    markup.targetGraphic.color = graphicColor;
                    RecordObject(markup.targetGraphic);
                }
                else if (markup.targetLineRenderer != null)
                {
                    Undo.RecordObject(markup.targetLineRenderer, "Apply UI Style");
                    markup.targetLineRenderer.startColor = graphicColor;
                    markup.targetLineRenderer.endColor = graphicColor;
                    RecordObject(markup.targetLineRenderer);
                }
            }
        }

        private static void RecordObject(Object comp)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(comp))
                PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
        }
#endif
	}

#if UNITY_EDITOR
    [CustomEditor(typeof(UIStyler))]
    internal class UIStylerEditor : UnityEditor.Editor
    {
        private SerializedProperty colorStyleProperty;

        private void OnEnable()
        {
            colorStyleProperty = serializedObject.FindProperty(nameof(UIStyler.uiStyle));
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(colorStyleProperty);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
                (target as UIStyler).ApplyStyle();

            if (GUILayout.Button("Fix Tablet Prefab"))
            {
                //copy the style variable
                SerializedProperty tempColorStyleProperty = colorStyleProperty.Copy();

                //check if we are in a prefab
                if (PrefabUtility.IsPartOfPrefabInstance(target))
                {
                    //loop through every child gameobject and revert to prefab
                    foreach (Transform child in (target as UIStyler).transform)
                    {
                        PrefabUtility.RevertPrefabInstance(child.gameObject, InteractionMode.AutomatedAction);
                    }
                }

                //apply the style variable
                UIStyler styler = (target as UIStyler);
                styler.uiStyle = tempColorStyleProperty.objectReferenceValue as UIStyle;

                //apply the style again
                styler.ApplyStyle();
            }

            if (colorStyleProperty.objectReferenceValue is UIStyle style)
            {
                //EditorGUILayout.Space();

                //if (GUILayout.Button("Apply Style"))
                //    (target as UIStyler).ApplyStyle();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(style.name), EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                SerializedObject styleObj = new SerializedObject(style);

                foreach (FieldInfo field in typeof(UIStyle).GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    SerializedProperty property = styleObj.FindProperty(field.Name);

                    if (property != null)
                        EditorGUILayout.PropertyField(property);
                }

                styleObj.ApplyModifiedProperties();

                if (EditorGUI.EndChangeCheck())
                    (target as UIStyler).ApplyStyle();
            }
            else
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Create New Style"))
                {
                    string saveLocation = EditorUtility.SaveFilePanelInProject("Style save location", "Style", "asset", "Choose a save location for the new style");

                    if (!string.IsNullOrEmpty(saveLocation))
                    {
                        var newStyle = ScriptableObject.CreateInstance<UIStyle>();

                        newStyle.name = Path.GetFileNameWithoutExtension(saveLocation); // I'm not sure if the name gets updated when someone changes the name manually so this may need to be revisited

                        AssetDatabase.CreateAsset(newStyle, saveLocation);
                        AssetDatabase.SaveAssets();

                        serializedObject.FindProperty(nameof(UIStyler.uiStyle)).objectReferenceValue = newStyle;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
#endif
}
