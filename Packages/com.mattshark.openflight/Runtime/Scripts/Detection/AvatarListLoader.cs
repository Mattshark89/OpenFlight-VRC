using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3;
using VRC.SDK3.StringLoading;
using VRC.Udon;

public class AvatarListLoader : UdonSharpBehaviour
{
	public VRCUrl URL = new VRCUrl("https://mattshark89.github.io/OpenFlight-VRC/Assets/OpenFlight-VRC/data.json");
	
	[System.NonSerialized]
	public string Output = "";
	public TextAsset OfflineJSON;
	public bool useOfflineJSON = false;

	//Event used to initiate a URL load
	public void LoadURL()
	{
		//reset the output
		Output = "";

		if (useOfflineJSON)
		{
			Output = OfflineJSON.text;
			Debug.Log("Using in-world JSON instead.");
			return;
		}
		//load the URL
		VRCStringDownloader.LoadUrl(URL, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
		Debug.Log("Loading URL...");
	}

	//if the URL successfully loads
	public override void OnStringLoadSuccess(IVRCStringDownload data)
	{
		string result = data.Result;
		Output = result;
		Debug.Log("Loaded URL successfully!");
	}

	//if the URL fails to load, fallback to the in-world stored JSON instead
	public override void OnStringLoadError(IVRCStringDownload data)
	{
		Output = OfflineJSON.text;
		Debug.Log("Failed to load URL! Using in-world JSON instead.");
	}
}
