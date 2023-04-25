using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NoFlyZone : Zone
{
	OpenFlight openFlight;

	void Start()
	{
		init();

		//finds the OpenFlight script in the scene
		openFlight = GameObject.Find("OpenFlight").GetComponent<OpenFlight>();
	}

	public void OnPlayerTriggerEnter()
	{
		//turns off flight when the player enters the no fly zone
		openFlight.ForceDisableFlight();
		zoneNotifier.notifyPlayer("Flight Disabled by World");
	}

	public void OnPlayerTriggerExit()
	{
		//turns flight back on when the player leaves the no fly zone
		openFlight.ResetForcedFlight();
		zoneNotifier.notifyPlayer("Flight Returned to Previous State");
	}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override Color GetGizmoColor()
    {
        return Color.blue;
    }
#endif
}
