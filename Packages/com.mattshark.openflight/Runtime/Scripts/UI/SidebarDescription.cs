
using UdonSharp;

using UnityEngine;
using UnityEngine.EventSystems;

using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    using UdonSharpEditor;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Events;

    using UnityEngine.Events;

    using VRC.Udon.Common.Interfaces;

    [CustomEditor(typeof(SidebarDescription))]
    public class SidebarDescriptionEditor : Editor
    {
        //we want to automatically setup the EventTrigger component
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SidebarDescription target = (SidebarDescription)this.target;

            //check if the EventTrigger component exists
            if (target.GetComponent<EventTrigger>() == null)
            {
                //add the EventTrigger component
                target.gameObject.AddComponent<EventTrigger>();

            }
            //setup the EventTrigger component
            var trigger = target.GetComponent<EventTrigger>();
            //clear the triggers
            trigger.triggers.Clear();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            UnityAction<string> baseEvent = UdonSharpEditorUtility.GetBackingUdonBehaviour(target).SendCustomEvent;
            UnityEventTools.AddStringPersistentListener(entry.callback, baseEvent, nameof(SidebarDescription.HoverEnter));
            trigger.triggers.Add(entry);
        }
    }
#endif

    [RequireComponent(typeof(EventTrigger))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SidebarDescription : UdonSharpBehaviour
    {
        public string title;
        public string description;
        private DescriptionSidebarController sidebar;
        void Start()
        {
            sidebar = GameObject.Find(DescriptionSidebarController.EXPECTEDNAME).GetComponent<DescriptionSidebarController>();
        }

        public void HoverEnter()
        {
            sidebar.SetTitle(title);
            sidebar.SetDescription(description);
        }
    }
}
