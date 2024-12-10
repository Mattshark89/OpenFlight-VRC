
using TMPro;

using UdonSharp;

using UnityEngine;
using UnityEngine.UI;

using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1000)]
    public class DescriptionSidebarController : UdonSharpBehaviour
    {
        internal const string EXPECTEDNAME = "DescriptionSidebarController";
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        void Start()
        {
            name = EXPECTEDNAME;
        }

        void OnEnable()
        {
            SetTitle("Info");
            SetDescription("Hover over something to get a description of what it does!");
        }

        internal void SetDescription(string description)
        {
            descriptionText.text = description;
            UpdateLayout();
        }

        internal void SetTitle(string title)
        {
            titleText.text = title;
            UpdateLayout();
        }

        internal void UpdateLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
    }
}
