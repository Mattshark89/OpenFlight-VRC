
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;
using OpenFlightVRC.UI;
using OpenFlightVRC.Effects;

// This script is used to initialize the local player's store so it has the correct references
namespace OpenFlightVRC.Net
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PoolController : UdonSharpBehaviour
    {
        public CyanPlayerObjectAssigner Assigner;
        public AvatarDetection avatarDetection;
        public WingFlightPlusGlide wingFlightPlusGlide;
        public OpenFlight openFlight;
        public ContributerDetection contributerDetection;

        public void _OnLocalPlayerAssigned()
        {
            //get the local player's store
            Component behaviour = Assigner._GetPlayerPooledUdon(Networking.LocalPlayer);

            PlayerInfoStore store = (PlayerInfoStore)behaviour;

            //set the values
            store.avatarDetection = avatarDetection;
            store.wingFlightPlusGlide = wingFlightPlusGlide;
            store.openFlight = openFlight;
            store.contributerDetection = contributerDetection;
        }
    }
}
