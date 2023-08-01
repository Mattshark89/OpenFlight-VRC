using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC
{
    public static class Logger
    {
        /// <summary> Logs a message to the console. </summary>
        /// <param name="message"> The message to log. </param>
        public static void Log(string message)
        {
            Debug.Log("[OpenFlight] " + message);
        }
    }
}
