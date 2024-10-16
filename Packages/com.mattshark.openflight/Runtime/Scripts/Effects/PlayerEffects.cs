/**
 * @ Maintainer: Happyrobot33
 */

using OpenFlightVRC.Net;
using OpenFlightVRC.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using static OpenFlightVRC.Util;

namespace OpenFlightVRC.Effects
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	/// <summary>
	/// This class is used to store and manage effects information, such as if the player is flying, flapping, or a contributer.
	/// </summary>
	public class PlayerEffects : LoggableUdonSharpBehaviour
	{
		public PoolGlobalManager PoolGlobalManager;

		#region Synced Variables
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

				//wakeup / sleep logic
				if (value)
				{
					HandleWingtips();
				}

				//if set to false and not the local players
				if (!value && !isLocalPlayersObject)
				{
					//if the player is not the local player, we need to delay the particles and sound
					DelayedFlyingShutoff = true;
				}

				ControlSound(GlideSound, SFX && value);
				float secondsToWait = CalculateNetworkLatencyDelay();
				if (VFX && !value)
				{
					SendCustomEventDelayedSeconds(nameof(DelayedLandingParticlesTrigger), secondsToWait);
				}

				if (VFX && value)
				{
					//start the particles. This ensures the synched objects are in the right place
					SendCustomEventDelayedSeconds(nameof(DelayedTrailParticlesStart), secondsToWait);
				}
				else
				{
					//stop the particles
					SendCustomEventDelayedSeconds(nameof(DelayedTrailParticlesStop), secondsToWait);
				}
			}
		}
		/// <summary>
		/// This is used to keep updating the particles and sound when a remote player is flying until the network latency delay is over
		/// </summary>
		private bool DelayedFlyingShutoff = false;

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
				//if the value is the same, dont set it
				if (value == _isFlapping)
				{
					return;
				}
				_isFlapping = value;

				//if SFX is on and rising edge of flapping
				if (SFX && value)
				{
					//play the flap sound
					FlapSound.PlayOneShot(FlapSound.clip);
				}
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

				//check if contributer
				if (value)
				{
					//set the trail particles to rainbow start color
					ParticleSystem.MainModule psmain = LeftWingTrail.main;
					psmain.startColor = _gradient;

					psmain = RightWingTrail.main;
					psmain.startColor = _gradient;

					psmain = LandingParticles.main;
					//make a copy of gradient
					ParticleSystem.MinMaxGradient rainbowGradient = _gradient;
					rainbowGradient.mode = ParticleSystemGradientMode.RandomColor;
					psmain.startColor = rainbowGradient;
				}
				else
				{
					//set to white
					ParticleSystem.MainModule psmain = LeftWingTrail.main;
					psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);

					psmain = RightWingTrail.main;
					psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);

					psmain = LandingParticles.main;
					psmain.startColor = new ParticleSystem.MinMaxGradient(Color.white);
				}
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
				if (isLocalPlayersObject)
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
				if (isLocalPlayersObject)
				{
					RequestSerialization();
				}
			}
		}
		#endregion

		#region User Settings
		[FieldChangeCallback(nameof(VFX))]
		private bool _VFX;
		public bool VFX
		{
			get { return _VFX; }
			set
			{
				if (value == _VFX)
				{
					return;
				}

				//if rising edge
				if (value)
				{
					//make sure effects are near the hands before starting
					HandleWingtips();

					LeftWingTrail.Play();
					RightWingTrail.Play();
				}

				//if falling edge
				if (!value)
				{
					LeftWingTrail.Stop();
					RightWingTrail.Stop();
					LeftWingTrail.Clear();
					RightWingTrail.Clear();
				}

				_VFX = value;
			}
		}

		[FieldChangeCallback(nameof(SFX))]
		private bool _SFX;
		public bool SFX
		{
			get { return _SFX; }
			set
			{
				if (value == _SFX)
				{
					return;
				}

				ControlSound(GlideSound, value && IsFlying);

				_SFX = value;
			}
		}

		[FieldChangeCallback(nameof(volume))]
		private float _volume;
		public float volume
		{
			get { return _volume; }
			set
			{
				if (value == _volume)
				{
					return;
				}

				//set the volume of the flap sound
				FlapSound.volume = value;

				//disabled as the glide sound is already dynamically controlled by the player's velocity, so we do the math there
				//set the volume of the glide sound
				//GlideSound.volume = value;

				_volume = value;
			}
		}
		#endregion

		public ParticleSystem LeftWingTrail;
		public GameObject LeftWingForceField;
		public ParticleSystem RightWingTrail;
		public GameObject RightWingForceField;
		public ParticleSystem LandingParticles;
		public GameObject SoundObject;
		public AudioSource FlapSound;
		public AudioSource GlideSound;

		[Tooltip("Controls the pitch of the glide sound based on the player's velocity. Horizontal axis is velocity, vertical axis is pitch")]
		public AnimationCurve glidePitchCurve;
		public AnimationCurve glideVolumeCurve;
		public AnimationCurve trailParticleSizeCurve;

		[Tooltip("Controls the start speed of the trail particles based on the player's velocity. Horizontal axis is velocity, vertical axis is start speed")]
		public AnimationCurve startSpeedCurve;

		private ParticleSystem.MinMaxGradient _gradient;

		[Header("Helpers")]
		public GameObject LeftHandRotation;
		public GameObject RightHandRotation;

		private VRCPlayerApi Owner;
		private bool isLocalPlayersObject;

		void Start()
        {
            _gradient = new ParticleSystem.MinMaxGradient(GetRainbowGradient());
            _gradient.mode = ParticleSystemGradientMode.Gradient;

            SFX = true;
            VFX = true;
            volume = 1f;

			//print the owner of this object
            Owner = Networking.GetOwner(gameObject);
            isLocalPlayersObject = Owner.isLocal;
            gameObject.name = Owner.displayName + "'s OF Effects";

			//register ourselves with the pool global manager
			PoolGlobalManager.RegisterStore(this);

			//set the owner of the trail objects
			//TODO: Check if this is even needed, as im pretty sure player objects garuntee the ownership anyway
            Networking.SetOwner(Owner, LeftHandRotation);
            Networking.SetOwner(Owner, RightHandRotation);

            //set our names to owned
            LeftHandRotation.name = Owner.displayName + "'s LeftHandRotation";
            RightHandRotation.name = Owner.displayName + "'s RightHandRotation";
        }

		void OnDestroy()
		{
			//unregister ourselves with the pool global manager
			PoolGlobalManager.UnregisterStore(this);
		}

		private float CalculateNetworkLatencyDelay()
		{
			//if we exit flying, play the landing particles
			float secondsToWait = Time.realtimeSinceStartup - Networking.SimulationTime(Owner);
			//cap it at 3 seconds due to the editor being odd
			secondsToWait = Mathf.Clamp(secondsToWait, 0, 1);
			//if unity editor, cap to 50ms since thats what it should be in desktop
#if UNITY_EDITOR
            secondsToWait = 0.05f;
#endif
			return secondsToWait;
		}

		public void DelayedLandingParticlesTrigger()
		{
			//move the landing particles to the player's feet
			LandingParticles.transform.position = Owner.GetPosition();

			//trigger burst particles
			LandingParticles.Emit(50);
		}

		public void DelayedTrailParticlesStart()
		{
			//start the particles
			SetParticleSystemEmission(LeftWingTrail, true);
			SetParticleSystemEmission(RightWingTrail, true);
			//enable the force fields
			LeftWingForceField.SetActive(true);
			RightWingForceField.SetActive(true);
		}

		public void DelayedTrailParticlesStop()
		{
			//stop the particles
			SetParticleSystemEmission(LeftWingTrail, false);
			SetParticleSystemEmission(RightWingTrail, false);
			//disable the force fields
			LeftWingForceField.SetActive(false);
			RightWingForceField.SetActive(false);

			DelayedFlyingShutoff = false;
		}

		public WingFlightPlusGlide WingFlightPlusGlide;
		public ContributerDetection ContributerDetection;
		public AvatarDetection AvatarDetection;

		//this is post late update to ensure the latest IK data is available
		public override void PostLateUpdate()
		{
			if (isLocalPlayersObject)
			{
				PackedData = Util.BitPackBool(WingFlightPlusGlide.isFlying, WingFlightPlusGlide.isFlapping, ContributerDetection.localPlayerIsContributer);
				WorldWingtipOffset = AvatarDetection.WingtipOffset * (float)AvatarDetection.d_spinetochest;
			}

			if (IsFlying || DelayedFlyingShutoff)
			{
				//local player only. We use VRC Object syncs on the trails
				//This is stupidly needed because we cant get the tracking data of remote players, it just returns the bone data instead
				if (isLocalPlayersObject)
				{
					//set the rotation store objects to the player's hand rotation
					LeftHandRotation.transform.rotation = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
					RightHandRotation.transform.rotation = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
				}

				//Audio Changing
				if (SFX)
				{
					#region Audio
					//continually move the audio to the player's chest
					SoundObject.transform.position = Owner.GetBonePosition(HumanBodyBones.Chest);
					float playerVelocity = Owner.GetVelocity().magnitude;
					//set the pitch of the glide sound based on the player's velocity
					float pitch = glidePitchCurve.Evaluate(playerVelocity);
					GlideSound.pitch = pitch;

					//set the volume of the glide sound based on the player's velocity, scaled by the volume variable
					float volume = glideVolumeCurve.Evaluate(playerVelocity);
					GlideSound.volume = volume * this.volume;
					#endregion
				}

				if (VFX)
				{
					#region VFX
					//adjust the start size of the trails based on the player's velocity
					float playerVelocity = Owner.GetVelocity().magnitude;
					float size = trailParticleSizeCurve.Evaluate(playerVelocity);
					//adjust the start speed of the trails based on the player's velocity
					float startSpeed = startSpeedCurve.Evaluate(playerVelocity);

					ParticleSystem.MainModule psmain = LeftWingTrail.main;
					psmain.startSize = size;
					psmain.startSpeed = startSpeed;

					psmain = RightWingTrail.main;
					psmain.startSize = size;
					psmain.startSpeed = startSpeed;

					HandleWingtips();
					#endregion
				}
			}

		}

		private void HandleWingtips()
		{
			//check if the player is valid
			if (Owner == null)
			{
				return;
			}

			//check if valid
			if (!Owner.IsValid())
			{
				return;
			}

			//copy the rotational information from the rotation store objects to the trail objects
			//instead of copying the position, we use the bone data so the position is always accurate
			//this implementation DOES mean the rotation will still lag ahead/behind the player, but it should be less noticeable than the position
			LeftWingTrail.transform.position = Owner.GetBonePosition(HumanBodyBones.LeftHand);
			LeftWingTrail.transform.rotation = LeftHandRotation.transform.rotation;
			RightWingTrail.transform.position = Owner.GetBonePosition(HumanBodyBones.RightHand);
			RightWingTrail.transform.rotation = RightHandRotation.transform.rotation;

			//set the wingtip transforms
			SetWingtipTransform(LeftWingTrail.gameObject, WorldWingtipOffset);
			SetWingtipTransform(RightWingTrail.gameObject, WorldWingtipOffset);
		}
	}
}
