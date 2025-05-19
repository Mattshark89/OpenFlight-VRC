/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;
using TMPro;
using static OpenFlightVRC.Util;
using System;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// The main script for the OpenFlight tablet itself, managing tabs and LOD fading
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class OpenFlightTablet : LoggableUdonSharpBehaviour
	{
		private VRCPlayerApi _localPlayer = null;
		/// <summary>
		/// The distance from the tablet the player has to be for it to fade out
		/// </summary>
		[Tooltip("The distance from the tablet the player has to be for it to fade out")]
		public int fadeDistance = 10;
		/// <summary>
		/// If the tablet is allowed to fade out when the player is far away
		/// </summary>
		[Tooltip("If the tablet is allowed to fade out when the player is far away")]
		public bool allowFade = true;
		/// <summary>
		/// If the tablet should automatically scale based on the player's eye height. This is HIGHLY recommended to be left on, as it will make the tablet look correct for all players. If you want to manually set the scale, turn this off and set the scale manually.
		/// </summary>
		[Tooltip("If the tablet should automatically scale based on the player's eye height. This is HIGHLY recommended to be left on, as it will make the tablet look correct for all players. If you want to manually set the scale, turn this off and set the scale manually.")]
		public bool automaticScale = true;
		public GameObject[] objectsToHideOnFade;
		public GameObject[] objectsToShowOnFade;
		public OpenFlight OpenFlight;
		public AvatarDetection AvatarDetection;

		public TextMeshProUGUI VersionInfo;

		public Button[] tabs;
		/// <summary>
		/// The tabs that should be initialized on start. This ensures Start() is called on all the tabs
		/// </summary>
		[Tooltip("The tabs that should be initialized on start. This ensures Start() is called on all the tabs")]
		public GameObject[] tabsToInitialize;
		private int _activeTab = 0;

		//Overwritten at start
		private Color _tabBaseColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		private Color _tabActiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		//the reason this is needed is stupid, but essentially if there isnt a delay between start going into update, the tablet will immediately hide itself breaking all the children UI scripts
		private int _fadeTimeout = fadeTimeoutStart;
		internal const int fadeTimeoutStart = 2;

		void Start()
		{
			//get the local player
			_localPlayer = Networking.LocalPlayer;

			//save the tab colors into this script
			_tabBaseColor = tabs[0].colors.normalColor;
			_tabActiveColor = tabs[0].colors.selectedColor;

			//initialize the tabs
			SetActiveTabMain();

			//set the version info text
			UpdateVersionInfo();

			//subscribe to the json list load event
			AvatarDetection.AddCallback(AvatarDetectionCallback.LoadJSON, this, nameof(UpdateVersionInfo));

			//enable the tabs that need to be initialized
			foreach (GameObject tab in tabsToInitialize)
			{
				tab.SetActive(true);
			}

			SendCustomEventDelayedFrames(nameof(FixInitialization), _fadeTimeout);
		}

		void Update()
		{
			//continually highlight the active tab
			SetActiveTab(_activeTab);

			//wait for udon to startup
			if (_fadeTimeout <= 0)
			{
				//check if the player is within the fade distance
				if (_localPlayer.IsValid() && Vector3.Distance(_localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, transform.position) > fadeDistance && allowFade)
				{
					//disable all the objects that should be hidden
					SetFadeState(false);
				}
				else
                {
                    //enable all the objects that should be hidden
                    SetFadeState(true);
                }
            }

			//decrement the fade timeout
			if (_fadeTimeout > 0)
			{
				_fadeTimeout--;
			}
		}

		public void FixInitialization()
		{
			//disable the tabs that were initialized
			foreach (GameObject tab in tabsToInitialize)
			{
				tab.SetActive(false);
			}
		}

        public void UpdateVersionInfo()
        {
			VersionInfo.text = String.Format("Open-Flight Ver {0}\nJSON Ver {1}\nJSON Date {2}", OpenFlight.OpenFlightVersion, AvatarDetection.jsonVersion, AvatarDetection.jsonDate);
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
			if (player.isLocal && automaticScale)
			{
				Logger.Log("Player eye height changed, updating tablet scale", this);
				transform.localScale = new Vector3(ScaleModifier(), ScaleModifier(), ScaleModifier());
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
					colors.normalColor = _tabActiveColor;
					tabs[i].colors = colors;
					_activeTab = i;
				}
				else
				{
					ColorBlock colors = tabs[i].colors;
					colors.normalColor = _tabBaseColor;
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
