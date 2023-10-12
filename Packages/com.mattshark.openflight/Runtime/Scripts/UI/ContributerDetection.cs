
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContributerDetection : UdonSharpBehaviour
    {
        public AvatarListLoader AvatarListLoader;
        public bool contributerInWorld = false;
        private bool _localPlayerIsContributer = false;
        public bool localPlayerIsContributer
        {
            get
            {
                //check if we should hide the local player's contributer status
                if (hideLocalPlayerContributerStatus)
                {
                    return false;
                }

                return _localPlayerIsContributer;
            }
            set
            {
                _localPlayerIsContributer = value;
            }
        }

        /// <summary> If true, the local player will not have their contributer status broadcasted to everyone else </summary>
        /// <remarks> This does not affect the contributer in world boolean, but does affect the <see cref="localPlayerIsContributer"/> boolean </remarks>
        public bool hideLocalPlayerContributerStatus = false;

        /// <summary>
        /// A formatted list of all the openflight contributers
        /// </summary>
        public string contributersString = "";
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

        //for whatever dumb reason this is needed since the callback doesnt work if it has a optional parameter??????
        public void CheckForContributers()
        {
            CheckForContributers(null);
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

            //format them into a string
            contributersString = "";
            for (int i = 0; i < contributers.Count; i++)
            {
                contributersString += contributers[i].String + ", ";
            }

            //populate our player list
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            //Quickly check if the local player is a contributer
            if (contributers.Contains(Networking.LocalPlayer.displayName))
            {
                localPlayerIsContributer = true;
                Logger.Log("Local player is a contributer!", this);
                contributerInWorld = true;
                return;
            }

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
