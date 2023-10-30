
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;
using TMPro;
using OpenFlightVRC.Net;
using System;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NetworkingDebug : LoggableUdonSharpBehaviour
    {
        private CyanPlayerObjectAssigner Assigner;
        public UdonBehaviour assignerProxy;
        private TextMeshProUGUI Text;
        void Start()
        {
            //get the real assigner from the proxy, if it exists
            if (assignerProxy.GetProgramVariable("target") != null)
            {
                //get the assigner as a component first
                UdonBehaviour assignerBehaviour = (UdonBehaviour)assignerProxy.GetProgramVariable("target");
                Assigner = (CyanPlayerObjectAssigner)assignerBehaviour.GetComponent(typeof(UdonBehaviour));
            }

            Text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            //query and show in a list all the information
            string text = "";

            text += "Network Clogged: " + Networking.IsClogged + "\n";
            text += "Network Settled: " + Networking.IsNetworkSettled + "\n";

            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            foreach (VRCPlayerApi player in players)
            {
                text += QueryForPlayer(player) + "\n";
            }

            Text.text = text;
        }

        private string QueryForPlayer(VRCPlayerApi player)
        {
            Component behaviour = Assigner._GetPlayerPooledUdon(player);

            //if null, player is not in the pool yet
            if (behaviour == null)
            {
                return player.displayName + " not in pool";
            }

            PlayerInfoStore store = (PlayerInfoStore)behaviour;

            string playerHeader = "<b>" + player.displayName + "</b>";

            //estimate ping based on time since startup and simulation time
            //in MS
            float latency = (Time.realtimeSinceStartup - Networking.SimulationTime(player)) * 1000;
            //round it
            latency = Mathf.Round(latency);

            //get the packed data in a bit string
            string packedData = Convert.ToString(store.PackedData, 2).PadLeft(8, '0');

            const string tab = "  ";

            return playerHeader + "\n" +
                tab + "latency (ms): " + latency + "\n" +
                tab + "Extracted Data:\n" +
                tab + tab + "isFlying: " + store.IsFlying + "\n" +
                tab + tab + "isFlapping: " + store.IsFlapping + "\n" +
                tab + tab + "isContributer: " + store.IsContributer + "\n" +
                tab + "Synched Data:\n" +
                tab + tab + "Packed Data: " + "0b" + packedData + "\n" +
                tab + tab + "WorldWingtipOffset: " + store.WorldWingtipOffset + "\n" +
                "---------------------";
        }
    }
}
