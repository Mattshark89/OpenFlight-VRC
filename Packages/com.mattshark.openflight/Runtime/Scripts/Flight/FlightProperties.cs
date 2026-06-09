
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using VRC.SDK3.Data;

namespace OpenFlightVRC
{
    public class FlightProperties : LoggableUdonSharpBehaviour
    {
        #region Settings
        /// <summary>
        /// Base strength of a flap. This value is affected by the avatar's armspan. See sizeCurve.
        /// </summary>
        /// <seealso cref="sizeCurve"/>
        [Header("Basic Settings")]
        // Both of these "base" values are by default affected by the avatar's armspan. See sizeCurve.
        [Tooltip("Want flaps to be stronger or weaker? Change this value first. (Default: 285)")]
        [Range(100, 800)]
        public int flapStrengthBase = 285;

        /// <summary>
        /// Base gravity while flying.
        /// </summary>
        [Tooltip("Base gravity while flying (Default: 0.4)")]
        public float flightGravityBase = 0.4f;

        /// <summary>
        /// Require the player to jump before flapping can occur? Makes it less likely to trigger a flap by accident.
        /// </summary>
        [Tooltip("Require the player to jump before flapping can occur? Makes it less likely to trigger a flap by accident. (Default: true)")]
        public bool requireJump = true;

        /// <summary>
        /// Allow locomotion (wasd/left joystick) while flying?
        /// </summary>
        [Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
        public bool allowLoco = false;

        /// <summary>
        /// Avatars using the avatar detection system may have wingtip, weight, etc. modifiers intended to personalize how they feel in the air. Set this value to true to use these modifiers or false if you want them disregarded for consistency. (Note: avatar size detection is not an Avatar Modifier; size-related calculations will always apply even if this setting is set to false. see <see cref="useAvatarScale"/>)
        /// </summary>
        [Tooltip(
            "Avatars using the avatar detection or contact system may have wingtip, weight, etc. modifiers intended to personalize how they feel in the air. Set this value to true to use these modifiers or false if you want them disregarded for consistency. (Note: avatar size detection is not an Avatar Modifier; size-related calculations will always apply even if this setting is set to false.) (Default: true)"
        )]
        public bool useAvatarModifiers = true;

        /// <summary>
        /// Use the avatar's scale to affect the physics of flight. This is useful for making smaller avatars feel lighter and larger avatars feel heavier.
        /// </summary>
        [Tooltip("Use the avatar's scale to affect the physics of flight. This is useful for making smaller avatars feel lighter and larger avatars feel heavier. (Default: true)")]
        public bool useAvatarScale = true;

        /// <summary>
        /// Allow gliding?
        /// </summary>
        [Tooltip("Allow gliding. (Default: true)")]
        public bool canGlide = true;

        /// <summary>
        /// Avatars can glide directly from a fall without having to flap first. This behavior is more intuitive for gliding off cliffs, but may cause players to trigger gliding on accident more often when they just want to fall.
        /// </summary>
        [Tooltip(
            "Avatars can glide directly from a fall without having to flap first. This behavior is more intuitive for gliding off cliffs, but may cause players to trigger gliding on accident more often when they just want to fall. (Default: false)"
        )]
        public bool fallToGlide = true;

        /// <summary>
        /// The number of ticks the player must be falling before automatically gliding. This is only used if <see cref="fallToGlide"/> is true.
        /// </summary>
        [Tooltip("The number of ticks the player must be falling before automatically gliding. This is only used if Fall to Glide is enabled. (Default: 20)")]
        [Range(1, 100)]
        public int fallToGlideActivationDelay = 20;

        #region Advanced Settings
        /// <summary>
        /// Angle to offset the gliding direction by from your hands.
        /// </summary>
        [Header("Advanced Settings (Only for specialized use!)")]
        [Tooltip("Angle to offset the gliding direction by from your hands. (Default: 0)")]
        public float glideAngleOffset = 0f;

        /// <summary>
        /// How much Flap Strength and Flight Gravity are affected by an avatar's armspan. Default values will make smaller avis feel lighter and larger avis heavier.
        /// </summary>
        [Tooltip(
            "How much Flap Strength and Flight Gravity are affected by an avatar's armspan. Default values will make smaller avis feel lighter and larger avis heavier."
        )]
        public AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0.05f, 2), new Keyframe(1, 1), new Keyframe(20, 0.00195f));

