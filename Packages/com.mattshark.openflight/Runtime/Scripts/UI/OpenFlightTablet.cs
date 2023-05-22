using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace OpenFlightVRC.UI
{
	public class OpenFlightTablet : UdonSharpBehaviour
	{
		VRCPlayerApi localPlayer = null;

		[System.NonSerialized]
		private float scalingOffset = 0.6183768f;
		public int fadeDistance = 10;
		public bool allowFade = true;
		public GameObject[] objectsToHideOnFade;
		public OpenFlight OpenFlight;
		public AvatarDetection AvatarDetection;

		public TextMeshProUGUI VersionInfo;

		public Button[] tabs;
		private int activeTab = 0;

		//Overwritten at start
		private Color tabBaseColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		private Color tabActiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

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

			//check if the player is within the fade distance
			if (Vector3.Distance(localPlayer.GetPosition(), transform.position) > fadeDistance && allowFade)
			{
				//disable all the objects that should be hidden
				foreach (GameObject obj in objectsToHideOnFade)
				{
					obj.SetActive(false);
				}
			}
			else
			{
				//enable all the objects that should be hidden
				foreach (GameObject obj in objectsToHideOnFade)
				{
					obj.SetActive(true);
				}
				//change the scale of the gameobject based on the players scale
				//add up all of the bone distances from the foot to the head
				Vector3 footR = localPlayer.GetBonePosition(HumanBodyBones.RightFoot);
				Vector3 LowerLegR = localPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg);
				Vector3 UpperLegR = localPlayer.GetBonePosition(HumanBodyBones.RightUpperLeg);
				Vector3 Hips = localPlayer.GetBonePosition(HumanBodyBones.Hips);
				Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
				Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);
				Vector3 Neck = localPlayer.GetBonePosition(HumanBodyBones.Neck);
				Vector3 Head = localPlayer.GetBonePosition(HumanBodyBones.Head);
				float PlayerScale = totalVectorDistance(new Vector3[] { footR, LowerLegR, UpperLegR, Hips, spine, chest, Neck, Head });

				//if the player scale is 0, that means the avatar uses a generic rig, placing all the bone transforms at origin
				//set the scale to 1 to make sure the tablet is visible anyway
				if (PlayerScale == 0)
				{
					PlayerScale = 1;
				}

				//if the player is too small, set the scale to 1
				PlayerScale = Mathf.Clamp(PlayerScale, 0.1f, float.MaxValue);

				//set this gameobjects scale to the players scale
				transform.localScale = new Vector3((float)PlayerScale * scalingOffset, (float)PlayerScale * scalingOffset, (float)PlayerScale * scalingOffset);

				//set the version info text
				VersionInfo.text =
					"Open-Flight Ver " + OpenFlight.OpenFlightVersion + "\nJSON Ver " + AvatarDetection.jsonVersion + "\nJSON Date " + AvatarDetection.jsonDate;
			}
		}

		/// <summary>
		/// Helper function to get the total distance of a vector array.
		/// this adds up all of the distances between each vector in the array in order, then returns the total distance
		/// </summary>
		/// <param name="vectors">The vector array to get the total distance of</param>
		/// <returns>The total distance of the vector array</returns>
		public float totalVectorDistance(Vector3[] vectors)
		{
			float totalDistance = 0;
			for (int i = 0; i < vectors.Length; i++)
			{
				if (i == 0)
				{
					continue;
				}
				else
				{
					totalDistance += Vector3.Distance(vectors[i], vectors[i - 1]);
				}
			}
			return totalDistance;
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
	}
}
