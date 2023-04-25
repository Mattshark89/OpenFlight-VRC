using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DefaultsZone : Zone
{
	WingFlightPlusGlide wingFlightPlusGlide;

	void Start()
	{
		init();

		//finds the OpenFlight script in the scene
		wingFlightPlusGlide = GameObject.Find("OpenFlight").GetComponent<OpenFlight>().wingedFlight.GetComponent<WingFlightPlusGlide>();
	}

	public void OnPlayerTriggerEnter()
	{
		//turns off flight when the player enters the no fly zone
		wingFlightPlusGlide.RestoreDefaults();
		zoneNotifier.notifyPlayer("Flight Settings Reset by World");
	}

	//TODO: This should be implemented to restore the player's flight settings to what they were before they entered the zone
	//The only issue with that is that we already have copys of variables for the default flight settings before they are changed by the player
	//storing ANOTHER copy of the flight settings would be a a nightmare to keep track of in WingFlightPlusGlide.cs
	//solution would be to find another implementation for this, then this event can be used
	public void OnPlayerTriggerExit()
	{
		//turns flight back on when the player leaves the no fly zone
		//zoneNotifier.notifyPlayer("Flight Settings Returned to Previous State");
	}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override Color GetGizmoColor()
    {
        return Color.yellow;
    }
#endif
}
