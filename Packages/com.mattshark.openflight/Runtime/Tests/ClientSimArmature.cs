using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSimArmature : MonoBehaviour
{
	[Range(0.0f, 30.0f)]
	public float speed = 1.0f;

	public float extent = 50f;

	public Vector3 armRotationOffset = new Vector3(0, 0, 0);

	void Update()
	{
		//wait until the Avatar_Utility gameobject is created
		if (GameObject.Find("Avatar_Utility"))
		{
			//get the Avatar_Utility gameobject
			GameObject avatarUtility = GameObject.Find("Avatar_Utility");

			//get the animator
			Animator animator = avatarUtility.GetComponent<Animator>();

			//get arm transform
			Transform L_arm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			Transform R_arm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

			if (armRotationOffset == Vector3.zero)
			{
				armRotationOffset = L_arm.localEulerAngles;
			}

			//rotate the elbow in a flapping motion along the x axis of the armature itself
			float angle = ((Mathf.Sin(Time.time * speed) + 1) / 2) * extent;
			L_arm.localRotation = Quaternion.Euler(armRotationOffset + new Vector3(-angle, 0, 0));
			R_arm.localRotation = Quaternion.Euler(armRotationOffset + new Vector3(-angle, 0, 0));

			//get both hand objects
			GameObject L_hand = GameObject.Find("DestkopTrackingData/Head/PlayerCamera/LeftHand");
			GameObject R_hand = GameObject.Find("DestkopTrackingData/Head/PlayerCamera/RightHand");

			//set their transforms and rotations to the armature
			L_hand.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
			L_hand.transform.rotation = animator.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
			R_hand.transform.position = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
			R_hand.transform.rotation = animator.GetBoneTransform(HumanBodyBones.RightHand).rotation;
		}
	}
}
