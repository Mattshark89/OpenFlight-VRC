/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using System;

namespace OpenFlightVRC
{
    /// <summary>
    /// This base class requirement is really stupid, but it is required for the CallbackUdonSharpBehaviour to work without U# throwing a bug that will likely never be fixed
    /// </summary>
    public abstract class CallbackUdonSharpBehaviourDataStorage : LoggableUdonSharpBehaviour
    {
        /// <summary>
        /// Stores all the data on callbacks
        /// </summary>
        /// <remarks> 
        /// This is a dictionary of all the callbacks that are currently being used. The key is the name of the callback, and the value is a key-value pairs, with the key being the object and the value being the method name
        /// </remarks>
        private protected DataDictionary _callbackData = new DataDictionary();
    }
}
