/**
 * @ Maintainer: Happyrobot33
 */

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
	/// <summary>
	/// Controls the text information on the networking tab of the tablet
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class NetworkingDebug : LoggableUdonSharpBehaviour
	{
		private CyanPlayerObjectAssigner _assigner;
		public UdonBehaviour assignerProxy;
		private TextMeshProUGUI _TMP;

		void Start()
		{
			//get the real assigner from the proxy, if it exists
			if (assignerProxy.GetProgramVariable("target") != null)
			{
				//get the assigner as a component first
				UdonBehaviour assignerBehaviour = (UdonBehaviour)assignerProxy.GetProgramVariable("target");
				_assigner = (CyanPlayerObjectAssigner)assignerBehaviour.GetComponent(typeof(UdonBehaviour));
			}

			_TMP = GetComponent<TextMeshProUGUI>();
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
				_TMP.text = _text;

				//reset the text
				_text = "";
				_playerCount = VRCPlayerApi.GetPlayerCount();
				_players = new VRCPlayerApi[_playerCount];
				VRCPlayerApi.GetPlayers(_players);
				_text += string.Format(
					"Network Clogged: {0}\nNetwork Settled: {1}\nPlayer Count: {2}\n",
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
			Component behaviour = _assigner._GetPlayerPooledUdon(player);

			//if null, player is not in the pool yet
			if (behaviour == null)
			{
				return player.displayName + " not in pool";
			}

			PlayerInfoStore store = (PlayerInfoStore)behaviour;

			string playerHeader = "<b>" + player.displayName + "</b>";

			//estimate ping based on time since startup and simulation time
			//in MS
			float IKLatency = (Time.realtimeSinceStartup - Networking.SimulationTime(player)) * 1000;
			IKLatency = Mathf.Round(IKLatency);
			float PlayerStorelatency = (Time.realtimeSinceStartup - Networking.SimulationTime(store.gameObject)) * 1000;
			PlayerStorelatency = Mathf.Round(PlayerStorelatency);

			//get the packed data in a bit string
			string packedData = Convert.ToString(store.PackedData, 2).PadLeft(8, '0');

			const string tab = "  ";

			return playerHeader
				+ "\n"
				+ tab
				+ "IK Latency (ms): "
				+ IKLatency
				+ "\n"
				+ tab
				+ "Player Store Latency (ms): "
				+ PlayerStorelatency
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
