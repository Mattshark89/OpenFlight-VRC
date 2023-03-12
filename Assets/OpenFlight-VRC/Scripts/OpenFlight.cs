
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using VRC.SDKBase;
using VRC.Udon;

public class OpenFlight : UdonSharpBehaviour {
	public string OpenFlightVersion = "1.0.0";
	public GameObject wingedFlight;
	public string flightMode = "Auto";
	
	[ReadOnly] public bool flightAllowed = false;
	
	void SwitchFlight() {
		wingedFlight.SetActive(false);
		flightAllowed = false;
	}

	public void FlightOn() {
		SwitchFlight();
		wingedFlight.SetActive(true);
        flightMode = "On";
		flightAllowed = true;
	}

	public void FlightOff() {
		SwitchFlight();
		wingedFlight.SetActive(false);
        flightMode = "Off";
		flightAllowed = false;
	}

    public void FlightAuto() {
		flightMode = "Auto";
		flightAllowed = false;
	}

	public void CanFly() {
		if (string.Equals(flightMode, "Auto")) {
			SwitchFlight();
			wingedFlight.SetActive(true);
			flightAllowed = true;
		}
	}

	public void CannotFly() {
		if (string.Equals(flightMode, "Auto")) {
			SwitchFlight();
			wingedFlight.SetActive(false);
			flightAllowed = false;
		}
	}
}
