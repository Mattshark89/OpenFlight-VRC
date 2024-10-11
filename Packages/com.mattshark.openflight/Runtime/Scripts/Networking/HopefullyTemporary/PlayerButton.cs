
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Net
{
    [RequireComponent(typeof(VRCPlayerObject))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerButton : LoggableUdonSharpBehaviour
    {
        public PlayerSettingsStoreControlsManager controlsManager;
        public TextMeshProUGUI buttonText;
        public GameObject reparentDestination;
        private Image buttonImage;

        private Color defaultColor;
        private VRCPlayerApi owner;
        private PlayerSettings store;

        void Start()
        {
            //reparent the button to the destination
            //transform.SetParent(reparentDestination.transform);
            //reset the XYZ, rotation, and scale of the button
            //transform.localPosition = Vector3.zero;
            //transform.localRotation = Quaternion.identity;
            //transform.localScale = Vector3.one;
            //set our button text to the player's name
            owner = Networking.GetOwner(gameObject);
            buttonText.text = owner.displayName;

            buttonImage = GetComponent<Image>();
            defaultColor = buttonImage.color;

            //update our highlight if local
            if (owner == Networking.LocalPlayer)
            {
                Highlight();
            }

            //setup our name
            gameObject.name = "PlayerButton_" + owner.displayName;

            //get our store
            store = Util.GetPlayerObjectOfType<PlayerSettings>(owner);

            //add a callback to check if we are interactive
            store.AddCallback(PlayerSettingsCallback.OnDataChanged, this, nameof(CheckInteractive));
            //we need this as OnDeserialization may not fire on remote or local player
            store.AddCallback(PlayerSettingsCallback.OnStartFinished, this, nameof(CheckInteractive));
        }

        public void Click()
        {
            controlsManager.SetReferencePlayerStore(owner);
        }

        public void Highlight()
        {
            buttonImage.color = Color.green;
        }

        public void Unhighlight()
        {
            buttonImage.color = defaultColor;
        }

        public void UpdateHighlight(VRCPlayerApi toMatch)
        {
            if (toMatch == owner)
            {
                Highlight();
            }
            else
            {
                Unhighlight();
            }
        }

        public void CheckInteractive()
        {
            SetInteractive(store.IsInitialized);
        }

        public void SetInteractive(bool interactive)
        {
            GetComponent<Button>().interactable = interactive;
        }
    }
}
