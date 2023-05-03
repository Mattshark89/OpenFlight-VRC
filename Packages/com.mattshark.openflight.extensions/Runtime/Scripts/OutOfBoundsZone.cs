using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//This zone is like a wind zone, but it progressively gets stronger the deeper the player goes into it, at its strongest making it impossible to move forward
namespace OpenFlightVRC.Extensions
{
	public class OutOfBoundsZone : DirectionalZone
	{
		public bool notifyPlayer = true;

		void Start()
		{
			init();
		}

		public void OnPlayerTriggerEnter()
		{
			if (notifyPlayer)
			{
				zoneNotifier.notifyPlayer("This area is out of bounds! Turn back!");
			}
		}

		public void OnPlayerTriggerStay()
		{
			//progressively push you back the deeper you go into the zone
			Vector3 currentPlayerVelocity = localPlayer.GetVelocity();
			//Convert the positive z vector of the zone to a world space vector
			Vector3 worldSpaceDirection = transform.TransformDirection(getDirectionVector());

			//calculate how deep into the zone the player is along the z axis
			float distance = Mathf.Abs((transform.InverseTransformPoint(localPlayer.GetPosition()).z / colliderDepth) - 0.5f);
			distance = Mathf.Clamp(distance, 0f, 1f);

			//calculate the strength based on the distance from the far side of the zone
			float strength = Mathf.Lerp(0f, 20f, distance);

			//push the player back based on the direction of the zone (positive z relative to the zone)
			Vector3 ModifiedVelocity = currentPlayerVelocity + (worldSpaceDirection * strength);

			localPlayer.SetVelocity(ModifiedVelocity);
		}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override Color GetGizmoColor()
    {
        return Color.red;
    }
#endif
	}
}
