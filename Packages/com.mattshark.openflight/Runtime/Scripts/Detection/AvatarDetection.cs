//mathf
using System.Collections;
//import TMP
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3;
using VRC.SDK3.Data;

namespace OpenFlightVRC
{
	public class AvatarDetection : UdonSharpBehaviour
	{
		VRCPlayerApi localPlayer = null;
		public string debugInfo = ""; //Contains all the debug info about avatar detection

  /*
  Spine to Chest: XXX
  Head to Neck: XXX
  Chest to Neck: XXX
  Left Shoulder to Left Upper Arm: XXX
  Left Upper Arm to Left Lower Arm: XXX
  Left Lower Arm to Left Hand: XXX
  Combined Bone Info: XXX
  Hash: XXX
  Allowed to Fly: XXX
  Detected Avatar Info:
  Name: XXX
  Creator: XXX
  Introducer: XXX
  Weight: XXX
  Wingtip Offset: XXX
  */

		double d_spinetochest = 0; //used to calculate the avatar scale
		double previous_d_spinetochest = 0; //used to see if the avatar has changed

		//external JSON list stuff
		public AvatarListLoader JSONLoader; //this is the script that loads the JSON list
		public OpenFlight OpenFlight;
		public WingFlightPlusGlide WingFlightPlusGlide;

		[System.NonSerialized]
		string jsonString = ""; //this is the JSON list in string form
		DataDictionary json; //this is the JSON list in a serialized form, allowing for JSON commands to be used on it
		public bool allowedToFly = false; //this is used to tell openflight if the avatar is allowed to fly
		public bool skipLoadingAvatar = true; //this is used to skip the loading avatar, as it is not a real avatar

		//gizmo related stuff
		public bool showWingTipGizmo = false;
		public GameObject wingtipGizmo; //this shows the wingtip offset as a sphere in game. Only works in VR due to implementation

		//information about the avatar that has been detected
		public string hashV1 = "0";
		public string hashV2 = "0";
		public float weight = 1;
		public float WingtipOffset = 0;
		public string name = ""; //this is the name of the avatar base
		public string creator = ""; //this is the person who created the avatar base
		public string introducer = ""; //this is the person who introduced the avatar to the JSON list itself

		//information about the json itself
		public string jsonVersion = "";
		public string jsonDate = "";

		void Start()
		{
			//get the local player
			localPlayer = Networking.LocalPlayer;

			debugInfo = "Loading JSON list...";
			JSONLoader.LoadURL(); //tell the JSON loader to try to load the JSON list from the github
		}

		void Update()
		{
			//if the JSON list is empty, then return
			if (jsonString == "" || jsonString == null)
			{
				jsonString = (string)JSONLoader.Output;
				LoadJSON();
				return;
			}

			//get spine and hips first, as they are used to calculate the avatar scale
			Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
			Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);

			//calculate the avatar scale using the distance from hips to spine
			d_spinetochest = Vector3.Distance(chest, spine);

			WingFlightPlusGlide.wingtipOffset = WingtipOffset;
			WingFlightPlusGlide.weight = weight;

