
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
        public float WingtipOffset = 0;
        [UdonSynced]
        public double d_spinetochest = 0;
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

        [Header("References set by Local Store Initializer")]
        public AvatarDetection _avatarDetection;
        public WingFlightPlusGlide _wingFlightPlusGlide;
        public OpenFlight _openFlight;
        public ContributerDetection _contributerDetection;

        void Start()
        {

        }

        void Update()
        {
            //check to make sure both scripts are available. If they arent, return
            if (_avatarDetection == null || _wingFlightPlusGlide == null || _openFlight == null)
            {
                return;
            }

            //if the local player owns this object, update the values
            if (Networking.LocalPlayer == Owner)
            {
                WingtipOffset = _avatarDetection.WingtipOffset;
                d_spinetochest = _avatarDetection.d_spinetochest;
                isFlying = _wingFlightPlusGlide.isFlying;
                isGliding = _wingFlightPlusGlide.isGliding;
                isFlapping = _wingFlightPlusGlide.isFlapping;
                flightMode = _openFlight.flightMode;
                isContributer = _contributerDetection.localPlayerIsContributer;
            }
        }

        public void _OnOwnerSet()
        {
            Logger.Log("Owner set to " + Owner.displayName, this);
        }

        public void _OnCleanup()
        {

        }
    }
}
