/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using OpenFlightVRC.Integrations.Cyan.PlayerObjectPool;
using OpenFlightVRC.UI;
using OpenFlightVRC.Effects;

// This script is used to initialize the local player's store so it has the correct references
namespace OpenFlightVRC.Net
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class PoolController : LoggableUdonSharpBehaviour
	{
		public CyanPlayerObjectAssigner Assigner;
		public AvatarDetection avatarDetection;
		public FlightProperties flightProperties;
		public OpenFlight openFlight;
		public ContributerDetection contributerDetection;

		private bool _SFX_OLD = true;
		public bool SFX = true;
		private bool _VFX_OLD = true;
		public bool VFX = true;

		private float _volume_OLD = 1f;

		[Range(0f, 1f)]
		public float volume = 1f;

		private EffectsHandler[] _effectHandlers = new EffectsHandler[0];

		void Update()
		{
			//TODO: make this not run every frame, and instead do it on property change
			//gotta figure out why GetProgramVariable doesnt like propertys
			if (_SFX_OLD == SFX && _VFX_OLD == VFX && _volume_OLD == volume)
				return;

			//set the variables of each effect handler
			foreach (EffectsHandler handler in _effectHandlers)
			{
				handler.SFX = SFX;
				handler.VFX = VFX;
				handler.volume = volume;
			}

			_SFX_OLD = SFX;
			_VFX_OLD = VFX;
			_volume_OLD = volume;
		}

		public void _OnLocalPlayerAssigned()
		{
			//get the local player's store
			Component behaviour = Assigner._GetPlayerPooledUdon(Networking.LocalPlayer);

			PlayerInfoStore store = (PlayerInfoStore)behaviour;

			//set the values
			store.AvatarDetection = avatarDetection;
			store.flightProperties = flightProperties;
			store.OpenFlight = openFlight;
			store.ContributerDetection = contributerDetection;

			#region Effects Handler Array Initialization
			//get every pooled udon object
			Component[] behaviours = Assigner.pooledUdon;

			//init the effect handlers array
			_effectHandlers = new EffectsHandler[behaviours.Length];

			//loop through each one
			foreach (Component b in behaviours)
			{
				//get effect handler underneath them
				EffectsHandler handler = b.GetComponentInChildren<EffectsHandler>();

				//add it to the array
				_effectHandlers[System.Array.IndexOf(behaviours, b)] = handler;
			}
			#endregion
		}
	}
}
