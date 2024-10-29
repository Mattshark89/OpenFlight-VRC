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

        /// <inheritdoc cref="Logger.Warning(string, LoggableUdonSharpBehaviour)"/>
		internal void Warning(string text) => Logger.Warning(text, this);

        /// <inheritdoc cref="Logger.WarningOnce(string, LoggableUdonSharpBehaviour)"/>
		internal void WarningOnce(string text) => Logger.WarningOnce(text, this);

        /// <inheritdoc cref="Logger.Error(string, LoggableUdonSharpBehaviour)"/>
		internal void Error(string text) => Logger.Error(text, this);

        /// <inheritdoc cref="Logger.ErrorOnce(string, LoggableUdonSharpBehaviour)"/>
		internal void ErrorOnce(string text) => Logger.ErrorOnce(text, this);
    }
}
