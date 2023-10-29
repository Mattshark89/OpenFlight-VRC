
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

        private DataList contributers = new DataList();
        private DataList contributersInWorld = new DataList();
        void Start()
        {
            //subscribe to the avatar list loader callback
            AvatarListLoader.AddCallback(this, "GetContributersList");
        }

        public void GetContributersList()
        {
            //deserialize
            bool success = VRCJson.TryDeserializeFromJson(AvatarListLoader.Output, out DataToken json);

            //grab the Contributers array
            contributers = json.DataDictionary["Contributers"].DataList;

            //quickly check if the local player is a contributer
            if (contributers.Contains(Networking.LocalPlayer.displayName))
            {
                localPlayerIsContributer = true;
                Logger.Log("Local player is a contributer!", this);
                contributerInWorld = true;
                contributersInWorld.Add(Networking.LocalPlayer.displayName);
            }

            //check all players for contributer status
            CheckForContributers();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            //check if the player is a contributer
            if (contributers.Contains(player.displayName))
            {
                Logger.Log(player.displayName + " is a contributer!", this);
                contributerInWorld = true;
                contributersInWorld.Add(player.displayName);
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            //check if the user that left was a contributer
            if (contributers.Contains(player.displayName) && contributerInWorld)
            {
                Logger.Log("Player that left was a contributer! Checking for remaining contributers...", this);
                contributersInWorld.Remove(player.displayName);

                //check if there are any contributers left
                if (contributersInWorld.Count == 0)
                {
                    Logger.Log("No contributers left!", this);
                    contributerInWorld = false;
                }
                else
                {
                    Logger.Log("Contributers left: " + contributersInWorld.Count, this);
                }
            }
        }

        /// <summary>
        /// Checks if there are any contributers in the instance
        /// </summary>
        public void CheckForContributers()
        {
            Logger.Log("Checking for contributers...", this);

            //format them into a string
            contributersString = "";
            for (int i = 0; i < contributers.Count; i++)
            {
                contributersString += contributers[i].String + ", ";
            }

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

                //check if the player is a contributer
                if (contributers.Contains(player.displayName))
                {
                    Logger.Log(player.displayName + " is a contributer!", this);
                    contributerInWorld = true;
                    contributersInWorld.Add(player.displayName);
                }
            }

            //check if there are any contributers in the instance
            if (!contributerInWorld)
            {
                Logger.Log("No contributers in the instance!", this);
            }
        }
    }
}
