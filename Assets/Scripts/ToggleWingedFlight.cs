
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleWingedFlight : UdonSharpBehaviour {
	// Settings:
	// 0- Flight Disabled
	// 1- Flight Enabled
	// 2- Flight Automatic
    public int setting = 2;
	public OpenFlight openFlight;
	public GameObject wingedFlight;
	
	void Start() {
		this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0.75f));
	}
	
	void Interact() {
		setting++;
		if (setting > 2) {setting = 0;}
		// Yes I can combine these. No I will not. Readability my dudes
        if (setting == 0) {
			// Attempt to disable flight
			openFlight.DisableFlight(true);
			this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0f));
		} else if (setting == 1) {
			// Attempt to enable winged flight
			openFlight.EnableWingedFlight(true);
			this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0.5f));
		} else if (setting == 2) {
			openFlight.EnableAutomaticMode();
			this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0.75f));
		}
    }

}
