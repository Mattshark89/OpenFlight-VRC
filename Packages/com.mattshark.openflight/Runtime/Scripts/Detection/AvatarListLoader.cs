/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;

namespace OpenFlightVRC
{
	public enum AvatarListLoaderCallback
	{
		/// <summary>
		/// The avatar list has been loaded and is ready to be ingested by any other scripts that want it
		/// </summary>
		AvatarListReady,
		/// <summary>
		/// The URL has been queried and the que will be open again
		/// </summary>
		URLLoadReturned,
		URLLoadFailed,
		URLLoadSuccess
	}
	/// <summary>
	/// This is used to query the Github data.json file for the list of avatars. It supports falling back to the in-world list if the Github list fails to load.
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class AvatarListLoader : CallbackUdonSharpBehaviour<AvatarListLoaderCallback>
	{
		public override string _logCategory { get => "Avatar DB"; }
		public VRCUrl URL = new VRCUrl("https://mattshark89.github.io/OpenFlight-VRC/data.json");

		/// <summary>
		/// The output of the json file. This is set by the <see cref="LoadURL"/> method, and is done asynchronously, so make sure your script waits for output to be set. See VRCStringDownloader for more information
		/// </summary>
		[System.NonSerialized]
		public string Output = "";
		/// <summary>
		/// The in-world json file. This is used if the URL fails to load
		/// </summary>
		public TextAsset OfflineJSON;

		/// <summary>
		/// If true, the URL will not be loaded, and the in-world JSON will be used instead
		/// </summary>
		public bool useOfflineJSON = false;

		/// <summary>
		/// 	Loads the URL and sets the Output property. This is done asynchronously, so make sure your script waits for output to be set. See VRCStringDownloader for more information
		/// </summary>
		public void LoadURL()
		{

			//initially trigger with the in-world list
			Output = OfflineJSON.text;
			Log(LogLevel.Info, "Using in-world JSON list until remote is available....");
			RunCallback(AvatarListLoaderCallback.AvatarListReady);

			if (useOfflineJSON)
			{
				Log(LogLevel.Info, string.Format("Skipping remote JSON list loading forcibly due to {0}", nameof(useOfflineJSON)));
				return;
			}

			VRCStringDownloader.LoadUrl(URL, (IUdonEventReceiver)this);
		}

		public override void OnStringLoadSuccess(IVRCStringDownload data)
		{
			string result = data.Result;
			Output = result;
			Log(LogLevel.Info, "Loaded Avatar List URL!");
			RunCallback(AvatarListLoaderCallback.AvatarListReady);
			RunCallback(AvatarListLoaderCallback.URLLoadReturned);
			RunCallback(AvatarListLoaderCallback.URLLoadSuccess);
		}

		//if the URL fails to load, fallback to the in-world stored JSON instead
		public override void OnStringLoadError(IVRCStringDownload data)
		{
			Output = OfflineJSON.text;
			Log(LogLevel.Warning, "Failed to load Avatar List URL! Using in-world JSON instead.");
			RunCallback(AvatarListLoaderCallback.AvatarListReady);
			RunCallback(AvatarListLoaderCallback.URLLoadReturned);
			RunCallback(AvatarListLoaderCallback.URLLoadFailed);
		}
	}
}
