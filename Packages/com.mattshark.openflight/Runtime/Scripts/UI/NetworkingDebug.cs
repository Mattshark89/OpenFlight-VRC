using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using OpenFlightVRC.Integrations.Cyan.PlayerObjectPool;
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

		private string _text = "";
		private VRCPlayerApi[] _players = new VRCPlayerApi[0];

		/// <summary>
		/// The current player being updated in the player list
		/// </summary>
		private int _currentPlayerBeingUpdated = 0;
		private int _playerCount = 0;

		void Update()
		{
			//The whole reason this code looks like this is to spread out the work over multiple frames
			if (_currentPlayerBeingUpdated == 0)
			{
				//push the text from the last frame to the UI
				Text.text = _text;

				//reset the text
				_text = "";
				_playerCount = VRCPlayerApi.GetPlayerCount();
				_players = new VRCPlayerApi[_playerCount];
				VRCPlayerApi.GetPlayers(_players);
				_text += string.Format(
					"Network CLogged: {0}\nNetwork Settled: {1}\nPlayer Count: {2}\n",
					Networking.IsClogged,
					Networking.IsNetworkSettled,
					_playerCount
				);
			}

			VRCPlayerApi player = _players[_currentPlayerBeingUpdated];
			//check if the player is valid
			if (player.IsValid())
			{
				_text += QueryForPlayer(player) + "\n";
			}

			_currentPlayerBeingUpdated++;

			//if we have updated all the players, reset the counter
			if (_currentPlayerBeingUpdated + 1 > _playerCount)
			{
				_currentPlayerBeingUpdated = 0;
			}
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

			return playerHeader
				+ "\n"
				+ tab
				+ "latency (ms): "
				+ latency
				+ "\n"
				+ tab
				+ "Extracted Data:\n"
				+ tab
				+ tab
				+ "isFlying: "
				+ store.IsFlying
				+ "\n"
				+ tab
				+ tab
				+ "isFlapping: "
				+ store.IsFlapping
				+ "\n"
				+ tab
				+ tab
				+ "isContributer: "
				+ store.IsContributer
				+ "\n"
				+ tab
				+ "Synched Data:\n"
				+ tab
				+ tab
				+ "Packed Data: "
				+ "0b"
				+ packedData
				+ "\n"
				+ tab
				+ tab
				+ "WorldWingtipOffset: "
				+ store.WorldWingtipOffset
				+ "\n"
				+ "---------------------";
		}
	}
}
