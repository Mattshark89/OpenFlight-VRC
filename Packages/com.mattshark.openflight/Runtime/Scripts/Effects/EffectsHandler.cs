
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

        private ParticleSystem.MinMaxGradient gradient;
        void Start()
        {
            //generate a rainbow gradient
            Gradient rainbowGradient = new Gradient();
            //make the gradient loop nicely
            rainbowGradient.SetKeys(
                GenerateRainbow(),
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f) }
            );

            gradient = new ParticleSystem.MinMaxGradient(rainbowGradient);
            gradient.mode = ParticleSystemGradientMode.Gradient;
        }

        private GradientColorKey[] GenerateRainbow()
        {
            //make sure the gradient loops back to the starting color
            GradientColorKey[] rainbow = new GradientColorKey[8];

            for (int i = 0; i < 8; i++)
            {
                var color = Color.HSVToRGB(i / 7f, 1, 1);
                rainbow[i] = new GradientColorKey(color, i / 7f);
            }

            return rainbow;
        }

        //TODO: Make this not so fucking horrible. This organizationally and likely performance wise is HORRIBLE and I hate looking at it like this
        //Ideally, we should switch this entire system over to some form of event based setup. Not sure if that is possible though
        void PostLateUpdate()
        {
            //if we dont have a player then return
            if (playerInfoStore.Owner == null)
                return;

            //check if contributer
            if (playerInfoStore.isContributer)
            {
                //set the trail particles to rainbow start color
                ParticleSystem.MainModule psmain = LeftWingTrail.main;
                psmain.startColor = gradient;

                psmain = RightWingTrail.main;
                psmain.startColor = gradient;
            }
            else
            {
                //set to white
                ParticleSystem.MainModule psmain = LeftWingTrail.main;
                psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);
            }

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

                Util.SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingTrail.gameObject, playerInfoStore.WingtipOffset, playerInfoStore.d_spinetochest);
                Util.SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingTrail.gameObject, playerInfoStore.WingtipOffset, playerInfoStore.d_spinetochest);
            }
            else
            {
                LeftWingTrail.Stop();
                RightWingTrail.Stop();
            }
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