			//if the player has changed avatars, do the hashing and determine if the avatar is allowed to fly
			//avatar change is done by checking if the distance from spine to chest has changed by a significant amount
			if (Mathf.Abs((float)d_spinetochest - (float)previous_d_spinetochest) > 0.001f)
			{
				previous_d_spinetochest = d_spinetochest;

				//get all the bones now
				Vector3 head = localPlayer.GetBonePosition(HumanBodyBones.Head);
				Vector3 neck = localPlayer.GetBonePosition(HumanBodyBones.Neck);
				Vector3 leftShoulder = localPlayer.GetBonePosition(HumanBodyBones.LeftShoulder);
				Vector3 LeftUpperArm = localPlayer.GetBonePosition(HumanBodyBones.LeftUpperArm);
				Vector3 LeftLowerArm = localPlayer.GetBonePosition(HumanBodyBones.LeftLowerArm);
				Vector3 LeftHand = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

				Vector3[] boneVectors = { chest, head, neck, leftShoulder, LeftUpperArm, LeftLowerArm, LeftHand };
				hashV1 = getHash(boneVectors, 1);
				hashV2 = getHash(boneVectors, 2);

				//check if the hash is the loading avatar, and if it is then dont check if the avatar is allowed to fly
				if (hashV2 == "1439458325v2" && skipLoadingAvatar)
				{
					debugInfo = "Loading Avatar Detected, ignoring...";
					name = "Loading Avatar";
					creator = "Loading Avatar";
					introducer = "Loading Avatar";
					weight = 1;
					WingtipOffset = 0;
					return;
				}

				//check if the avatar is allowed to fly
				allowedToFly = isAvatarAllowedToFly(hashV1, hashV2);

				//tell openflight if the avatar is allowed to fly
				if (allowedToFly)
				{
					OpenFlight.CanFly();
				}
				else
				{
					OpenFlight.CannotFly();
					WingFlightPlusGlide.wingtipOffset = 0;
					WingFlightPlusGlide.weight = 1;
				}

				//print all the info to the text
				debugInfo =
					"HashV1 (Do not submit): "
					+ hashV1
					+ "\nHashV2: "
					+ hashV2
					+ "\nAllowed to Fly: "
					+ allowedToFly
					+ "\n\nDetected Avatar Info: "
					+ "\nName: "
					+ name
					+ "\nCreator: "
					+ creator
					+ "\nIntroduced by: "
					+ introducer
					+ "\nWeight: "
					+ weight
					+ "\nWingtip Offset: "
					+ WingtipOffset;
			}

			//gizmo stuff
			visualizeWingTips();
		}

		int getBoneDistance(Vector3 bone1, Vector3 bone2, int scalingFactor)
		{
			return Mathf.FloorToInt(Vector3.Distance(bone1, bone2) / (float)d_spinetochest * scalingFactor);
		}

		bool isAvatarAllowedToFly(string in_hashV1, string in_hashV2)
		{
			DataDictionary bases = json["Bases"].DataDictionary;
			DataToken[] baseKeys = bases.GetKeys().ToArray();
			for (int i = 0; i < bases.Count; i++)
			{
				DataDictionary avi_base = bases[baseKeys[i]].DataDictionary;
				DataToken[] avi_base_keys = avi_base.GetKeys().ToArray();
				for (int j = 0; j < avi_base.Count; j++)
				{
					DataDictionary variant = avi_base[avi_base_keys[j]].DataDictionary;
					DataToken[] avi_variant_keys = variant.GetKeys().ToArray();
					DataToken[] hashArray = variant["Hash"].DataList.ToArray();
					for (int k = 0; k < hashArray.Length; k++)
					{
						string hash = hashArray[k].String;
						if (hash == in_hashV1.ToString() || hash == in_hashV2.ToString())
						{
							name = variant["Name"].String;
							creator = variant["Creator"].String;
							introducer = variant["Introducer"].String;
							weight = (float)variant["Weight"].Number;
							WingtipOffset = (float)variant["WingtipOffset"].Number;
							return true;
						}
					}
				}
			}

			name = "Unknown";
			creator = "Unknown";
			introducer = "Unknown";
			weight = 1;
			WingtipOffset = 0;
			return false;
		}

		void LoadJSON()
		{
			if (jsonString != "" && jsonString != null)
			{
				//purely temp variable due to needing to use out
				DataToken jsonDataToken;
				bool success = VRCJson.TryDeserializeFromJson(jsonString, out jsonDataToken);
				if (!success)
				{
					debugInfo = "Failed to load JSON list!";
					Debug.LogError("Failed to load JSON list! This shouldnt occur unless we messed up the JSON, or VRChat broke something!");
					return;
				}
				json = jsonDataToken.DataDictionary;
				jsonVersion = json["JSON Version"].String;
				jsonDate = json["JSON Date"].String;
			}
		}

