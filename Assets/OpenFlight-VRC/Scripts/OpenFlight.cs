
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class OpenFlight : UdonSharpBehaviour {
	public string OpenFlightVersion = "1.0.0";
	public GameObject wingedFlight;
	public string flightMode = "Auto";
	
	void SwitchFlight() {
		wingedFlight.SetActive(false);
	}

	public void FlightOn() {
		SwitchFlight();
		wingedFlight.SetActive(true);
        flightMode = "On";
	}

	public void FlightOff() {
		SwitchFlight();
		wingedFlight.SetActive(false);
        flightMode = "Off";
	}

    public void FlightAuto() {
		flightMode = "Auto";
	}

	public void CanFly() {
		if (string.Equals(flightMode, "Auto")) {
			SwitchFlight();
			wingedFlight.SetActive(true);
		}
	}

	public void CannotFly() {
		if (string.Equals(flightMode, "Auto")) {
			SwitchFlight();
			wingedFlight.SetActive(false);
		}
	}
}
