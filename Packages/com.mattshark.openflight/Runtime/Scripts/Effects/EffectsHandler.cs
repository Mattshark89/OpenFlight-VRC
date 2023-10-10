
using System;
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
        public bool SFX = true;
        public AudioSource FlapSound;

        public AudioSource GlideSound;
        public float minGlidePitch = 0.5f;
        public float maxGlidePitch = 1.5f;
        public float minGlideVelocity = 5f;
        public float maxGlideVelocity = 20f;

        //TODO: Make this not so fucking horrible. This organizationally and likely performance wise is HORRIBLE and I hate looking at it like this
        //Ideally, we should switch this entire system over to some form of event based setup. Not sure if that is possible though
        void PostLateUpdate()
        {
            //if we dont have a player then return
            if (playerInfoStore.Owner == null)
                return;

            //continually move ourselves to the player's position
            transform.position = playerInfoStore.Owner.GetPosition();

            if (playerInfoStore.isFlapping && SFX)
            {
                StartPlaying(FlapSound);
            }
            else
            {
                FlapSound.Stop();
            }

            if (playerInfoStore.isGliding && SFX)
            {
                //check if the player is going fast enough for the gliding sound to play
                float playerVelocity = playerInfoStore.Owner.GetVelocity().magnitude;

                if (playerVelocity > minGlideVelocity)
                {
                    //set the pitch of the glide sound based on the player's velocity
                    float pitch = Mathf.Lerp(minGlidePitch, maxGlidePitch, Mathf.InverseLerp(minGlideVelocity, maxGlideVelocity, playerVelocity));
                    GlideSound.pitch = pitch;
                    StartPlaying(GlideSound);
                }
                else
                {
                    GlideSound.Stop();
                }
            }
            else
            {
                GlideSound.Stop();
            }

            //if gliding, play the trails
            if (playerInfoStore.isGliding && VFX)
            {
                StartPlaying(LeftWingTrail);
                StartPlaying(RightWingTrail);

                SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingTrail.gameObject);
                SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingTrail.gameObject);
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

            Vector3 WingTipPosition = position + (rotation * new Vector3(0, 0, (float)playerInfoStore.WingtipOffset * (float)playerInfoStore.d_spinetochest));

            wingtip.transform.position = WingTipPosition;
        }

        /// <summary>
        /// This can take in a particle system or audio source, and will start it playing if it is not already, otherwise it does nothing
        /// </summary>
        /// <param name="effect"></param>
        private void StartPlaying(Component effect)
        {
            Type type = effect.GetType();

            if (type == typeof(ParticleSystem))
            {
                ParticleSystem particleSystem = (ParticleSystem)effect;

                if (!particleSystem.isPlaying)
                {
                    particleSystem.Play();
                }
            }
            else if (type == typeof(AudioSource))
            {
                AudioSource audioSource = (AudioSource)effect;

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
        }
    }
}
