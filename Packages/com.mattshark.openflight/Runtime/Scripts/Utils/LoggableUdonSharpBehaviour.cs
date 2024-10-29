/**
 * @ Maintainer: Happyrobot33
 */


using UdonSharp;
using UnityEngine;

namespace OpenFlightVRC
{
    /// <summary>
    /// A simple class to derive from that emulates static variables for logging purposes
    /// </summary>
    public abstract class LoggableUdonSharpBehaviour : UdonSharpBehaviour
    {
        /// <summary>
        /// The object to log to. This essentially acts as a static variable in disguise since U# does not support static variables
        /// </summary>
        [HideInInspector]
        public Logger _logProxy;

        // This is setup like this so we can eliminate the need for the this keyword when logging
        /// <inheritdoc cref="Logger.Log(string, LoggableUdonSharpBehaviour)"/>
        internal void Log(string text) => Logger.Log(text, this);

        /// <inheritdoc cref="Logger.LogOnce(string, LoggableUdonSharpBehaviour)"/>
		internal void LogOnce(string text) => Logger.LogOnce(text, this);

        /// <inheritdoc cref="Logger.LogWarning(string, LoggableUdonSharpBehaviour)"/>
		internal void LogWarning(string text) => Logger.LogWarning(text, this);

        /// <inheritdoc cref="Logger.LogWarningOnce(string, LoggableUdonSharpBehaviour)"/>
		internal void LogWarningOnce(string text) => Logger.LogWarningOnce(text, this);

        /// <inheritdoc cref="Logger.LogError(string, LoggableUdonSharpBehaviour)"/>
		internal void LogError(string text) => Logger.LogError(text, this);

        /// <inheritdoc cref="Logger.LogErrorOnce(string, LoggableUdonSharpBehaviour)"/>
		internal void LogErrorOnce(string text) => Logger.LogErrorOnce(text, this);
    }
}
