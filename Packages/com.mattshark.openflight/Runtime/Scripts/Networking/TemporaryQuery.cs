
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;
using TMPro;

namespace OpenFlightVRC.Net
{
    public class TemporaryQuery : UdonSharpBehaviour
    {
        public CyanPlayerObjectAssigner Assigner;
        public TextMeshProUGUI Text;
        void Start()
        {

        }

        void Update()
        {
            //query and show in a list all the information
            string text = "";
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

            return playerHeader + "\n" +
                "WingtipOffset: " + store.WingtipOffset + "\n" +
                "isFlying: " + store.isFlying + "\n" +
                "isGliding: " + store.isGliding + "\n" +
                "isFlapping: " + store.isFlapping + "\n" +
                "flightMode: " + store.flightMode + "\n" +
                "d_spinetochest: " + store.d_spinetochest + "\n" +
                "-------";
        }
    }
}
