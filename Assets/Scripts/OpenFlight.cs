
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class OpenFlight : UdonSharpBehaviour {
	[HideInInspector]
	public bool frozen = false;
	public GameObject wingedFlight;
	
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
		}
	}

	public void EnableAutomaticMode() {
		frozen = false;
	}
	
	public void EnableWingedFlight(bool freeze = false) {
		if ((!frozen) || freeze) {
			SwitchFlight();
			wingedFlight.SetActive(true);
		}
		if (freeze) {
			frozen = true;
		}
	}
}
