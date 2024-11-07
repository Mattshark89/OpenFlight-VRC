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
        /// What category should this script log under. Sample override looks like this <code>public override string _logCategory { get => "MyCategory"; }</code>
        /// </summary>
        public abstract string _logCategory { get; } //This annoyingly has to be public, as internal doesnt seem to compile properly and will just lead to seeing this value as null even if you override it

        /// <summary>
        /// The object to log to. This essentially acts as a static variable in disguise since U# does not support static variables
        /// </summary>
        internal Logger _logProxy;

        // This is setup like this so we can eliminate the need for the this keyword when logging
        /// <inheritdoc cref="Logger.Log(LogLevel, string, bool, LoggableUdonSharpBehaviour)"/>
        internal void Log(LogLevel level, string text, bool once = false) => Logger.Log(level, text, once, this);
    }
}
