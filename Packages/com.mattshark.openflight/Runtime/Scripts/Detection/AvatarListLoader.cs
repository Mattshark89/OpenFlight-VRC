/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;

namespace OpenFlightVRC
{
	/// <summary>
	/// This is used to query the Github data.json file for the list of avatars. It supports falling back to the in-world list if the Github list fails to load.
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class AvatarListLoader : LoggableUdonSharpBehaviour
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
				RunCallbacks();
				return;
			}
			Logger.Log("Loading Avatar List URL...", this);
			VRCStringDownloader.LoadUrl(URL, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
		}

		/// <summary>
		/// An array of behaviours to call back to when the URL is loaded
		/// </summary>
		private UdonSharpBehaviour[] _callbackBehaviours = new UdonSharpBehaviour[0];

		/// <summary>
		/// An array of function names to call back to when the URL is loaded
		/// </summary>
		private string[] _callbackFuncNames = new string[0];

		//TODO: Convert this to a datadictonary instead of two synchronized arrays
		/// <summary>
		/// Adds a callback to the list of callbacks to run when the URL is loaded
		/// </summary>
		/// <param name="behaviour">The behaviour to call back to</param>
		/// <param name="callback">The function name to call back to</param>
		internal void AddCallback(UdonSharpBehaviour behaviour, string callback)
		{
			_callbackBehaviours = _callbackBehaviours.Append(behaviour);
			_callbackFuncNames = _callbackFuncNames.Append(callback);
		}

		public override void OnStringLoadSuccess(IVRCStringDownload data)
		{
			string result = data.Result;
			Output = result;
			Logger.Log("Loaded Avatar List URL!", this);
			RunCallbacks();
		}

		/// <summary>
		/// Runs all callbacks
		/// </summary>
		private void RunCallbacks()
		{
			for (int i = 0; i < _callbackBehaviours.Length; i++)
			{
				Logger.Log(
					"Triggering callback {"
						+ Logger.ColorizeFunction(_callbackBehaviours[i], _callbackFuncNames[i])
						+ "} on ["
						+ Logger.ColorizeScript(_callbackBehaviours[i])
						+ "]",
					this
				);
				_callbackBehaviours[i].SendCustomEvent(_callbackFuncNames[i]);
			}
		}

		//if the URL fails to load, fallback to the in-world stored JSON instead
		public override void OnStringLoadError(IVRCStringDownload data)
		{
			Output = OfflineJSON.text;
			Logger.Log("Failed to load Avatar List URL! Using in-world JSON instead.", this);
			RunCallbacks();
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
