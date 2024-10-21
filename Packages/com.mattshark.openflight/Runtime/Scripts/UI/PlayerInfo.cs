/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using OpenFlightVRC.Net;
using System;
using OpenFlightVRC.Effects;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// Controls the text information on the networking tab of the tablet
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class PlayerInfo : LoggableUdonSharpBehaviour
	{
		public TextMeshProUGUI GlobalInfo;
		public PlayerUIDropdown playerDropdown;
		public TextMeshProUGUI playerInfoText;

		void Start()
		{
			//playerDropdown.AddCallback(PlayerUIDropdownCallback.ValueChanged, this, nameof(OnDropdownChange));
		}

		void Update()
		{
			int playerCount = VRCPlayerApi.GetPlayerCount();

			//get the general global information about the network
			GlobalInfo.text = string.Format(
					"Network Clogged: {0}\nNetwork Settled: {1}\nPlayer Count: {2}\n",
					Networking.IsClogged,
					Networking.IsNetworkSettled,
					playerCount
				);

			//get and validate the player
			VRCPlayerApi player = playerDropdown.selectedPlayer;
			if (player == null || !Utilities.IsValid(player))
			{
				playerInfoText.text = "Player is not valid";
				return;
			}

			//player is valid past this point
			float IKLatency = (Time.realtimeSinceStartup - Networking.SimulationTime(player)) * 1000;
			IKLatency = Mathf.Round(IKLatency);

			//we need to get the players storage object for both settings and for metrics
			PoolObjectReferenceManager reference = Util.GetPlayerObjectOfType<PoolObjectReferenceManager>(player);
			PlayerSettings settings = reference.PlayerSettingsStore;
			PlayerMetrics metrics = reference.PlayerMetricsStore;
			PlayerEffects effects = reference.PlayerEffects;

			string packedData = Convert.ToString(effects.PackedData, 2).PadLeft(8, '0');

			playerInfoText.text = string.Format(
@"Player ID: {0}
Basic Info:
	IK Latency: {1}ms
Metrics:
	Time Spent Flying: {2}
	Flap Count: {3}
	Distance Traveled: {4}m
Effects System:
	Packed Data: 0b{5}
	isFlying: {6}
	isFlapping: {7}
	isContributer: {8}
Settings System:
	DB Size: {9} bytes
	Slot Count: {10}
",
				player.playerId,
				IKLatency,
				TimeSpan.FromTicks(metrics.TicksSpentFlying).ToString(@"d\:hh\:mm\:ss\:fff"),
				metrics.FlapCount,
				Math.Round(metrics.DistanceTraveled, 2),
				packedData,
				effects.IsFlying,
				effects.IsFlapping,
				effects.IsContributer,
				settings._RemoteSpaceUsed(),
				settings._GetSlotCount(true)
			);
		}
	}
}
