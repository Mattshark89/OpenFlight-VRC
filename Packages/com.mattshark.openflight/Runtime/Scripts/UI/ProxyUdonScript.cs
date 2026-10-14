/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// This entire script is a mess, but this is basically here so all of the scripts that are on the tablet can refer to this instead of the actual scripts.
	/// this allows the tablet to be placed without also needing to place the standalone script together.
	/// feel free to add events here if you need to call them from the tablet.
	/// </summary>
	/// <remarks>
	/// We HIGHLY recommend you do not use these methods as a API for your own scripts. This is NOT garunteed to be stable
	/// </remarks>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class ProxyUdonScript : LoggableUdonSharpBehaviour
	{
		/// <summary>
		/// The UdonBehaviour to proxy
		/// </summary>
		public UdonBehaviour target;
		public GameObject targetGameObject;

		public void Start()
		{
			//try to initialize defaults
			InitializeDefaults();
		}

		public void FlightOn()
		{
			if (target != null)
				target.SendCustomEvent(nameof(OpenFlightVRC.OpenFlight.FlightOn));
		}

		public void FlightOff()
		{
			if (target != null)
				target.SendCustomEvent(nameof(OpenFlightVRC.OpenFlight.FlightOff));
		}

		public void FlightAuto()
		{
			if (target != null)
				target.SendCustomEvent(nameof(OpenFlightVRC.OpenFlight.FlightAuto));
		}

		public void reloadJSON()
		{
			if (target != null)
				target.SendCustomEvent(nameof(OpenFlightVRC.AvatarDetection.reloadJSON));
		}

		// TODO: I dont think this is required anymore?
		public void showGizmo()
		{
			if (target != null)
				target.SendCustomEvent("showGizmo");
		}

		// TODO: I dont think this is required anymore?
		public void hideGizmo()
		{
			if (target != null)
				target.SendCustomEvent("hideGizmo");
		}

		public void OnDisable()
		{
			if (targetGameObject != null)
				targetGameObject.SetActive(false);
		}

		public void OnEnable()
		{
			if (targetGameObject != null)
				targetGameObject.SetActive(true);
		}

		public void RestoreDefaults()
		{
			if (target != null)
				target.SendCustomEvent(nameof(OpenFlightVRC.FlightProperties.RestoreDefaults));
		}

		public void InitializeDefaults()
		{
			if (target != null)
				target.SendCustomEvent(nameof(OpenFlightVRC.FlightProperties.InitializeDefaults));
		}
	}
}
