
using OpenFlightVRC.Net;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// future random idea im storing here
// if you are a openflight contributer, use rainbow particles instead of the default
// allow them to be disabled in the settings
namespace OpenFlightVRC.Effects
{
    public class EffectsHandler : UdonSharpBehaviour
    {
        public PlayerInfoStore _playerInfoStore;

        public ParticleSystem LeftWingTrail;
        public ParticleSystem RightWingTrail;
        void Start()
        {

        }

        void Update()
        {
            //if gliding, play the trails
            if (_playerInfoStore.isGliding)
            {
                //if not playing
                if (!LeftWingTrail.isPlaying)
                {
                    LeftWingTrail.Play();
                    RightWingTrail.Play();
                }

                SetWingtipTransform(_playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingTrail.gameObject);
                SetWingtipTransform(_playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingTrail.gameObject);
            }
            else
            {
                LeftWingTrail.Stop();
                RightWingTrail.Stop();
            }
        }

        private void SetWingtipTransform(VRCPlayerApi.TrackingData data, GameObject wingtip)
        {
            Vector3 position = data.position;
            Quaternion rotation = data.rotation;

            Vector3 WingTipPosition = position + (rotation * new Vector3(0, 0, (float)_playerInfoStore.WingtipOffset * (float)_playerInfoStore.d_spinetochest));

            wingtip.transform.position = WingTipPosition;
        }
    }
}
