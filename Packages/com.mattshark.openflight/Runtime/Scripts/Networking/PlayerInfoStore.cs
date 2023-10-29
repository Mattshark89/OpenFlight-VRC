
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Engines;
using OpenFlightVRC.Effects;
using OpenFlightVRC.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Net
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerInfoStore : UdonSharpBehaviour
    {
        /// <summary> Current player on this object, null if none </summary>
        public VRCPlayerApi Owner;

        [FieldChangeCallback(nameof(IsFlying))]
        private bool _isFlying;
        public bool IsFlying
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

        [FieldChangeCallback(nameof(IsFlapping))]
        private bool _isFlapping;
        public bool IsFlapping
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
        [FieldChangeCallback(nameof(IsContributer))]
        private bool _isContributer;
        public bool IsContributer
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

        [UdonSynced, FieldChangeCallback(nameof(WorldWingtipOffset))]
        private float _WorldWingtipOffset;
        public float WorldWingtipOffset
        {
            get { return _WorldWingtipOffset; }
            set
            {
                //if the value is the same, return instead of setting it
                if (value == _WorldWingtipOffset)
                {
                    return;
                }

                _WorldWingtipOffset = value;

                //if local player, request serialization
                if (_isLocalPlayer)
                {
                    RequestSerialization();
                }
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(PackedData))]
        private byte _packedData;
        /// <summary>
        /// The packed data for this player. When set, it will unpack the data and set the values accordingly, and request serialization
        /// </summary>
        /// <remarks>
        /// The data is packed as follows:
        /// 0: IsFlying
        /// 1: IsFlapping
        /// 2: IsContributer
        /// </remarks>
        public byte PackedData
        {
            get { return _packedData; }
            set
            {
                //if the value is the same, return instead of setting it
                if (value == _packedData)
                {
                    return;
                }
                _packedData = value;

                //unpack the data
                bool[] unpackedData = Util.BitUnpackBool(_packedData);

                //set the values
                IsFlying = unpackedData[0];
                IsFlapping = unpackedData[1];
                IsContributer = unpackedData[2];

                //if local player, request serialization
                if (_isLocalPlayer)
                {
                    RequestSerialization();
                }
            }
        }

        private bool _isLocalPlayer;

        internal AvatarDetection avatarDetection;
        internal WingFlightPlusGlide wingFlightPlusGlide;
        internal OpenFlight openFlight;
        internal ContributerDetection contributerDetection;
        public EffectsHandler effectsHandler;

        void Start() { }

        void Update()
        {
            //check to make sure both scripts are available. If they arent, return
            if (avatarDetection == null || wingFlightPlusGlide == null || openFlight == null)
            {
                return;
            }

            //if the local player owns this object, update the values
            if (_isLocalPlayer)
            {
                //IsFlying = wingFlightPlusGlide.isFlying;
                //IsFlapping = wingFlightPlusGlide.isFlapping;
                //IsContributer = contributerDetection.localPlayerIsContributer;
                PackedData = Util.BitPackBool(wingFlightPlusGlide.isFlying, wingFlightPlusGlide.isFlapping, contributerDetection.localPlayerIsContributer);
                WorldWingtipOffset = avatarDetection.WingtipOffset * (float)avatarDetection.d_spinetochest;
            }
        }

        public void _OnOwnerSet()
        {
            Logger.Log("Owner set to " + Owner.displayName, this);
            effectsHandler.OwnerChanged();
            _isLocalPlayer = Networking.LocalPlayer == Owner;
        }

        public void _OnCleanup()
        {

        }
    }
}
