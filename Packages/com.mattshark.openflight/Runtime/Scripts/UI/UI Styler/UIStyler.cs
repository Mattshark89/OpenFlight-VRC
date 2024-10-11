/**
 * @ Maintainer: Happyrobot33
 */

using UnityEngine;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using VRC.SDK3.Components;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif

namespace OpenFlightVRC.UI
{
	[AddComponentMenu("OpenFlight/UI/Styler")]
	internal class UIStyler : MonoBehaviour, VRC.SDKBase.IEditorOnly
	{
#pragma warning disable CS0649
		public UIStyle uiStyle;
#pragma warning restore CS0649

		private void Reset()
		{
			hideFlags = HideFlags.DontSaveInBuild;
		}

		public static Dictionary<StyleClass, FieldInfo> GetStyleFieldMap()
		{
			Dictionary<StyleClass, FieldInfo> fieldLookup = new Dictionary<StyleClass, FieldInfo>();

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

                //check for ignore alpha flag
                if (markup.ignoreAlpha)
                {
                    graphicColor.a = 1;
                }

                if (markup.styleClass == StyleClass.TextHighlight)
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
                else if (markup.styleClass == StyleClass.TextCaret)
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
                else if (markup.styleClass == StyleClass.ActiveTab)
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
                    else
                    {
                        if (markup.targetGraphic != null)
                        {
                            Undo.RecordObject(markup.targetGraphic, "Apply UI Style");
                            markup.targetGraphic.color = graphicColor;
                            RecordObject(markup.targetGraphic);
                        }
                    }
                }
                else if (markup.styleClass == StyleClass.InActiveTab)
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
                    else
                    {
                        if (markup.targetGraphic != null)
                        {
                            Undo.RecordObject(markup.targetGraphic, "Apply UI Style");
                            markup.targetGraphic.color = graphicColor;
                            RecordObject(markup.targetGraphic);
                        }
                    }
                }
                else if (markup.styleClass == StyleClass.HighlightedButton)
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
                    else
                    {
                        if (markup.targetGraphic != null)
                        {
                            Undo.RecordObject(markup.targetGraphic, "Apply UI Style");
                            markup.targetGraphic.color = graphicColor;
                            RecordObject(markup.targetGraphic);
                        }
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

        //This is setup like this because doing so avoids a massive ammount of warning spam that occurs upon joining a world otherwise
        //TLDR: This removes the styling components completely from the build
        [PostProcessSceneAttribute]
        public static void OnPostProcessScene()
        {
            //apply the style
            UIStyler[] stylers = GameObject.FindObjectsOfType<UIStyler>();
            foreach (UIStyler styler in stylers)
            {
                styler.ApplyStyle();
            }
            //find all style markup components
            UIStyleMarkup[] markups = GameObject.FindObjectsOfType<UIStyleMarkup>();

            //destroy the markup component itself
            foreach (UIStyleMarkup markup in markups)
            {
                Object.DestroyImmediate(markup);
            }

            //destroy self component
            Object.DestroyImmediate(GameObject.FindObjectOfType<UIStyler>());
        }
#endif
	}

#if UNITY_EDITOR
    [CustomEditor(typeof(UIStyler))]
    internal class UIStylerEditor : UnityEditor.Editor
    {
        private SerializedProperty colorStyleProperty;

        [SerializeField]
        private bool showUnused = false;

        [SerializeField]
        private int currentStyleDropdownIndex = 0;

        private string[] styleNames = new string[0];
        private List<UIStyle> styles = new List<UIStyle>();

        //This is called only when the component is first visible in the inspector
        //essentially the equivalent of Start() for the inspector
        private void OnEnable()
        {
            colorStyleProperty = serializedObject.FindProperty(nameof(UIStyler.uiStyle));

	#region Style Dropdown Initialization
            //attempt to find all styles relating to the package that this script is in
            styles = new List<UIStyle>();
            string[] guids = AssetDatabase.FindAssets("t:UIStyle");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UIStyle styleAsset = AssetDatabase.LoadAssetAtPath<UIStyle>(path);
                if (styleAsset != null)
                {
                    styles.Add(styleAsset);
                }
            }

            //create a list of style names for the dropdown
            styleNames = new string[styles.Count];
            for (int i = 0; i < styles.Count; i++)
            {
                styleNames[i] = styles[i].name;
            }