        /// <summary>
		/// Modifier for horizontal flap strength. Makes flapping forwards easier.
		/// </summary>
		[Tooltip("Modifier for horizontal flap strength. Makes flapping forwards easier. (Default: 1.5)")]
        public float horizontalStrengthMod = 1.5f;

        /// <summary>
        /// How tight you want your turns while gliding. May be dynamically decreased by Avatar Modifier: weight.
        /// </summary>
        /// <remarks>
        /// Do not reduce this below 1; it will break under some weight values if you do
        /// </remarks>
        [Tooltip("How tight you want your turns while gliding. May be dynamically decreased by Avatar Modifier: weight. (Default: 2.5)")]
        [Range(1f, 5f)]
        public float glideControl = 2.5f; // Do not reduce this below 1; it will break under some weight values if you do

        /// <summary>
        /// Slows gliding down over time.
        /// </summary>
        [Tooltip("Slows gliding down over time. (Default: 0.02)")]
        [Range(0f, 0.2f)]
        public float airFriction = 0.02f;

        /// <summary>
        /// If enabled, flight gravity will use Gravity Curve's curve instead of Size Curve's curve multiplied by Flight Gravity Base.
        /// </summary>
        [Tooltip("If enabled, flight gravity will use Gravity Curve's curve instead of Size Curve's curve multiplied by Flight Gravity Base. (Default: false)")]
        public bool useGravityCurve = false;

        /// <summary>
        /// Similar to Size Curve, but instead of modifying Flap Strength, it only affects Gravity. This value is ignored (Size Curve will be used instead) unless Use Gravity Curve is enabled.
        /// </summary>
        [Tooltip(
            "Similar to Size Curve, but instead of modifying Flap Strength, it only affects Gravity. This value is ignored (Size Curve will be used instead) unless Use Gravity Curve is enabled."
        )]
        public AnimationCurve gravityCurve = new AnimationCurve(new Keyframe(0.05f, 0.4f), new Keyframe(1, 0.2f), new Keyframe(20, 0.00039f));

        /// <summary>
		/// If a GameObject with a TextMeshPro component is attached here, debug some basic info into it. (Default: unset)
		/// </summary>
		[Tooltip("If a GameObject with a TextMeshPro component is attached here, debug some basic info into it. (Default: unset)")]
        public TextMeshProUGUI debugOutput;

        /// <summary>
		/// If enabled, banking to the left or right will force the player to rotate.
		/// </summary>
		/// <remarks>
		/// Can possibly cause network lag, but in testing it doesnt seem to.
		/// </remarks>
		[Tooltip("Banking to the left or right will force the player to rotate. (Default: true)")]
		public bool bankingTurns = true;

        /// <summary>
        /// If enabled, gravity and movement will be saved each time the user takes off, instead of just at the start of the world.
        /// </summary>
        [Tooltip("If enabled, gravity and movement will be saved each time the user takes off, instead of just at the start of the world. (Default: false)")]
        public bool dynamicPlayerPhysics = false;

        /// <summary>
        /// If enabled then OpenFlight will pop up with a notification if your avatar can fly
        /// </summary>
        [Tooltip("If enabled, send notifications to players whenever flight status is changed, for example swapping into flight capable avatar or enabling flight manually or through contacts.")]
        public bool notifications = true;

        /// <summary>
        /// Udon behavior handling the contact system, for telling an avatar it is flying and an avatar telling OpenFlight it can fly.
        /// </summary>
        [Tooltip("Has to link to the correct contact udon behavior for contact detection and sending to work.")]
        public Contact.AvatarContacts AviContact;

        /// <summary>
        /// Udon behavior detecting which avatar is used, also holds the values for weight and wingtipoffset.
        /// </summary>
        public AvatarDetection AviDetect;
		#endregion
        #endregion

