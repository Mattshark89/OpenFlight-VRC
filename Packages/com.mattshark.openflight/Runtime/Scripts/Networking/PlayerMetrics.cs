
using System;
using TMPro;
using UdonSharp;
using UnityEngine;

using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Net
{
    public enum PlayerMetricsCallback
    {
        /// <summary>
        /// Called when the data changes
        /// </summary>
        OnDataChanged
    }
    //TODO: Make this also back up to the player data system like the settings do, or it will be lost on ID mismatch!
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(VRCEnablePersistence))]
    public class PlayerMetrics : CallbackUdonSharpBehaviour<PlayerMetricsCallback>
    {
        #region Metrics
        #region Flying Time
        /// <summary>
        /// The total time spent flying
        /// </summary>
        /// <remarks>
        /// Can store all the way up to 10675199:02:48:05:477 (Days, Hours, Minutes, Seconds, Milliseconds)
        /// </remarks>
        [UdonSynced]
        public long TicksSpentFlying = 0;
        private long takeOffTick = 0;
        #endregion
        [UdonSynced]
        public long FlapCount = 0;
        #region Distance Traveled
        [UdonSynced]
        public float DistanceTraveled = 0;
        private Vector3 lastPosition;
        #endregion
        private bool isFlying = false;
        #endregion

        public WingFlightPlusGlide WingFlightPlusGlide;
        private VRCPlayerApi Owner;
        private VRCPlayerApi LocalPlayer;

        void Start()
        {
            Owner = Networking.GetOwner(gameObject);
            LocalPlayer = Networking.LocalPlayer;

            if(Owner == LocalPlayer)
            {
                //subscribe to the flight start and end events
                WingFlightPlusGlide.AddCallback(WingFlightPlusGlideCallback.TakeOff, this, nameof(FlightStart));
                WingFlightPlusGlide.AddCallback(WingFlightPlusGlideCallback.Land, this, nameof(FlightEnd));
                WingFlightPlusGlide.AddCallback(WingFlightPlusGlideCallback.Flap, this, nameof(IncrementFlapCount));
                //save the current position
                lastPosition = Owner.GetPosition();
            }
        }

        void Update()
        {
            if (Owner == LocalPlayer && isFlying)
            {
                float distance = Vector3.Distance(lastPosition, Owner.GetPosition());

                //if the distance is too large, its likely a teleport or respawn, so we ignore it
                if (distance < 100)
                    DistanceTraveled += distance;
                
                lastPosition = Owner.GetPosition();
            }
        }

        void OnDestroy()
        {
            if (Owner == LocalPlayer)
            {
                WingFlightPlusGlide.RemoveCallback(WingFlightPlusGlideCallback.TakeOff, this, nameof(FlightStart));
                WingFlightPlusGlide.RemoveCallback(WingFlightPlusGlideCallback.Land, this, nameof(FlightEnd));
                WingFlightPlusGlide.RemoveCallback(WingFlightPlusGlideCallback.Flap, this, nameof(IncrementFlapCount));
            }
        }

        public void IncrementFlapCount()
        {
            FlapCount++;
        }

        public void FlightStart()
        {
            takeOffTick = DateTime.Now.Ticks;
            isFlying = true;
        }

        public void FlightEnd()
        {
            TicksSpentFlying += DateTime.Now.Ticks - takeOffTick;
            isFlying = false;
            //Only serialize everything if the player lands
            RequestSerialization();
            RunCallback(PlayerMetricsCallback.OnDataChanged);
        }

        public override void OnDeserialization()
        {
            RunCallback(PlayerMetricsCallback.OnDataChanged);
        }
    }
}
