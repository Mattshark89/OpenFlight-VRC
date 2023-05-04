using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Extensions
{
	public class DefaultsZone : Zone
	{
		public OpenFlight openFlight;
		WingFlightPlusGlide wingFlightPlusGlide;
		public bool notifyPlayer = true; //whether or not to notify the player when they enter the zone

		void Start()
		{
			init();

			//grabs the WingFlightPlusGlide script from openFlight
			if (openFlight != null) {
				wingFlightPlusGlide = openFlight.wingedFlight.GetComponent<WingFlightPlusGlide>();
			} //an else block here could return an error. "Assign the OpenFlight value in the inspector"
		}

		public void OnPlayerTriggerEnter()
		{
			//turns off flight when the player enters the no fly zone
			wingFlightPlusGlide.RestoreDefaults();
			if (notifyPlayer)
			{
				zoneNotifier.notifyPlayer("Flight Settings Reset by World");
			}
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
}
