
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;
using OpenFlightVRC.UI;
using OpenFlightVRC.Effects;

// This script is used to initialize the local player's store so it has the correct references
namespace OpenFlightVRC.Net
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PoolController : UdonSharpBehaviour
    {
        public CyanPlayerObjectAssigner Assigner;
        public AvatarDetection avatarDetection;
        public WingFlightPlusGlide wingFlightPlusGlide;
        public OpenFlight openFlight;
        public ContributerDetection contributerDetection;

        public bool SFX = true;
        public bool VFX = true;

        private EffectsHandler[] EffectHandlers = new EffectsHandler[0];

        void Update()
        {
            //set the variables of each effect handler
            foreach (EffectsHandler handler in EffectHandlers)
            {
                handler.SFX = SFX;
                handler.VFX = VFX;
            }
        }

        public void _OnLocalPlayerAssigned()
        {
            //get the local player's store
            Component behaviour = Assigner._GetPlayerPooledUdon(Networking.LocalPlayer);

            PlayerInfoStore store = (PlayerInfoStore)behaviour;

            //set the values
            store.avatarDetection = avatarDetection;
            store.wingFlightPlusGlide = wingFlightPlusGlide;
            store.openFlight = openFlight;
            store.contributerDetection = contributerDetection;

            #region Effects Handler Array Initialization
            //get every pooled udon object
            Component[] behaviours = Assigner.pooledUdon;

            //init the effect handlers array
            EffectHandlers = new EffectsHandler[behaviours.Length];

            //loop through each one
            foreach (Component b in behaviours)
            {
                //get effect handler underneath them
                EffectsHandler handler = b.GetComponentInChildren<EffectsHandler>();

                //add it to the array
                EffectHandlers[System.Array.IndexOf(behaviours, b)] = handler;
            }
            #endregion
        }
    }
}
