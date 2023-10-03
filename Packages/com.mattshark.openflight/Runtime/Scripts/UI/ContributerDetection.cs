
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    public class ContributerDetection : UdonSharpBehaviour
    {
        public AvatarListLoader AvatarListLoader;
        public bool contributerInWorld = false;
        void Start()
        {
            //subscribe to the avatar list loader callback
            AvatarListLoader.AddCallback(this, "CheckForContributers");
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            CheckForContributers();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            CheckForContributers(player);
        }

        /// <summary>
        /// Checks if there are any contributers in the instance
        /// </summary>
        /// <param name="leavingPlayer">The player that is leaving the instance. If this is not null, the player will be ignored when checking for contributers. This is needed for to prevent timing issues</param>
        public void CheckForContributers(VRCPlayerApi leavingPlayer = null)
        {
            Logger.Log("Checking for contributers...", this);

            //check to make sure the output isnt empty
            if (AvatarListLoader.Output == "")
            {
                Logger.LogWarning("Data is empty!", this);
                return;
            }

            //deserialize
            bool success = VRCJson.TryDeserializeFromJson(AvatarListLoader.Output, out DataToken json);

            //grab the Contributers array
            DataList contributers = json.DataDictionary["Contributers"].DataList;

            //populate our player list
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            //loop through each user in the instance and check if they are a contributer
            foreach (VRCPlayerApi player in players)
            {
                //check if the player is valid
                if (!player.IsValid())
                {
                    continue;
                }

                //check to make sure they arent a leaving player
                if (leavingPlayer != null && player.playerId == leavingPlayer.playerId)
                {
                    continue;
                }

                //check if the player is a contributer
                if (contributers.Contains(player.displayName))
                {
                    Logger.Log(player.displayName + " is a contributer!", this);
                    contributerInWorld = true;
                    return;
                }
            }

            //if we get here, no contributers were found
            contributerInWorld = false;
            Logger.Log("No contributers found!", this);
        }
    }
}
