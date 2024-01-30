/**
 * @ Maintainer: Happyrobot33
 */

using UnityEngine;
using VRC.SDKBase;

namespace OpenFlightVRC
{
	/// <summary>
	/// A collection of useful functions that I cant find a better place for
	/// </summary>
	public class Util : LoggableUdonSharpBehaviour
	{
		/// <summary>
		/// Returns a a byte that is made up of the bools in the array
		/// </summary>
		/// <param name="bools">The bools to pack into a byte</param>
		/// <returns>A byte made up of the bools in the array</returns>
		public static byte BitPackBool(params bool[] bools)
		{
			if (bools.Length > 8)
			{
				Logger.LogError("Too many bools to pack into a byte!", null);
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
	}
}