		/// <summary>
		/// 	Tells the script to reload the JSON file and then recheck your worn avatar for flight
		/// </summary>
		public void reloadJSON()
		{
			debugInfo = "Loading JSON list...";
			//get the JSON list
			JSONLoader.LoadURL();

			jsonString = "";
			d_spinetochest = 0;
			previous_d_spinetochest = 1000f;
		}

		//bonePositions [chest, head, neck, leftShoulder, LeftUpperArm, LeftLowerArm, LeftHand]
		string getHash(Vector3[] bonePositions, int version)
		{
			int scalingFactor;
			int d_necktohead;
			int d_chesttoneck;
			int d_leftshouldertoleftupperarm;
			int d_leftupperarmtoleftlowerarm;
			int d_leftlowertolefthand;
			string boneInfo;
			switch (version)
			{
				case 1:
					scalingFactor = 1000;
					d_necktohead = getBoneDistance(bonePositions[2], bonePositions[1], scalingFactor);
					d_chesttoneck = getBoneDistance(bonePositions[0], bonePositions[2], scalingFactor);
					d_leftshouldertoleftupperarm = getBoneDistance(bonePositions[3], bonePositions[4], scalingFactor);
					d_leftupperarmtoleftlowerarm = getBoneDistance(bonePositions[4], bonePositions[5], scalingFactor);
					d_leftlowertolefthand = getBoneDistance(bonePositions[5], bonePositions[6], scalingFactor);

					boneInfo = d_necktohead + "." + d_chesttoneck + "." + d_leftshouldertoleftupperarm + "." + d_leftupperarmtoleftlowerarm + "." + d_leftlowertolefthand;
					return boneInfo.GetHashCode().ToString();
				case 2:
					scalingFactor = 100;
					d_necktohead = getBoneDistance(bonePositions[2], bonePositions[1], scalingFactor);
					d_chesttoneck = getBoneDistance(bonePositions[0], bonePositions[2], scalingFactor);
					d_leftshouldertoleftupperarm = getBoneDistance(bonePositions[3], bonePositions[4], scalingFactor);
					d_leftupperarmtoleftlowerarm = getBoneDistance(bonePositions[4], bonePositions[5], scalingFactor);
					d_leftlowertolefthand = getBoneDistance(bonePositions[5], bonePositions[6], scalingFactor);

					boneInfo = d_necktohead + "." + d_chesttoneck + "." + d_leftshouldertoleftupperarm + "." + d_leftupperarmtoleftlowerarm + "." + d_leftlowertolefthand;
					return boneInfo.GetHashCode().ToString() + "v2";
				default:
					Debug.LogError("Invalid Hash Version Sent");
					return "0";
			}
		}

		//TODO: Clean up this code so it isnt so segmented
		void visualizeWingTips()
		{
			//reset the wingtip gizmo rotation
			wingtipGizmo.transform.rotation = Quaternion.identity;

			//move a gameobject to the visualize the wingtips
			Vector3 rightHandPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
			Quaternion rightHandRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;

			//calculate the wingtip position by adding the offset to the right hand position in the direction of the right hand rotation
			Vector3 WingTipPosition = rightHandPosition + (rightHandRotation * Vector3.forward * new Vector3(0, 0, (float)WingtipOffset * (float)d_spinetochest).z);

			wingtipGizmo.transform.position = WingTipPosition;
			wingtipGizmo.transform.RotateAround(rightHandPosition, rightHandRotation * Vector3.up, 70);
		}

		//this can be used for other scripts to check if the avatar is allowed to fly again
		/// <summary>
		/// 	Reevaluates whether or not you should be able to fly
		/// </summary>
		public void ReevaluateFlight()
		{
			d_spinetochest = 0;
			previous_d_spinetochest = 1000f;
			//Debug.Log("Reevaluating flight");
		}
	}
}
