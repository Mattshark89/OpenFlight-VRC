using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using VRC.SDKBase;
using VRC.Udon;

//This chunk of code allows the OpenFlight version number to be set automatically from the package.json file
//its done using this method for dumb unity reasons but it works so whatever
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

public class OpenFlightScenePostProcessor {
	[PostProcessSceneAttribute]
	public static void OnPostProcessScene() {
		//get the openflight version from the package.json file
		string packageJson = System.IO.File.ReadAllText("Packages/com.mattshark.openflight/package.json");
		string version = packageJson.Split(new string[] { "\"version\": \"" }, System.StringSplitOptions.None)[1].Split('"')[0];
		//find all the OpenFlight scripts in the scene
		OpenFlight[] openFlightScripts = Object.FindObjectsOfType<OpenFlight>();
		foreach (OpenFlight openFlightScript in openFlightScripts)
		{
			//set their version number
			openFlightScript.OpenFlightVersion = version;
		}
	}
}
#endif

public class OpenFlight : UdonSharpBehaviour
{
	//this removes any override that the editor might have set through the inspector ([HideInInspector] does NOT do that)
	[System.NonSerialized]
	public string OpenFlightVersion = "?.?.?";
	public GameObject wingedFlight;
	public AvatarDetection avatarDetection;
	public string flightMode = "Auto";
	string flightModePrevious = "Auto";
	public string zoneNotifierInfo = "";
	private VRCPlayerApi LocalPlayer;

	[ReadOnly]
	public bool flightAllowed = false;
	public bool flightForcedOff = false; //used for external scripts to force flight off no matter what

	void SwitchFlight()
	{
		wingedFlight.SetActive(false);
		flightAllowed = false;
	}

	public void Start()
	{
		LocalPlayer = Networking.LocalPlayer;
		if (!LocalPlayer.IsUserInVR())
		{
			FlightOff();
		}
	}

	public void FlightOn()
	{
		if (LocalPlayer.IsUserInVR() && !flightForcedOff)
		{
			SwitchFlight();
			wingedFlight.SetActive(true);
			flightMode = "On";
			flightAllowed = true;
		}
	}

	public void FlightOff()
	{
		if (!flightForcedOff)
		{
			SwitchFlight();
			wingedFlight.SetActive(false);
			flightMode = "Off";
			flightAllowed = false;
		}
	}

	public void FlightAuto()
	{
		if (LocalPlayer.IsUserInVR() && !flightForcedOff)
		{
			flightMode = "Auto";
			flightAllowed = false;

			//tell the avatar detection script to check if the player can fly again
			if (avatarDetection != null)
			{
				avatarDetection.ReevaluateFlight();
			}
		}
	}

	public void CanFly()
	{
		if (string.Equals(flightMode, "Auto") && !flightForcedOff)
		{
			SwitchFlight();
			wingedFlight.SetActive(true);
			flightAllowed = true;
		}
	}

	public void CannotFly()
	{
		if (string.Equals(flightMode, "Auto") && !flightForcedOff)
		{
			SwitchFlight();
			wingedFlight.SetActive(false);
			flightAllowed = false;
		}
	}

	//These are used by scripts that need to force flight on or off no matter what the player wants
	public void ForceDisableFlight()
	{
		SwitchFlight();
		wingedFlight.SetActive(false);
		flightModePrevious = flightMode;
		flightMode = "Forced Off";
		zoneNotifierInfo = "Flight Disabled by World";
		flightAllowed = false;
		flightForcedOff = true;
	}

	public void ResetForcedFlight()
	{
		flightForcedOff = false;
		flightMode = flightModePrevious;
		zoneNotifierInfo = "Flight Returned to Previous State";
		if (string.Equals(flightMode, "Auto"))
		{
			FlightAuto();
		}

		if (string.Equals(flightMode, "On"))
		{
			FlightOn();
		}

		if (string.Equals(flightMode, "Off"))
		{
			FlightOff();
		}
	}
}
