using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Ideally if you are in T pose with wind coming from below you, it'd treat your wing area as 100%, possibly still modified by how close your hands are to your body. As you rotate your hands to 90 degrees along their axis, your wing area would go to 0
//we should calculate wing area for each joint, and then take the average of all of them to get the final wing area

//the local y axis of the windforce object will be the wind direction
namespace OpenFlightVRC.Extensions
{
	public class WingArea : UdonSharpBehaviour
	{
		public static float GetWingArea(VRCPlayerApi localPlayer, Vector3 windDirection)
		{
			float leftArmAlignment = Mathf.Abs(Vector3.Dot(localPlayer.GetBoneRotation(HumanBodyBones.LeftUpperArm) * Vector3.forward, windDirection));
			float rightArmAlignment = Mathf.Abs(Vector3.Dot(localPlayer.GetBoneRotation(HumanBodyBones.RightUpperArm) * Vector3.forward, windDirection));

			float leftForearmAlignment = Mathf.Abs(Vector3.Dot(localPlayer.GetBoneRotation(HumanBodyBones.LeftLowerArm) * Vector3.forward, windDirection));
			float rightForearmAlignment = Mathf.Abs(Vector3.Dot(localPlayer.GetBoneRotation(HumanBodyBones.RightLowerArm) * Vector3.forward, windDirection));

			float leftHandAlignment = Mathf.Abs(Vector3.Dot(localPlayer.GetBoneRotation(HumanBodyBones.LeftHand) * Vector3.forward, windDirection));
			float rightHandAlignment = Mathf.Abs(Vector3.Dot(localPlayer.GetBoneRotation(HumanBodyBones.RightHand) * Vector3.forward, windDirection));

			float averageAlignment = (leftArmAlignment + rightArmAlignment + leftForearmAlignment + rightForearmAlignment + leftHandAlignment + rightHandAlignment) / 6f;

			return averageAlignment;
		}
	}
}
