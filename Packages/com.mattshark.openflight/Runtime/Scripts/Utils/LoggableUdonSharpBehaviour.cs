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
    public class LoggableUdonSharpBehaviour : UdonSharpBehaviour
    {
        /// <summary>
        /// The object to log to. This essentially acts as a static variable in disguise since U# does not support static variables
        /// </summary>
        [HideInInspector]
        public UI.LoggerProxy _logProxy;
    }
}
