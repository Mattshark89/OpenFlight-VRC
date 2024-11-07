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
    /// <typeparam name="EnumType">The type of the callback ID. This should be an enum</typeparam>
    public abstract class CallbackUdonSharpBehaviour<EnumType> : CallbackUdonSharpBehaviourDataStorage where EnumType : Enum
    {
        //FOREWARNING: There is some stupid as fuck behaviour going on in here.
        //Normally, you would want to put any variables you are using in the same class file you are using them in
        //BUT, due to a U# bug related to generic classes, I need to put them in a inherited class that doesnt spawn multiple versions
        //of itself. If they were to be put in this level, U# would complain about symbols being duplicated due to the generic class being expanded

        //TLDR, any variables to inject ***CANNOT*** be defined in here, and instead need to be defined in the parent class

        /// <summary>
        /// Adds a callback based on the callback ID
        /// </summary>
        /// <param name="callbackID">The ID of the callback to add. This should be an enum</param>
        /// <param name="behaviour">The behaviour to run the callback on. Use <see cref="this"/> to run the callback on the behaviour this script is attached to</param>
        /// <param name="methodNames">The name of the method to run when this callback is triggered. Recommended to use <see cref="nameof"/> to get the method name</param>
        /// <returns>Returns a true if the callback was added successfully, and false if the callback already exists</returns>
        public bool AddCallback(EnumType callbackID, UdonSharpBehaviour behaviour, params string[] methodNames)
        {
            bool success = true;
            int id;

            for (int i = 0; i < methodNames.Length; i++)
            {
                id = Convert.ToInt32(callbackID);

                //add the id dictionary, only making it if it doesn't exist
                if (!_callbackData.ContainsKey(id))
                {
                    _callbackData.Add(id, new DataDictionary());
                }

                //add the behaviour key, only making it if it doesn't exist
                if (!_callbackData[id].DataDictionary.ContainsKey(behaviour))
                {
                    _callbackData[id].DataDictionary.Add(behaviour, new DataList());
                }

                //add the method name, only if its not already there
                if (!_callbackData[id].DataDictionary[behaviour].DataList.Contains(methodNames[i]))
                {
                    Log(LogLevel.Callback, string.Format("Subscribed [{0}.{1}] to Callback [{2}]", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodNames[i]), Logger.ColorizeFunction(this, callbackID.ToString())));
                    _callbackData[id].DataDictionary[behaviour].DataList.Add(methodNames[i]);
                }
                else
                {
                    Log(LogLevel.Warning, string.Format("Tried to register [{0}.{1}] to [{2}] when it was already registered!", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodNames[i]), Logger.ColorizeFunction(this, callbackID.ToString())));
                    return false;
                }
                success &= true;
            }

            return success;
        }

        /// <summary>
        /// Removes a callback based on the callback ID
        /// </summary>
        /// <inheritdoc cref="AddCallback(Enum, UdonSharpBehaviour, string)"/>
        /// <returns>Returns a true if the callback was removed successfully, and false if the callback does not exist</returns>
        public bool RemoveCallback(EnumType callbackID, UdonSharpBehaviour behaviour, string methodName)
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
                        Log(LogLevel.Callback, string.Format("Removed [{0}.{1}] from Callback [{2}] of [{3}]", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodName), Logger.ColorizeFunction(this, callbackID.ToString()), Logger.ColorizeScript(this)));
                        return true;
                    }
                }
            }
            Log(LogLevel.Warning, string.Format("Tried to remove [{0}.{1}] from [{2}] when it didnt exist!", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodName), Logger.ColorizeFunction(this, callbackID.ToString())));
            return false;
        }

        /// <summary>
        /// Runs all the callbacks for the given callback ID
        /// </summary>
        /// <param name="callbackID"><inheritdoc cref="AddCallback(Enum, UdonSharpBehaviour, string)" path="/param[@name='callbackID']"/></param>
        internal void RunCallback(EnumType callbackID)
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

                        //check if the behaviour is null, and if it is, remove it
                        if (behaviour == null)
                        {
                            Log(LogLevel.Warning, string.Format("Behaviour for callback [{0}] is null, removing callback", Logger.ColorizeFunction(behaviour, methodName)));
                            methods.Remove(methodName);
                            continue;
                        }

                        //run the method
                        behaviour.SendCustomEvent(methodName);
                        Log(LogLevel.Callback, string.Format("Running [{0}.{1}] for callback [{2}]", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodName), Logger.ColorizeFunction(this, callbackID.ToString())));
                    }
                }
            }
        }
    }
}
