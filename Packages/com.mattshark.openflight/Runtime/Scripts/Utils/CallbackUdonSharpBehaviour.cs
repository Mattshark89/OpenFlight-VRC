/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using System;
using System.Linq;

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

            DataList subscribedMethods = new DataList();
            DataList failedMethods = new DataList();

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
                    subscribedMethods.Add(string.Format("{0}.{1}", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodNames[i])));
                    _callbackData[id].DataDictionary[behaviour].DataList.Add(methodNames[i]);
                }
                else
                {
                    failedMethods.Add(string.Format("{0}.{1}", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodNames[i])));
                    success = false;
                }
            }

            if (subscribedMethods.Count > 0) Log(LogLevel.Callback, string.Format("Subscribed [{0}] to Callback [{1}]", subscribedMethods.ToArray<string>().Join(", "), Logger.ColorizeFunction(this, callbackID.ToString())));
            if (failedMethods.Count > 0) Log(LogLevel.Warning, string.Format("Failed to subscribe [{0}] to Callback [{1}] as they are already registered!", failedMethods.ToArray<string>().Join(", "), Logger.ColorizeFunction(this, callbackID.ToString())));

            return success;
        }

        /// <summary>
        /// Removes a callback based on the callback ID
        /// </summary>
        /// <inheritdoc cref="AddCallback(Enum, UdonSharpBehaviour, string)"/>
        /// <returns>Returns a true if the callback was removed successfully, and false if the callback does not exist</returns>
        public bool RemoveCallback(EnumType callbackID, UdonSharpBehaviour behaviour, string methodName)
        {
            //TODO: Parameterize this one like all the others
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

                DataList calledMethods = new DataList();
                DataList failedMethods = new DataList();

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
                            failedMethods.Add(string.Format("{0}.{1}", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodName)));
                            methods.Remove(methodName);
                        }
                        else
                        {
                            //run the method
                            behaviour.SendCustomEvent(methodName);
                            calledMethods.Add(string.Format("{0}.{1}", Logger.ColorizeScript(behaviour), Logger.ColorizeFunction(behaviour, methodName)));
                        }
                    }
                }

                if (calledMethods.Count > 0) Log(LogLevel.Callback, string.Format("Running [{0}] for Callback [{1}]", calledMethods.ToArray<string>().Join(", "), Logger.ColorizeFunction(this, callbackID.ToString())));
                if (failedMethods.Count > 0) Log(LogLevel.Warning, string.Format("Failed to run [{0}] for Callback [{1}] as they were null and have been removed", failedMethods.ToArray<string>().Join(", "), Logger.ColorizeFunction(this, callbackID.ToString())));
            }
        }
    }
}
