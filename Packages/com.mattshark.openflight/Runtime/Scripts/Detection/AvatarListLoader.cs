using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3;
using VRC.SDK3.StringLoading;
using VRC.Udon;

namespace OpenFlightVRC
{
	public class AvatarListLoader : UdonSharpBehaviour
	{
		public VRCUrl URL = new VRCUrl("https://mattshark89.github.io/OpenFlight-VRC/data.json");

		/// <summary>
		/// The output of the json file. This is set by the <see cref="LoadURL"/> method, and is done asynchronously, so make sure your script waits for output to be set. See VRCStringDownloader for more information
		/// </summary>
		[System.NonSerialized]
		public string Output = "";
		public TextAsset OfflineJSON;

		/// <summary>
		/// If true, the URL will not be loaded, and the in-world JSON will be used instead
		/// </summary>
		public bool useOfflineJSON = false;

		/// <summary>
		/// Initiate a asynchronous URL load
		/// </summary>
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
}
