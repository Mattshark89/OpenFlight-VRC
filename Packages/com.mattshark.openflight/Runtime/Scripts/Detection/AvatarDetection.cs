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
using static OpenFlightVRC.Util;

namespace OpenFlightVRC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
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

        public double d_spinetochest = 0; //used to calculate the avatar scale

        //external JSON list stuff
        public AvatarListLoader JSONLoader; //this is the script that loads the JSON list
		public OpenFlight OpenFlight;
		public WingFlightPlusGlide WingFlightPlusGlide;

		[System.NonSerialized]
		string jsonString = ""; //this is the JSON list in string form
		DataDictionary json; //this is the JSON list in a serialized form, allowing for JSON commands to be used on it
		public bool allowedToFly = false; //this is used to tell openflight if the avatar is allowed to fly
		public bool skipLoadingAvatar = true; //this is used to skip the loading avatar, as it is not a real avatar

		//information about the avatar that has been detected
		public string hashV1 = "0";
        internal float[] hashV1Distances = new float[5];
		public string hashV2 = "0";
        internal float[] hashV2Distances = new float[5];
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
            //set the callback of the jsonloader to rehash
            JSONLoader.AddCallback(this, "LoadJSON");

            JSONLoader.LoadURL(); //tell the JSON loader to try to load the JSON list from the github
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
                Logger.Log("Avatar Changed, reevaluating flight...", this);
				RunDetection();
			}
		}

		void RunDetection()
		{
			//if the JSON list is empty, then return
			if (jsonString.Length == 0 || jsonString == null)
			{
                Logger.Log("JSON list is empty, returning...", this);
				return;
			}
            Logger.Log("Running Detection...", this);

			//get spine and hips first, as they are used to calculate the avatar scale
			Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
			Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);

			//calculate the avatar scale using the distance from hips to spine
			d_spinetochest = Vector3.Distance(chest, spine);

			WingFlightPlusGlide.wingtipOffset = WingtipOffset;
			WingFlightPlusGlide.weight = weight;

			//get all the bones
			Vector3 head = localPlayer.GetBonePosition(HumanBodyBones.Head);
			Vector3 neck = localPlayer.GetBonePosition(HumanBodyBones.Neck);
			Vector3 leftShoulder = localPlayer.GetBonePosition(HumanBodyBones.LeftShoulder);
			Vector3 LeftUpperArm = localPlayer.GetBonePosition(HumanBodyBones.LeftUpperArm);
			Vector3 LeftLowerArm = localPlayer.GetBonePosition(HumanBodyBones.LeftLowerArm);
			Vector3 LeftHand = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

			Vector3[] boneVectors = { chest, head, neck, leftShoulder, LeftUpperArm, LeftLowerArm, LeftHand };
			hashV1 = GetHash(boneVectors, 1);
			hashV2 = GetHash(boneVectors, 2);

            Logger.Log("HashV1: " + hashV1, this);
            Logger.Log("HashV2: " + hashV2, this);

			//check if the hash is the loading avatar, and if it is then dont check if the avatar is allowed to fly
			if (hashV2 == "1439458325v2" && skipLoadingAvatar)
			{
				debugInfo = "Loading Avatar Detected, ignoring...";
				name = "Loading Avatar";
				creator = "Loading Avatar";
				introducer = "Loading Avatar";
				weight = 1;
				WingtipOffset = 0;
                Logger.Log("Loading Avatar Detected, ignoring...", this);
				return;
			}

			//check if the avatar is allowed to fly
			allowedToFly = IsAvatarAllowedToFly(hashV1, hashV2);

			//tell openflight if the avatar is allowed to fly
			if (allowedToFly)
			{
				OpenFlight.CanFly();
                Logger.Log("Avatar is allowed to fly!", this);
			}
			else
			{
				OpenFlight.CannotFly();
				WingFlightPlusGlide.wingtipOffset = 0;
				WingFlightPlusGlide.weight = 1;
                Logger.Log("Avatar is not allowed to fly!", this);
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

		bool IsAvatarAllowedToFly(string in_hashV1, string in_hashV2)
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
					//DataToken[] avi_variant_keys = variant.GetKeys().ToArray();
					DataToken[] hashArray = variant["Hash"].DataList.ToArray();
					for (int k = 0; k < hashArray.Length; k++)
					{
						string hash = hashArray[k].String;
						if (hash == in_hashV1 || hash == in_hashV2)
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

		public void LoadJSON()
		{
            Logger.Log("Deserializing JSON list...", this);
			jsonString = JSONLoader.Output;
			//purely temp variable due to needing to use out
			bool success = VRCJson.TryDeserializeFromJson(jsonString, out DataToken jsonDataToken);
			if (!success)
			{
				debugInfo = "Failed to load JSON list!";
                Logger.LogError("Failed to load JSON list! This shouldnt occur unless we messed up the JSON, or VRChat broke something!", this);
				return;
			}
			json = jsonDataToken.DataDictionary;
			jsonVersion = json["JSON Version"].String;
			jsonDate = json["JSON Date"].String;
			RunDetection();
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
		}

		//bonePositions [chest, head, neck, leftShoulder, LeftUpperArm, LeftLowerArm, LeftHand]
		string GetHash(Vector3[] bonePositions, int version)
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
                    d_necktohead = GetBoneDistance(bonePositions[2], bonePositions[1], scalingFactor, (float)d_spinetochest);
                    d_chesttoneck = GetBoneDistance(bonePositions[0], bonePositions[2], scalingFactor, (float)d_spinetochest);
                    d_leftshouldertoleftupperarm = GetBoneDistance(bonePositions[3], bonePositions[4], scalingFactor, (float)d_spinetochest);
                    d_leftupperarmtoleftlowerarm = GetBoneDistance(bonePositions[4], bonePositions[5], scalingFactor, (float)d_spinetochest);
                    d_leftlowertolefthand = GetBoneDistance(bonePositions[5], bonePositions[6], scalingFactor, (float)d_spinetochest);

                    hashV1Distances[0] = d_necktohead;
                    hashV1Distances[1] = d_chesttoneck;
                    hashV1Distances[2] = d_leftshouldertoleftupperarm;
                    hashV1Distances[3] = d_leftupperarmtoleftlowerarm;
                    hashV1Distances[4] = d_leftlowertolefthand;

                    boneInfo = d_necktohead + "." + d_chesttoneck + "." + d_leftshouldertoleftupperarm + "." + d_leftupperarmtoleftlowerarm + "." + d_leftlowertolefthand;
					return boneInfo.GetHashCode().ToString();
				case 2:
					scalingFactor = 100;
                    d_necktohead = GetBoneDistance(bonePositions[2], bonePositions[1], scalingFactor, (float)d_spinetochest);
                    d_chesttoneck = GetBoneDistance(bonePositions[0], bonePositions[2], scalingFactor, (float)d_spinetochest);
                    d_leftshouldertoleftupperarm = GetBoneDistance(bonePositions[3], bonePositions[4], scalingFactor, (float)d_spinetochest);
                    d_leftupperarmtoleftlowerarm = GetBoneDistance(bonePositions[4], bonePositions[5], scalingFactor, (float)d_spinetochest);
                    d_leftlowertolefthand = GetBoneDistance(bonePositions[5], bonePositions[6], scalingFactor, (float)d_spinetochest);

                    hashV2Distances[0] = d_necktohead;
                    hashV2Distances[1] = d_chesttoneck;
                    hashV2Distances[2] = d_leftshouldertoleftupperarm;
                    hashV2Distances[3] = d_leftupperarmtoleftlowerarm;
                    hashV2Distances[4] = d_leftlowertolefthand;

                    boneInfo = d_necktohead + "." + d_chesttoneck + "." + d_leftshouldertoleftupperarm + "." + d_leftupperarmtoleftlowerarm + "." + d_leftlowertolefthand;
					return boneInfo.GetHashCode().ToString() + "v2";
				default:
                    Logger.LogError("Invalid Hash Version Sent", this);
					return "0";
			}
		}

		//this can be used for other scripts to check if the avatar is allowed to fly again
		/// <summary>
		/// 	Reevaluates whether or not you should be able to fly
		/// </summary>
		public void ReevaluateFlight()
		{
			d_spinetochest = 0;
			RunDetection();
			//Debug.Log("Reevaluating flight");
		}
	}
}
