
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
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EffectsHandler : UdonSharpBehaviour
    {
        public PlayerInfoStore playerInfoStore;

        [Header("VFX")]
        public bool VFX = true;
        public ParticleSystem LeftWingTrail;
        public ParticleSystem RightWingTrail;

        [Header("Sounds")]
        public bool Sounds = true;
        public AudioSource FlapSound;
        public AudioSource GlideSound;

        void Update()
        {
            //if every performance control is off just completely skip
            if (!VFX && !Sounds)
                return;

            //if we dont have a player then return
            if (playerInfoStore.Owner == null)
                return;

            //continually move ourselves to the player's position
            transform.position = playerInfoStore.Owner.GetPosition();

            #region Gliding
            //if gliding, play the trails
            if (playerInfoStore.isGliding)
            {
                #region Trails
                if (VFX)
                {
                    StartPlaying(LeftWingTrail);
                    StartPlaying(RightWingTrail);

                    SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingTrail.gameObject);
                    SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingTrail.gameObject);
                }
                #endregion

                #region Sounds
                if (Sounds)
                    StartPlaying(GlideSound);
                #endregion
            }
            else
            {
                LeftWingTrail.Stop();
                RightWingTrail.Stop();

                //TODO: Make the audio fade out instead of stopping
                GlideSound.Stop();
            }
            #endregion
        }

        private void SetWingtipTransform(VRCPlayerApi.TrackingData data, GameObject wingtip)
        {
            Vector3 position = data.position;
            Quaternion rotation = data.rotation;

            Vector3 WingTipPosition = position + (rotation * new Vector3(0, 0, (float)playerInfoStore.WingtipOffset * (float)playerInfoStore.d_spinetochest));

            wingtip.transform.position = WingTipPosition;
        }

        /// <summary>
        /// This can take in a particle system or audio source, and will start it playing if it is not already, otherwise it does nothing
        /// </summary>
        /// <param name="effect"></param>
        private void StartPlaying(Component effect)
        {
            switch (effect)
            {
                case ParticleSystem particleSystem:
                    if (!particleSystem.isPlaying)
                        particleSystem.Play();
                    break;
                case AudioSource audioSource:
                    if (!audioSource.isPlaying)
                        audioSource.Play();
                    break;
                default:
                    break;
            }
        }
    }
}
