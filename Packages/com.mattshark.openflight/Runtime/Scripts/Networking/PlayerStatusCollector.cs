
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
        public bool readyForUse = false;
        void Start()
        {
            //Logger.Log("PlayerStatusCollector started, ID: " + ID, this);
        }

        void Update()
        {

        }

        public void OnOwnershipTransferred(VRCPlayerApi player)
        {
            Logger.Log("Ownership transferred to " + Networking.GetOwner(gameObject).displayName, this);
            GainOwner(player);
        }

        public void GainOwner(VRCPlayerApi player)
        {
            Logger.Log("Gained owner", this);
            readyForUse = true;
        }

        public void LoseOwner()
        {
            Logger.Log("Lost owner", this);
            readyForUse = false;
        }
    }
}
