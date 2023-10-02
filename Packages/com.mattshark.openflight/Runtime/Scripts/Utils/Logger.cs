using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace OpenFlightVRC
{
    /// <summary>
    /// A simple logger that prefixes all messages with [OpenFlight]
    /// </summary>
	public class Logger : UdonSharpBehaviour
	{
		void Start()
		{
			Log("Logging started");
		}

        const string color = "orange";
		const string prefix = "[" + "<color=" + color + ">" + "OpenFlight" + "</color>" + "] ";

        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="text">The text to print to the console</param>
		internal static void Log(string text)
		{
            Debug.Log(prefix + text);
		}

        /// <summary>
        /// Logs a warning to the console
        /// </summary>
        /// <param name="text">The text to print to the console</param>
        internal static void LogWarning(string text)
        {
            Debug.LogWarning(prefix + text);
        }

        /// <summary>
        /// Logs an error to the console
        /// </summary>
        /// <param name="text">The text to print to the console</param>
        internal static void LogError(string text)
        {
            Debug.LogError(prefix + text);
        }
	}
}
