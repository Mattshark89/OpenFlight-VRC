
using OpenFlightVRC.Effects;
using OpenFlightVRC.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Net
{
    public class PlayerInfoStore : UdonSharpBehaviour
    {
        /// <summary> Current player on this object, null if none </summary>
        public VRCPlayerApi Owner;

        [UdonSynced]
        public bool isFlying = false;
        [UdonSynced]
        public bool isGliding = false;
        [UdonSynced]
        public bool isFlapping = false;
        [UdonSynced]
        public string flightMode = "Auto";
        [UdonSynced]
        public bool isContributer = false;

        internal AvatarDetection avatarDetection;
        internal WingFlightPlusGlide wingFlightPlusGlide;
        internal OpenFlight openFlight;
        internal ContributerDetection contributerDetection;
        public EffectsHandler effectsHandler;

        void Start()
        {

        }

        void Update()
        {
            //check to make sure both scripts are available. If they arent, return
            if (avatarDetection == null || wingFlightPlusGlide == null || openFlight == null)
            {
                return;
            }

            //if the local player owns this object, update the values
            if (Networking.LocalPlayer == Owner)
            {
                isFlying = wingFlightPlusGlide.isFlying;
                isGliding = wingFlightPlusGlide.isGliding;
                isFlapping = wingFlightPlusGlide.isFlapping;
                flightMode = openFlight.flightMode;
                isContributer = contributerDetection.localPlayerIsContributer;
            }
        }

        public void _OnOwnerSet()
        {
            Logger.Log("Owner set to " + Owner.displayName, this);
            effectsHandler.OwnerChanged();
        }

        public void _OnCleanup()
        {

        }
    }
}
