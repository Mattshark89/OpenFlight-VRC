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
    /// A Base class that allows for callbacks to be added and run
    /// </summary>
    public class CallbackUdonSharpBehaviour : LoggableUdonSharpBehaviour
    {
        /// <summary>
        /// Stores all the data on callbacks
        /// </summary>
        /// <remarks> 
        /// This is a dictionary of all the callbacks that are currently being used. The key is the name of the callback, and the value is a key-value pairs, with the key being the object and the value being the method name
        /// </remarks>
        private DataDictionary _callbackData = new DataDictionary();

        /// <summary>
        /// Adds a callback based on the callback ID
        /// </summary>
        /// <param name="callbackID">The ID of the callback to add. This should be an enum</param>
        /// <param name="behaviour">The behaviour to run the callback on. Use <see cref="this"/> to run the callback on the behaviour this script is attached to</param>
        /// <param name="methodName">The name of the method to run when this callback is triggered. Recommended to use <see cref="nameof"/> to get the method name</param>
        /// <returns>Returns a true if the callback was added successfully, and false if the callback already exists</returns>
        public bool AddCallback(Enum callbackID, UdonSharpBehaviour behaviour, string methodName)
        {
            int id = Convert.ToInt32(callbackID);

            //add the id dictionary, only making it if it doesn't exist
            if (!_callbackData.ContainsKey(id))
                _callbackData.Add(id, new DataDictionary());

            //add the behaviour key, only making it if it doesn't exist
            if (!_callbackData[id].DataDictionary.ContainsKey(behaviour))
                _callbackData[id].DataDictionary.Add(behaviour, new DataList());

            //add the method name, only if its not already there
            if (!_callbackData[id].DataDictionary[behaviour].DataList.Contains(methodName))
            {
                //Logger.Log(String.Format("Added callback [{0}] for [{1}]", Logger.ColorizeFunction(behaviour, methodName), Logger.ColorizeFunction(behaviour, methodName)), this);
                _callbackData[id].DataDictionary[behaviour].DataList.Add(methodName);
            }
            else
            {
                Logger.LogWarning(String.Format("[{0}] Tried to register a callback for [{1}] that already exists!", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodName)), this);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Removes a callback based on the callback ID
        /// </summary>
        /// <inheritdoc cref="AddCallback(Enum, UdonSharpBehaviour, string)"/>
        /// <returns>Returns a true if the callback was removed successfully, and false if the callback does not exist</returns>
        public bool RemoveCallback(Enum callbackID, UdonSharpBehaviour behaviour, string methodName)
        {
            int id = Convert.ToInt32(callbackID);
            //check if the callback exists
            if (_callbackData.ContainsKey(id))
            {
                DataDictionary data = _callbackData[id].DataDictionary;

                //check if the behaviour exists
                if (data.ContainsKey(behaviour))
                {
                    DataList methods = data[behaviour].DataList;

                    //check if the method exists
                    if (methods.Contains(methodName))
                    {
                        methods.Remove(methodName);
                        return true;
                    }
                }
            }
            Logger.LogWarning(String.Format("[{0}] Tried to remove a callback for [{1}] that does not exist!", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodName)), this);
            return false;
        }

        /// <summary>
        /// Runs all the callbacks for the given callback ID
        /// </summary>
        /// <param name="callbackID"><inheritdoc cref="AddCallback(Enum, UdonSharpBehaviour, string)" path="/param[@name='callbackID']"/></param>
        internal void RunCallback(Enum callbackID)
        {
            int id = Convert.ToInt32(callbackID);
            //check if the callback exists
            if (_callbackData.ContainsKey(id))
            {
                DataDictionary data = _callbackData[id].DataDictionary;

                DataList keys = data.GetKeys();

                //convert to an array of tokens
                DataToken[] tokens = keys.ToArray();

                //run all the callbacks
                for (int i = 0; i < tokens.Length; i++)
                {
                    //get the behaviour
                    UdonSharpBehaviour behaviour = (UdonSharpBehaviour)tokens[i].Reference;

                    //get the method name list
                    DataList methods = data[tokens[i]].DataList;
                    for (int j = 0; j < methods.Count; j++)
                    {
                        string methodName = methods[j].String;

                        //run the method
                        behaviour.SendCustomEvent(methodName);
                        Logger.Log(String.Format("Running callback [{0}] for [{1}]", Logger.ColorizeFunction(behaviour, methodName), Logger.ColorizeScript(behaviour)), this);
                    }
                }
            }
        }
    }
}
