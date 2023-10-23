
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

        [Header("VFX")]
        public bool VFX = true;
        public ParticleSystem LeftWingTrail;
        public ParticleSystem RightWingTrail;

        [Header("Sounds")]
        public bool SFX = true;
        public AudioSource FlapSound;

        public AudioSource GlideSound;
        [Tooltip("Controls the pitch of the glide sound based on the player's velocity. Horizontal axis is velocity, vertical axis is pitch")]
        public AnimationCurve glidePitchCurve;
        public AnimationCurve glideVolumeCurve;
        public AnimationCurve trailParticleSizeCurve;
        [Tooltip("Controls the start speed of the trail particles based on the player's velocity. Horizontal axis is velocity, vertical axis is start speed")]
        public AnimationCurve startSpeedCurve;

        private ParticleSystem.MinMaxGradient gradient;
        void Start()
        {
            gradient = new ParticleSystem.MinMaxGradient(GetRainbowGradient());
            gradient.mode = ParticleSystemGradientMode.Gradient;
        }

        internal void OwnerChanged()
        {
            //set the owner of the trail objects
            Networking.SetOwner(playerInfoStore.Owner, LeftWingTrail.gameObject);
            Networking.SetOwner(playerInfoStore.Owner, RightWingTrail.gameObject);
        }

        /// <summary>
        /// Called when the player's flying variable changes
        /// </summary>
        /// <param name="boolState">The state of the flying bool for the player</param>
        internal void OnFlyingChanged(bool boolState)
        {
            ControlSound(GlideSound, SFX && boolState);
        }

        /// <summary>
        /// Called when the player's gliding variable changes
        /// </summary>
        /// <param name="boolState">The state of the gliding bool for the player</param>
        internal void OnGlideChanged(bool boolState)
        {
            //ControlSound(GlideSound, SFX && boolState);

            SetParticleSystemEmission(LeftWingTrail, VFX && boolState);
            SetParticleSystemEmission(RightWingTrail, VFX && boolState);
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
            }
            else
            {
                //set to white
                ParticleSystem.MainModule psmain = LeftWingTrail.main;
                psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);

                psmain = RightWingTrail.main;
                psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);
            }
        }

        void Update()
        {
            //if we dont have a player then return
            if (playerInfoStore.Owner == null)
                return;

            //continually move ourselves to the player's chest
            transform.position = playerInfoStore.Owner.GetBonePosition(HumanBodyBones.Chest);

            //Audio Changing
            if (SFX)
            {
                float playerVelocity = playerInfoStore.Owner.GetVelocity().magnitude;
                if (playerInfoStore.isGliding)
                {
                    //set the pitch of the glide sound based on the player's velocity
                    //float pitch = Mathf.Lerp(minGlidePitch, maxGlidePitch, Mathf.InverseLerp(minGlideVelocity, maxGlideVelocity, playerVelocity));
                    float pitch = glidePitchCurve.Evaluate(playerVelocity);
                    GlideSound.pitch = pitch;

                    //set the volume of the glide sound based on the player's velocity
                    //float volume = Mathf.Lerp(minGlideVolume, maxGlideVolume, Mathf.InverseLerp(minGlideVelocity, maxGlideVelocity, playerVelocity));
                    float volume = glideVolumeCurve.Evaluate(playerVelocity);
                    GlideSound.volume = volume;
                }
            }
            else
            {
                //if SFX is off, stop the glide sound
                GlideSound.Stop();
            }

            if (VFX)
            {
                if (playerInfoStore.isGliding)
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

                    //local player only. We use VRC Object syncs on the trails
                    //This is stupidly needed because we cant get the tracking data of remote players, it just returns the bone data instead
                    if (playerInfoStore.Owner.isLocal)
                    {
                        //set the wingtip transforms
                        SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingTrail.gameObject, playerInfoStore.avatarDetection.WingtipOffset, playerInfoStore.avatarDetection.d_spinetochest);
                        SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingTrail.gameObject, playerInfoStore.avatarDetection.WingtipOffset, playerInfoStore.avatarDetection.d_spinetochest);
                    }
                }
            }
            else
            {
                //if VFX is off, stop the glide sound
                LeftWingTrail.Stop();
                RightWingTrail.Stop();
            }
        }
    }
}
