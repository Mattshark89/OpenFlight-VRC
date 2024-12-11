
using TMPro;

using UdonSharp;

using UnityEngine;
using UnityEngine.UI;

using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{

    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Events;

    using UnityEngine.Events;

    using VRC.Udon.Common.Interfaces;

    [CustomEditor(typeof(OptionController))]
    public class OptionControllernEditor : Editor
    {
        //we want to automatically setup the EventTrigger component
        public override void OnInspectorGUI()
        {
            OptionController target = (OptionController)this.target;

            //show a few configuration options
            if (GUILayout.Button("Make Slider"))
            {
                SelectOptionSetup(target, OptionType.Slider);
            }

            if (GUILayout.Button("Make Dropdown"))
            {
                SelectOptionSetup(target, OptionType.Dropdown);
            }

            if (GUILayout.Button("Make SubOption"))
            {
                SelectOptionSetup(target, OptionType.SubOption);
            }

            if (GUILayout.Button("Make SingleOption"))
            {
                SelectOptionSetup(target, OptionType.SingleOption);
            }

            //watch for changes
            EditorGUI.BeginChangeCheck();

            //text input box
            target.nameText.text = EditorGUILayout.TextField("Name", target.nameText.text);

            //set object name based on the name text
            target.gameObject.name = target.nameText.text;

            if (EditorGUI.EndChangeCheck() || GUILayout.Button("Force Apply"))
            {
                //make sure the prefab instance overrides is saved
                PrefabUtility.RecordPrefabInstancePropertyModifications(target.nameText);

                EditorUtility.SetDirty(target);
            }
            base.OnInspectorGUI();
        }

        private enum OptionType
        {
            Slider,
            Dropdown,
            SubOption,
            SingleOption
        }

        private void SelectOptionSetup(OptionController target, OptionType typ)
        {
            SetState(target.SubOptionToggle, typ == OptionType.SubOption);
            SetState(target.SingleOptionToggle, typ == OptionType.SingleOption);
            SetState(target.Slider, typ == OptionType.Slider);
            SetState(target.SliderInput, typ == OptionType.Slider);
            SetState(target.Dropdown, typ == OptionType.Dropdown);
        }


        private void SetState(Component comp, bool state)
        {
            comp.gameObject.SetActive(state);
        }
    }
#endif

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OptionController : UdonSharpBehaviour
    {
        public OptionController[] SubOptions;

        [Header("Internal Controls")]
        public Toggle SubOptionToggle;
        public Toggle SingleOptionToggle;
        public Slider Slider;
        public TMP_InputField SliderInput;
        public TMP_Dropdown Dropdown;
        public TextMeshProUGUI nameText;
        void Start()
        {

        }
        
        public void SubOptionToggleUpdate()
        {
            SetInteractability(SubOptionToggle.isOn, true);
            UpdateSubOptionInteractability(SubOptionToggle.isOn);
        }

        private void UpdateSubOptionInteractability(bool input)
        {
            foreach (var option in SubOptions)
            {
                option.SetInteractability(input, false);
            }
        }


        public void SingleOptionToggleUpdate()
        {
            Debug.Log("Single Option Toggle Value: " + SingleOptionToggle.isOn);
        }

        public void SliderUpdate()
        {
            Debug.Log("Slider Value: " + Slider.value);
        }

        public void SliderInputUpdate()
        {
            Debug.Log("Slider Input Value: " + SliderInput.text);
        }

        public void DropdownUpdate()
        {
            Debug.Log("Dropdown Value: " + Dropdown.value);
        }

        internal void SetInteractability(bool interactable, bool topLevelAction)
        {
            if (!topLevelAction)
            {
                SubOptionToggle.interactable = interactable;
            }
            SingleOptionToggle.interactable = interactable;
            Slider.interactable = interactable;
            Dropdown.interactable = interactable;
            SliderInput.interactable = interactable;

            UpdateSubOptionInteractability(interactable);
        }
    }
}
