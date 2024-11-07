/**
 * @ Maintainer: Happyrobot33
 */

using OpenFlightVRC.Effects;
using OpenFlightVRC.UI;
using UdonSharp;
using VRC.SDKBase;
using VRC.SDK3.Data;

namespace OpenFlightVRC.Net
{
    /// <summary>
    /// Manages the local global pool settings, such as if VFX, SFX are enabled on all the effects handlers
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PoolGlobalManager : LoggableUdonSharpBehaviour
    {
        public const string POOLGLOBALCATEGORY = "Pool Managers";
        public override string _logCategory { get => POOLGLOBALCATEGORY; }
        public bool VFX = true;
        private bool _VFX_last = true;
        public bool SFX = true;
        private bool _SFX_last = true;
        public float volume = 1;
        private float _volume_last = 1;

        private DataList stores;

        void Update()
        {
            if (VFX != _VFX_last || SFX != _SFX_last || volume != _volume_last)
            {
                UpdateAllEffectsHandlers();

                _VFX_last = VFX;
                _SFX_last = SFX;
                _volume_last = volume;
            }
        }

        /// <summary>
        /// Registers a store to be updated when the global settings change
        /// </summary>
        /// <param name="store"></param>
        internal void RegisterStore(DataToken store)
        {
            if (stores == null)
            {
                stores = new DataList();
            }
            stores.Add(store);

            PlayerEffects EffectsHandler = (PlayerEffects)store.Reference;
            if (EffectsHandler != null)
            {
                UpdateEffectHandler(EffectsHandler);
            }
        }

        /// <summary>
        /// Unregisters a store from being updated when the global settings change
        /// </summary>
        /// <param name="store"></param>
        internal void UnregisterStore(DataToken store)
        {
            if (stores == null)
            {
                return;
            }
            stores.Remove(store);
        }

        private void UpdateAllEffectsHandlers()
        {
            foreach (DataToken store in stores.ToArray())
            {
                PlayerEffects EffectsHandler = (PlayerEffects)store.Reference;
                if (EffectsHandler != null)
                {
                    UpdateEffectHandler(EffectsHandler);
                }
            }
        }

        private void UpdateEffectHandler(PlayerEffects EffectsHandler)
        {
            EffectsHandler.VFX = VFX;
            EffectsHandler.SFX = SFX;
            EffectsHandler.volume = volume;
        }
    }
}
