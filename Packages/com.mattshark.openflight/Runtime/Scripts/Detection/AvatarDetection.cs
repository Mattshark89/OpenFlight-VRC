/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using static OpenFlightVRC.Util;

namespace OpenFlightVRC
{
	public enum AvatarDetectionCallback
	{
		RunDetection,
		LoadJSON
	}

	/// <summary>
	/// A script to detect the avatar worn and check if it is allowed to fly
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class AvatarDetection : CallbackUdonSharpBehaviour
	{
		private VRCPlayerApi _localPlayer = null;

		/// <summary>
		/// Whether or not to skip loading the avatar entirely
		/// </summary>
		public bool skipLoadingAvatar = true;

		/// <summary>
		/// Contains all the debug info about avatar detection
		/// </summary>
		/// <remarks>
		/// Spine to Chest: XXX
		/// Head to Neck: XXX
		/// Chest to Neck: XXX
		/// Left Shoulder to Left Upper Arm: XXX
		/// Left Upper Arm to Left Lower Arm: XXX
		/// Left Lower Arm to Left Hand: XXX
		/// Combined Bone Info: XXX
		/// Hash: XXX
		/// Allowed to Fly: XXX
		/// Detected Avatar Info:
		/// Name: XXX
		/// Creator: XXX
		/// Introducer: XXX
		/// Weight: XXX
		/// Wingtip Offset: XXX </remarks>
		[ReadOnlyInspector]
		public string debugInfo = "";

		/// <summary>
		/// The distance from the spine to the chest, used to calculate the avatar scale
		/// </summary>
		[ReadOnlyInspector]
		public double d_spinetochest = 0;

		//external JSON list stuff
		#region Script References
		/// <summary>
		/// The AvatarListLoader script that is used to load the avatar list
		/// </summary>
		public AvatarListLoader JSONLoader;

		/// <summary>
		/// The OpenFlight script, used to enable/disable flight
		/// </summary>
		public OpenFlight OpenFlight;
		/// <summary>
		/// The WingFlightPlusGlide script, needed to set the flight properties
		/// </summary>
		public WingFlightPlusGlide WingFlightPlusGlide;
		#endregion
		#region JSON Info
		[System.NonSerialized]
		private string _jsonString = "";
		private DataDictionary _json;
		/// <summary>
		/// The version of the JSON file that was loaded
		/// </summary>
		[ReadOnlyInspector]
		public string jsonVersion = "";
		/// <summary>
		/// The date of the JSON file that was loaded
		/// </summary>
		[ReadOnlyInspector]
		public string jsonDate = "";
		#endregion
		/// <summary>
		/// Whether or not the user is allowed to fly
		/// </summary>
		[ReadOnlyInspector]
		public bool allowedToFly = false;

		#region Avatar Information
		/// <summary>
		/// The current hash of the avatar worn
		/// </summary>
		[ReadOnlyInspector]
		public string hash = "0";
		internal float[] HashDistances = new float[5];
		/// <summary>
		/// The weight of the currently worn avatar
		/// </summary>
		[ReadOnlyInspector]
		public float weight = 1;
		/// <summary>
		/// The offset of the wing tip for the current avatar
		/// </summary>
		[ReadOnlyInspector]
		public float WingtipOffset = 0;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
		/// <summary>
		/// 	The name of the currently worn avatar. Defaults to `N/A` if not in the list, or `Loading Avatar` if you are in the loading avatar / mecanim default rig
		/// </summary>
		[ReadOnlyInspector]
		public string name = ""; //this is the name of the avatar base
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
		/// <summary>
		/// 	The creator of the currently worn avatar. Defaults to `N/A` if not in the list, or `Loading Avatar` if you are in the loading avatar / mecanim default rig
		/// </summary>
		[ReadOnlyInspector]
		public string creator = ""; //this is the person who created the avatar base
		/// <summary>
		/// 	The introducer of the currently worn avatar. Defaults to `N/A` if not in the list, or `Loading Avatar` if you are in the loading avatar / mecanim default rig
		/// </summary>
		[ReadOnlyInspector]
		public string introducer = ""; //this is the person who introduced the avatar to the JSON list itself
		/// <summary>
		/// The hash of the default mecanim humanoid rig, which is also what VRChat uses as the loading avatar
		/// </summary>
		const string LOADINGAVATARHASH = "1439458325v2";
		#endregion

		void Start()
		{
			_localPlayer = Networking.LocalPlayer;

			debugInfo = "Loading JSON list...";
			JSONLoader.AddCallback(AvatarListLoaderCallback.AvatarListLoaded, this, nameof(LoadJSON));

			JSONLoader.LoadURL();
		}

		public override void OnAvatarChanged(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				Logger.Log("Avatar Changed, reevaluating flight...", this);
				RunDetection();
			}
		}