        [FieldChangeCallback(nameof(isFlying))]
		private bool _isFlying = false;
        
        [HideInInspector]
        /// <summary> If true, the player is currently flying. </summary>
        public bool isFlying // Currently in the air after/during a flap
        {
            get { return _isFlying; }
            set
            {
                if (value == _isFlying)
                {
                    return;
                }
                _isFlying = value;

                //forward the event to the AvatarContacts handler
                AviContact.OnFlyingChanged(_isFlying);
            }
        }

        [HideInInspector]
        /// <summary> If true, the player is currently gliding. </summary>
        public bool isGliding = false; // Has arms out while flying

        [HideInInspector]
		/// <summary> If true, the player is currently in the process of flapping. </summary>
		public bool isFlapping = false; // Doing the arm motion
        
        [HideInInspector]
		public float armspan = 1f;

        // State Control Variables
        /// <summary>
        /// Cached reference to the local player. This is set in Start() and should not be modified.
        /// </summary>
        private VRCPlayerApi LocalPlayer;
        

        public void Start()
		{
			LocalPlayer = Networking.LocalPlayer;
		}

        private readonly Collider[] _colliders = new Collider[50];
		/// <summary>
		/// Utility method to detect main menu status. Technique pulled from <see href="https://github.com/Superbstingray/UdonPlayerPlatformHook">UdonPlayerPlatformHook</see>
		/// </summary>
		/// <returns>True if the main menu is open, false otherwise</returns>
		internal bool IsMainMenuOpen()
		{
			const int layer = 2 << 18;
			//swapped from OverlapSphere to OverlapSphereNonAlloc, as it does not allocate memory each time it is called,
			//saving on garbage collection. Also doesnt require a .Length check, as it returns the number of colliders it found inherently.
			//Second note, instead of using localplayer position, we use the head position, as the player position can desync depending on Holoport and VRC changes.
			int uiColliderCount = Physics.OverlapSphereNonAlloc(LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, 10f, _colliders, layer);
			//commented out due to extern count, this uses 3
			//return uiColliderCount == 8 || uiColliderCount == 9 || uiColliderCount == 10;

			//this uses 2 externs
			return 8 <= uiColliderCount && uiColliderCount <= 10;
		}

        /// <summary>
		/// Utility method to detect if the player is in a station.
		/// </summary>
		/// <returns>True if the player is in a station, false otherwise</returns>
		internal bool IsPlayerInStation()
		{
			//player local layer, which is layer 9
			const int layer = 2 << 9;
			int colliderCount = Physics.OverlapSphereNonAlloc(LocalPlayer.GetPosition(), 50f, _colliders, layer);

			//if the count is 0, then we can garuntee the player is in a station
			if (colliderCount == 0)
			{
				return true;
			}

			//loop over the colliders and if there is colliders that are null, then the player is not in a station
			for (int i = 0; i < colliderCount; i++)
			{
				if (_colliders[i] == null)
				{
					return false;
				}
			}

			return true;
		}

        internal float GetFlapStrength()
		{
			float flapStrengthMod = useAvatarModifiers ? AviDetect.WingtipOffset * 8 : 10;

			return sizeCurve.Evaluate(GetArmspanValue()) * (flapStrengthBase + flapStrengthMod);
		}
        

        /// <summary>
		/// Returns the current armspan value, or a value of 1 if <see cref="useAvatarScale"/> is false.
		/// </summary>
		/// <returns></returns>
		private float GetArmspanValue()
		{
			return useAvatarScale ? armspan : 1.0f;
		}

        internal float GetFlightGravity()
		{
			float gravity = useGravityCurve
				? gravityCurve.Evaluate(GetArmspanValue()) * GetArmspanValue()
				: sizeCurve.Evaluate(GetArmspanValue()) * flightGravityBase * GetArmspanValue();

			if (useAvatarModifiers)
			{
				// default settings
				return gravity * AviDetect.weight;
			}
			return gravity;
		}

