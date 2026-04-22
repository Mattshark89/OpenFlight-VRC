/**
 * @ Maintainer: Happyrobot33
 */

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;

namespace OpenFlightVRC
{
	public enum AvatarListLoaderCallback
	{
		AvatarListLoaded
	}
	/// <summary>
	/// This is used to query the Github data.json file for the list of avatars. It supports falling back to the in-world list if the Github list fails to load.
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class AvatarListLoader : CallbackUdonSharpBehaviour
	{
		public VRCUrl URL = new VRCUrl("https://mattshark89.github.io/OpenFlight-VRC/data.json");
		
		/// <summary>
		/// The time to wait in seconds before starting the download.
		/// </summary>
		public float LoadDelay = 0f;

		/// <summary>
		/// The output of the json file. This is set by the <see cref="LoadURL"/> or <see cref="LoadURLDelayed"/> method, and is done asynchronously, so make sure your script waits for output to be set. See VRCStringDownloader for more information
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


		private void Start()
		{
			SendCustomEventDelayedFrames(nameof(LoadURLDelayed), 1); // prevents a race condition when trying to apply the offline JSON before all scripts could register for the callback
		}

		/// <summary>
		/// Starts the URL loading processes and sets the Output property once done. This is done asynchronously, so make sure your script waits for output to be set. See <see cref="VRCStringDownloader"/> for more information.
		/// </summary>
		public void LoadURL()
		{
			LoadOfflineJson();
			
			if (!useOfflineJSON)
				LoadRemoteString();
		}

		/// <summary>
		/// Starts the URL loading processes after a delay and sets the Output property once done. This is done asynchronously, so make sure your script waits for output to be set. See <see cref="VRCStringDownloader"/> for more information.
		/// </summary>
		public void LoadURLDelayed()
		{
			LoadOfflineJson();
			
			if (!useOfflineJSON)
				SendCustomEventDelayedSeconds(nameof(LoadRemoteString), LoadDelay);
		}
		
		/// <summary>
		/// Applies the in-world JSON to the Output property. And triggers the <see cref="AvatarListLoaderCallback.AvatarListLoaded"/> callback.
		/// </summary>
		public void LoadOfflineJson()
		{
			Output = OfflineJSON.text;
			Logger.Log("Loading in-world JSON list.", this);
			RunCallback(AvatarListLoaderCallback.AvatarListLoaded);
		}
		
        /// <summary>
        /// Starts the actual string loading process. Only used internally.
        /// </summary>
        public void LoadRemoteString()
        {
	        Logger.Log("Loading Avatar List URL...", this);
            VRCStringDownloader.LoadUrl(URL, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
        }

		public override void OnStringLoadSuccess(IVRCStringDownload data)
		{
			string result = data.Result;
			Output = result;
			Logger.Log("Loaded Avatar List URL!", this);
			RunCallback(AvatarListLoaderCallback.AvatarListLoaded);
		}

		// If the URL fails to load, fallback to the in-world stored JSON instead.
		public override void OnStringLoadError(IVRCStringDownload data)
		{
			Logger.Log("Failed to load Avatar List URL! Using in-world JSON instead.", this);
			LoadOfflineJson();
		}
	}

	//array extension to append
	public static class ArrayExtensions
	{
		/// <summary>
		/// Appends an item to an array
		/// </summary>
		/// <typeparam name="T">The type of the array</typeparam>
		/// <param name="array">The array to append to</param>
		/// <param name="item">The item to append</param>
		/// <returns>The new array with the item appended</returns>
		public static T[] Append<T>(this T[] array, T item)
		{
			if (array == null)
			{
				return new T[] { item };
			}

			T[] result = new T[array.Length + 1];
			array.CopyTo(result, 0);
			result[array.Length] = item;
			return result;
		}
	}
}
