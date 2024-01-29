using OpenFlightVRC.Effects;
using OpenFlightVRC.UI;
using UdonSharp;
using VRC.SDKBase;

namespace OpenFlightVRC.Net
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class PlayerInfoStore : LoggableUdonSharpBehaviour
	{
		/// <summary> Current player on this object, null if none </summary>
		public VRCPlayerApi Owner;

		[FieldChangeCallback(nameof(IsFlying))]
		private bool _isFlying;
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
		internal WingFlightPlusGlide WingFlightPlusGlide;
		internal OpenFlight OpenFlight;
		internal ContributerDetection ContributerDetection;
		public EffectsHandler effectsHandler;

		void Update()
		{
			//check to make sure both scripts are available. If they arent, return
			if (AvatarDetection == null || WingFlightPlusGlide == null || OpenFlight == null)
			{
				return;
			}

			//if the local player owns this object, update the values
			if (_isLocalPlayer)
			{
				//IsFlying = wingFlightPlusGlide.isFlying;
				//IsFlapping = wingFlightPlusGlide.isFlapping;
				//IsContributer = contributerDetection.localPlayerIsContributer;
				PackedData = Util.BitPackBool(WingFlightPlusGlide.isFlying, WingFlightPlusGlide.isFlapping, ContributerDetection.localPlayerIsContributer);
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
