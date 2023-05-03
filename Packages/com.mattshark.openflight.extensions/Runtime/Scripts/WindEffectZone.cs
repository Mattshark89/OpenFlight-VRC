using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//TODO: Tie in the air friction, weight and wingspan values into this somehow
public class WindEffectZone : DirectionalZone
{
	[Tooltip("This is the speed of wind a player will experience. Players velocity will be altered by how much of their wing area is exposed to the wind")]
	public float WindSpeed = 10f;

	void Start()
	{
		init();
	}

	public void OnPlayerTriggerStay()
	{
		//get the local players velocity
		Vector3 currentPlayerVelocity = localPlayer.GetVelocity();
		//Convert the positive z vector of the zone to a world space vector
		Vector3 worldSpaceDirection = transform.TransformDirection(getDirectionVector());

		//get the wing area of the player
		float wingArea = WingArea.GetWingArea(localPlayer, worldSpaceDirection);

		//calculate the velocity to add based on the wing area exposed
		//Time.deltaTime is used to make the wind speed consistent regardless of framerate (FixedUpdate in this case)
		//Note: FixedUpdate is still based on headset HZs, so it will be inconsistent between headsets
		//TODO: make this headset independent
		float calculatedVelocity = (WindSpeed * wingArea) * Time.deltaTime;

		//determine how fast the player is moving along the direction of the zone
		float playerSpeed = Vector3.Dot(currentPlayerVelocity, worldSpaceDirection);

		//this is a weak implementation of a drag force, as it doesnt actually do any delta calculation
		//should hopefully be accurate enough though
		if (playerSpeed < WindSpeed && calculatedVelocity > 0.03f)
		{
			//push the player back based on the direction of the zone (positive z relative to the zone)
			Vector3 ModifiedVelocity = currentPlayerVelocity + (worldSpaceDirection * calculatedVelocity);

			localPlayer.SetVelocity(ModifiedVelocity);
		}

		//Debug.Log("Player speed: " + playerSpeed + " Calculated strength: " + calculatedVelocity);
	}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override Color GetGizmoColor()
    {
        return Color.cyan;
    }
#endif
}
