using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class OpenFlightTablet : UdonSharpBehaviour
	{
        VRCPlayerApi localPlayer = null;
		public int fadeDistance = 10;
		public bool allowFade = true;
		public GameObject[] objectsToHideOnFade;
        public GameObject[] objectsToShowOnFade;
		public OpenFlight OpenFlight;
		public AvatarDetection AvatarDetection;

		public TextMeshProUGUI VersionInfo;

		public Button[] tabs;
		private int activeTab = 0;

		//Overwritten at start
		private Color tabBaseColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		private Color tabActiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		//the reason this is needed is stupid, but essentially if there isnt a delay between start going into update, the tablet will immediately hide itself breaking all the children UI scripts
		private int fadeTimeout = 2;
		void Start()
		{
			//get the local player
			localPlayer = Networking.LocalPlayer;

			//save the tab colors into this script
			tabBaseColor = tabs[0].colors.normalColor;
			tabActiveColor = tabs[0].colors.selectedColor;

			//initialize the tabs
			SetActiveTabMain();
		}

		void Update()
		{
			//continually highlight the active tab
			SetActiveTab(activeTab);

			//wait for udon to startup
			if (fadeTimeout <= 0)
			{
				//check if the player is within the fade distance
				if (Vector3.Distance(localPlayer.GetPosition(), transform.position) > fadeDistance && allowFade)
                {
                    //disable all the objects that should be hidden
                    SetFadeState(false);
                }
                else
				{
                    //enable all the objects that should be hidden
                    SetFadeState(true);

					//set the version info text
					VersionInfo.text =
						"Open-Flight Ver " + OpenFlight.OpenFlightVersion + "\nJSON Ver " + AvatarDetection.jsonVersion + "\nJSON Date " + AvatarDetection.jsonDate;
				}
			}

			//decrement the fade timeout
			if (fadeTimeout > 0)
			{
				fadeTimeout--;
			}
		}

        /// <summary>
        /// Controls the fade state of the tablet
        /// </summary>
        /// <param name="state">The state to set the tablet to</param>
        private void SetFadeState(bool state)
        {
            foreach (GameObject obj in objectsToHideOnFade)
            {
                obj.SetActive(state);
            }
            foreach (GameObject obj in objectsToShowOnFade)
            {
                obj.SetActive(!state);
            }
        }

        public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float eyeHeight)
		{
			if (player.isLocal)
			{
                Logger.Log("Player eye height changed, updating tablet scale", this);
                transform.localScale = new Vector3(Util.ScaleModifier(), Util.ScaleModifier(), Util.ScaleModifier());
            }
		}

		/// <summary>
		/// Sets the active tab to the given tab number
		/// </summary>
		/// <param name="tab">The tab number to set active</param>
		public void SetActiveTab(int tab)
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				if (i == tab)
				{
					ColorBlock colors = tabs[i].colors;
					colors.normalColor = tabActiveColor;
					tabs[i].colors = colors;
					activeTab = i;
				}
				else
				{
					ColorBlock colors = tabs[i].colors;
					colors.normalColor = tabBaseColor;
					tabs[i].colors = colors;
				}
			}
		}

		//these are dummy events for the buttons to call, since Udon doesn't support sending parameters to events
		public void SetActiveTabMain()
		{
			SetActiveTab(0);
		}

		public void SetActiveTabSettings()
		{
			SetActiveTab(1);
		}

		public void SetActiveTabDebug()
		{
			SetActiveTab(2);
		}

        public void SetActiveTabChangeLog()
        {
            SetActiveTab(3);
        }

        public void SetActiveTabNetworking()
        {
            SetActiveTab(4);
        }
	}
}
