
using System;
using OpenFlightVRC.Net;
using UdonSharp;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Components;
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
        private Animator animatorController;

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

            //get the animator controller
            animatorController = GetComponent<Animator>();
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

        public void OwnerChanged()
        {
            //set the owner of the trail objects
            Networking.SetOwner(playerInfoStore.Owner, LeftWingTrail.gameObject);
            Networking.SetOwner(playerInfoStore.Owner, RightWingTrail.gameObject);
        }

        //TODO: Make this not so fucking horrible. This organizationally and likely performance wise is HORRIBLE and I hate looking at it like this
        //Ideally, we should switch this entire system over to some form of event based setup. Not sure if that is possible though
        void Update()
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

                psmain = RightWingTrail.main;
                psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);
            }

            //continually move ourselves to the player's position
            transform.position = playerInfoStore.Owner.GetPosition();

            //if gliding, play the trails
            //make sure this is before the animator updates so the trails teleport BEFORE emitting
            //local player only. We use VRC Object syncs on the trails
            //This is stupidly needed because we cant get the tracking data of remote players, it just returns the bone data instead
            if (playerInfoStore.Owner.isLocal)
            {
                //set the wingtip transforms
                Util.SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), LeftWingTrail.gameObject, playerInfoStore.avatarDetection.WingtipOffset, playerInfoStore.avatarDetection.d_spinetochest);
                Util.SetWingtipTransform(playerInfoStore.Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), RightWingTrail.gameObject, playerInfoStore.avatarDetection.WingtipOffset, playerInfoStore.avatarDetection.d_spinetochest);
            }

            //Push the values into the animator controller
            animatorController.SetBool("isFlapping", playerInfoStore.isFlapping);
            animatorController.SetBool("isGliding", playerInfoStore.isGliding);
            animatorController.SetBool("isFlying", playerInfoStore.isFlying);
            animatorController.SetBool("SFX", SFX);
            animatorController.SetBool("VFX", VFX);
            //animatorController.SetBool("isContributer", playerInfoStore.isContributer);

            float playerVelocity = playerInfoStore.Owner.GetVelocity().magnitude;
            animatorController.SetBool("aboveGlideVelocity", playerVelocity > minGlideVelocity);

            if (playerInfoStore.isGliding && SFX)
            {
                //set the pitch of the glide sound based on the player's velocity
                float pitch = Mathf.Lerp(minGlidePitch, maxGlidePitch, Mathf.InverseLerp(minGlideVelocity, maxGlideVelocity, playerVelocity));
                GlideSound.pitch = pitch;
            }
        }
    }
}
