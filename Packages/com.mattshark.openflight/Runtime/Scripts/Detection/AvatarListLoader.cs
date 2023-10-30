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
			//reset the output
			Output = "";

			if (useOfflineJSON)
			{
				Output = OfflineJSON.text;
                Logger.Log("Force-using in-world JSON list", this);
                RunCallbacks();
				return;
			}
			//load the URL
			VRCStringDownloader.LoadUrl(URL, (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
            Logger.Log("Loading Avatar List URL...", this);
		}

        UdonSharpBehaviour[] callbackBehaviours = new UdonSharpBehaviour[0];
        string[] callbackFuncNames = new string[0];

        internal void AddCallback(UdonSharpBehaviour behaviour, string callback)
        {
            //add the callback to the array
            callbackBehaviours = callbackBehaviours.Append(behaviour);
            callbackFuncNames = callbackFuncNames.Append(callback);
        }

		//if the URL successfully loads
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
