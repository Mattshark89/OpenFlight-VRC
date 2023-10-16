
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
        public GameObject LeftWingtip;
        public ParticleSystem LeftWingTrail;
        public GameObject RightWingtip;
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
            gradient = new ParticleSystem.MinMaxGradient(GetRainbowGradient());
            gradient.mode = ParticleSystemGradientMode.Gradient;
        }

        public void OwnerChanged()
        {
            //set the owner of the trail objects
            Networking.SetOwner(playerInfoStore.Owner, LeftWingTrail.gameObject);
            Networking.SetOwner(playerInfoStore.Owner, RightWingTrail.gameObject);
        }

        /// <summary>
        /// Called when the player's gliding variable changes
        /// </summary>
        /// <param name="boolState">The state of the gliding bool for the player</param>
        public void OnGlideChanged(bool boolState)
        {
            ControlSound(GlideSound, SFX && boolState);

            SetParticleSystemEmission(LeftWingTrail, VFX && boolState);
            SetParticleSystemEmission(RightWingTrail, VFX && boolState);
        }

        /// <summary>
        /// Called when the player's flap variable changes
        /// </summary>
        /// <param name="boolState">The state of the flapping bool for the player</param>
        public void OnFlappingChanged(bool boolState)
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
        public void OnContributerChanged(bool boolState)
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

        /// <summary> Used to detect teleporting </summary>
        Vector3 PlayerPos;
        void Update()
        {
            //if we dont have a player then return
            if (playerInfoStore.Owner == null)
                return;

            //continually move ourselves to the player's chest
            transform.position = playerInfoStore.Owner.GetBonePosition(HumanBodyBones.Chest);

            //detect if the player has been teleported by checking for a large enough delta between the player's position and our position
            if (Vector3.Distance(PlayerPos, transform.position) > 1f)
            {
                //disable emission
                SetParticleSystemEmission(LeftWingTrail, false);
                SetParticleSystemEmission(RightWingTrail, false);
            }

            //Audio Changing
            if (SFX)
            {
                float playerVelocity = playerInfoStore.Owner.GetVelocity().magnitude;
                if (playerInfoStore.isGliding)
                {
                    //set the pitch of the glide sound based on the player's velocity
                    float pitch = Mathf.Lerp(minGlidePitch, maxGlidePitch, Mathf.InverseLerp(minGlideVelocity, maxGlideVelocity, playerVelocity));
                    GlideSound.pitch = pitch;
                }
            }

            if (VFX)
            {
                if (playerInfoStore.isGliding)
                {
                    //local player only. We use VRC Object syncs on the trails
                    //This is stupidly needed because we cant get the tracking data of remote players, it just returns the bone data instead
                    if (playerInfoStore.Owner.isLocal)
                    {
                        //set the wingtip transforms
                        SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingtip, playerInfoStore.avatarDetection.WingtipOffset, playerInfoStore.avatarDetection.d_spinetochest);
                        SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingtip, playerInfoStore.avatarDetection.WingtipOffset, playerInfoStore.avatarDetection.d_spinetochest);
                    }
                }
            }
        }

        void PostLateUpdate()
        {
            //save the player position
            PlayerPos = transform.position;
        }
    }
}
