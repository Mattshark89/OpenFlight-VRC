using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using VRC.SDKBase;
using VRC.Udon;

public class OpenFlight : UdonSharpBehaviour
{
	//this removes any override that the editor might have set through the inspector ([HideInInspector] does NOT do that)
	[System.NonSerialized]
	public string OpenFlightVersion = "1.0.0";
	public GameObject wingedFlight;
	public AvatarDetection avatarDetection;
	public string flightMode = "Auto";
	private VRCPlayerApi LocalPlayer;

	[ReadOnly]
	public bool flightAllowed = false;

	public TextAsset packageJson;

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

		//determine the version by grabbing from package.json
		if (packageJson != null)
		{
			string version = packageJson.text;
			version = version.Substring(version.IndexOf("version") + 10); //this makes version look like "1.0.0"
			//remove the first " from the version number
			version = version.Substring(1);
			version = version.Substring(0, version.IndexOf("\"")); //this removes the last " from the version number
			OpenFlightVersion = version;
		}
		else
		{
			OpenFlightVersion = "N/A";
			Debug.LogError("OpenFlight: package.json not found. Version number not set.");
		}
	}

	public void FlightOn()
	{
		if (LocalPlayer.IsUserInVR())
		{
			SwitchFlight();
			wingedFlight.SetActive(true);
			flightMode = "On";
			flightAllowed = true;
		}
	}

	public void FlightOff()
	{
		SwitchFlight();
		wingedFlight.SetActive(false);
		flightMode = "Off";
		flightAllowed = false;
	}

	public void FlightAuto()
	{
		if (LocalPlayer.IsUserInVR())
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
		if (string.Equals(flightMode, "Auto"))
		{
			SwitchFlight();
			wingedFlight.SetActive(true);
			flightAllowed = true;
		}
	}

	public void CannotFly()
	{
		if (string.Equals(flightMode, "Auto"))
		{
			SwitchFlight();
			wingedFlight.SetActive(false);
			flightAllowed = false;
		}
	}
}
