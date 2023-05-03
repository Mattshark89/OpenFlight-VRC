using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace OpenFlightVRC
{
	public class OpenFlightTablet : UdonSharpBehaviour
	{
		VRCPlayerApi localPlayer = null;
		public float scalingOffset = 0.1f;
		public int fadeDistance = 10;
		public bool allowFade = true;
		public GameObject[] objectsToHideOnFade;
		public OpenFlight OpenFlight;
		public AvatarDetection AvatarDetection;

		public TextMeshProUGUI VersionInfo;

		public Color activeTabColor;
		public Color inactiveTabColor;

		public Button[] tabs;

		void Start()
		{
			//get the local player
			localPlayer = Networking.LocalPlayer;

			//initialize the tabs
			SetActiveTabMain();
		}

		void Update()
		{
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

				//set this gameobjects scale to the players scale
				transform.localScale = new Vector3((float)PlayerScale * scalingOffset, (float)PlayerScale * scalingOffset, (float)PlayerScale * scalingOffset);

				//set the version info text
				VersionInfo.text =
					"Open-Flight Ver " + OpenFlight.OpenFlightVersion + "\nJSON Ver " + AvatarDetection.jsonVersion + "\nJSON Date " + AvatarDetection.jsonDate;
			}
		}

		//Helper function to get the total distance of a vector array
		//this adds up all of the distances between each vector in the array in order, then returns the total distance
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

		public void SetActiveTab(int tab)
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				if (i == tab)
				{
					tabs[i].GetComponent<Image>().color = activeTabColor;
				}
				else
				{
					tabs[i].GetComponent<Image>().color = inactiveTabColor;
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
