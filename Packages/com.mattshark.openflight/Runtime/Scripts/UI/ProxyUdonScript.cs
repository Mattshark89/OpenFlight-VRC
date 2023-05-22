using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/*
This entire script is a mess, but this is basically here so all of the scripts that are on the tablet can refer to this instead of the actual scripts.
this allows the tablet to be placed without also needing to place the standaolne script together.
feel free to add events here if you need to call them from the tablet.
*/

namespace OpenFlightVRC.UI
{
	public class ProxyUdonScript : UdonSharpBehaviour
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
				target.SendCustomEvent("FlightOn");
		}

		public void FlightOff()
		{
			if (target != null)
				target.SendCustomEvent("FlightOff");
		}

		public void FlightAuto()
		{
			if (target != null)
				target.SendCustomEvent("FlightAuto");
		}

		public void reloadJSON()
		{
			if (target != null)
				target.SendCustomEvent("reloadJSON");
		}

		public void showGizmo()
		{
			if (target != null)
				target.SendCustomEvent("showGizmo");
		}

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
				target.SendCustomEvent("RestoreDefaults");
		}

		public void InitializeDefaults()
		{
			if (target != null)
				target.SendCustomEvent("InitializeDefaults");
		}
	}
}
