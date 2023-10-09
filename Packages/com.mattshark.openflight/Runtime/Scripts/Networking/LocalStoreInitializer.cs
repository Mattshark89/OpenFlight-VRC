
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;
using OpenFlightVRC.UI;

// This script is used to initialize the local player's store so it has the correct references
namespace OpenFlightVRC.Net
{
    public class LocalStoreInitializer : UdonSharpBehaviour
    {
        public CyanPlayerObjectAssigner Assigner;
        public AvatarDetection _avatarDetection;
        public WingFlightPlusGlide _wingFlightPlusGlide;
        public OpenFlight _openFlight;
        public ContributerDetection _contributerDetection;
        public void _OnLocalPlayerAssigned()
        {
            //get the local player's store
            Component behaviour = Assigner._GetPlayerPooledUdon(Networking.LocalPlayer);

            PlayerInfoStore store = (PlayerInfoStore)behaviour;

            //set the values
            store._avatarDetection = _avatarDetection;
            store._wingFlightPlusGlide = _wingFlightPlusGlide;
            store._openFlight = _openFlight;
            store._contributerDetection = _contributerDetection;
        }
    }
}
