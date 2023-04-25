using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WindZone : DirectionalZone
{
	public float minWind = 0f;
	public float maxWind = 10f;

	void Start()
	{
		init();
	}

	public void OnPlayerTriggerStay()
	{
		//progressively push you back the deeper you go into the zone
		Vector3 currentPlayerVelocity = localPlayer.GetVelocity();
		//Convert the positive z vector of the zone to a world space vector
		Vector3 worldSpaceDirection = transform.TransformDirection(Vector3.forward);

		//get the wing area of the player
		float wingArea = WingArea.GetWingArea(localPlayer, worldSpaceDirection);

		//calculate the strength based on the wing area exposed
		float calculatedStrength = Mathf.Lerp(minWind / 100, maxWind / 100, wingArea);

		//push the player back based on the direction of the zone (positive z relative to the zone)
		Vector3 ModifiedVelocity = currentPlayerVelocity + (worldSpaceDirection * calculatedStrength);

		localPlayer.SetVelocity(ModifiedVelocity);

		//Debug.Log("Wing area: " + wingArea + " Calculated strength: " + calculatedStrength);
	}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override Color GetGizmoColor()
    {
        return Color.cyan;
    }
#endif
}
