using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

//The purpose of this script is to display a message to the player when they enter a control zone of some sort,
//such as a no fly zone

public class ZoneNotifier : UdonSharpBehaviour
{
	OpenFlight openFlight;
	VRCPlayerApi localPlayer = null;
	TextMeshProUGUI zoneNotifierText;
	public GameObject zoneNotifierTextObject;

	void Start()
	{
		//finds the OpenFlight script in the scene
		openFlight = GameObject.Find("OpenFlight").GetComponent<OpenFlight>();

		//finds the local player
		localPlayer = Networking.LocalPlayer;

		//finds the zone notifier text object
		zoneNotifierText = zoneNotifierTextObject.GetComponent<TextMeshProUGUI>();

		//turns off the zone notifier text object
		zoneNotifierTextObject.SetActive(false);
	}

	void Update()
	{
		if (openFlight.zoneNotifierInfo != "")
		{
			zoneNotifierText.text = openFlight.zoneNotifierInfo;
			openFlight.zoneNotifierInfo = "";

			//Turn the text object off and on again to make it update
			zoneNotifierTextObject.SetActive(false);
			zoneNotifierTextObject.SetActive(true);
		}
	}
}
