
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/*
This script collects information from openflight systems on the local machine, and makes them available to other players via the network
Every player will have one of these scripts owned by them
*/
namespace OpenFlightVRC.Net
{
    public class PlayerStatusCollector : UdonSharpBehaviour
    {
        /// <summary> The ID of the player that owns this script. This gets set statically when the world is built </summary>
        public int ID;
        void Start()
        {
            Logger.Log("PlayerStatusCollector started, ID: " + ID, this);
        }
    }
}