        /// <summary>
		/// Stores the default values of all settings fields in the script.
		/// </summary>
		private DataDictionary defaultsStore;

        /// <summary>
        /// Initializes all default values. This should not be called by end users in most cases.
        /// </summary>
        public void InitializeDefaults()
        {
            defaultsStore = new DataDictionary();
            defaultsStore.SetValue((DataToken)nameof(flapStrengthBase), flapStrengthBase);
            defaultsStore.SetValue((DataToken)nameof(flightGravityBase), flightGravityBase);
            defaultsStore.SetValue((DataToken)nameof(requireJump), requireJump);
            defaultsStore.SetValue((DataToken)nameof(allowLoco), allowLoco);
            defaultsStore.SetValue((DataToken)nameof(useAvatarModifiers), useAvatarModifiers);
            //defaultsStore.SetValue((DataToken)nameof(wingtipOffset), wingtipOffset);
            defaultsStore.SetValue((DataToken)nameof(canGlide), canGlide);
            defaultsStore.SetValue((DataToken)nameof(fallToGlide), fallToGlide);
            defaultsStore.SetValue((DataToken)nameof(horizontalStrengthMod), horizontalStrengthMod);
            defaultsStore.SetValue((DataToken)nameof(glideControl), glideControl);
            defaultsStore.SetValue((DataToken)nameof(airFriction), airFriction);
            defaultsStore.SetValue((DataToken)nameof(useGravityCurve), useGravityCurve);
            defaultsStore.SetValue((DataToken)nameof(bankingTurns), bankingTurns);
            defaultsStore.SetValue((DataToken)nameof(glideAngleOffset), glideAngleOffset);
            defaultsStore.SetValue((DataToken)nameof(useAvatarScale), useAvatarScale);
            defaultsStore.SetValue((DataToken)nameof(fallToGlideActivationDelay), fallToGlideActivationDelay);
            Logger.Log(string.Format("Defaults initialized ({0} values).", defaultsStore.Count), this);
        }

        /// <summary>
        /// Restores all values to their prefab defaults
        /// </summary>
        public void RestoreDefaults()
        {
            flapStrengthBase = GetDefaultValue(nameof(flapStrengthBase)).Int;
            flightGravityBase = GetDefaultValue(nameof(flightGravityBase)).Float;
            requireJump = GetDefaultValue(nameof(requireJump)).Boolean;
            allowLoco = GetDefaultValue(nameof(allowLoco)).Boolean;
            useAvatarModifiers = GetDefaultValue(nameof(useAvatarModifiers)).Boolean;
            //wingtipOffset = GetDefaultValue(nameof(wingtipOffset)).Float;
            canGlide = GetDefaultValue(nameof(canGlide)).Boolean;
            fallToGlide = GetDefaultValue(nameof(fallToGlide)).Boolean;
            horizontalStrengthMod = GetDefaultValue(nameof(horizontalStrengthMod)).Float;
            glideControl = GetDefaultValue(nameof(glideControl)).Float;
            airFriction = GetDefaultValue(nameof(airFriction)).Float;
            useGravityCurve = GetDefaultValue(nameof(useGravityCurve)).Boolean;
            bankingTurns = GetDefaultValue(nameof(bankingTurns)).Boolean;
            glideAngleOffset = GetDefaultValue(nameof(glideAngleOffset)).Float;
            useAvatarScale = GetDefaultValue(nameof(useAvatarScale)).Boolean;
            fallToGlideActivationDelay = GetDefaultValue(nameof(fallToGlideActivationDelay)).Int;
            Logger.Log(string.Format("Defaults restored ({0} values).", defaultsStore.Count), this);
        }
        
        /// <summary>
		/// Gets the default value for a given key. If the key is not found, a warning is logged and a default value of 0 is returned.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private DataToken GetDefaultValue(string key)
		{
			if (defaultsStore.TryGetValue(key, out DataToken value))
			{
				return value;
			}
			else
			{
				Logger.LogError("Key not found in defaults store: " + key, this);
				return new DataToken(0);
			}
		}
    }
}
