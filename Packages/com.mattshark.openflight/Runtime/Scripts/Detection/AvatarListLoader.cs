using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3;
using VRC.SDK3.StringLoading;
using VRC.Udon;
using VRC.SDK3.Data;
using System;

namespace OpenFlightVRC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarListLoader : LoggableUdonSharpBehaviour
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
        UdonSharpBehaviour[] callbackBehaviours = new UdonSharpBehaviour[0];
        /// <summary>
        /// An array of function names to call back to when the URL is loaded
        /// </summary>
        string[] callbackFuncNames = new string[0];

        //TODO: Convert this to a datadictonary instead of two synchronized arrays
        /// <summary>
        /// Adds a callback to the list of callbacks to run when the URL is loaded
        /// </summary>
        /// <param name="behaviour">The behaviour to call back to</param>
        /// <param name="callback">The function name to call back to</param>
        internal void AddCallback(UdonSharpBehaviour behaviour, string callback)
        {
            callbackBehaviours = callbackBehaviours.Append(behaviour);
            callbackFuncNames = callbackFuncNames.Append(callback);
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
            for (int i = 0; i < callbackBehaviours.Length; i++)
            {
                Logger.Log("Triggering callback {" + Logger.ColorizeFunction(callbackBehaviours[i], callbackFuncNames[i]) + "} on [" + Logger.ColorizeScript(callbackBehaviours[i]) + "]", this);
                callbackBehaviours[i].SendCustomEvent(callbackFuncNames[i]);
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