            //attempt to find the index of the current style
            UIStyler styler = (target as UIStyler);
            for (int i = 0; i < styleNames.Length; i++)
            {
                try
                {
                    if (styleNames[i] == styler.uiStyle.name)
                    {
                        currentStyleDropdownIndex = i;
                        break;
                    }
                }
                catch (UnityEngine.MissingReferenceException)
                {
                    Debug.LogWarning("Previous style was deleted. Style has been set randomly.");
                    EditorUtility.DisplayDialog("Style Deleted", "The style that was previously set has been deleted. A random style has been set instead.", "OK");
                    //pick a random number
                    int randomIndex = Random.Range(0, styles.Count);

                    currentStyleDropdownIndex = randomIndex;
                    styler.uiStyle = styles[randomIndex];
                    colorStyleProperty.objectReferenceValue = styles[randomIndex];
                    serializedObject.ApplyModifiedProperties();
                    //apply the style
                    styler.ApplyStyle();
                    break;
                }
            }
	#endregion
        }

        public override void OnInspectorGUI()
        {
            UIStyler styler = (target as UIStyler);
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(colorStyleProperty);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                //change the dropdown to match the style variable
                styler = (target as UIStyler);
                for (int i = 0; i < styleNames.Length; i++)
                {
                    if (styleNames[i] == styler.uiStyle.name)
                    {
                        currentStyleDropdownIndex = i;
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            currentStyleDropdownIndex = EditorGUILayout.Popup("Style", currentStyleDropdownIndex, styleNames);
            if (EditorGUI.EndChangeCheck())
            {
                //change the colorStyleProperty and styler.uiStyle to match the dropdown
                styler = (target as UIStyler);
                styler.uiStyle = AssetDatabase.LoadAssetAtPath<UIStyle>(AssetDatabase.GetAssetPath(styles[currentStyleDropdownIndex]));
                colorStyleProperty.objectReferenceValue = styler.uiStyle;
                serializedObject.ApplyModifiedProperties();
            }

            if (EditorGUI.EndChangeCheck())
            {
                //apply the style
                styler = (target as UIStyler);
                styler.ApplyStyle();
            }

            if (GUILayout.Button("Fix Tablet Prefab"))
            {
                //get a reference to the style asset itself
                UIStyle tempColorStyleObject = AssetDatabase.LoadAssetAtPath<UIStyle>(AssetDatabase.GetAssetPath(styles[currentStyleDropdownIndex]));

                //check if we are in a prefab
                if (PrefabUtility.IsPartOfPrefabInstance(target))
                {
                    //get every child transform
                    RectTransform[] children = (target as UIStyler).GetComponentsInChildren<RectTransform>(true);

                    try
                    {
                        //start asset editing
                        AssetDatabase.StartAssetEditing();

                        foreach (RectTransform child in children)
                        {
                            //revert the prefab component instance
                            PrefabUtility.RevertObjectOverride(child, InteractionMode.AutomatedAction);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(child);
                        }
                    }
                    finally
                    {
                        //stop asset editing
                        AssetDatabase.StopAssetEditing();
                    }
                }

                //apply the style variable
                styler = (target as UIStyler);
                styler.uiStyle = tempColorStyleObject;
                serializedObject.ApplyModifiedProperties();

                //apply the style again
                styler.ApplyStyle();

                //rebuild all layouts
                foreach (LayoutGroup layoutGroup in (target as UIStyler).GetComponentsInChildren<LayoutGroup>(true))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
                }
                return;
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

                var lookup = UIStyler.GetStyleFieldMap();
                UIStyleMarkup[] markups = (target as UIStyler).GetComponentsInChildren<UIStyleMarkup>(true);

                SerializedProperty[] unusedProperties = new SerializedProperty[typeof(UIStyle).GetFields(BindingFlags.Public | BindingFlags.Instance).Length];

                foreach (FieldInfo field in typeof(UIStyle).GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    SerializedProperty property = styleObj.FindProperty(field.Name);

                    //check if the field is used in any children
                    bool used = false;
                    foreach (UIStyleMarkup markup in markups)
                    {
                        if (lookup[markup.styleClass].Name == field.Name)
                        {
                            used = true;
                            break;
                        }
                    }

                    if (property != null && used)
                        EditorGUILayout.PropertyField(property);
                    else if (property != null)
                        unusedProperties[System.Array.IndexOf(styleObj.targetObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance), field)] = property;
                }

                styleObj.ApplyModifiedProperties();

	#region Unused Properties Foldout
                //display unused properties in a foldout
                if (unusedProperties.Length > 0)
                {
                    EditorGUILayout.Space();
                    showUnused = EditorGUILayout.Foldout(showUnused, "Unused Properties");
                    if (showUnused)
                    {
                        //display a info text
                        EditorGUILayout.HelpBox("These properties are not used in any children of this UIStyler. They are here for convenience.", MessageType.Info);
                        EditorGUI.indentLevel++;
                        foreach (SerializedProperty property in unusedProperties)
                        {
                            if (property != null)
                                EditorGUILayout.PropertyField(property);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
	#endregion

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
