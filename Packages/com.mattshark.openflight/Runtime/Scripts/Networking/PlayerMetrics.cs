
using System;

using TMPro;

using UdonSharp;

using UnityEngine;

using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.Persistence;
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
    //TODO: Possible make this coroutine sync every like 5 to 10 seconds?
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(VRCEnablePersistence))]
    public class PlayerMetrics : CallbackUdonSharpBehaviour<PlayerMetricsCallback>
    {
        public override string _logCategory { get => "Metrics"; }
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

        [UdonSynced]
        public bool initialized = false;
        #endregion

        public const string MetricsBackupKey = Util.playerDataFolderKey + "MetricsBackup";

        public WingFlightPlusGlide WingFlightPlusGlide;
        private VRCPlayerApi Owner;
        private VRCPlayerApi LocalPlayer;

        void Start()
        {
            Owner = Networking.GetOwner(gameObject);
            LocalPlayer = Networking.LocalPlayer;

            if (Owner == LocalPlayer)
            {
                //subscribe to the flight start and end events
                WingFlightPlusGlide.AddCallback(WingFlightPlusGlideCallback.TakeOff, this, nameof(FlightStart));
                WingFlightPlusGlide.AddCallback(WingFlightPlusGlideCallback.Land, this, nameof(FlightEnd));
                WingFlightPlusGlide.AddCallback(WingFlightPlusGlideCallback.Flap, this, nameof(IncrementFlapCount));
                //save the current position
                lastPosition = Owner.GetPosition();
            }
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            //if not local player, dont worry about it
            if (player != LocalPlayer)
                return;

            //check if initialized
            if (initialized)
            {
                //that means we have proper object data, so instead we should update the backup
                if (VRCJson.TrySerializeToJson(SerializeData(), JsonExportType.Minify, out DataToken jsonData))
                {
                    PlayerData.SetString(MetricsBackupKey, jsonData.String);
                    Log(LogLevel.Info, "PlayerMetrics backup data updated successfully");
                }
                else
                {
                    Log(LogLevel.Error, "PlayerMetrics backup data could not be serialized, backup has not been updated!");
                }
            }
            else
            {
                Log(LogLevel.Info, "PlayerMetrics not initialized, attempting to restore potential backup data");
                //this means we either lost the object data or never had any to begin with
                //to determine further, check the player data
                if (PlayerData.TryGetString(player, MetricsBackupKey, out string backupData))
                {
                    //if we have backup data, deserialize it
                    if (VRCJson.TryDeserializeFromJson(backupData, out DataToken result))
                    {
                        Log(LogLevel.Info, "PlayerMetrics backup data restored successfully");
                        DeserializeData(result.DataDictionary);
                    }
                    else
                    {
                        Log(LogLevel.Error, "PlayerMetrics backup data could not be deserialized, backup has not been restored!");
                    }
                }
                else
                {
                    Log(LogLevel.Warning, "PlayerMetrics backup data not found, no backup data to restore!");
                }
            }

            initialized = true;
            RequestSerialization();
        }

        private DataDictionary SerializeData()
        {
            DataDictionary data = new DataDictionary();
            data.Add(new DataToken(nameof(TicksSpentFlying)), TicksSpentFlying);
            data.Add(new DataToken(nameof(FlapCount)), FlapCount);
            data.Add(new DataToken(nameof(DistanceTraveled)), DistanceTraveled);
            return data;
        }

        private void DeserializeData(DataDictionary data)
        {
            Util.TryApplySetting(data, nameof(TicksSpentFlying), ref TicksSpentFlying);
            Util.TryApplySetting(data, nameof(FlapCount), ref FlapCount);
            Util.TryApplySetting(data, nameof(DistanceTraveled), ref DistanceTraveled);
            RequestSerialization();
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
