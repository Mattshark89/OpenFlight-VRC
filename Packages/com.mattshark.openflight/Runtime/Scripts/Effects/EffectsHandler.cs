
using System;
using OpenFlightVRC.Net;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using static OpenFlightVRC.Util;

namespace OpenFlightVRC.Effects
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EffectsHandler : UdonSharpBehaviour
    {
        public PlayerInfoStore playerInfoStore;

        [FieldChangeCallback(nameof(VFX))]
        private bool _VFX;
        public bool VFX
        {
            get { return _VFX; }
            set
            {
                if (value == _VFX)
                {
                    return;
                }

                //if rising edge, allow the particles to play
                if (value)
                {
                    //make sure effects are near the hands before starting
                    HandleWingtips();

                    //start the particles
                    LeftWingTrail.Play();
                    RightWingTrail.Play();
                }

                //if falling edge
                if (!value)
                {
                    //stop the particles
                    LeftWingTrail.Stop();
                    RightWingTrail.Stop();
                    //remove all particles
                    LeftWingTrail.Clear();
                    RightWingTrail.Clear();
                }

                _VFX = value;
            }
        }
        public ParticleSystem LeftWingTrail;
        public ParticleSystem RightWingTrail;
        public ParticleSystem LandingParticles;

        [FieldChangeCallback(nameof(SFX))]
        private bool _SFX;
        public bool SFX
        {
            get { return _SFX; }
            set
            {
                if (value == _SFX)
                {
                    return;
                }

                ControlSound(GlideSound, value && playerInfoStore.IsFlying);

                _SFX = value;
            }
        }
        public GameObject SoundObject;
        public AudioSource FlapSound;

        public AudioSource GlideSound;
        [Tooltip("Controls the pitch of the glide sound based on the player's velocity. Horizontal axis is velocity, vertical axis is pitch")]
        public AnimationCurve glidePitchCurve;
        public AnimationCurve glideVolumeCurve;
        public AnimationCurve trailParticleSizeCurve;
        [Tooltip("Controls the start speed of the trail particles based on the player's velocity. Horizontal axis is velocity, vertical axis is start speed")]
        public AnimationCurve startSpeedCurve;

        private ParticleSystem.MinMaxGradient gradient;
        [Header("Helpers")]
        public GameObject LeftHandRotation;
        public GameObject RightHandRotation;
        void Start()
        {
            gradient = new ParticleSystem.MinMaxGradient(GetRainbowGradient());
            gradient.mode = ParticleSystemGradientMode.Gradient;

            SFX = true;
            VFX = true;
        }

        internal void OwnerChanged()
        {
            //set the owner of the trail objects
            Networking.SetOwner(playerInfoStore.Owner, LeftHandRotation);
            Networking.SetOwner(playerInfoStore.Owner, RightHandRotation);

            //initialize to asleep state
            sleeping = true;

            //set our name to owned
            LeftHandRotation.name = playerInfoStore.Owner.displayName + "'s LeftHandRotation";
            RightHandRotation.name = playerInfoStore.Owner.displayName + "'s RightHandRotation";
            LandingParticles.name = playerInfoStore.Owner.displayName + "'s LandingParticles";
        }

        internal void OnCleanup()
        {
            //set our name to unowned
            LeftHandRotation.name = "LeftHandRotation (Unowned)";
            RightHandRotation.name = "RightHandRotation (Unowned)";
            LandingParticles.name = "LandingParticles (Unowned)";
        }

        /// <summary>
        /// Called when the player's flying variable changes
        /// </summary>
        /// <param name="boolState">The state of the flying bool for the player</param>
        internal void OnFlyingChanged(bool boolState)
        {
            //wakeup / sleep logic
            if (boolState)
            {
                sleeping = false;

                HandleWingtips();
            }
            else
            {
                sleeping = true;
            }

            ControlSound(GlideSound, SFX && boolState);
            float secondsToWait = CalculateNetworkLatencyDelay();
            if (VFX && !boolState)
            {
                SendCustomEventDelayedSeconds(nameof(DelayedLandingParticlesTrigger), secondsToWait);
            }

            if (VFX && boolState)
            {
                //start the particles. This ensures the synched objects are in the right place
                SendCustomEventDelayedSeconds(nameof(DelayedTrailParticlesStart), secondsToWait);
            }
            else
            {
                //stop the particles
                SendCustomEventDelayedSeconds(nameof(DelayedTrailParticlesStop), secondsToWait);
            }
        }

        private float CalculateNetworkLatencyDelay()
        {
            //if we exit flying, play the landing particles
            float secondsToWait = Time.realtimeSinceStartup - Networking.SimulationTime(playerInfoStore.Owner);
            //cap it at 3 seconds due to the editor being odd
            secondsToWait = Mathf.Clamp(secondsToWait, 0, 1);
            //if unity editor, cap to 50ms since thats what it should be in desktop
#if UNITY_EDITOR
            secondsToWait = 0.05f;
#endif
            return secondsToWait;
        }

        public void DelayedLandingParticlesTrigger()
        {
            //move the landing particles to the player's feet
            LandingParticles.transform.position = playerInfoStore.Owner.GetPosition();

            //trigger burst particles
            LandingParticles.Emit(50);
        }

        public void DelayedTrailParticlesStart()
        {
            //start the particles
            SetParticleSystemEmission(LeftWingTrail, true);
            SetParticleSystemEmission(RightWingTrail, true);
        }

        public void DelayedTrailParticlesStop()
        {
            //stop the particles
            SetParticleSystemEmission(LeftWingTrail, false);
            SetParticleSystemEmission(RightWingTrail, false);
        }

        /// <summary>
        /// Called when the player's flap variable changes
        /// </summary>
        /// <param name="boolState">The state of the flapping bool for the player</param>
        internal void OnFlappingChanged(bool boolState)
        {
            //if SFX is on and rising edge of flapping
            if (SFX && boolState)
            {
                //play the flap sound
                FlapSound.PlayOneShot(FlapSound.clip);
            }
        }

        /// <summary>
        /// Called when the player's contributer variable changes
        /// </summary>
        /// <param name="boolState">The state of the contributer bool for the player</param>
        internal void OnContributerChanged(bool boolState)
        {
            //check if contributer
            if (boolState)
            {
                //set the trail particles to rainbow start color
                ParticleSystem.MainModule psmain = LeftWingTrail.main;
                psmain.startColor = gradient;

                psmain = RightWingTrail.main;
                psmain.startColor = gradient;

                psmain = LandingParticles.main;
                //make a copy of gradient
                ParticleSystem.MinMaxGradient rainbowGradient = gradient;
                rainbowGradient.mode = ParticleSystemGradientMode.RandomColor;
                psmain.startColor = rainbowGradient;
            }
            else
            {
                //set to white
                ParticleSystem.MainModule psmain = LeftWingTrail.main;
                psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);

                psmain = RightWingTrail.main;
                psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);

                psmain = LandingParticles.main;
                psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);
            }
        }

        private bool sleeping = false;
        void Update()
        {
            //if we dont have a player then return
            if (playerInfoStore.Owner == null)
                return;

            //if we are sleeping, return
            if (sleeping)
                return;
        
            if (playerInfoStore.IsFlying)
            {
                //local player only. We use VRC Object syncs on the trails
                //This is stupidly needed because we cant get the tracking data of remote players, it just returns the bone data instead
                if (playerInfoStore.Owner.isLocal)
                {
                    //set the rotation store objects to the player's hand rotation
                    LeftHandRotation.transform.rotation = playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
                    RightHandRotation.transform.rotation = playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
                }
            }

            //if both are off, return to save on network traffic and performance
            if (!SFX && !VFX)
                return;

            //Audio Changing
            if (SFX)
            {
                //continually move the audio to the player's chest
                SoundObject.transform.position = playerInfoStore.Owner.GetBonePosition(HumanBodyBones.Chest);
                float playerVelocity = playerInfoStore.Owner.GetVelocity().magnitude;
                if (playerInfoStore.IsFlying)
                {
                    //set the pitch of the glide sound based on the player's velocity
                    float pitch = glidePitchCurve.Evaluate(playerVelocity);
                    GlideSound.pitch = pitch;

                    //set the volume of the glide sound based on the player's velocity
                    float volume = glideVolumeCurve.Evaluate(playerVelocity);
                    GlideSound.volume = volume;
                }
            }

            if (VFX)
            {
                if (playerInfoStore.IsFlying)
                {
                    //adjust the start size of the trails based on the player's velocity
                    float playerVelocity = playerInfoStore.Owner.GetVelocity().magnitude;
                    float size = trailParticleSizeCurve.Evaluate(playerVelocity);
                    //adjust the start speed of the trails based on the player's velocity
                    float startSpeed = startSpeedCurve.Evaluate(playerVelocity);

                    ParticleSystem.MainModule psmain = LeftWingTrail.main;
                    psmain.startSize = size;
                    psmain.startSpeed = startSpeed;

                    psmain = RightWingTrail.main;
                    psmain.startSize = size;
                    psmain.startSpeed = startSpeed;

                    HandleWingtips();
                }
            }
        }

        private void HandleWingtips()
        {
            //skip if player undefined
            if (playerInfoStore.Owner == null)
                return;

            //copy the rotational information from the rotation store objects to the trail objects
            //instead of copying the position, we use the bone data so the position is always accurate
            //this implementation DOES mean the rotation will still lag ahead/behind the player, but it should be less noticeable than the position
            LeftWingTrail.transform.position = playerInfoStore.Owner.GetBonePosition(HumanBodyBones.LeftHand);
            LeftWingTrail.transform.rotation = LeftHandRotation.transform.rotation;
            RightWingTrail.transform.position = playerInfoStore.Owner.GetBonePosition(HumanBodyBones.RightHand);
            RightWingTrail.transform.rotation = RightHandRotation.transform.rotation;

            //set the wingtip transforms
            SetWingtipTransform(LeftWingTrail.gameObject, playerInfoStore.WorldWingtipOffset);
            SetWingtipTransform(RightWingTrail.gameObject, playerInfoStore.WorldWingtipOffset);
        }
    }
}
