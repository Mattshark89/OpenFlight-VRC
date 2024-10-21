
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Net
{
    /// <summary>
    /// The goal of this class is purely to act as a variable reference to various child pooled objects
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(VRCPlayerObject))]
    public class PoolObjectReferenceManager : LoggableUdonSharpBehaviour
    {
        public Effects.PlayerEffects PlayerEffects;
        public PlayerSettings PlayerSettingsStore;
        public PlayerMetrics PlayerMetricsStore;

        void Start()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            gameObject.name = string.Format("{0}'s OF Net", owner.displayName);
        }
    }
}
