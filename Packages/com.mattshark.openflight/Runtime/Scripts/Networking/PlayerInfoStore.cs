/**
 * @ Maintainer: Happyrobot33
 */

using OpenFlightVRC.Effects;
using OpenFlightVRC.UI;
using UdonSharp;
using VRC.SDKBase;

namespace OpenFlightVRC.Net
{
	/// <summary>
	/// This class is used to store player information, such as if they are flying, flapping, or a contributer.
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class PlayerInfoStore : LoggableUdonSharpBehaviour
	{
		/// <summary> Current player on this object, null if none </summary>
		public VRCPlayerApi Owner;

		[FieldChangeCallback(nameof(IsFlying))]
		private bool _isFlying;
		/// <summary>
		/// If the player is flying or not. When set, it will forward the event to the effects handler
		/// </summary>
		public bool IsFlying
		{
			get { return _isFlying; }
			set
			{
				if (value == _isFlying)
				{
					return;
				}
				_isFlying = value;

				//forward the event to the effects handler
				effectsHandler.OnFlyingChanged(value);
			}
		}

		[FieldChangeCallback(nameof(IsFlapping))]
		private bool _isFlapping;
		/// <summary>
		/// If the player is flapping or not. When set, it will forward the event to the effects handler
		/// </summary>
		public bool IsFlapping
		{
			get { return _isFlapping; }
			set
			{
				//if the value is the same, return instead of setting it
				if (value == _isFlapping)
				{
					return;
				}
				_isFlapping = value;

				//forward the event to the effects handler
				effectsHandler.OnFlappingChanged(value);
			}
		}

		[FieldChangeCallback(nameof(IsContributer))]
		private bool _isContributer;
		/// <summary>
		/// If the player is a contributer or not. When set, it will forward the event to the effects handler
		/// </summary>
		/// <remarks>
		/// This isnt straightforward due to some sphagetti code when I first implemented the contributers feature.
		/// Needs to be refactored to be more straightforward, as this is just really used as a flag to allow users to hide themselves as a contributer, instead of being the main source of truth on if someone is a contributer
		/// </remarks>
		public bool IsContributer
		{
			get { return _isContributer; }
			set
			{
				//if the value is the same, return instead of setting it
				if (value == _isContributer)
				{
					return;
				}
				_isContributer = value;

				effectsHandler.OnContributerChanged(value);
			}
		}

		[UdonSynced, FieldChangeCallback(nameof(WorldWingtipOffset))]
		private float _WorldWingtipOffset;
		/// <summary>
		/// The world wingtip offset for this player. This is WORLD RELATIVE, NOT player size relative.
		/// </summary>
		public float WorldWingtipOffset
		{
			get { return _WorldWingtipOffset; }
			set
			{
				//if the value is the same, return instead of setting it
				if (value == _WorldWingtipOffset)
				{
					return;
				}

				_WorldWingtipOffset = value;

				//if local player, request serialization
				if (_isLocalPlayer)
				{
					RequestSerialization();
				}
			}
		}

		[UdonSynced, FieldChangeCallback(nameof(PackedData))]
		private byte _packedData;

		/// <summary>
		/// The packed data for this player. When set, it will unpack the data and set the values accordingly, and request serialization
		/// </summary>
		/// <remarks>
		/// The data is packed as follows:
		/// 0: IsFlying
		/// 1: IsFlapping
		/// 2: IsContributer
		/// </remarks>
		public byte PackedData
		{
			get { return _packedData; }
			set
			{
				//if the value is the same, return instead of setting it
				if (value == _packedData)
				{
					return;
				}
				_packedData = value;

				//unpack the data
				bool[] unpackedData = Util.BitUnpackBool(_packedData);

				//set the values
				IsFlying = unpackedData[0];
				IsFlapping = unpackedData[1];
				IsContributer = unpackedData[2];

				//if local player, request serialization
				if (_isLocalPlayer)
				{
					RequestSerialization();
				}
			}
		}

		private bool _isLocalPlayer;

		internal AvatarDetection AvatarDetection;
		internal FlightProperties flightProperties;
		internal OpenFlight OpenFlight;
		internal ContributerDetection ContributerDetection;
		public EffectsHandler effectsHandler;

		void Update()
		{
			//check to make sure both scripts are available. If they arent, return
			if (AvatarDetection == null || flightProperties == null || OpenFlight == null)
			{
				return;
			}

			//if the local player owns this object, update the values
			if (_isLocalPlayer)
			{
				//IsFlying = wingFlightPlusGlide.isFlying;
				//IsFlapping = wingFlightPlusGlide.isFlapping;
				//IsContributer = contributerDetection.localPlayerIsContributer;
				PackedData = Util.BitPackBool(flightProperties.isFlying, flightProperties.isFlapping, ContributerDetection.localPlayerIsContributer);
				WorldWingtipOffset = AvatarDetection.WingtipOffset * (float)AvatarDetection.d_spinetochest;
			}
		}

#pragma warning disable IDE1006 // Naming Styles
		public void _OnOwnerSet()
#pragma warning restore IDE1006 // Naming Styles
		{
			Logger.Log("Owner set to " + Owner.displayName, this);
			effectsHandler.OwnerChanged();
			_isLocalPlayer = Networking.LocalPlayer == Owner;

			//change the name of this object to the player's name
			gameObject.name = Owner.displayName + "'s PlayerInfoStore";
		}

#pragma warning disable IDE1006 // Naming Styles
		public void _OnCleanup()
#pragma warning restore IDE1006 // Naming Styles
		{
			//cleanup and set the name to not owned
			gameObject.name = "PlayerInfoStore (Not Owned)";
			effectsHandler.OnCleanup();
		}
	}
}
