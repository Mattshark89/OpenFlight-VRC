
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

        [UdonSynced, FieldChangeCallback(nameof(isFlying))]
        private bool _isFlying;
        public bool isFlying
        {
            get { return _isFlying; }
            set
            {
                if (value == _isFlying)
                {
                    return;
                }
                _isFlying = value;

                //forward the event to the effects handler
                effectsHandler.OnFlyingChanged(value);
            }
        }
        [UdonSynced, FieldChangeCallback(nameof(isGliding))]
        private bool _isGliding;
        public bool isGliding
        {
            get { return _isGliding; }
            set
            {
                //if the value is the same, return instead of setting it
                if (value == _isGliding)
                {
                    return;
                }
                _isGliding = value;

                //forward the event to the effects handler
                effectsHandler.OnGlideChanged(value);
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(isFlapping))]
        private bool _isFlapping;
        public bool isFlapping
        {
            get { return _isFlapping; }
            set
            {
                //if the value is the same, return instead of setting it
                if (value == _isFlapping)
                {
                    return;
                }
                _isFlapping = value;

                //forward the event to the effects handler
                effectsHandler.OnFlappingChanged(value);
            }
        }
        [UdonSynced]
        public string flightMode = "Auto";
        [UdonSynced, FieldChangeCallback(nameof(isContributer))]
        private bool _isContributer;
        public bool isContributer
        {
            get { return _isContributer; }
            set
            {
                //if the value is the same, return instead of setting it
                if (value == _isContributer)
                {
                    return;
                }
                _isContributer = value;

                effectsHandler.OnContributerChanged(value);
            }
        }

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
