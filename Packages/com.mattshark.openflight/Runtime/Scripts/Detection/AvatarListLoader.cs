/**
 * @ Maintainer: Happyrobot33
 */

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
			Output = "";

			if (useOfflineJSON)
			{
				Output = OfflineJSON.text;
				Logger.Log("Force-using in-world JSON list", this);
				RunCallback(AvatarListLoaderCallback.AvatarListLoaded);
				return;
			}

			//initially trigger with the in-world list
			Output = OfflineJSON.text;
			Logger.Log("Using in-world JSON list until remote is available....", this);
			RunCallback(AvatarListLoaderCallback.AvatarListLoaded);

			VRCStringDownloader.LoadUrl(URL, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
		}

		public override void OnStringLoadSuccess(IVRCStringDownload data)
		{
			string result = data.Result;
			Output = result;
			Logger.Log("Loaded Avatar List URL!", this);
			RunCallback(AvatarListLoaderCallback.AvatarListLoaded);
		}

		//if the URL fails to load, fallback to the in-world stored JSON instead
		public override void OnStringLoadError(IVRCStringDownload data)
		{
			Output = OfflineJSON.text;
			Logger.Log("Failed to load Avatar List URL! Using in-world JSON instead.", this);
			RunCallback(AvatarListLoaderCallback.AvatarListLoaded);
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
