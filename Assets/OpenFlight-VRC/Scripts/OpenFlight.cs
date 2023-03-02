
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class OpenFlight : UdonSharpBehaviour {
	public string OpenFlightVersion = "1.0.0";
	[HideInInspector]
	public bool frozen = false;
	public GameObject wingedFlight;
	public string flightMode = "Auto";
	
	void SwitchFlight() {
		if (wingedFlight != null) {
			wingedFlight.SetActive(false);
		}
	}
	
	public void DisableFlight(bool freeze = false) {
		if ((!frozen) || freeze) {
			SwitchFlight();
		}
		if (freeze) {
			frozen = true;
			flightMode = "Off";
		}
	}

	public void EnableAutomaticMode() {
		frozen = false;
		flightMode = "Auto";
	}
	
	public void EnableWingedFlight(bool freeze = false) {
		if ((!frozen) || freeze) {
			SwitchFlight();
			wingedFlight.SetActive(true);
		}
		if (freeze) {
			frozen = true;
			flightMode = "On";
		}
	}

	//these are needed since I cant pass a bool to the functions using a UI button
	public void ForceWingedFlight() {
		EnableWingedFlight(true);
	}

	public void ForceDisableFlight() {
		DisableFlight(true);
	}
}
