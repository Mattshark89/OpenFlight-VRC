/**
 * @ Maintainer: Happyrobot33
 */

using System;
using System.Text.RegularExpressions;

using OpenFlightVRC.Net;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace OpenFlightVRC
{
	/// <summary>
	/// A collection of useful functions that I cant find a better place for
	/// </summary>
	public static class Util
	{
        /// <summary>
        /// Prefix for all player data keys
        /// </summary>
        public const string playerDataFolderKey = "OpenFlightVRC/";

		
        /// <summary>
        /// Gets a setting from the settings dictionary
        /// </summary>
        /// <param name="settingsLocation"> The location of the settings </param>
        /// <param name="key"> The key of the setting </param>
        /// <param name="token"> The resulting token of the setting </param>
        /// <returns> True if the setting was retrieved, false if it failed </returns>
        public static bool GetSetting(DataDictionary settingsLocation, string key, out DataToken token)
        {
            if (settingsLocation.TryGetValue(key, out token))
            {
                return true;
            }
            else
            {
				Logger.Log(LogLevel.Warning, string.Format("Failed to get setting {0} from settings. Keeping current setting. Error reason: {1}", key, token.Error.ToString()));
                return false;
            }
        }

		/// <summary>
        /// Tries to apply a setting to a variable reference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingsLocation">Where the settings are stored</param>
        /// <param name="keyName">The key of the setting</param>
        /// <param name="variableReference">The variable to apply the setting to</param>
        /// <returns>True if the setting was applied, false if it failed</returns>
        public static bool TryApplySetting<T>(DataDictionary settingsLocation, string keyName, ref T variableReference)
        {
            bool ableToGetSetting = GetSetting(settingsLocation, keyName, out DataToken token);
            if (ableToGetSetting)
            {
                /*
				just for reference
				nameof(System.Boolean) = "Boolean"
				typeof(T).Name = "Boolean"
				
				a single in this case actually means float
				*/

                //enforce the type of the setting
                switch (typeof(T).Name)
                {
                    case nameof(System.Boolean):
                        variableReference = (T)(object)token.Boolean;
                        break;
                    case nameof(System.Single):
                        variableReference = (T)(object)token.Number;
                        break;
                    case nameof(System.Int32):
                        variableReference = (T)(object)token.Number;
                        break;
					case nameof(System.String):
						variableReference = (T)(object)token.String;
						break;
					case nameof(System.Byte):
						variableReference = (T)(object)token.Number;
						break;
					case nameof(System.Int64):
						variableReference = (T)(object)token.Number;
						break;
					case nameof(System.Double):
						variableReference = (T)(object)token.Number;
						break;
                    default:
						Logger.Log(LogLevel.Error, string.Format("Failed to apply setting {0}. Unsupported type {1}", keyName, token.TokenType.ToString()));
                        return false;
                }
                return true;
            }

            return false;
        }

		/// <summary>
		/// Returns a a byte that is made up of the bools in the array
		/// </summary>
		/// <param name="bools">The bools to pack into a byte</param>
		/// <returns>A byte made up of the bools in the array</returns>
		public static byte BitPackBool(params bool[] bools)
		{
			if (bools.Length > 8)
			{
				Logger.Log(LogLevel.Error, "Too many bools to pack into a byte!");
				return 0;
			}

			byte result = 0;
			int length = bools.Length;
			for (int i = 0; i < length; i++)
			{
				if (bools[i])
				{
					result |= (byte)(1 << i);
				}
			}
			return result;
		}

		/// <summary>
		/// Returns an array of bools that are made up of the bits in the byte
		/// </summary>
		/// <param name="b">The byte to unpack</param>
		/// <returns>An array of bools made up of the bits in the byte</returns>
		public static bool[] BitUnpackBool(byte b)
		{
			bool[] result = new bool[8];
			for (int i = 0; i < 8; i++)
			{
				result[i] = (b & (1 << i)) != 0;
			}
			return result;
		}

		/// <summary>
		/// Returns a byte that is made up of the enums in the array.
		/// This is equivalent to <code>ENUM1.flag1 | ENUM2.flag2</code>
		/// U# doesnt support the | operator on enums, so this is a workaround
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static T OrEnums<T>(params T[] flags) where T : System.Enum
		{
			T result = (T)(object)0;
			foreach (T flag in flags)
			{
				result = (T)(object)(Convert.ToInt64(result) | Convert.ToInt64(flag));
			}
			return result;
		}

		/// <summary>
		/// Returns a byte that is made up of the enums in the array.
		/// This is equivalent to <code><![CDATA[ENUM1.flag1 & ENUM2.flag2]]></code>
		/// U# doesnt support the <![CDATA[&]]> operator on enums, so this is a workaround
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static T AndEnums<T>(params T[] flags) where T : System.Enum
		{
			T result = flags[0];
			foreach (T flag in flags)
			{
				result = (T)(object)(Convert.ToInt64(result) & Convert.ToInt64(flag));
			}
			return result;
		}

		/// <summary>
		/// Returns the distance between two bones, modified by the scaling factor and spine
		/// </summary>
		/// <param name="bone1">The first bone</param>
		/// <param name="bone2">The second bone</param>
		/// <param name="scalingFactor">The scaling factor</param>
		/// <param name="d_spinetochest">The distance between the spine and the chest</param>
		/// <returns></returns>
		public static int GetBoneDistance(Vector3 bone1, Vector3 bone2, int scalingFactor, float d_spinetochest)
		{
			return Mathf.FloorToInt(Vector3.Distance(bone1, bone2) / d_spinetochest * scalingFactor);
		}

		/// <summary>
		/// Moves a gameobject to a wingtip position
		/// </summary>
		/// <param name="bone">The tracking data of the player</param>
		/// <param name="objectToMove">The wingtip gameobject</param>
		/// <param name="wingtipOffset">The offset of the wingtip</param>
		/// <param name="d_spinetochest">The distance between the spine and the chest</param>
		public static void SetWingtipTransform(VRCPlayerApi.TrackingData bone, GameObject objectToMove, float wingtipOffset, double d_spinetochest)
		{
			objectToMove.transform.rotation = Quaternion.identity;
			Vector3 position = bone.position;
			Quaternion rotation = bone.rotation;

			Vector3 wingTipPosition = position + (rotation * Vector3.forward * new Vector3(0, 0, wingtipOffset * (float)d_spinetochest).z);
			FinalizeWingtipPosition(objectToMove, position, rotation, wingTipPosition);
		}

		private static void FinalizeWingtipPosition(GameObject objectToMove, Vector3 position, Quaternion rotation, Vector3 wingTipPosition)
		{
			objectToMove.transform.position = wingTipPosition;

			//rotate so it goes in the correct direction
			objectToMove.transform.RotateAround(position, rotation * Vector3.up, 70);
		}

		/// <summary>
		/// Moves a gameobject to a wingtip position, but based on the object to moves current rotation and position
		/// </summary>
		/// <param name="objectToMove">The wingtip gameobject</param>
		/// <param name="worldWingtipOffset">The offset of the wingtip, in world space</param>
		public static void SetWingtipTransform(GameObject objectToMove, float worldWingtipOffset)
		{
			Vector3 position = objectToMove.transform.position;
			Quaternion rotation = objectToMove.transform.rotation;

			Vector3 wingTipPosition = position + (rotation * Vector3.forward * new Vector3(0, 0, worldWingtipOffset).z);

			FinalizeWingtipPosition(objectToMove, position, rotation, wingTipPosition);
		}

		/// <summary>
		/// Helper function to get the total distance of a vector array.
		/// this adds up all of the distances between each vector in the array in order, then returns the total distance
		/// </summary>
		/// <param name="vectors">The vector array to get the total distance of</param>
		/// <returns>The total distance of the vector array</returns>
		public static float TotalVectorDistance(params Vector3[] vectors)
		{
			float totalDistance = 0;
			for (int i = 0; i < vectors.Length; i++)
			{
				if (i != 0)
				{
					totalDistance += Vector3.Distance(vectors[i], vectors[i - 1]);
				}
			}
			return totalDistance;
		}

		/// <summary>
		/// Gets a scale modifier based on the player's scale in order to scale things uniformly
		/// </summary>
		/// <remarks>
		/// This really should be replaced with the newer GetAvatarEyeHeightAsMeters function from VRC, but testing to make sure it behaves the same is needed and is difficult to do in unity since clientsim doesnt emulate scaling as of now
		/// </remarks>
		/// <returns>A float to scale by. 1 is the base scale of the loading avatar</returns>
		public static float ScaleModifier()
		{
			VRCPlayerApi localPlayer = Networking.LocalPlayer;

			//change the scale of the gameobject based on the players scale
			//add up all of the bone distances from the foot to the head
			Vector3 footR = localPlayer.GetBonePosition(HumanBodyBones.RightFoot);
			Vector3 lowerLegR = localPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg);
			Vector3 upperLegR = localPlayer.GetBonePosition(HumanBodyBones.RightUpperLeg);
			Vector3 hips = localPlayer.GetBonePosition(HumanBodyBones.Hips);
			Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
			Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);
			Vector3 neck = localPlayer.GetBonePosition(HumanBodyBones.Neck);
			Vector3 head = localPlayer.GetBonePosition(HumanBodyBones.Head);
			float playerScale = TotalVectorDistance(footR, lowerLegR, upperLegR, hips, spine, chest, neck, head);

			//if the player is too small, set the scale to 0.1
			playerScale = Mathf.Max(playerScale, 0.1f);

			//this is determined experimentally, and makes the loading avatar scale 1
			const float scalingOffset = 0.6183768f;

			return playerScale * scalingOffset;
		}

		/// <summary>
		/// Creates and returns a rainbow gradient for use in particle systems
		/// </summary>
		/// <returns>A rainbow gradient</returns>
		public static Gradient GetRainbowGradient()
		{
			GradientColorKey[] rainbow = new GradientColorKey[8];

			for (int i = 0; i < 8; i++)
			{
				float hue = Mathf.InverseLerp(0, 7, i);
				var color = Color.HSVToRGB(hue, 1, 1);
				rainbow[i] = new GradientColorKey(color, hue);
			}

			Gradient gradient = new Gradient();

			gradient.colorKeys = rainbow;

			return gradient;
		}

		/// <summary>
		/// Sets the emission of a particle system
		/// </summary>
		/// <param name="ps">The particle system to set the emission of</param>
		/// <param name="enabled">If the particle system should be emitting or not</param>
		public static void SetParticleSystemEmission(ParticleSystem ps, bool enabled)
		{
			ParticleSystem.EmissionModule emission = ps.emission;
			emission.enabled = enabled;
		}

		/// <summary>
		/// Controls the sound of an audio source, taking into account if it is already playing or not
		/// </summary>
		/// <param name="source">The audio source to control</param>
		/// <param name="enabled">If the audio source should be playing or not</param>
		/// <returns>If the audio source was changed</returns>
		public static bool ControlSound(AudioSource source, bool enabled)
		{
			if (enabled && !source.isPlaying)
			{
				source.Play();
				return true;
			}
			else if (source.isPlaying)
			{
				source.Stop();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets all player objects of a specific type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T[] GetAllPlayerObjectsOfType<T>()
		{
			//get the full player list
			VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
			VRCPlayerApi.GetPlayers(players);

			T[] scripts = new T[players.Length];

			for (int i = 0; i < players.Length; i++)
            {
                VRCPlayerApi player = players[i];
                scripts[i] = GetPlayerObjectOfType<T>(player);
            }

            return scripts;
		}

		/// <summary>
		/// Gets a player object of a specific type passed into it
		/// </summary>
		/// <typeparam name="T">The type of object to get</typeparam>
		/// <param name="player">The player to get the object from</param>
		/// <returns>The object of the type passed in</returns>
        public static T GetPlayerObjectOfType<T>(VRCPlayerApi player)
        {
            //TODO: If something is introduced that allows us to ask specifically for tagged objects, use that instead so we dont have to loop through all objects
            GameObject[] objects = Networking.GetPlayerObjects(player);

            //loop through all objects to find the one we want
            foreach (GameObject obj in objects)
            {
                T script = obj.GetComponent<T>();
                if (script != null)
                {
					return script;
                }
            }

			Logger.Log(LogLevel.Error, "Could not find type on player " + player.displayName, true);
			return default;
		}

		/// <summary>
		/// Gets the estimated latency of a player in milliseconds
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static float Latency(VRCPlayerApi player)
		{
			return (Time.realtimeSinceStartup - Networking.SimulationTime(player)) * 1000;
		}

		/// <summary>
		/// Gets the estimated latency of a gameobject in milliseconds
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static float Latency(GameObject obj)
		{
			return (Time.realtimeSinceStartup - Networking.SimulationTime(obj)) * 1000;
		}

		/// <summary>
		/// Converts a markdown string to a rich text string
		/// </summary>
		/// <param name="markdown">
		/// A markdown string
		/// </param>
		/// <returns>
		/// A rich text string
		/// </returns>
		public static string MarkdownToRichText(this string markdown)
		{
			string richText = markdown;

			//setup all the rules
			DataDictionary rules = new DataDictionary();

			//since U# doesnt support initializers in a function, we have to do this
			//suppress the warning
			#pragma warning disable IDE0028
			//the (?gm) is a inline options set
			//group 1 is the text that will be used in the replacement

			//Headers
			rules.Add(@"(?m:^#{6}\s?([^\n]+))", "<size=067%><b>{0}</b><size=100%>");
			rules.Add(@"(?m:^#{5}\s?([^\n]+))", "<size=083%><b>{0}</b><size=100%>");
			rules.Add(@"(?m:^#{4}\s?([^\n]+))", "<size=100%><b>{0}</b><size=100%>");
			rules.Add(@"(?m:^#{3}\s?([^\n]+))", "<size=117%><b>{0}</b><size=100%>");
			rules.Add(@"(?m:^#{2}\s?([^\n]+))", "<size=150%><b>{0}</b><size=100%>");
			rules.Add(@"(?m:^#{1}\s?([^\n]+))", "<size=200%><b>{0}</b><size=100%>");

			//Bold
			rules.Add(@"\*\*\s?([^\n]+)\*\*", "<b>{0}</b>");
			rules.Add(@"__\s?([^\n]+)__", "<b>{0}</b>");

			//italic
			rules.Add(@"\*\s?([^\n]+)\*", "<i>{0}</i>");
			rules.Add(@"_\s?([^\n]+)_", "<i>{0}</i>");

			//strikethrough
			rules.Add(@"\~\~\s?([^\n]+)\~\~", "<s>{0}</s>");

			//images
			rules.Add(@"!\[([^\]]+)\]\(([^\)]+)\)", "<color=#0000EE>{0}</color>");

			//additional, non-markdown spec rules
			//github PR and Issue links
			rules.Add(@"(?m:https?://github.com/.+?/.+?/pull/(\d+))", "<color=#0000EE>PR #{0}</color>"); //pull
			rules.Add(@"(?m:https?://github.com/.+?/.+?/issues/(\d+))", "<color=#0000EE>Issue #{0}</color>"); //issue
			rules.Add(@"(?m:https?://github.com/.+?/.+?/compare/(\w+-\d+\.\d+\.\d+\.+\w+-\d+\.\d+\.\d+))", "<color=#0000EE>PR #{0}</color>"); //compare

			//attempt at code blocking, this isnt technically correct but hopefully its good enough
			rules.Add(@"(?m:\`\s?([^\n]+)\`)", "<color=#FFFFFF><mark=#FFFFFF11>{0}</mark></color>");
			//no multiline support as of yet, this is weird to figure out
			//rules.Add(@"(?m:\`\`\`\s?([^\n]+)\`\`\`)", "<color=#FFFFFF><mark=#FFFFFF11>{0}</mark></color>");
			#pragma warning restore IDE0028

			//apply each rule
			DataList keys = rules.GetKeys();
			for (int i = 0; i < rules.Count; i++)
			{
				//Logger.Log("Applying rule " + keys[i]);
				string rule = keys[i].ToString();
				string replacement = rules[rule].ToString();
				//get all matches
				MatchCollection matches = Regex.Matches(richText, rule);
				for (int j = 0; j < matches.Count; j++)
				{
					//Logger.Log("Match found: " + matches[j].Groups[0].Value);
					Match match = matches[j];
					//replace each match
					string fullmatch = match.Groups[0].Value;
					string extractedText = match.Groups[1].Value;
					//Logger.Log("Replacing " + fullmatch + " with " + string.Format(replacement, extractedText));
					richText = richText.Replace(fullmatch, string.Format(replacement, extractedText));
				}
			}

			return richText;
		}
    }
}
