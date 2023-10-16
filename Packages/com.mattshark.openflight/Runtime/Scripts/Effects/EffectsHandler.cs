
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
                    else
                    {
                        AccountForNetworkTimeTravel(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingTrail.gameObject, LeftWingtip);
                        AccountForNetworkTimeTravel(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingTrail.gameObject, RightWingtip);
                    }
                }
            }
        }

        /// <summary>
        /// Accounts for VRCObjectSync time travelling forward / backwards in time relative to remote players.
        /// We account for this by essentially teleporting the wingtip to the correct position but still keeping the offset that the player has sent us in the VRCObjectSync
        /// </summary>
        /// <remarks>This does NOT account for the rotation desync, but this should be way less noticable compared to the position desync</remarks>
        /// <param name="bone">The tracking data of the bone</param>
        /// <param name="EffectEndpoint">The wingtip transform</param>
        /// <param name="SyncedObject">The VRCObjectSync object</param>
        private void AccountForNetworkTimeTravel(VRCPlayerApi.TrackingData bone, GameObject EffectEndpoint, GameObject SyncedObject)
        {
            //get the current position of the networked object
            Vector3 currentPos = SyncedObject.transform.position;

            //get the position of the bone
            Vector3 bonePos = bone.position;

            //calculate the difference between the two
            Vector3 difference = bonePos - currentPos;

            //offset the wingtip by the difference
            EffectEndpoint.transform.position += difference;
        }
    }
}