		//detect when the avatars player scale changes and re save the spine to chest distance
		public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float eyeHeight)
		{
			//check if local player
			if (!player.isLocal)
			{
				return;
			}
			Logger.Log(string.Format("Avatar Scale Changed, reevaluating {0}...", nameof(d_spinetochest)), this);
			d_spinetochest = CalculateAvatarScale();
		}

		/// <summary>
		/// Calculates the avatar scale using the distance from hips to spine.
		/// </summary>
		/// <param name="chest">The chest bone position</param>
		/// <param name="spine">The spine bone position</param>
		/// <returns>The avatar scale</returns>
		private double CalculateAvatarScale(out Vector3 spine, out Vector3 chest)
		{
			spine = _localPlayer.GetBonePosition(HumanBodyBones.Spine);
			chest = _localPlayer.GetBonePosition(HumanBodyBones.Chest);
			return Vector3.Distance(chest, spine);
		}

		private double CalculateAvatarScale()
		{
			return CalculateAvatarScale(out Vector3 spine, out Vector3 chest);
		}

		private void RunDetection()
		{
			if (string.IsNullOrEmpty(_jsonString))
			{
				Logger.Log("JSON list is empty, returning...", this);
				return;
			}
			Logger.Log("Running Detection...", this);

			//we need a accurate avatar scale for the hash to work
			d_spinetochest = CalculateAvatarScale(out Vector3 spine, out Vector3 chest);

			WingFlightPlusGlide.wingtipOffset = WingtipOffset;
			WingFlightPlusGlide.weight = weight;

			//get all the bones
			Vector3 head = _localPlayer.GetBonePosition(HumanBodyBones.Head);
			Vector3 neck = _localPlayer.GetBonePosition(HumanBodyBones.Neck);
			Vector3 leftShoulder = _localPlayer.GetBonePosition(HumanBodyBones.LeftShoulder);
			Vector3 leftUpperArm = _localPlayer.GetBonePosition(HumanBodyBones.LeftUpperArm);
			Vector3 leftLowerArm = _localPlayer.GetBonePosition(HumanBodyBones.LeftLowerArm);
			Vector3 leftHand = _localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

			Vector3[] boneVectors = { chest, head, neck, leftShoulder, leftUpperArm, leftLowerArm, leftHand };
			hash = GenerateHash(boneVectors, 2);

			Logger.Log("Avatar Hash: " + hash, this);

			if (hash == LOADINGAVATARHASH && skipLoadingAvatar)
			{
				debugInfo = "Loading Avatar Detected, ignoring...";
				name = "Loading Avatar";
				creator = "Loading Avatar";
				introducer = "Loading Avatar";
				weight = 1;
				WingtipOffset = 0;
				Logger.Log("Loading Avatar Detected, ignoring...", this);
			}
			else
			{
				allowedToFly = IsAvatarAllowedToFly(hash);

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
			}

			//print all the info to the text
			debugInfo =
				"HashV2: "
				+ hash
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

			RunCallback(AvatarDetectionCallback.RunDetection);
		}

		/// <summary>
		/// Processes the timer and logs the time it took to lookup the hash
		/// </summary>
		/// <param name="stopwatch"></param>
		/// <param name="hash"></param>
		private void ProcessTimer(System.Diagnostics.Stopwatch stopwatch, string hash)
		{
			stopwatch.Stop();
			Logger.Log("Hash Lookup Time: " + stopwatch.ElapsedMilliseconds + "ms for hash: " + hash, this);
		}

		/// <summary>
		/// Checks the hash against the JSON list to see if the avatar is allowed to fly or not
		/// </summary>
		/// <param name="in_hash">The hash of the avatar</param>
		/// <returns>Whether or not the avatar is allowed to fly</returns>
		private bool IsAvatarAllowedToFly(string in_hash)
        {
            //start a timer
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            DataToken hash_token = new DataToken(in_hash);

            //if error token, return false and log error
            if (hash_token.Error != DataError.None)
            {
                Logger.LogError("Invalid Hash Sent, received error: " + hash_token.Error + " with hash: " + in_hash, this);
                return false;
            }

            //Attempt to use the hashtable method first, otherwise fall back to the old crawling method
            if (_json.ContainsKey("HashTable"))
            {
                //this means we have the fast lookup option available
                DataDictionary hashTable = _json["HashTable"].DataDictionary;
                if (hashTable.TryGetValue(hash_token, out DataToken data))
                {
                    DataDictionary variant = data.DataDictionary;
					SetInfo(variant["Name"].String, variant["Creator"].String, variant["Introducer"].String, (float)variant["Weight"].Number, (float)variant["WingtipOffset"].Number);
                    ProcessTimer(sw, in_hash);
                    return true;
                }
                else
                {
					SetInfoDefault();
                    ProcessTimer(sw, in_hash);
                    return false;
                }
            }

            Logger.LogWarning("HashTable not found, falling back to old crawling method", this);

            //Old crawling method
            DataDictionary bases = _json["Bases"].DataDictionary;
            DataToken[] baseKeys = bases.GetKeys().ToArray();
            for (int i = 0; i < bases.Count; i++)
            {
                DataDictionary avi_base = bases[baseKeys[i]].DataDictionary;
                DataToken[] avi_base_keys = avi_base.GetKeys().ToArray();
                for (int j = 0; j < avi_base.Count; j++)
                {
                    DataDictionary variant = avi_base[avi_base_keys[j]].DataDictionary;

                    if (variant["Hash"].DataList.Contains(hash_token))
                    {
						SetInfo(variant["Name"].String, variant["Creator"].String, variant["Introducer"].String, (float)variant["Weight"].Number, (float)variant["WingtipOffset"].Number);
                        ProcessTimer(sw, in_hash);
                        return true;
                    }
                }
            }

            SetInfoDefault();
            ProcessTimer(sw, in_hash);
            return false;
        }

		/// <summary>
		/// Sets the info of the avatar to the given values
		/// </summary>
		/// <param name="name"></param>
		/// <param name="creator"></param>
		/// <param name="introducer"></param>
		/// <param name="weight"></param>
		/// <param name="wingtipOffset"></param>
        private void SetInfo(string name, string creator, string introducer, float weight, float wingtipOffset)
		{
			this.name = name;
			this.creator = creator;
			this.introducer = introducer;
			this.weight = weight;
			WingtipOffset = wingtipOffset;
		}
        
		/// <summary>
		/// Sets the info of the avatar to the default values
		/// </summary>
		private void SetInfoDefault()
		{
			const string DEFAULT = "N/A";
			SetInfo(DEFAULT, DEFAULT, DEFAULT, 1, 0);
		}

        /// <summary>
        /// Deserializes the JSON list after being told its available
        /// </summary>
        public void LoadJSON()
		{
			Logger.Log("Deserializing JSON list...", this);
			_jsonString = JSONLoader.Output;

			//Return type is if the deserialization was successful or not
			if (!VRCJson.TryDeserializeFromJson(_jsonString, out DataToken jsonDataToken))
			{
				debugInfo = "Failed to load JSON list!";
				Logger.LogError("Failed to load JSON list! This shouldnt occur unless we messed up the JSON, or VRChat broke something!", this);
				return;
			}
			_json = jsonDataToken.DataDictionary;
			jsonVersion = _json["JSON Version"].String;
			jsonDate = _json["JSON Date"].String;

			//generate the hashtable
			Logger.Log("Generating Hash Lookup Table...", this);
			//setup and start stopwatch
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			DataDictionary hashTable = new DataDictionary();

			DataDictionary bases = _json["Bases"].DataDictionary;
			DataToken[] baseKeys = bases.GetKeys().ToArray();
			for (int j = 0; j < bases.Count; j++)
			{
				DataDictionary avi_base = bases[baseKeys[j]].DataDictionary;
				DataToken[] avi_base_keys = avi_base.GetKeys().ToArray();
				for (int k = 0; k < avi_base.Count; k++)
				{
					DataDictionary variant = avi_base[avi_base_keys[k]].DataDictionary.DeepClone();
					DataToken[] hashes = variant["Hash"].DataList.ToArray();
					//remove hashes from the variant
					variant.Remove("Hash");
					for (int l = 0; l < hashes.Length; l++)
					{
						hashTable.Add(hashes[l], variant);
					}
				}
			}

			_json.SetValue("HashTable", hashTable);
			sw.Stop();
			int ms = sw.Elapsed.Milliseconds;
			//calculate the time it took to generate each individual hash
			int hashCount = hashTable.Count;
			double averageTime = (double)ms / (double)hashCount;
			Logger.Log(string.Format("Hash Lookup Table Generated! Took: {0}ms, Average Time Per Hash: {1}ms", ms, averageTime), this);

			RunCallback(AvatarDetectionCallback.LoadJSON);
			RunDetection();
		}

		/// <summary>
		/// 	Tells the script to reload the JSON file and then recheck your worn avatar for flight
		/// </summary>
		public void reloadJSON()
		{
			debugInfo = "Loading JSON list...";
			JSONLoader.LoadURL();

			_jsonString = "";
			d_spinetochest = 0;
		}

		/// <summary>
		/// Generates a hash based on the bone positions
		/// </summary>
		/// <param name="bonePositions">[chest, head, neck, leftShoulder, LeftUpperArm, LeftLowerArm, LeftHand]</param>
		/// <param name="version"></param>
		/// <returns>The generated hash</returns>
		private string GenerateHash(Vector3[] bonePositions, int version)
		{
			int scalingFactor;
			int d_necktohead;
			int d_chesttoneck;
			int d_leftshouldertoleftupperarm;
			int d_leftupperarmtoleftlowerarm;
			int d_leftlowertolefthand;
			string boneInfo;

			//if you are wondering why this switch case is here, it is to support future versions of hashing, and was previously here to support v1 hashes. its left here so the implementation is already in place
			switch (version)
			{
				case 2:
					scalingFactor = 100;
					d_necktohead = GetBoneDistance(bonePositions[2], bonePositions[1], scalingFactor, (float)d_spinetochest);
					d_chesttoneck = GetBoneDistance(bonePositions[0], bonePositions[2], scalingFactor, (float)d_spinetochest);
					d_leftshouldertoleftupperarm = GetBoneDistance(bonePositions[3], bonePositions[4], scalingFactor, (float)d_spinetochest);
					d_leftupperarmtoleftlowerarm = GetBoneDistance(bonePositions[4], bonePositions[5], scalingFactor, (float)d_spinetochest);
					d_leftlowertolefthand = GetBoneDistance(bonePositions[5], bonePositions[6], scalingFactor, (float)d_spinetochest);

					HashDistances[0] = d_necktohead;
					HashDistances[1] = d_chesttoneck;
					HashDistances[2] = d_leftshouldertoleftupperarm;
					HashDistances[3] = d_leftupperarmtoleftlowerarm;
					HashDistances[4] = d_leftlowertolefthand;

					boneInfo = d_necktohead + "." + d_chesttoneck + "." + d_leftshouldertoleftupperarm + "." + d_leftupperarmtoleftlowerarm + "." + d_leftlowertolefthand;
					return boneInfo.GetHashCode().ToString() + "v2";
				default:
					Logger.LogError("Invalid Hash Version Sent", this);
					return "0";
			}
		}

		/// <summary>
		/// Reevaluates whether or not you should be able to fly
		/// </summary>
		public void ReevaluateFlight()
		{
			d_spinetochest = 0;
			RunDetection();
		}
	}
}
